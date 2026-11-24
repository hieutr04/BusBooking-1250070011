// Screenshots and interactions of the actual SQL-backed application, never mock HTML.
import {chromium} from '@playwright/test';
import fs from 'node:fs/promises';
const f=JSON.parse(await fs.readFile(process.env.FIXTURES,'utf8'));
if(!f.database?.endsWith('_Tests'))throw new Error('Use isolated _Tests fixtures with fictitious passengers only.');
const base=process.env.BASE_URL||'http://localhost:5190',results=[],consoleErrors=[];
await fs.mkdir('ui-results',{recursive:true});const browser=await chromium.launch();
const assert=(ok,m)=>{if(!ok)throw new Error(m);};
async function login(data,workspace){const c=await browser.newContext();const p=await c.newPage();p.on('pageerror',e=>consoleErrors.push(e.message));await p.goto(base+'/Account/Login');await p.locator('#Email').fill(data.Email);await p.locator('#Password').fill(data.password);await Promise.all([p.waitForURL(u=>u.pathname===workspace),p.getByRole('button',{name:'Đăng nhập →'}).click()]);return {c,p};}
try{
 const publicContext=await browser.newContext();const guest=await publicContext.newPage();guest.on('pageerror',e=>consoleErrors.push(e.message));
 const admin=await login(f.admin,'/Admin'),staff=await login(f.staff,'/Staff'),driver=await login(f.driver,'/Driver');
 const customer=await login(f.customers[0],'/');
 const routes=[['home',guest,'/'],['results',guest,'/Trips?date='+new Date(Date.now()+86400000).toISOString().slice(0,10)],['seats',guest,'/Trips/Details/'+f.tripId],['login',guest,'/Account/Login'],['admin',admin.p,'/Admin'],['admin-table',admin.p,'/Admin/Trips'],['admin-form',admin.p,'/Admin/TripEdit'],['staff',staff.p,'/Staff'],['driver',driver.p,'/Driver'],['my-tickets',customer.p,'/Bookings']];
 for(const width of [375,390,768,1024,1440])for(const [name,p,url] of routes){
  await p.setViewportSize({width,height:1000});const response=await p.goto(base+url);assert(response.status()===200,`${name} HTTP ${response.status()}`);
  await p.evaluate(()=>document.fonts.ready);assert(await p.evaluate(()=>document.documentElement.scrollWidth<=innerWidth),`${name} overflow at${width}`);
  await p.screenshot({path:`ui-results/${name}-${width}.png`,fullPage:true});results.push({name,width,status:'PASS'});
 }
 await admin.p.setViewportSize({width:375,height:950});await admin.p.goto(base+'/Admin');await admin.p.locator('[data-bs-target="#workspace-sidebar"]').first().click();await admin.p.locator('#workspace-sidebar.show').waitFor();await admin.p.keyboard.press('Escape');await admin.p.locator('#workspace-sidebar.show').waitFor({state:'hidden'});
 assert(await admin.p.locator('[data-bs-target="#workspace-sidebar"]').first().evaluate(e=>document.activeElement===e),'Sidebar focus not restored');results.push({name:'Sidebar_Escape_focus',status:'PASS'});
 await guest.goto(base+'/Account/Login');await guest.locator('[data-password-target="Password"]').click();assert(await guest.locator('#Password').getAttribute('type')==='text','Password toggle');
 await admin.p.goto(base+'/Admin/LayoutEdit');await admin.p.locator('#Rows').fill('3');await admin.p.locator('#Columns').fill('4');await admin.p.locator('#Floors').fill('2');assert(await admin.p.locator('.layout-preview-seat').count()===24,'Preview does not follow form');
 await customer.p.goto(base+'/Bookings');const ticket=customer.p.locator('a[href*="/Bookings/Details/"]').first();if(await ticket.count()){await ticket.click();await customer.p.screenshot({path:'ui-results/ticket-details.png',fullPage:true});results.push({name:'Ticket_details_screenshot',status:'PASS'});}else results.push({name:'Ticket_details_screenshot',status:'CHƯA KIỂM CHỨNG',reason:'Run existing browser.mjs first to create a test booking.'});
 await publicContext.grantPermissions([]);await guest.emulateMedia({reducedMotion:'reduce'});await guest.goto(base+'/');assert(await guest.evaluate(()=>matchMedia('(prefers-reduced-motion: reduce)').matches),'Reduce Motion');
 assert(consoleErrors.length===0,'Browser page errors: '+consoleErrors.join(';'));
}catch(e){results.push({status:'FAIL',error:e.stack});process.exitCode=1;}finally{await fs.writeFile('ui-results/results.json',JSON.stringify({utc:new Date().toISOString(),results,consoleErrors},null,2));await browser.close();}

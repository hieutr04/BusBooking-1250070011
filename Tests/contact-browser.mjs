// Run ONLY against a local test database, after applying AddContactMessages.
// Creates one synthetic contact; never deletes existing records or sends email.
import { chromium } from '@playwright/test';
import assert from 'node:assert/strict';
import fs from 'node:fs/promises';

const base=process.env.APP_URL||'http://localhost:5190';
assert(['localhost','127.0.0.1','[::1]'].includes(new URL(base).hostname),'Local test application required.');
assert.equal(process.env.BB_ALLOW_TEST_WRITES,'YES','Set BB_ALLOW_TEST_WRITES=YES only for a TEST database.');
const password=process.env.BB_ROLE_PASSWORD;
assert(password,'Set BB_ROLE_PASSWORD to the test account password; do not commit it.');
const output=new URL('./TestResults/contact/',import.meta.url);
await fs.mkdir(output,{recursive:true});
const checks=[];
const pass=(name)=>checks.push({name,status:'PASS'});
const browser=await chromium.launch();
const guest=await browser.newContext({ignoreHTTPSErrors:true});
const page=await guest.newPage();
const subject=`CONTACT-TEST-${Date.now()}`;
let error;
try {
 await page.goto(`${base}/lien-he`);
 assert.equal((await page.locator('#contact-form').count()),1);pass('Contact GET as Guest');
 const original=await page.locator('#contact-form').evaluate(form=>Object.fromEntries(new FormData(form)));
 const data={...original,FullName:'KIỂM THỬ LIÊN HỆ',Email:'contact-test@example.invalid',Phone:'',Subject:subject,Body:'<script>window.__contactProbe=true</script> Nội dung tổng hợp để kiểm tra encode.'};
 const post=form=>guest.request.post(`${base}/lien-he`,{form,maxRedirects:0});
 let response=await post(data);assert.equal(response.status(),302);pass('Valid POST persists before redirect');
 await page.goto(`${base}/lien-he`);assert.match(await page.locator('main').innerText(),/Đã ghi nhận liên hệ trong hệ thống/);pass('PRG success notice');
 response=await post(data);assert.equal(response.status(),302);pass('Same submission token replay is accepted idempotently');
 response=await post({...data,Body:'Nội dung thay đổi trên cùng mã biểu mẫu đã sử dụng.'});assert.equal(response.status(),409);pass('Changed replay rejected');
 response=await post({...data,Email:'invalid-email',Body:'ngắn'});assert.equal(response.status(),400);pass('Server validation');
 const withoutCsrf={...data};delete withoutCsrf.__RequestVerificationToken;
 response=await post(withoutCsrf);assert.equal(response.status(),400);pass('Missing anti-forgery rejected');
 response=await post({...data,SubmissionToken:'tampered'});assert.equal(response.status(),400);pass('Tampered submission token rejected');
 response=await post({...data,Subject:'x'.repeat(161)});assert.equal(response.status(),400);pass('Overlong subject rejected');
 response=await post({...data,Body:'x           '});assert.equal(response.status(),400);pass('Whitespace-padded short body rejected');
 response=await post(data);assert.equal(response.status(),429);pass('Ninth POST within contact window rate limited');

 const signIn=async(role)=>{
  const context=await browser.newContext({ignoreHTTPSErrors:true});const tab=await context.newPage();
  await tab.goto(`${base}/Account/Login`);
  await tab.getByLabel('Email',{exact:true}).fill(`${role}@busbooking.test`);
  await tab.locator('#Password').fill(password);
  await Promise.all([tab.waitForURL(url=>!url.pathname.toLowerCase().includes('/account/login')),tab.locator('form button[type=submit],form button:not([type])').last().click()]);
  return {context,tab};
 };
 const admin=await signIn('admin');
 assert.match(new URL(admin.tab.url()).pathname,/^\/Admin\/?$/i);pass('Admin login destination');
 await admin.tab.goto(`${base}/Admin/Contacts`);
 const row=admin.tab.locator('tbody tr').filter({hasText:subject});assert.equal(await row.count(),1);pass('Exactly one SQL-backed record for replayed submission');
 const detailsUrl=new URL(await row.getByRole('link',{name:'Xem chi tiết'}).getAttribute('href'),base).href;
 await row.getByRole('link',{name:'Xem chi tiết'}).click();
 assert((await admin.tab.locator('.contact-message-body').innerText()).includes('<script>'));
 assert.equal(await admin.tab.locator('.contact-message-body script').count(),0);
 assert.equal(await admin.tab.evaluate(()=>window.__contactProbe),undefined);pass('Admin detail HTML encoded');
 await admin.tab.getByRole('button',{name:'Đánh dấu đã đọc'}).click();
 await admin.tab.waitForLoadState();assert.match(await admin.tab.locator('.contact-channels').innerText(),/Đã đọc/);pass('Mark read persisted');
 const denied=async(context,url)=>{
  const result=await context.request.get(url,{maxRedirects:0});
  assert(result.status()===403||(result.status()===302&&(result.headers().location||'').includes('/Home/Status/403')));
 };
 for(const role of ['customer','staff','driver']){
  const actor=await signIn(role);await denied(actor.context,`${base}/Admin/Contacts`);await denied(actor.context,detailsUrl);pass(`${role} cannot read contact list or detail`);await actor.context.close();
 }
 response=await guest.request.get(detailsUrl,{maxRedirects:0});assert([302,401].includes(response.status()));pass('Guest cannot read contact detail');

 // Block external network while retaining localhost, matching "localhost without Internet".
 await page.route('**/*',route=>new URL(route.request().url()).origin===new URL(base).origin?route.continue():route.abort());
 for(const width of [375,390,768,1024,1440]){
  await page.setViewportSize({width,height:1000});
  for(const route of ['/','/Trips','/lien-he']){
   await page.goto(base+route);await page.locator('main').waitFor();
   assert(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth+1),`Horizontal overflow ${route} at ${width}`);
   const broken=await page.locator('img').evaluateAll(images=>images.filter(img=>img.complete&&img.naturalWidth===0).length);assert.equal(broken,0);
  }
  pass(`Local images and no horizontal overflow at ${width}px`);
 }
 await page.setViewportSize({width:1440,height:1000});await page.goto(base+'/');await page.screenshot({path:new URL('home-desktop.png',output).pathname,fullPage:true});
 await page.setViewportSize({width:390,height:1000});await page.screenshot({path:new URL('home-mobile.png',output).pathname,fullPage:true});
 await page.goto(base+'/lien-he');await page.screenshot({path:new URL('contact-mobile.png',output).pathname,fullPage:true});
 await page.setViewportSize({width:1440,height:1000});await page.screenshot({path:new URL('contact-desktop.png',output).pathname,fullPage:true});
 await page.goto(base+'/Trips');await page.screenshot({path:new URL('trips-desktop.png',output).pathname,fullPage:true});
 await admin.tab.goto(`${base}/Admin/Contacts`);await admin.tab.screenshot({path:new URL('admin-contacts.png',output).pathname,fullPage:true});
 await page.route('**/images/banners/contact-journey.webp',route=>route.abort());await page.goto(base+'/lien-he');
 await page.waitForFunction(()=>document.querySelector('.contact-landscape img')?.dataset.fallback==='1');
 assert.match(await page.locator('.contact-landscape img').getAttribute('src'),/landscape\.svg/);pass('Local image fallback');
 await page.emulateMedia({reducedMotion:'reduce'});assert(await page.evaluate(()=>matchMedia('(prefers-reduced-motion: reduce)').matches));pass('Reduced motion preference');
 await admin.context.close();
} catch(ex){error=ex.message;checks.push({name:'Stopped on failing assertion',status:'FAIL',detail:error});}
finally{await browser.close();await fs.writeFile(new URL('results.json',output),JSON.stringify({checks,syntheticSubject:subject,error},null,2));}
console.log(JSON.stringify({passed:checks.filter(x=>x.status==='PASS').length,error,output:output.pathname}));
if(error)process.exitCode=1;

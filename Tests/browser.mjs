// Run against an isolated SQL Server _Tests DB after --fixtures-only (see README).
import {chromium} from '@playwright/test';
import fs from 'node:fs/promises';
const fixture=JSON.parse(await fs.readFile(process.env.FIXTURES,'utf8'));
const base=process.env.BASE_URL||'http://localhost:5190';
if(!fixture.database?.endsWith('_Tests'))throw new Error('Browser tests require isolated _Tests fixtures.');
const browser=await chromium.launch();const results=[];const metrics=[];
const assert=(ok,message)=>{if(!ok)throw new Error(message);};
async function account(data,expected='/'){const c=await browser.newContext();const p=await c.newPage();await p.goto(base+'/Account/Login');await p.locator('#Email').fill(data.Email);await p.locator('#Password').fill(data.password);await Promise.all([p.waitForURL(url=>url.pathname===expected),p.getByRole('button',{name:'Đăng nhập →'}).click()]);return {c,p};}
try{
 const a=await account(fixture.customers[0]),b=await account(fixture.customers[1]);
 for(const {p} of [a,b]){await p.exposeFunction('recordMetric',v=>metrics.push(v));await p.addInitScript(()=>window.addEventListener('seat-realtime-metric',e=>window.recordMetric(e.detail)));await p.goto(base+'/Trips/Details/'+fixture.tripId);await p.locator('.seat-button').first().waitFor();}
 for(const width of [375,390,768,1024,1440]){await a.p.setViewportSize({width,height:950});assert(await a.p.evaluate(()=>document.documentElement.scrollWidth<=innerWidth),'Horizontal page overflow at '+width);results.push({name:'viewport_'+width,status:'PASS'});}
 await a.p.setViewportSize({width:1024,height:950});
 const seat=a.p.locator('.seat-button.status-Available').first(),code=await seat.locator('span').textContent();
 await seat.click();await a.p.locator('#hold-button').click();await a.p.locator('.seat-button.mine').first().waitFor();
 assert(await a.p.locator('#checkout-panel').isVisible(),'Hold acquired but checkout hidden');
 assert((await a.p.locator('#selected-label').textContent()).includes(code),'Held seat absent from summary');
 assert((await a.p.locator('#seat-total').textContent()).replace(/[^0-9]/g,'')!=='0','Held total reset to zero');
 assert(await a.p.locator('#pickup-stop option').count()>0&&await a.p.locator('#dropoff-stop option').count()>0,'Stop list missing');
 const chosenPickup=await a.p.locator('#pickup-stop').inputValue(),chosenDropoff=await a.p.locator('#dropoff-stop').inputValue();
 results.push({name:'Held_summary_checkout_and_stops',status:'PASS'});
 await b.p.locator('.seat-button.status-Held').filter({hasText:code}).first().waitFor();results.push({name:'SignalR_other_client_hold',status:'PASS'});
 const before=await a.p.locator('#countdown').textContent();await a.p.reload();await a.p.locator('.seat-button.mine').first().waitFor();const after=await a.p.locator('#countdown').textContent();assert(after<=before,'Reload extended countdown');assert(await a.p.locator('#pickup-stop').inputValue()===chosenPickup&&await a.p.locator('#dropoff-stop').inputValue()===chosenDropoff,'Reload lost stop choice');
 await b.c.setOffline(true);await a.p.locator('#release-button').click();await a.p.getByRole('dialog').getByRole('button',{name:'Nhả giữ chỗ',exact:true}).click();await a.p.locator('.seat-button.mine').waitFor({state:'detached'});await b.c.setOffline(false);await b.p.bringToFront();await b.p.locator('#refresh-seats').click();await b.p.locator('.seat-button.status-Available').filter({hasText:code}).first().waitFor();results.push({name:'Reconnect_snapshot',status:'PASS'});
 // Complete an actual booking and verify another customer cannot read it.
 await a.p.locator('.seat-button.status-Available').first().click();await a.p.locator('#hold-button').click();await a.p.locator('#passenger-fields input').first().waitFor();
 await a.p.locator('#contact-name').fill('Khách browser test');await a.p.locator('#contact-phone').fill('0900000000');
 for(const input of await a.p.locator('#passenger-fields input').all())await input.fill('Hành khách browser test');
 await a.p.locator('#checkout-form input[type=checkbox]').check();await Promise.all([a.p.waitForURL(url=>url.searchParams.has('confirmedBookingId')),a.p.locator('#confirm-button').click()]);
 await a.p.locator('#booking-confirmed').waitFor();
 const confirmationUrl=a.p.url();
 assert((await b.p.goto(confirmationUrl)).status()===404,'Customer IDOR: B read A trip confirmation');
 if(await a.p.locator('#payment-qr').count())await a.p.locator('#booking-confirmed .payment-qr-image img').waitFor();
 await Promise.all([a.p.waitForURL(/\/Bookings\/Details\//),a.p.locator('#booking-confirmed a').click()]);
 assert(await a.p.locator('.saved-stops').isVisible(),'Booking stop snapshot missing');const bookingUrl=a.p.url();assert((await b.p.goto(bookingUrl)).status()===403,'Customer IDOR: B read A booking');results.push({name:'Customer_IDOR',status:'PASS'});
 const paymentDemoEnabled=await a.p.locator('#simulate-payment').count()>0;
 if(paymentDemoEnabled){
  const qr=a.p.getByRole('img',{name:/Mã VietQR cho vé/});
  await qr.waitFor();
  assert(await qr.evaluate(img=>img.complete&&img.naturalWidth>0&&img.src.startsWith('data:image/png;base64,')),'VietQR image failed to render locally');
  await a.p.locator('#simulate-payment').click();
  const simulated=a.p.getByRole('heading',{name:'Đã thanh toán',exact:true});
  await simulated.waitFor();
  await a.p.reload();
  await simulated.waitFor();
  assert(await a.p.locator('#simulate-payment').count()===0,'Simulation is offered again after it is saved');
  assert(await a.p.locator('#boarding-passes').isVisible(),'Paid booking has no check-in ticket');
  results.push({name:'Qr_payment_persists_and_issues_ticket',status:'PASS'});
 }
 const staff=await account(fixture.staff,'/Staff'),driver=await account(fixture.driver,'/Driver'),admin=await account(fixture.admin,'/Admin'),otherDriver=await account(fixture.otherDriver,'/Driver');
 assert((await a.p.goto(base+'/Admin')).status()===403,'Customer reached admin');
 assert((await driver.p.goto(base+'/Staff')).status()===403,'Driver reached staff');
 await driver.p.goto(base+'/Driver/Details/'+fixture.tripId);assert(await driver.p.getByRole('heading',{name:'Tiến trình chuyến'}).isVisible(),'Assigned driver cannot read trip');
 await staff.p.goto(base+'/Staff');assert(await staff.p.getByRole('heading',{name:'Tra cứu và xử lý vé'}).isVisible(),'Staff route failed');
 assert((await staff.p.goto(base+'/Admin')).status()===403,'Staff reached admin');
 assert((await otherDriver.p.goto(base+'/Driver/Details/'+fixture.tripId)).status()===403,'Unassigned driver IDOR');
 assert((await admin.p.goto(base+'/Admin')).status()===200,'Admin dashboard failed');
 if(paymentDemoEnabled){
  await admin.p.goto(bookingUrl);
  assert(await admin.p.getByRole('heading',{name:'Đã thanh toán',exact:true}).isVisible(),'Admin cannot see the saved payment');
 }
 const driverCsrf=(await (await driver.p.request.get(base+'/api/seats/csrf')).json()).token;
 assert((await driver.p.request.post(base+'/api/seats/hold',{headers:{'X-CSRF-TOKEN':driverCsrf},data:{tripId:fixture.tripId,seatIds:[fixture.seatIds[2]],requestKey:crypto.randomUUID()}})).status()===403,'Driver API hold authorization');
 const anonymous=await browser.newContext();assert((await anonymous.request.post(base+'/api/seats/hold',{data:{tripId:fixture.tripId,seatIds:[fixture.seatIds[3]],requestKey:crypto.randomUUID()}})).status()===401,'Anonymous API hold authorization');await anonymous.close();
 assert((await staff.p.request.post(base+'/api/seats/hold',{data:{tripId:fixture.tripId,seatIds:[fixture.seatIds[3]],requestKey:crypto.randomUUID()}})).status()===400,'Missing anti-forgery token accepted');
 for(const payment of ['due','paid','refund']){
  assert((await staff.p.goto(base+'/Staff?payment='+payment)).status()===200,'Staff payment filter failed');
  assert(await staff.p.locator('#payment').inputValue()===payment,'Staff payment filter selection lost');
 }
 await staff.p.goto(bookingUrl);
 assert(await staff.p.locator('.payment-entry').count()===0,'Staff still has a manual receipt form');
 const staffCsrf=(await (await staff.p.request.get(base+'/api/seats/csrf')).json()).token;
 const manual=await staff.p.request.post(base+'/Staff/Money',{headers:{'X-CSRF-TOKEN':staffCsrf},form:{
  BookingId:new URL(bookingUrl).pathname.split('/').pop(),RequestKey:crypto.randomUUID(),Refund:'false',Amount:'40000'
 }});
 assert(manual.status()===400,'Manual receipt endpoint accepted payment');
 results.push({name:'Staff_payment_filters_and_manual_receipt_disabled',status:'PASS'});
 results.push({name:'Role_routes_and_API_matrix',status:'PASS'});
 await fs.writeFile('browser-results.json',JSON.stringify({utc:new Date().toISOString(),results,metrics,metricDefinition:'Server monotonic time from post-commit publication observation to client ACK arriving back at server. RTT/instrumented upper observation, NOT one-way SQL commit latency.'},null,2));
}catch(e){results.push({status:'FAIL',error:e.stack});await fs.writeFile('browser-results.json',JSON.stringify({results,metrics},null,2));throw e;}finally{await browser.close();}

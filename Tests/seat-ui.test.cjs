// Executes production JS against a DOM + HTTP fixtures. This does NOT test MVC/SQL/SignalR server or responsive rendering.
const {JSDOM}=require('jsdom');
const fs=require('node:fs');const path=require('node:path');const assert=require('node:assert/strict');
const root=path.resolve(__dirname,'..');const js=n=>fs.readFileSync(path.join(root,'BusBooking/wwwroot/js',n),'utf8');
const cases=[];const check=(name,test)=>{test();cases.push({name,status:'PASS',scope:'DOM/HTTP fixtures, production JavaScript'});};
const flush=async()=>{for(let i=0;i<12;i++)await new Promise(r=>setImmediate(r));};
function seats(rows,cols,floors){const list=[];for(let f=1;f<=floors;f++)for(let r=1;r<=rows;r++)for(let c=1;c<=cols;c++)list.push({id:list.length+1,code:String.fromCharCode(64+f)+String((r-1)*cols+c).padStart(2,'0'),row:r,column:c,floor:f,status:'Available',mine:false});return list;}
function dom(){
 const ids=['seat-map','selected-label','seat-total','summary-hold-label','pickup-address','dropoff-address','pickup-summary','dropoff-summary','stop-error','pickup-notice','stop-duration','illustrative-stops','seat-message','passenger-fields','countdown','connection-status'];
 const html='<div id="seat-app" data-trip="6" data-auth="true"><div>'+ids.map(id=>'<div id="'+id+'"></div>').join('')+'</div><a id="stop-support" href="/lien-he" hidden>Liên hệ</a><select id="pickup-stop"></select><select id="dropoff-stop"></select><button id="hold-button">Đặt vé</button><button id="continue-hold" hidden>Tiếp tục</button><button id="refresh-seats">Đồng bộ</button><section id="checkout-panel" hidden><select id="hold-list"></select><form id="checkout-form"><input id="contact-name" required><input id="contact-phone" required><input id="policy-accept" type="checkbox" required><button id="confirm-button">Xác nhận</button></form><button id="release-button">Nhả</button></section></div>';
 const d=new JSDOM(html,{url:'http://localhost:5190/Trips/Details/6',runScripts:'outside-only',pretendToBeVisual:true});
 const w=d.window;w.matchMedia=()=>({matches:true});w.HTMLElement.prototype.scrollIntoView=()=>{};w.setInterval=()=>0;return d;
}
async function main(){
 const d=dom(),w=d.window;w.eval(js('seat-layout.js'));
 const map=w.document.getElementById('seat-map');
 w.BusSeatLayout.render(map,seats(8,4,1),{});
 check('32 seats preserve IDs/codes, visual aisle and unique coordinates',()=>{
  const buttons=[...map.querySelectorAll('button')];assert.equal(buttons.length,32);assert.equal(new Set(buttons.map(b=>b.dataset.seatId)).size,32);
  assert.equal(buttons[0].querySelector('.seat-code').textContent,'A01');assert.equal(buttons.at(-1).querySelector('.seat-code').textContent,'A32');
  assert.equal(buttons[1].style.gridColumn,'2');assert.equal(buttons[2].style.gridColumn,'4');assert.equal(map.querySelectorAll('.seat-deck').length,1);
 });
 w.BusSeatLayout.render(map,seats(4,3,2),{});
 check('24 berths, two decks of 12, no phantom aisle seats',()=>{
  assert.deepEqual([...map.querySelectorAll('.seat-deck')].map(n=>n.querySelectorAll('button').length),[12,12]);
  assert.deepEqual([...map.querySelectorAll('.floor-title')].map(n=>n.textContent),['Tầng dưới','Tầng trên']);
  assert.equal(map.querySelectorAll('button').length,24);assert.equal(map.querySelectorAll('button')[12].querySelector('.seat-code').textContent,'B01');
 });
 const now=Date.now(),holdId='aaaaaaaa-aaaa-4aaa-aaaa-aaaaaaaaaaaa';let holdPayload,updatePayload,failHold=false;
 const state={tripId:6,revision:1,serverUtc:new Date(now).toISOString(),canBook:true,fare:320000,columns:4,seats:seats(8,4,1),myHolds:[],arriveEarlyMinutes:15,
  stops:[{id:10,stopOrder:0,name:'Điểm đầu · Đà Lạt',address:null,atUtc:'2026-11-04T16:00:00Z',canPickup:true,canDropoff:false,isIllustrative:true},
   {id:11,stopOrder:10,name:'Điểm học tập · Đức Trọng',address:'Địa chỉ được cấu hình',atUtc:'2026-11-04T18:00:00Z',canPickup:true,canDropoff:true,isIllustrative:true},
   {id:12,stopOrder:1000,name:'Điểm cuối · TP. Hồ Chí Minh',address:null,atUtc:'2026-11-05T00:00:00Z',canPickup:false,canDropoff:true,isIllustrative:true}]};
 w.fetch=async(url,options={})=>{
  const body=options.body?JSON.parse(options.body):null;let value,status=200;
  if(url.endsWith('/csrf'))value={token:'fixture-csrf'};
  else if(url.endsWith('/hold')){
   holdPayload=body;
   if(failHold){status=409;value={message:'Ghế vừa được giữ bởi người khác.'};}
   else{const h={id:holdId,status:'Active',expiresAtUtc:new Date(now+300000).toISOString(),seatIds:body.seatIds,pickupStopId:body.pickupStopId,dropoffStopId:body.dropoffStopId};
    state.myHolds=[h];for(const s of state.seats)if(body.seatIds.includes(s.id)){s.status='Held';s.mine=true;}state.revision++;value=h;}
  }else if(url.endsWith('/stops')){updatePayload=body;state.myHolds[0].pickupStopId=body.pickupStopId;state.myHolds[0].dropoffStopId=body.dropoffStopId;value=state.myHolds[0];}
  else if(url.endsWith('/release')){state.myHolds=[];for(const s of state.seats){s.status='Available';s.mine=false;}state.revision++;value={released:true};}
  else value=state;
  return {ok:status===200,status,json:async()=>JSON.parse(JSON.stringify(value))};
 };
 const hub={on(){},onreconnecting(){},onreconnected(){},onclose(){},start:async()=>{},invoke:async()=>{}};
 w.signalR={HubConnectionBuilder:class{withUrl(){return this;}withAutomaticReconnect(){return this;}build(){return hub;}}};
 w.eval(js('seats.js'));await flush();
 check('Dropdown dates use Vietnam timezone and midnight next date',()=>{
  assert.match(w.document.getElementById('pickup-summary').textContent,/23:00/);
  assert.match(w.document.getElementById('dropoff-summary').textContent,/05\/11\/2026/);
  assert.match(w.document.getElementById('dropoff-summary').textContent,/07:00/);
  assert.match(w.document.getElementById('pickup-notice').textContent,/22:45/);
 });
 check('BusGo stop presentation hides blank addresses, removes only known labels, keeps IDs',()=>{
  const pickup=w.document.getElementById('pickup-stop');
  assert.equal(pickup.options[0].value,'10');assert.match(pickup.options[0].textContent,/Đà Lạt/);
  assert.doesNotMatch(pickup.textContent,/học tập|Điểm đầu ·/);
  assert.equal(w.document.getElementById('pickup-address').hidden,true);
  assert.equal(pickup.hasAttribute('aria-describedby'),false);
  assert.match(w.document.getElementById('stop-duration').textContent,/8 giờ/);
  assert.match(w.document.getElementById('pickup-notice').textContent,/22:45 ngày 04\/11\/2026 tại Đà Lạt/);
 });
 w.document.querySelector('.seat-button').click();w.document.getElementById('hold-button').click();await flush();
 check('Acquire clears selection but summary shows held seats and actual total',()=>{
  assert.equal(holdPayload.pickupStopId,10);assert.equal(holdPayload.dropoffStopId,12);
  assert.equal(w.document.getElementById('selected-label').textContent,'A01');assert.match(w.document.getElementById('seat-total').textContent,/320\.000/);
  assert.equal(w.document.getElementById('checkout-panel').hidden,false);assert.equal(w.document.getElementById('hold-button').hidden,true);
  assert.equal(w.document.querySelectorAll('#passenger-fields input').length,1);assert.equal(w.document.querySelectorAll('.seat-button.active-hold').length,1);
 });
 w.document.getElementById('contact-name').value='Không mất dữ liệu';w.document.querySelector('#passenger-fields input').value='Hành khách đã nhập';
 const expiry=state.myHolds[0].expiresAtUtc;
 const pickup=w.document.getElementById('pickup-stop');pickup.value='11';pickup.dispatchEvent(new w.Event('change'));await flush();
 check('Change stop persists through API without acquiring/extending hold',()=>{
  assert.equal(updatePayload.holdId,holdId);assert.equal(updatePayload.pickupStopId,11);assert.equal(state.myHolds[0].expiresAtUtc,expiry);
  assert.match(w.document.getElementById('pickup-summary').textContent,/01:00/);
  assert.match(w.document.getElementById('pickup-summary').textContent,/05\/11\/2026/);
  assert.equal(w.document.querySelector('#passenger-fields input').value,'Hành khách đã nhập');
 });
 w.document.getElementById('refresh-seats').click();await flush();
 check('Configured address is shown accessibly after changing points',()=>{
  assert.equal(w.document.getElementById('pickup-address').hidden,false);
  assert.equal(w.document.getElementById('pickup-address').textContent,'Địa chỉ được cấu hình');
  assert.equal(pickup.getAttribute('aria-describedby'),'pickup-address');
  assert.match(w.document.getElementById('stop-duration').textContent,/6 giờ/);
  assert.match(w.document.getElementById('pickup-notice').textContent,/00:45 ngày 05\/11\/2026 tại Đức Trọng/);
 });
 check('Snapshot refresh preserves draft, chosen stops, held total and 300s deadline',()=>{
  assert.equal(w.document.getElementById('contact-name').value,'Không mất dữ liệu');assert.equal(pickup.value,'11');
  assert.equal(state.myHolds[0].expiresAtUtc,expiry);assert.match(w.document.getElementById('seat-total').textContent,/320\.000/);
 });
 // New page, same server-side hold: no browser storage needed.
 const fresh=dom();fresh.window.fetch=w.fetch;fresh.window.signalR=w.signalR;fresh.window.eval(js('seat-layout.js'));fresh.window.eval(js('seats.js'));await flush();
 check('New page restores hold, point selection, passenger inputs and total from snapshot',()=>{
  assert.equal(fresh.window.document.getElementById('pickup-stop').value,'11');assert.equal(fresh.window.document.getElementById('checkout-panel').hidden,false);
  assert.equal(fresh.window.document.getElementById('selected-label').textContent,'A01');assert.match(fresh.window.document.getElementById('seat-total').textContent,/320\.000/);
 });fresh.window.close();
 // Two private holds must keep their own points and drafts when changing the active hold.
 const second={id:'bbbbbbbb-bbbb-4bbb-bbbb-bbbbbbbbbbbb',status:'Active',expiresAtUtc:new Date(now+250000).toISOString(),seatIds:[2],pickupStopId:10,dropoffStopId:11};
 state.myHolds.push(second);state.seats[1].status='Held';state.seats[1].mine=true;state.revision++;
 w.document.getElementById('refresh-seats').click();await flush();
 const list=w.document.getElementById('hold-list');list.value=second.id;list.dispatchEvent(new w.Event('change'));await flush();
 check('Multiple holds show own seat/total/point selection',()=>{assert.equal(w.document.getElementById('selected-label').textContent,'A02');assert.equal(pickup.value,'10');assert.equal(w.document.getElementById('dropoff-stop').value,'11');});
 w.document.querySelector('#passenger-fields input').value='Người đi lượt hai';
 list.value=holdId;list.dispatchEvent(new w.Event('change'));await flush();
 check('Switch back restores correct passenger draft and stop IDs',()=>{assert.equal(w.document.getElementById('selected-label').textContent,'A01');assert.equal(w.document.querySelector('#passenger-fields input').value,'Hành khách đã nhập');assert.equal(pickup.value,'11');});
 state.myHolds=state.myHolds.filter(h=>h.id===holdId);state.seats[1].status='Available';state.seats[1].mine=false;
 w.document.getElementById('release-button').click();await flush();failHold=true;
 w.document.querySelector('.seat-button').click();w.document.getElementById('hold-button').click();await flush();
 check('HTTP 409 displays failure, creates no fake hold or success panel',()=>{
  assert.match(w.document.getElementById('seat-message').textContent,/người khác/);assert.equal(w.document.getElementById('checkout-panel').hidden,true);
  assert.equal(w.document.querySelectorAll('.seat-button.mine').length,0);assert.equal(w.document.getElementById('hold-button').disabled,false);
 });
 // Eligibility flags are supplied by the real server policy; this test verifies UI handling only.
 state.stops=[{id:20,stopOrder:-1,name:'Bến đón đã cấu hình',address:'Địa chỉ đã cấu hình',atUtc:'2026-11-04T16:00:00Z',canPickup:true,canDropoff:false,canSelect:true,locationSpecific:true},
  ...state.stops.map(s=>({...s,canSelect:false,locationSpecific:false})),
  {id:21,stopOrder:1001,name:'Bến trả đã cấu hình',address:'Địa chỉ trả đã cấu hình',atUtc:'2026-11-05T00:00:00Z',canPickup:false,canDropoff:true,canSelect:true,locationSpecific:true}];
 const oldHold={id:holdId,status:'Active',expiresAtUtc:new Date(now+300000).toISOString(),seatIds:[1],pickupStopId:10,dropoffStopId:12};
 state.myHolds=[oldHold,{...oldHold,id:second.id,seatIds:[2],pickupStopId:11}];state.revision++;
 w.document.getElementById('refresh-seats').click();await flush();
 check('Retained legacy point remains selected only for its own active hold',()=>{
  assert.equal(pickup.value,'10');assert.equal(w.document.getElementById('dropoff-stop').value,'12');
  assert.equal([...pickup.options].some(o=>o.value==='11'),false);assert.equal([...pickup.options].some(o=>o.value==='20'),true);
  assert.equal(w.document.getElementById('stop-support').hidden,false);assert.equal(state.myHolds[0].expiresAtUtc,oldHold.expiresAtUtc);
 });
 list.value=second.id;list.dispatchEvent(new w.Event('change'));await flush();
 check('Switching holds exposes that hold saved point without offering another legacy choice',()=>{assert.equal(pickup.value,'11');assert.equal([...pickup.options].some(o=>o.value==='10'),false);});
 state.myHolds=[];state.revision++;w.document.getElementById('refresh-seats').click();await flush();
 check('New booking sees specific endpoints only; no old locality/held-only options',()=>{
  assert.equal(pickup.value,'20');assert.equal(w.document.getElementById('dropoff-stop').value,'21');assert.equal(pickup.options.length,1);
  assert.equal(w.document.getElementById('stop-support').hidden,true);
 });
 d.window.close();
 const result={utc:new Date().toISOString(),scope:'Production seat-layout.js and seats.js with jsdom/HTTP fixtures. Not a server/SQL/SignalR, CSS layout or screenshot test.',cases};
 const out=path.join(root,'Documentation/validation/buscenter-seat-ui.json');fs.mkdirSync(path.dirname(out),{recursive:true});fs.writeFileSync(out,JSON.stringify(result,null,2));console.log(JSON.stringify(result,null,2));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

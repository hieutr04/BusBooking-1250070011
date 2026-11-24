"use strict";
(()=>{
 const app=document.getElementById('seat-app');if(!app)return;
 const trip=Number(app.dataset.trip),auth=app.dataset.auth==='true',exchange=app.dataset.exchange||null;
 const el=id=>document.getElementById(id),money=n=>new Intl.NumberFormat('vi-VN',{style:'currency',currency:'VND'}).format(n);
 const utc=v=>Date.parse(v.endsWith('Z')||/[+-]\d\d:\d\d$/.test(v)?v:v+'Z');
 const dateTime=v=>new Intl.DateTimeFormat('vi-VN',{timeZone:'Asia/Ho_Chi_Minh',hour:'2-digit',minute:'2-digit',day:'2-digit',month:'2-digit',year:'numeric',hourCycle:'h23'}).format(new Date(typeof v==='number'?v:utc(v)));
 let snapshot=null,selected=new Set(),activeHold=null,renderedHoldId=null,revision=-1,wanted=-1,syncRunning=false,syncAgain=false,busy=false,csrf='',clockUtc=0,clockMono=0,pendingHold=null,pendingConfirm=null,healthy=false,expiredSync=false;
 let pickupId=Number(app.dataset.pickup)||null,dropoffId=null,stopStamp='',choiceHoldId=null;
 const drafts=new Map();
 function message(s,error=false){el('seat-message').textContent=s;el('seat-message').classList.toggle('error',error);}
 async function request(path,body){
  const controller=new AbortController(),timer=setTimeout(()=>controller.abort(),20000);
  try{
   const response=await fetch('/api/seats/'+path,{method:body?'POST':'GET',headers:body?{'Content-Type':'application/json','X-CSRF-TOKEN':csrf}:{},body:body?JSON.stringify(body):undefined,signal:controller.signal,cache:'no-store'});
   if(!response.ok){let data={};try{data=await response.json();}catch{}const err=new Error(data.message||({400:'Kiểm tra dữ liệu hoặc tải lại trang để làm mới phiên.',401:'Vui lòng đăng nhập lại.',403:'Bạn không có quyền thực hiện thao tác.',409:'Ghế hoặc chuyến vừa thay đổi.',503:'Dịch vụ tạm thời chưa sẵn sàng. Vui lòng thử lại.'}[response.status]||'Không thể hoàn tất yêu cầu.'));err.status=response.status;throw err;}
   return await response.json();
  }catch(e){if(!e.status)throw new Error('Mất kết nối hoặc hết thời gian chờ. Thử lại cùng thao tác để kiểm tra kết quả.');throw e;}
  finally{clearTimeout(timer);}
 }
 const stopName=value=>String(value||'').replace(/^(?:Điểm học tập|Điểm mô phỏng|Điểm đầu|Điểm cuối)\s*·\s*/iu,'').trim();
 const duration=minutes=>{const h=Math.floor(minutes/60),m=minutes%60;return h?(m?`${h} giờ ${m} phút`:`${h} giờ`):`${m} phút`;};
 const deadline=value=>{const parts=new Intl.DateTimeFormat('vi-VN',{timeZone:'Asia/Ho_Chi_Minh',hour:'2-digit',minute:'2-digit',day:'2-digit',month:'2-digit',year:'numeric',hourCycle:'h23'}).formatToParts(new Date(value));const get=t=>parts.find(p=>p.type===t).value;return `${get('hour')}:${get('minute')} ngày ${get('day')}/${get('month')}/${get('year')}`;};
 const address=(id,select,value)=>{
  const node=el(id),text=String(value||'').trim();
  node.textContent=text;
  node.hidden=!text;
  if(text)el(select).setAttribute('aria-describedby',id);else el(select).removeAttribute('aria-describedby');
  const map=el(id+'-map');
  if(map){
   map.hidden=!text;
   if(text)map.href='https://www.google.com/maps/search/?api=1&query='+encodeURIComponent(text);
   else map.removeAttribute('href');
  }
 };
 const pair=()=>({p:snapshot?.stops?.find(s=>s.id===pickupId),d:snapshot?.stops?.find(s=>s.id===dropoffId)});
 function validPair(){const {p,d}=pair();return !!(p?.canPickup&&d?.canDropoff&&p.stopOrder<d.stopOrder&&utc(p.atUtc)<utc(d.atUtc));}
 function remainingMs(){return activeHold?Math.max(0,utc(activeHold.expiresAtUtc)-(clockUtc+performance.now()-clockMono)):0;}
 function buttons(){
  if(el('hold-button')){el('hold-button').disabled=busy||!healthy||!snapshot?.canBook||selected.size===0||selected.size>6||!!activeHold||!validPair();el('hold-button').hidden=!!activeHold||!!pendingConfirm;el('hold-button').textContent=busy&&pendingHold?'Đang giữ chỗ…':'Giữ chỗ';}
  el('continue-hold').hidden=!activeHold||!!pendingConfirm;el('continue-hold').disabled=busy;
  el('confirm-button').disabled=busy||!healthy||(!pendingConfirm&&(!snapshot?.canBook||!activeHold||remainingMs()<=0||!validPair()));
  el('confirm-button').textContent=pendingConfirm?'Kiểm tra lại thao tác đặt vé':busy?'Đang xử lý…':'Xác nhận đặt vé';
  el('release-button').disabled=busy||!activeHold||!!pendingConfirm;el('refresh-seats').disabled=busy;
  for(const id of ['pickup-stop','dropoff-stop'])el(id).disabled=busy||!!pendingConfirm||!snapshot?.stops?.length;
 }
 function selectedSeatIds(){return activeHold?.seatIds||pendingConfirm?.passengers.map(p=>p.tripSeatId)||[...selected];}
 function summary(){
  const ids=new Set(selectedSeatIds()),seats=snapshot?.seats.filter(s=>ids.has(s.id))||[];
  el('selected-label').textContent=seats.map(s=>s.code).join(', ')||'Chưa chọn';
  el('seat-total').textContent=money(seats.length*(snapshot?.fare||0));
  el('summary-hold-label').textContent=activeHold?'Ghế thuộc lượt giữ đang chọn':pendingConfirm?'Đang kiểm tra kết quả xác nhận':'Chọn ghế rồi bấm Giữ chỗ';
  buttons();
 }
 function option(select,s){const o=document.createElement('option');o.value=String(s.id);o.textContent=dateTime(s.atUtc)+' · '+stopName(s.name);select.append(o);}
 function renderStops(){
  const stops=(snapshot.stops||[]).filter(s=>s.canSelect!==false||s.id===activeHold?.pickupStopId||s.id===activeHold?.dropoffStopId),stamp=JSON.stringify(stops)+":"+(activeHold?.id||""),ps=el('pickup-stop'),ds=el('dropoff-stop');
  if(stamp!==stopStamp){stopStamp=stamp;ps.replaceChildren();ds.replaceChildren();
   for(const s of stops.filter(s=>s.canPickup&&(s.canSelect!==false||s.id===activeHold?.pickupStopId)))option(ps,s);
   for(const s of stops.filter(s=>s.canDropoff&&(s.canSelect!==false||s.id===activeHold?.dropoffStopId)))option(ds,s);
  }
  if(!stops.some(s=>s.id===pickupId&&s.canPickup))pickupId=stops.find(s=>s.canPickup)?.id??null;
  if(!stops.some(s=>s.id===dropoffId&&s.canDropoff))dropoffId=[...stops].reverse().find(s=>s.canDropoff)?.id??null;
  ps.value=String(pickupId??'');ds.value=String(dropoffId??'');
  const {p,d}=pair();
  for(const o of ps.options){const s=stops.find(x=>x.id===Number(o.value));o.disabled=s.canSelect===false&&s.id!==activeHold?.pickupStopId;}
  for(const o of ds.options){const s=stops.find(x=>x.id===Number(o.value));o.disabled=(!!p&&s.stopOrder<=p.stopOrder)||(s.canSelect===false&&s.id!==activeHold?.dropoffStopId);}
  address('pickup-address','pickup-stop',p?.address);
  address('dropoff-address','dropoff-stop',d?.address);
  el('pickup-summary').textContent=p?stopName(p.name)+' · '+dateTime(p.atUtc):'Chưa có lịch điểm';
  el('dropoff-summary').textContent=d?stopName(d.name)+' · '+dateTime(d.atUtc):'Chưa có lịch điểm';
  el('stop-error').textContent=validPair()?'':stops.length?'Điểm trả phải sau điểm đón.':'Chưa có điểm đón/trả phù hợp. Vui lòng chọn chuyến khác hoặc liên hệ hỗ trợ.';
  if(el('stop-support'))el('stop-support').hidden=!(p?.locationSpecific===false||d?.locationSpecific===false);
  el('pickup-notice').textContent=validPair()?'Vui lòng có mặt trước '+deadline(utc(p.atUtc)-snapshot.arriveEarlyMinutes*60000)+' tại '+stopName(p.name)+'.':'';
  el('stop-duration').textContent=validPair()?'Thời gian dự kiến: '+duration(Math.round((utc(d.atUtc)-utc(p.atUtc))/60000)):'';
 }
 function stashDraft(){if(!renderedHoldId)return;drafts.set(renderedHoldId,{name:el('contact-name').value,phone:el('contact-phone').value,passengers:[...el('passenger-fields').querySelectorAll('input')].map(x=>({id:x.dataset.seat,value:x.value})),checked:el('policy-accept').checked});}
 function checkout(){
  const mine=snapshot.myHolds||[],old=activeHold?.id;
  const next=mine.find(h=>h.id===old)||mine[0]||null;
  if(renderedHoldId!==next?.id)stashDraft();activeHold=next;
  const list=el('hold-list');list.replaceChildren();
  for(const h of mine){const o=document.createElement('option');o.value=h.id;o.textContent='Ghế '+snapshot.seats.filter(s=>h.seatIds.includes(s.id)).map(s=>s.code).join(', ');o.selected=h.id===activeHold?.id;list.append(o);}
  el('checkout-panel').hidden=!activeHold&&!pendingConfirm;
  if(activeHold){pickupId=activeHold.pickupStopId??pickupId;dropoffId=activeHold.dropoffStopId??dropoffId;choiceHoldId=activeHold.id;}
  else if(choiceHoldId&&!pendingConfirm){choiceHoldId=null;pickupId=null;dropoffId=null;}
  if(renderedHoldId!==activeHold?.id&&!pendingConfirm){
   renderedHoldId=activeHold?.id??null;expiredSync=false;
   const fields=el('passenger-fields');fields.replaceChildren();
   const draft=drafts.get(renderedHoldId);
   if(activeHold){for(const id of activeHold.seatIds){const s=snapshot.seats.find(s=>s.id===id),l=document.createElement('label'),input=document.createElement('input');
    l.htmlFor='passenger-'+id;l.textContent='Hành khách ghế '+(s?.code||id);input.id=l.htmlFor;input.dataset.seat=String(id);input.required=true;input.minLength=2;input.maxLength=100;input.value=draft?.passengers.find(p=>p.id===String(id))?.value||'';fields.append(l,input);}
    if(draft){el('contact-name').value=draft.name;el('contact-phone').value=draft.phone;el('policy-accept').checked=draft.checked;}
   }
  }
  renderStops();tick();buttons();
 }
 function render(){if(!snapshot)return;window.BusSeatLayout.render(el('seat-map'),snapshot.seats,{selectedIds:selected,activeHoldIds:new Set(activeHold?.seatIds||[]),
  disabled:!snapshot.canBook||busy||!auth||!!activeHold||!!pendingConfirm,onSelect:s=>{
   if(selected.has(s.id))selected.delete(s.id);else if(selected.size<6)selected.add(s.id);else message('Chọn tối đa 6 ghế mỗi lần.');pendingHold=null;render();}});summary();}
 async function sync(){
  if(syncRunning){syncAgain=true;return;}syncRunning=true;
  try{do{syncAgain=false;const before=performance.now(),s=await request(String(trip)),after=performance.now();
   if(s.revision<revision){syncAgain=true;continue;}
   snapshot=s;revision=s.revision;clockUtc=utc(s.serverUtc);clockMono=(before+after)/2;healthy=true;
   for(const id of selected)if(!s.seats.some(x=>x.id===id&&x.status==='Available'))selected.delete(id);
   checkout();render();if(s.revision<wanted)syncAgain=true;
  }while(syncAgain);
  }catch(e){healthy=false;message(e.status===401?e.message:'Không tải được danh sách ghế. Vui lòng thử lại.',true);buttons();}finally{syncRunning=false;}
 }
 async function mutate(action){if(busy)return;busy=true;buttons();try{await action();}catch(e){message(e.message,true);await sync();}finally{busy=false;buttons();render();}}
 function focusCheckout(){el('checkout-panel').scrollIntoView({behavior:matchMedia('(prefers-reduced-motion: reduce)').matches?'auto':'smooth',block:'start'});el('contact-name').focus({preventScroll:true});}
 el('continue-hold').addEventListener('click',focusCheckout);
 el('refresh-seats').addEventListener('click',sync);
 el('hold-list').addEventListener('change',()=>{stashDraft();activeHold=snapshot.myHolds.find(h=>h.id===el('hold-list').value)||null;checkout();render();});
 for(const [id,field] of [['pickup-stop','pickup'],['dropoff-stop','dropoff']])el(id).addEventListener('change',()=>{
  const value=Number(el(id).value);if(field==='pickup')pickupId=value;else dropoffId=value;
  if(field==='pickup'&&!validPair())dropoffId=[...(snapshot.stops||[])].reverse().find(s=>s.canDropoff&&s.stopOrder>(pair().p?.stopOrder??Infinity))?.id??null;
  renderStops();buttons();
  if(activeHold&&validPair())mutate(async()=>{const h=await request('stops',{holdId:activeHold.id,pickupStopId:pickupId,dropoffStopId:dropoffId});activeHold=h;await sync();});
  else pendingHold=null;
 });
 el('hold-button')?.addEventListener('click',()=>mutate(async()=>{
  if(!validPair())throw new Error('Chọn điểm đón/trả hợp lệ.');
  if(!pendingHold)pendingHold={tripId:trip,seatIds:[...selected].sort((a,b)=>a-b),requestKey:crypto.randomUUID(),pickupStopId:pickupId,dropoffStopId:dropoffId};
  const h=await request('hold',pendingHold);pendingHold=null;selected.clear();activeHold=h;
  message(h.status==='Active'?'Đã giữ ghế 5 phút. Hoàn tất thông tin và xác nhận đặt vé trước khi hết hạn.':'Thao tác cũ đã xử lý. Kiểm tra vé hoặc giữ chỗ hiện tại.');
  await sync();if(activeHold)focusCheckout();
 }));
 el('release-button').addEventListener('click',async()=>{
  const holdId=activeHold?.id;
  if(!holdId)return;
  if(window.confirmTransitAction&&!(await window.confirmTransitAction('Các ghế của lượt này sẽ được mở lại cho hành khách khác.','Nhả giữ chỗ')))return;
  if(activeHold?.id!==holdId||remainingMs()<=0||pendingConfirm)return;
  await mutate(async()=>{
   await request('release',{holdId});
   drafts.delete(holdId);
   activeHold=null;
   message('Đã nhả giữ chỗ.');
   await sync();
  });
  if(!activeHold&&!pendingConfirm)el('refresh-seats').focus({preventScroll:true});
 });
 el('checkout-form').addEventListener('submit',e=>{e.preventDefault();if(!e.target.reportValidity())return;mutate(async()=>{
  if(!pendingConfirm){if(!activeHold||!validPair())return;pendingConfirm={holdId:activeHold.id,contactName:el('contact-name').value,phone:el('contact-phone').value,
   passengers:[...el('passenger-fields').querySelectorAll('input')].map(x=>({tripSeatId:Number(x.dataset.seat),name:x.value})),
   customerEmail:el('customer-email')?.value||null,exchangeBookingId:exchange,pickupStopId:pickupId,dropoffStopId:dropoffId};}
  try{
   const r=await request('confirm',pendingConfirm);
   pendingConfirm=null;
   if(r.showTripConfirmation){
    const confirmation=new URL(location.pathname,location.origin);
    confirmation.searchParams.set('confirmedBookingId',r.id);
    confirmation.hash='booking-confirmed';
    location.assign(confirmation.href);
   }else location.assign(r.url);
  }
  catch(err){if(err.status&&err.status!==503)pendingConfirm=null;throw err;}
 });});
 function tick(){if(!activeHold)return;const sec=Math.ceil(remainingMs()/1000);el('countdown').textContent=String(Math.floor(sec/60)).padStart(2,'0')+':'+String(sec%60).padStart(2,'0');
  el('countdown').parentElement.classList.toggle('ending',sec<=60);
  if(sec===0&&!expiredSync){expiredSync=true;message('Đã hết thời gian giữ chỗ. Đang cập nhật trạng thái.');sync();}buttons();}
 setInterval(tick,250);
 const hub=new signalR.HubConnectionBuilder().withUrl('/hubs/seats').withAutomaticReconnect([0,2000,5000,10000]).build();
 const connection=(txt,ok=false)=>{el('connection-status').textContent=txt;el('connection-status').classList.toggle('online',ok);};
 hub.on('TripChanged',e=>{if(e.tripId!==trip||e.revision<=revision)return;
  hub.invoke('AckRevision',trip,e.revision).then(ms=>{if(ms!==null)window.dispatchEvent(new CustomEvent('seat-realtime-metric',{detail:{tripId:trip,revision:e.revision,postCommitObservedToAckMs:ms}}));}).catch(()=>{});
  wanted=Math.max(wanted,e.revision);sync();});
 hub.onreconnecting(()=>connection('Đang kết nối lại…'));
 hub.onreconnected(async()=>{try{await hub.invoke('JoinTrip',trip);await sync();connection('Đã kết nối trực tiếp',true);}catch{connection('Đang cập nhật trạng thái');}});
 hub.onclose(()=>{connection('Kết nối gián đoạn · đang cập nhật lại');setTimeout(start,5000);});
 async function start(){try{await hub.start();await hub.invoke('JoinTrip',trip);await sync();connection('Đã kết nối trực tiếp',true);}catch{connection('Đang cập nhật trạng thái');setTimeout(start,5000);}}
 document.addEventListener('visibilitychange',()=>{if(!document.hidden)sync();});window.addEventListener('online',sync);
 setInterval(()=>{if(!document.hidden)sync();},15000);
 (async()=>{try{csrf=(await request('csrf')).token;}catch(e){message(e.message,true);}await sync();start();})();
})();

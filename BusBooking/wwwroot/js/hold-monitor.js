"use strict";
(() => {
 const app=document.getElementById('hold-monitor');
 if(!app)return;
 const get=id=>document.getElementById(id);
 const trip=Number(app.dataset.trip);
 const status=get('monitor-status');
 const list=get('monitor-holds');
 let running=false, again=false, live=false, expiryTimer, countdowns=[];
 let serverTime=0, receivedAt=0;
 let previousSeats=new Map();

 function tick(){
  const now=serverTime+performance.now()-receivedAt;
  for(const item of countdowns){
   const seconds=Math.max(0,Math.ceil((item.expires-now)/1000));
   item.element.textContent=seconds===0?'Đã hết hạn':Math.floor(seconds/60)+':'+String(seconds%60).padStart(2,'0');
  }
 }

 function render(snapshot){
  serverTime=Date.parse(snapshot.serverUtc);
  receivedAt=performance.now();
  const count={Available:0,Held:0,Booked:0};
  for(const seat of snapshot.seats)count[seat.status]++;
  get('count-available').textContent=count.Available;
  get('count-held').textContent=count.Held;
  get('count-booked').textContent=count.Booked;
  get('count-expired').textContent=snapshot.expiredSeats;
  get('monitor-totals').hidden=false;
  const map=get('monitor-map');
  const changed=new Set(snapshot.seats.filter(seat=>previousSeats.get(seat.id)!==seat.status).map(seat=>seat.id));
  if(!map.hasChildNodes()||changed.size||snapshot.seats.length!==previousSeats.size){
   window.BusSeatLayout.render(map,snapshot.seats,{interactive:false});
   if(!snapshot.seats.length)map.textContent='Chuyến chưa có sơ đồ ghế.';
   if(previousSeats.size){
    for(const seat of map.querySelectorAll('[data-seat-id]')){
     if(changed.has(Number(seat.dataset.seatId)))seat.classList.add('monitor-seat-changed');
    }
   }
   previousSeats=new Map(snapshot.seats.map(seat=>[seat.id,seat.status]));
  }
  list.replaceChildren();
  countdowns=snapshot.holds.map(hold=>{
   const row=document.createElement('li');
   const label=document.createElement('strong');
   label.textContent=hold.codes.join(', ');
   const element=document.createElement('span');
   row.append(label,element);
   list.append(row);
   return {element,expires:Date.parse(hold.expiresAtUtc)};
  });
  if(!countdowns.length){
   const empty=document.createElement('li');
   empty.textContent='Không có lượt giữ đang hiệu lực.';
   list.append(empty);
  }
  tick();
  clearTimeout(expiryTimer);
  if(countdowns.length)expiryTimer=setTimeout(refresh,Math.max(200,countdowns[0].expires-serverTime+200));
  status.textContent=(live?'Đang nhận cập nhật trực tiếp':'Đang cập nhật định kỳ')+' · Phiên bản '+snapshot.revision;
 }

 async function refresh(){
  if(running){again=true;return;}
  running=true;
  try {
   const response=await fetch(app.dataset.url,{cache:'no-store',headers:{Accept:'application/json'}});
   if(!response.ok||response.redirected)throw new Error('Snapshot unavailable');
   render(await response.json());
  }catch{
   status.textContent='Chưa cập nhật được. Dữ liệu đang hiển thị có thể đã cũ.';
  }finally{
   running=false;
   if(again){again=false;refresh();}
  }
 }

 get('monitor-refresh').addEventListener('click',refresh);
 setInterval(tick,250);
 setInterval(refresh,10000);
 refresh();
 if(!window.signalR)return;
 const connection=new signalR.HubConnectionBuilder().withUrl('/hubs/seats').withAutomaticReconnect().build();
 async function join(){
  try{await connection.invoke('JoinTrip',trip);live=true;}catch{live=false;}
  refresh();
 }
 async function start(){
  try{await connection.start();await join();}catch{setTimeout(start,10000);}
 }
 connection.on('TripChanged',message=>{if(message.tripId===trip)refresh();});
 connection.onreconnecting(()=>{live=false;status.textContent='Đang kết nối lại…';});
 connection.onreconnected(join);
 connection.onclose(()=>{live=false;status.textContent='Đang cập nhật định kỳ';setTimeout(start,10000);});
 start();
})();

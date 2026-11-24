"use strict";
// Original vector shapes and one shared renderer for trip seats and admin preview.
(()=>{
 const labels={Available:'Trống',Held:'Đang giữ',Booked:'Đã đặt'};
 const paths={seat:'M7 5a3 3 0 0 1 3-3h12a3 3 0 0 1 3 3v14H7z M5 14H3v11h26V14h-2 M7 21h18v7H7z M7 28v2m18-2v2',
  berth:'M6 3h20v26H6z M9 6h14v7H9z M6 16h20 M9 19h14 M9 23h14 M9 29v2m14-2v2'};
 function icon(kind){const svg=document.createElementNS('http://www.w3.org/2000/svg','svg');
  svg.setAttribute('viewBox','0 0 32 34');svg.setAttribute('aria-hidden','true');svg.classList.add('seat-shape');
  const p=document.createElementNS(svg.namespaceURI,'path');p.setAttribute('d',paths[kind]);svg.append(p);return svg;}
 function render(container,seats,options={}){
  const focusId=document.activeElement?.dataset?.seatId;
  container.replaceChildren();container.classList.add('seat-decks');
  const floors=[...new Set(seats.map(s=>s.floor))].sort((a,b)=>a-b),kind=floors.length>1?'berth':'seat';
  container.classList.toggle('two-decks',floors.length>1);
  container.classList.toggle('sleeper-32',floors.length===2&&seats.length===32&&seats.every(s=>s.column<=2));
  const columns=Math.max(1,...seats.map(s=>s.column));
  for(const floor of floors){
   const deck=document.createElement('section');deck.className='seat-deck '+kind;
   const title=document.createElement('h3');title.className='floor-title';title.textContent=floors.length>1?(floor===1?'Tầng dưới':floor===2?'Tầng trên':'Tầng '+floor):'Sơ đồ ghế ngồi';
   const front=document.createElement('div');front.className='deck-front';
   const steering=document.createElement('span');steering.className='steering';steering.textContent=floor===1?'◉':'↑';steering.setAttribute('aria-hidden','true');
   const direction=document.createElement('span');direction.textContent=floor===1?'Đầu xe · vị trí tài xế':'Phía đầu xe';front.append(steering,direction);
   const grid=document.createElement('div');grid.className='seat-grid real-seat-grid';
   // Aisles occupy visual tracks only. No phantom seats or rewritten row/column IDs.
   const tracks=columns===4?'1fr 1fr minmax(14px,.55fr) 1fr 1fr':
    columns===3?'1fr minmax(12px,.45fr) 1fr minmax(12px,.45fr) 1fr':
    columns===2?'1fr minmax(14px,.6fr) 1fr':'1fr';
   grid.style.gridTemplateColumns=tracks;
   for(const s of seats.filter(s=>s.floor===floor).sort((a,b)=>a.row-b.row||a.column-b.column)){
    const selected=options.selectedIds?.has(s.id),active=options.activeHoldIds?.has(s.id);
    const state=active?'Lượt giữ này':s.mine?'Của tôi':selected?'Đang chọn':labels[s.status]||s.status;
    const button=document.createElement(options.interactive===false?'div':'button');
    if(button.tagName==='BUTTON')button.type='button';
    button.className='seat-button status-'+s.status+(s.mine?' mine':'')+(selected?' selected':'')+(active?' active-hold':'');
    button.dataset.seatId=String(s.id);button.style.gridRow=String(s.row);
    button.style.gridColumn=String(columns===4?s.column+(s.column>2?1:0):columns===3?s.column*2-1:columns===2?s.column*2-1:s.column);
    if(button.tagName==='BUTTON')button.disabled=!!options.disabled||s.status!=='Available';
    if(button.tagName==='BUTTON')button.setAttribute('aria-pressed',String(!!selected||!!active));
    button.setAttribute('aria-label',s.code+', '+(kind==='berth'?'giường':'ghế')+', tầng '+s.floor+', hàng '+s.row+', '+state);
    const code=document.createElement('span');code.className='seat-code';code.textContent=s.code;
    const text=document.createElement('small');text.textContent=state;button.append(icon(kind),code,text);
    if(options.onSelect&&button.tagName==='BUTTON')button.addEventListener('click',()=>options.onSelect(s));
    grid.append(button);
   }
   const count=document.createElement('p');count.className='deck-caption';count.textContent=seats.filter(s=>s.floor===floor).length+(kind==='berth'?' giường':' ghế')+' · Khoảng trống là lối đi';
   deck.append(title,front,grid,count);container.append(deck);
  }
  container.setAttribute('aria-busy','false');
  if(focusId){const next=[...container.querySelectorAll('[data-seat-id]')].find(b=>b.dataset.seatId===focusId);if(next&&!next.disabled)next.focus({preventScroll:true});}
 }
 window.BusSeatLayout={render};
})();

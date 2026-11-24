"use strict";
(() => {
 const sidebar=document.querySelector('#workspace-sidebar');
 if(sidebar){
  const button=document.querySelector('[aria-controls="workspace-sidebar"]');
  sidebar.addEventListener('shown.bs.offcanvas',()=>button?.setAttribute('aria-expanded','true'));
  sidebar.addEventListener('hidden.bs.offcanvas',()=>button?.setAttribute('aria-expanded','false'));
  const media=window.matchMedia('(min-width: 992px)');
  const adjust=()=>{if(media.matches&&window.bootstrap){bootstrap.Offcanvas.getInstance(sidebar)?.hide();}};
  media.addEventListener('change',adjust);
 }
 const preview=document.querySelector('#layout-preview');if(!preview)return;
 const field=id=>document.getElementById(id);
 function draw(){
  const rows=Number(field('Rows')?.value),cols=Number(field('Columns')?.value),floors=Number(field('Floors')?.value);preview.replaceChildren();
  if(!Number.isInteger(rows)||!Number.isInteger(cols)||!Number.isInteger(floors)||rows<1||rows>20||cols<1||cols>4||floors<1||floors>2){const message=document.createElement('p');message.textContent='Nhập 1–20 hàng, 1–4 cột và 1–2 tầng để xem trước.';preview.append(message);return;}
  const text=document.createElement('p');text.className='layout-preview-note';text.textContent=`Cấu hình đang nhập: ${rows*cols*floors} ghế · Chưa lưu`;preview.append(text);
  const seats=[];
  for(let floor=1;floor<=floors;floor++)for(let row=1;row<=rows;row++)for(let col=1;col<=cols;col++)
   seats.push({id:seats.length+1,code:String.fromCharCode(64+floor)+String((row-1)*cols+col).padStart(2,'0'),row,column:col,floor,status:'Available'});
  const map=document.createElement('div');preview.append(map);window.BusSeatLayout.render(map,seats,{interactive:false});
 }
 for(const id of ['Rows','Columns','Floors'])field(id)?.addEventListener('input',draw);draw();
})();

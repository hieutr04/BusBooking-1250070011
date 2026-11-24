"use strict";
(()=>{
 const form=document.getElementById('stops-editor');
 const format=new Intl.DateTimeFormat('vi-VN',{timeZone:'Asia/Ho_Chi_Minh',day:'2-digit',month:'2-digit',year:'numeric',hour:'2-digit',minute:'2-digit',hourCycle:'h23'});
 const time=(start,minutes)=>Number.isFinite(minutes)&&minutes>=0&&start?format.format(new Date(Date.parse(start)+minutes*60000)):'Chọn thời gian';
 if(form&&form.dataset.locked!=='true'){
  const rows=document.getElementById('stop-rows');
  function reindex(){[...rows.rows].forEach((row,i)=>row.querySelectorAll('[name]').forEach(x=>x.name=x.name.replace(/Stops\[\d+\]/,'Stops['+i+']')));}
  function updateRow(row){const input=row.querySelector('[name$=".OffsetMinutes"]'),output=row.querySelector('.stop-local-time');if(input&&output)output.textContent=time(form.dataset.startUtc,Number(input.value));}
  rows.addEventListener('input',e=>{const row=e.target.closest('tr');if(row)updateRow(row);});
  rows.addEventListener('click',e=>{if(!e.target.closest('.remove-stop'))return;if(rows.rows.length<=2){alert('Cần ít nhất điểm đầu và điểm cuối.');return;}e.target.closest('tr').remove();reindex();});
  document.getElementById('add-stop')?.addEventListener('click',()=>{
   if(rows.rows.length>=20){alert('Mỗi chuyến có tối đa 20 điểm.');return;}
   const row=document.createElement('tr');
   row.innerHTML='<td><input name="Stops[0].Id" value="0" type="hidden"><input class="name-input" name="Stops[0].Name" required maxlength="160" aria-label="Tên điểm"></td><td><input name="Stops[0].Address" maxlength="500" aria-label="Địa chỉ điểm"></td><td><input name="Stops[0].OffsetMinutes" value="0" type="number" min="0" max="10080" required aria-label="Phút sau khởi hành"></td><td><small class="stop-local-time"></small></td><td class="checkbox-cell"></td><td><button class="btn-text remove-stop" type="button">Bỏ dòng</button></td>';
   const cell=row.querySelector('.checkbox-cell');
   for(const [field,label] of [['CanPickup','Đón'],['CanDropoff','Trả'],['IsActive','Hoạt động']]){
    const l=document.createElement('label'),c=document.createElement('input'),h=document.createElement('input');
    c.type='checkbox';c.name='Stops[0].'+field;c.value='true';c.checked=true;h.type='hidden';h.name=c.name;h.value='false';l.append(c,label);cell.append(l,h);
   }
   const metadata=document.createElement('input');metadata.type='hidden';metadata.name='Stops[0].IsIllustrative';metadata.value='true';cell.append(metadata);
   rows.insertBefore(row,rows.lastElementChild);reindex();updateRow(row);row.querySelector('.name-input').focus();
  });
 }
 const append=document.getElementById('append-stop-form');
 if(append){const input=document.getElementById('append-minutes'),output=document.getElementById('append-time');const update=()=>{output.textContent=time(append.dataset.startUtc,Number(input.value));};input.addEventListener('input',update);update();}
 const specific=document.getElementById('specific-stop-form');
 if(specific){const input=document.getElementById('specific-minutes'),output=document.getElementById('specific-time');const update=()=>{const minutes=Number(input.value);output.textContent=minutes===Number(specific.dataset.duration)?format.format(new Date(specific.dataset.endUtc)):time(specific.dataset.startUtc,minutes);};input.addEventListener('input',update);update();}
})();

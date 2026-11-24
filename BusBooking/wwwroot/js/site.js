"use strict";
(() => {
 document.documentElement.classList.add('js');
 const toggle=document.querySelector('.nav-toggle'),nav=document.querySelector('#mainnav');
 const closeNav=()=>{nav?.classList.remove('open');toggle?.setAttribute('aria-expanded','false');toggle?.setAttribute('aria-label','Mở menu');};
 toggle?.addEventListener('click',()=>{const open=nav.classList.toggle('open');toggle.setAttribute('aria-expanded',String(open));toggle.setAttribute('aria-label',open?'Đóng menu':'Mở menu');});
 document.addEventListener('keydown',e=>{if(e.key!=='Escape')return;const openMenu=document.querySelector('.account-menu[open]');if(openMenu){openMenu.open=false;openMenu.querySelector('summary').focus();}else if(nav?.classList.contains('open')){closeNav();toggle.focus();}});
 document.addEventListener('click',e=>{document.querySelectorAll('.account-menu[open]').forEach(d=>{if(!d.contains(e.target))d.open=false;});});
 const path=location.pathname.toLowerCase().replace(/\/$/,'')||'/';
 document.querySelectorAll('#mainnav [data-nav]').forEach(a=>{const section=a.dataset.nav;const active=section==='home'&&(path==='/'||path==='/home'||path==='/home/index')||section==='policy'&&path==='/home/policy'||section==='contact'&&path==='/lien-he'||section==='trips'&&path.startsWith('/trips')||section==='bookings'&&path.startsWith('/bookings');if(active){a.classList.add('active');a.setAttribute('aria-current','page');}});
 document.querySelectorAll('[data-password-target]').forEach(button=>button.addEventListener('click',()=>{const input=document.getElementById(button.dataset.passwordTarget);if(!input)return;const show=input.type==='password';input.type=show?'text':'password';button.textContent=show?'Ẩn':'Hiện';button.setAttribute('aria-pressed',String(show));button.setAttribute('aria-label',show?'Ẩn mật khẩu':'Hiện mật khẩu');}));
 const dialog=document.getElementById('transit-confirm');
 window.confirmTransitAction=(message,label='Xác nhận')=>{
  if(!dialog?.showModal)return Promise.resolve(window.confirm(message));
  if(dialog.open)return Promise.resolve(false);
  document.getElementById('transit-dialog-message').textContent=message;
  document.getElementById('transit-dialog-accept').textContent=label;
  dialog.returnValue='';
  return new Promise(resolve=>{
   dialog.addEventListener('close',()=>resolve(dialog.returnValue==='accept'),{once:true});
   dialog.showModal();
  });
 };
 let confirmedForm=null;
 document.addEventListener('submit',async e=>{
  if(e.defaultPrevented||e.target.id==='checkout-form')return;
  const form=e.target,button=e.submitter,confirmation=button?.dataset.confirm;
  if(form.getAttribute('aria-busy')==='true'){e.preventDefault();return;}
  if(confirmation&&confirmedForm!==form){
   e.preventDefault();
   if(await window.confirmTransitAction(confirmation,button.textContent.trim())&&form.isConnected){
    confirmedForm=form;
    try{form.requestSubmit(button);}finally{confirmedForm=null;}
   }
   return;
  }
  if(['get','dialog'].includes(form.method.toLowerCase()))return;
  form.setAttribute('aria-busy','true');
  if(button){
   button.dataset.uiSubmitting='true';
   button.setAttribute('aria-disabled','true');
  }
 });
 const resetSubmitting=()=>{document.querySelectorAll('[data-ui-submitting]').forEach(b=>{b.removeAttribute('aria-disabled');delete b.dataset.uiSubmitting;});document.querySelectorAll('form[aria-busy]').forEach(f=>f.removeAttribute('aria-busy'));};
 window.addEventListener('pageshow',resetSubmitting);
 const deadline=document.querySelector('[data-payment-deadline]');
 if(deadline){
  const expires=Date.parse(deadline.dataset.paymentDeadline);
  const serverTime=Date.parse(deadline.dataset.paymentNow);
  const started=Date.now();
  const payment=deadline.closest('.payment-demo');
  const clock=deadline.querySelector('[data-payment-clock]');
  const button=payment.querySelector('#simulate-payment');
  const expired=payment.querySelector('[data-payment-expired]');
  const tick=()=>{
   const seconds=Math.max(0,Math.ceil((expires-serverTime-(Date.now()-started))/1000));
   clock.textContent=[Math.floor(seconds/60),seconds%60].map(value=>String(value).padStart(2,'0')).join(':');
   if(seconds>0)return true;
   button.disabled=true;
   expired.hidden=false;
   return false;
  };
  if(tick()){
   const timer=setInterval(()=>{if(!tick())clearInterval(timer);},250);
   window.addEventListener('pageshow',tick);
   document.addEventListener('visibilitychange',()=>{if(!document.hidden)tick();});
  }
 }
 const imageFallback=img=>{if(!(img instanceof HTMLImageElement)||img.dataset.fallback)return;img.dataset.fallback='1';img.src=img.dataset.imageFallback||'/images/bus-small.svg';if(img.dataset.fallbackAlt)img.alt=img.dataset.fallbackAlt;};
 document.addEventListener('error',e=>imageFallback(e.target),true);
 document.querySelectorAll('img').forEach(img=>{if(img.complete&&img.naturalWidth===0)imageFallback(img);});
})();

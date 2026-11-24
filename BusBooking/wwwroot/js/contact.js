"use strict";
(() => {
 const form=document.getElementById('contact-form');if(!form)return;
 const fields=['FullName','Email','Phone','Subject','Body'];
 const body=form.elements.namedItem('Body'),counter=document.getElementById('contact-counter');
 const updateCount=()=>{counter.textContent=`${body.value.length.toLocaleString('vi-VN')} / 4.000`;};
 body.addEventListener('input',updateCount);updateCount();
 const messageFor=input=>{
  const value=input.value.trim();
  if(input.required&&!value)return 'Vui lòng nhập trường này.';
  if(value&&input.type==='email'&&input.validity.typeMismatch)return 'Email liên hệ không hợp lệ.';
  if(input.name==='Phone'&&value&&!/^\+?[0-9 ()\-.]{7,25}$/.test(value))return 'Số điện thoại cần 7–25 ký tự, gồm số và dấu +, cách, (), - hoặc .';
  if(input.name==='Phone'&&value){const digits=value.replace(/\D/g,'').length;if(digits<7||digits>15)return 'Số điện thoại cần từ 7 đến 15 chữ số.';}
  if(input.minLength>0&&value.length<input.minLength)return `Vui lòng nhập ít nhất ${input.minLength} ký tự.`;
  if(input.maxLength>0&&input.value.length>input.maxLength)return `Vui lòng nhập tối đa ${input.maxLength} ký tự.`;
  return '';
 };
 const validate=input=>{
  const message=messageFor(input),error=document.getElementById(`${input.name}-error`);
  input.setCustomValidity(message);input.setAttribute('aria-invalid',String(Boolean(message)));
  if(error)error.textContent=message;return !message;
 };
 form.noValidate=true;
 fields.forEach(name=>{const input=form.elements.namedItem(name);input.addEventListener('input',()=>validate(input));});
 form.addEventListener('submit',event=>{
  let first=null;
  fields.forEach(name=>{const input=form.elements.namedItem(name);if(!validate(input)&&!first)first=input;});
  if(first){event.preventDefault();first.focus();return;}
  const button=form.querySelector('button[type=submit]');
  if(button.disabled){event.preventDefault();return;}
  button.querySelector('span').textContent='Đang ghi nhận…';
 });
 window.addEventListener('pageshow',()=>{form.querySelector('button[type=submit] span').textContent='Gửi liên hệ';});
})();

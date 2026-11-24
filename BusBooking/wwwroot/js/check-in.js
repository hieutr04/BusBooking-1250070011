"use strict";
(() => {
 const get=id=>document.getElementById(id);
 const scanner=get('checkin-scanner');
 if(!scanner||!window.jsQR)return;
 const video=get('checkin-preview'),button=get('checkin-camera'),status=get('checkin-status');
 const canvas=document.createElement('canvas');
 const context=canvas.getContext('2d',{willReadFrequently:true});
 let stream=null, timer, cameraRequest=0;
 scanner.hidden=false;
 if(!navigator.mediaDevices?.getUserMedia)button.hidden=true;

 function stop(){
  cameraRequest++;
  clearTimeout(timer);
  stream?.getTracks().forEach(track=>track.stop());
  stream=null;
  video.srcObject=null;
  video.hidden=true;
  button.textContent='Mở camera';
 }

 function decode(source,width,height){
  const scale=Math.min(1,900/Math.max(width,height));
  canvas.width=Math.max(1,Math.round(width*scale));
  canvas.height=Math.max(1,Math.round(height*scale));
  context.drawImage(source,0,0,canvas.width,canvas.height);
  const pixels=context.getImageData(0,0,canvas.width,canvas.height);
  return window.jsQR(pixels.data,pixels.width,pixels.height,{inversionAttempts:'dontInvert'})?.data;
 }

 function fill(value){
  if(!/^BUSCENTER-CHECKIN:[0-9a-f]{32}$/i.test(value??'')){
   status.textContent='Ảnh không có mã check-in Bus Center hợp lệ.';
   return false;
  }
  stop();
  const input=get('checkin-code');
  input.value=value;
  input.focus();
  status.textContent='Đã đọc mã. Bấm Xác nhận lên xe để lưu.';
  return true;
 }

 function scan(){
  if(!stream)return;
  if(video.readyState>=2&&video.videoWidth&&video.videoHeight){
   const value=decode(video,video.videoWidth,video.videoHeight);
   if(value&&fill(value))return;
  }
  timer=setTimeout(scan,250);
 }

 button.addEventListener('click',async()=>{
  if(stream){stop();status.textContent='Đã tắt camera.';return;}
  const request=++cameraRequest;
  try{
   const media=await navigator.mediaDevices.getUserMedia({video:{facingMode:{ideal:'environment'}},audio:false});
   if(request!==cameraRequest){media.getTracks().forEach(track=>track.stop());return;}
   stream=media;
   video.srcObject=media;
   video.hidden=false;
   button.textContent='Tắt camera';
   await video.play();
   if(request!==cameraRequest)return;
   status.textContent='Đưa mã QR vào khung hình.';
   scan();
  }catch{
   if(request!==cameraRequest)return;
   stop();
   status.textContent='Không mở được camera. Hãy chọn ảnh QR hoặc nhập mã.';
  }
 });

 get('checkin-file').addEventListener('change',async event=>{
  const file=event.target.files[0];
  if(!file)return;
  stop();
  try{
   const image=await createImageBitmap(file);
   try{fill(decode(image,image.width,image.height));}finally{image.close();}
  }catch{status.textContent='Không đọc được ảnh. Hãy thử ảnh QR khác.';}
  event.target.value='';
 });
 get('checkin-form').addEventListener('submit',stop);
 window.addEventListener('pagehide',stop);
})();

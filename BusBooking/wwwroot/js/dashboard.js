"use strict";
(() => {
 const trend=document.getElementById('revenue-trend'),trendSource=document.getElementById('trend-data');
 const reduced=window.matchMedia('(prefers-reduced-motion: reduce)').matches;
 const currency=new Intl.NumberFormat('vi-VN',{style:'currency',currency:'VND'});
 const number=new Intl.NumberFormat('vi-VN',{maximumFractionDigits:2});
 const moneyTick=value=>{
   if(Math.abs(value)>=1e9)return number.format(value/1e9)+' tỷ';
   if(Math.abs(value)>=1e6)return number.format(value/1e6)+' triệu';
   if(Math.abs(value)>=1e3)return number.format(value/1e3)+' nghìn';
   return number.format(value);
 };
 if(typeof Chart==='undefined')return;
 if(trend&&trendSource){
   const points=JSON.parse(trendSource.textContent);
   const yearly=trend.dataset.yearly==='true';
   new Chart(trend,{
     type:'line',
     data:{
       labels:points.map(point=>point.Label),
       datasets:[
         {label:yearly?'Năm đang xem':'Kỳ đang xem',data:points.map(point=>point.Receipts),borderColor:'#087f73',backgroundColor:'#087f73',pointRadius:points.length===1?4:0,borderWidth:2},
         {label:yearly?'Năm trước':'Kỳ liền trước',data:points.map(point=>point.PreviousReceipts),borderColor:'#7e92ab',backgroundColor:'#7e92ab',pointRadius:points.length===1?4:0,borderWidth:2,borderDash:[5,4]}
       ]
     },
     options:{
       responsive:true,maintainAspectRatio:false,animation:reduced?false:{duration:300},
       interaction:{mode:'index',intersect:false},
       plugins:{
         legend:{position:'bottom',labels:{color:'#52667b',usePointStyle:true,pointStyle:'line',boxWidth:18}},
         tooltip:{callbacks:{label:context=>context.dataset.label+' · '+currency.format(context.parsed.y)}}
       },
       scales:{
         y:{beginAtZero:true,suggestedMax:1,grid:{color:'#e7edf4'},border:{display:false},ticks:{color:'#52667b',maxTicksLimit:5,precision:0,callback:moneyTick}},
         x:{grid:{display:false},border:{display:false},ticks:{color:'#52667b',maxRotation:0,maxTicksLimit:yearly?12:6}}
       }
     }
   });
 }
 const canvas=document.getElementById('revenue-chart'),source=document.getElementById('chart-data');
 if(!canvas||!source)return;
 const rows=JSON.parse(source.textContent);
 new Chart(canvas,{type:'bar',data:{labels:rows.map(x=>x.Label),datasets:[{label:'Đã thanh toán',data:rows.map(x=>x.Receipts),backgroundColor:'#087f73',borderRadius:5,maxBarThickness:45},{label:'Đã hoàn tiền',data:rows.map(x=>x.Refunds),backgroundColor:'#829c92',borderRadius:5,maxBarThickness:45}]},options:{responsive:true,maintainAspectRatio:false,animation:reduced?false:{duration:300},plugins:{legend:{position:'bottom',labels:{boxWidth:10,boxHeight:10,padding:20,color:'#526d5d',font:{size:10}}},tooltip:{callbacks:{label:c=>c.dataset.label+': '+currency.format(c.parsed.y)}}},scales:{y:{beginAtZero:true,title:{display:true,text:'VND',color:'#718575',font:{size:9}},grid:{color:'#eef3ee'},border:{display:false},ticks:{color:'#6f8476',font:{size:9},callback:moneyTick}},x:{grid:{display:false},border:{display:false},ticks:{color:'#667f6c',font:{size:9}}}}}});
})();

"use strict";
document.querySelectorAll('.layout-saved-preview').forEach(section=>{
 const data=section.querySelector('script[type="application/json"]'),map=section.querySelector('.saved-seat-map');
 if(data&&map)window.BusSeatLayout.render(map,JSON.parse(data.textContent),{interactive:false});
});

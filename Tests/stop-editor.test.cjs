// Production JavaScript in a DOM fixture, not Razor or SQL execution.
const {JSDOM}=require('jsdom');const fs=require('node:fs');const path=require('node:path');const assert=require('node:assert/strict');
const root=path.resolve(__dirname,'..');const script=fs.readFileSync(path.join(root,'BusBooking/wwwroot/js/stop-editor.js'),'utf8');const cases=[];
const check=(name,run)=>{run();cases.push({name,status:'PASS',scope:'DOM fixture, production stop-editor.js'});};
function dom(locked){return new JSDOM(`<form id="stops-editor" data-locked="${locked}" data-start-utc="2026-11-04T16:00:00Z"><table><tbody id="stop-rows"><tr><td><input name="Stops[0].Id" value="1"><input name="Stops[0].OffsetMinutes" value="0"><small class="stop-local-time"></small></td></tr><tr><td><input name="Stops[1].Id" value="2"><input name="Stops[1].OffsetMinutes" value="420"><small class="stop-local-time"></small></td></tr></tbody></table><button id="add-stop" type="button">Thêm</button></form><form id="append-stop-form" data-start-utc="2026-11-04T16:00:00Z"><input id="append-minutes" value="90"><small id="append-time"></small></form>`,{runScripts:'outside-only'});}
const locked=dom(true);locked.window.eval(script);
check('Manual append preview works while full editor remains locked',()=>{
 assert.match(locked.window.document.getElementById('append-time').textContent,/00:30/);assert.match(locked.window.document.getElementById('append-time').textContent,/05\/11\/2026/);
 locked.window.document.getElementById('add-stop').click();assert.equal(locked.window.document.querySelectorAll('#stop-rows tr').length,2);
 const input=locked.window.document.getElementById('append-minutes');input.value='180';input.dispatchEvent(new locked.window.Event('input'));assert.match(locked.window.document.getElementById('append-time').textContent,/02:00/);
});locked.window.close();
const open=dom(false);open.window.eval(script);open.window.document.getElementById('add-stop').click();
check('Full editor inserts before endpoint, reindexes names, retains provenance hidden',()=>{
 const rows=open.window.document.querySelectorAll('#stop-rows tr');assert.equal(rows.length,3);assert.equal(rows[2].querySelector('input').value,'2');
 assert.equal(rows[1].querySelector('[name="Stops[1].IsIllustrative"]').value,'true');assert.equal(rows[1].querySelector('[name="Stops[1].IsIllustrative"]').type,'hidden');
 assert.equal(rows[2].querySelector('[name="Stops[2].Id"]').value,'2');assert.equal(open.window.document.activeElement,rows[1].querySelector('.name-input'));
 const minutes=rows[1].querySelector('[name$=".OffsetMinutes"]');minutes.value='180';minutes.dispatchEvent(new open.window.Event('input',{bubbles:true}));assert.match(rows[1].querySelector('.stop-local-time').textContent,/02:00/);
 rows[1].querySelector('.remove-stop').click();assert.equal(open.window.document.querySelector('[name="Stops[1].Id"]').value,'2');
});open.window.close();
const specific=new JSDOM('<form id="specific-stop-form" data-start-utc="2026-11-04T16:00:00Z" data-end-utc="2026-11-04T23:00:00Z" data-duration="420"><input id="specific-minutes" value="420"><small id="specific-time"></small></form>',{runScripts:'outside-only'});specific.window.eval(script);
check('Specific endpoint preview uses exact arrival UTC+7 and updates after minute input',()=>{
 assert.match(specific.window.document.getElementById('specific-time').textContent,/06:00/);assert.match(specific.window.document.getElementById('specific-time').textContent,/05\/11\/2026/);
 const input=specific.window.document.getElementById('specific-minutes');input.value='0';input.dispatchEvent(new specific.window.Event('input'));assert.match(specific.window.document.getElementById('specific-time').textContent,/23:00/);
});specific.window.close();
const result={utc:new Date().toISOString(),scope:'No SQL, MVC, browser rendering or screenshots',cases};fs.writeFileSync(path.join(root,'Documentation/validation/buscenter-stop-editor.json'),JSON.stringify(result,null,2));console.log(JSON.stringify(result,null,2));

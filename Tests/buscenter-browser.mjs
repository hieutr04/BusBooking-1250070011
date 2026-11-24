// Run only against an actual local MVC app backed by isolated SQL _Tests fixtures.
// Does not build a static page or fabricate screenshots when the app is unavailable.
import fs from 'node:fs/promises';import path from 'node:path';import { chromium,expect } from '@playwright/test';
const base=process.env.BASE_URL||'http://localhost:5190';const url=new URL(base);
if(!['localhost','127.0.0.1','[::1]'].includes(url.hostname))throw Error('Only local test app allowed.');
if(!process.env.FIXTURES)throw Error('Set FIXTURES to the SQL harness fixtures.json, database ending _Tests.');
const fixture=JSON.parse(await fs.readFile(process.env.FIXTURES,'utf8'));
if(!fixture.database?.endsWith('_Tests')||!fixture.tripId)throw Error('Refuse non-test fixtures.');
const output=path.resolve('buscenter-browser-results');await fs.mkdir(output,{recursive:true});const results=[];
const browser=await chromium.launch();
try{
 for(const width of [375,390,768,1024,1440]){
  const context=await browser.newContext({viewport:{width,height:1000},reducedMotion:'reduce'});const page=await context.newPage();const errors=[];
  page.on('console',m=>{if(m.type()==='error')errors.push(m.text());});page.on('pageerror',e=>errors.push(e.message));page.on('response',r=>{if(r.status()>=500)errors.push(`HTTP ${r.status()} ${new URL(r.url()).pathname}`);});
  try{
   for(const [name,route] of [['home','/'],['contact','/lien-he'],['trip-details',`/Trips/Details/${fixture.tripId}`]]){
    const response=await page.goto(base+route,{waitUntil:'networkidle'});if(!response||response.status()!==200)throw Error(`${route}: HTTP ${response?.status()}`);
    await expect(page).toHaveTitle(/Bus Center/);await expect(page.locator('.photo-credit,figcaption')).toHaveCount(0);
    if(name==='home'){
     const hero=page.locator('.buscenter-hero .hero-photo img');await expect(hero).toBeVisible();await expect.poll(()=>hero.evaluate(img=>img.complete&&img.naturalWidth>0)).toBe(true);
     await expect(page.locator('form.search-form[method=get] [name=origin]')).toBeVisible();await expect(page.locator('form.search-form [name=date]')).toBeVisible();
     if(width<=767){const toggle=page.locator('.nav-toggle');if(await toggle.isVisible()){await toggle.click();await expect(toggle).toHaveAttribute('aria-expanded','true');await page.keyboard.press('Escape');await expect(toggle).toHaveAttribute('aria-expanded','false');}}
    }
    if(name==='contact'){await expect(page.getByRole('heading',{name:'Liên hệ với chúng tôi',exact:true})).toBeVisible();await expect(page.locator('#contact-form input[name=SubmissionToken]')).toHaveCount(1);await expect(page.locator('#contact-form input[name=__RequestVerificationToken]')).toHaveCount(1);}
    if(name==='trip-details'){await expect(page.locator('#seat-map .seat-button').first()).toBeVisible();await expect(page.locator('#pickup-stop')).toBeVisible();}
    const overflow=await page.evaluate(()=>document.documentElement.scrollWidth>innerWidth+1);if(overflow)throw Error(`Horizontal overflow ${width} ${route}`);
    if(errors.length)throw Error(errors.join('\n'));
    await page.screenshot({path:path.join(output,`${name}-${width}.png`),fullPage:true});results.push({name,width,status:'PASS',source:'Actual local MVC HTTP response, screenshot',http:response.status()});
   }
   if(errors.length)throw Error(errors.join('\n'));
  }catch(e){results.push({width,status:'FAIL',error:String(e)});throw e;}finally{await context.close();}
 }
}finally{await browser.close();await fs.writeFile(path.join(output,'results.json'),JSON.stringify({utc:new Date().toISOString(),results},null,2));}

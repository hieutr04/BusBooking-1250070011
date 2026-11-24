// Native HTTP requests with independent browser cookie jars. Requires real .NET/SQL _Tests fixtures.
import {request} from '@playwright/test';
import fs from 'node:fs/promises';
const fixture=JSON.parse(await fs.readFile(process.env.FIXTURES,'utf8'));
if(!fixture.database?.endsWith('_Tests'))throw new Error('Isolated _Tests DB required.');
const base=process.env.BASE_URL||'http://localhost:5190',results=[];
const assert=(ok,m)=>{if(!ok)throw new Error(m);};
const decode=s=>s.replaceAll('&amp;','&').replaceAll('&quot;','"').replaceAll('&#x2B;','+');
async function login(account,returnUrl){
 const ctx=await request.newContext({baseURL:base,maxRedirects:0});
 const r=await ctx.get('/Account/Login'+(returnUrl===null?'':'?returnUrl='+encodeURIComponent(returnUrl)));
 const m=(await r.text()).match(/name="__RequestVerificationToken"[^>]*value="([^"]+)"/);assert(m,'CSRF token missing');
 const response=await ctx.post('/Account/Login',{form:{Email:account.Email,Password:account.password,ReturnUrl:returnUrl||'',__RequestVerificationToken:decode(m[1])}});
 return {ctx,response};
}
const cases=[
 ['Admin',fixture.admin,null,'/Admin'],['Admin',fixture.admin,'/','/Admin'],['Admin',fixture.admin,'/Home/Index','/Admin'],['Admin',fixture.admin,'/Bookings','/Admin'],['Admin',fixture.admin,'/Admin/Trips','/Admin/Trips'],
 ['Staff',fixture.staff,null,'/Staff'],['Staff',fixture.staff,'/Admin','/Staff'],['Staff',fixture.staff,'/Staff/Requests','/Staff/Requests'],
 ['Driver',fixture.driver,null,'/Driver'],['Driver',fixture.driver,'/Staff','/Driver'],['Driver',fixture.driver,'/Driver/Details/'+fixture.tripId,'/Driver/Details/'+fixture.tripId],
 ['Customer',fixture.customers[0],null,'/'],['Customer',fixture.customers[0],'/Trips/Details/'+fixture.tripId,'/Trips/Details/'+fixture.tripId],['Customer',fixture.customers[0],'/Admin','/']
];
for(const [role,data] of [['Admin',fixture.admin],['Staff',fixture.staff],['Driver',fixture.driver],['Customer',fixture.customers[0]]]){
 const fallback={Admin:'/Admin',Staff:'/Staff',Driver:'/Driver',Customer:'/'}[role];
 for(const target of ['https://evil.example','//evil.example','/Account/Login','/Account/Register','/Account/Logout','/%2f%2fevil.example','/Trips/../Admin'])cases.push([role,data,target,fallback]);
}
for(const [role,data,input,expected] of cases){
 const {ctx,response}=await login(data,input);
 try{
  assert(response.status()===302,'Login expected302, got'+response.status());const location=response.headers().location;
  assert(new URL(location,base).origin===new URL(base).origin,'External redirect');assert(location===expected,`${role}: ${input} => ${location}, expected ${expected}`);
  const again=await ctx.get('/Account/Login');assert(again.status()===302,'Authenticated Login did not redirect');assert(again.headers().location===({Admin:'/Admin',Staff:'/Staff',Driver:'/Driver',Customer:'/'}[role]),'Authenticated workspace default');
  if(role==='Admin'){const home=await ctx.get('/');assert(home.status()===200,'Admin cannot intentionally view public home');}
  const token=(await (await ctx.get('/api/seats/csrf')).json()).token;
  assert((await ctx.post('/Account/Logout',{headers:{'X-CSRF-TOKEN':token}})).status()===302,'Logout failed');
  const locked=await ctx.get('/Admin');assert(locked.status()===302&&locked.headers().location.includes('/Account/Login'),'Workspace not protected after logout');
  results.push({role,input,expected,actual:location,status:'PASS'});
 }catch(e){results.push({role,input,expected,status:'FAIL',error:e.stack});process.exitCode=1;}finally{await ctx.dispose();}
}
await fs.writeFile('login-redirect-results.json',JSON.stringify({utc:new Date().toISOString(),results},null,2));

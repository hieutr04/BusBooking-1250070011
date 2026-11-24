// k6 read-load baseline. Never evidence of seat contention: use SqlTests --http for that.
import http from 'k6/http';
import {check} from 'k6';
export const options={stages:[{duration:'20s',target:20},{duration:'20s',target:50},{duration:'20s',target:100},{duration:'10s',target:0}]};
export default function(){const r=http.get(`${__ENV.BASE_URL||'http://localhost:5190'}/api/seats/${__ENV.TRIP_ID}`);check(r,{'snapshot status 200':x=>x.status===200});}

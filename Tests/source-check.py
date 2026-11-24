#!/usr/bin/env python3
"""Static packaging/config checks only. Does NOT compile C#/Razor or test SQL/HTTP/UI."""
import json,re,subprocess,sys,xml.etree.ElementTree as ET
from pathlib import Path
root=Path(__file__).resolve().parents[1]
checks=[]
def check(name,ok,detail=''):
 checks.append({'name':name,'status':'PASS' if ok else 'FAIL','detail':detail})
for p in root.rglob('*.json'):
 if 'Documentation' in p.parts or 'TestResults' in p.parts:continue
 try:json.loads(p.read_text(encoding='utf-8-sig'));check('json:'+str(p.relative_to(root)),True)
 except Exception as e:check('json:'+str(p.relative_to(root)),False,str(e))
for p in root.rglob('*.csproj'):
 try:
  doc=ET.parse(p);check('csproj:'+str(p.relative_to(root)),doc.findtext('.//TargetFramework')=='net9.0')
 except Exception as e:check('csproj:'+str(p.relative_to(root)),False,str(e))
projects=re.findall(r'= "[^"]+", "([^"]+\.csproj)"',(root/'BusBooking.sln').read_text())
check('solution_project_paths',len(projects)==3 and all((root/Path(x.replace('\\','/'))).is_file() for x in projects))
csproj=(root/'BusBooking/BusBooking.csproj').read_text()
check('sql_server_only',all(x not in csproj for x in ['Sqlite','InMemory','Npgsql','MySql']))
check('microsoft_versions_aligned',all(v=='9.0.9' for v in re.findall(r'Include="Microsoft[^"]+" Version="([^"]+)"',csproj)))
for p in (root/'BusBooking/Views').rglob('*.cshtml'):
 for asset in re.findall(r'(?:src|href)="~/(wwwroot/)?([^"?]+)"',p.read_text()):
  check('asset:'+asset[1],(root/'BusBooking/wwwroot'/asset[1]).is_file(),str(p.relative_to(root)))
for d in ['README','REQUIREMENTS_TRACEABILITY','DATABASE_DESIGN','ERD','USE_CASE','ACTIVITY_DIAGRAMS','SEQUENCE_DIAGRAMS','SEAT_STATE_MACHINE','HOLD_ALGORITHM','REALTIME_DESIGN','PERMISSION_MATRIX','SQL_SERVER_SETUP','ASSET_SOURCES','TEST_RESULTS','CONCURRENCY_TEST_REPORT','PERFORMANCE_REPORT','LIMITATIONS','DEFENSE_GUIDE']:
 check('document:'+d,(root/(d+'.md')).is_file())
for license in ['bootstrap/LICENSE','signalr/LICENSE','chartjs/LICENSE']:
 check('license:'+license,(root/'BusBooking/wwwroot/lib'/license).is_file())
for p in list((root/'BusBooking/wwwroot/js').glob('*.js'))+list((root/'Tests').glob('*.mjs')):
 try:
  r=subprocess.run(['node','--check',str(p)],capture_output=True,text=True)
  check('js_syntax:'+str(p.relative_to(root)),r.returncode==0,r.stderr.strip())
 except FileNotFoundError:checks.append({'name':'js_syntax:'+str(p.relative_to(root)),'status':'CHƯA KIỂM CHỨNG','detail':'Node unavailable'})
result={'kind':'STATIC_SOURCE_ONLY','does_not_verify':['C# compilation','Razor','SQL Server','HTTP','browser','concurrency','300 second hold'],'checks':checks,'files':sum(1 for p in root.rglob('*') if p.is_file()),'failed':sum(x['status']=='FAIL' for x in checks)}
print(json.dumps(result,ensure_ascii=False,indent=2))
sys.exit(1 if result['failed'] else 0)

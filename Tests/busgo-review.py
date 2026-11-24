#!/usr/bin/env python3
"""Source checks only. NOT a C#/Razor build, SQL, HTTP, browser or concurrency test.
Optional: python Tests/busgo-review.py --baseline input.zip
Requires tree-sitter, tree-sitter-c-sharp, tinycss2; Node for JavaScript syntax.
"""
import argparse,hashlib,json,re,subprocess,zipfile
from pathlib import Path
import tree_sitter,tree_sitter_c_sharp,tinycss2
root=Path(__file__).resolve().parents[1];web=root/'BusBooking';checks=[]
def check(name,ok,detail=''):
 checks.append(dict(name=name,status='PASS' if ok else 'FAIL',detail=detail))
parser=tree_sitter.Parser(tree_sitter.Language(tree_sitter_c_sharp.language()))
for p in root.rglob('*.cs'):
 tree=parser.parse(p.read_bytes());check('C# syntax '+str(p.relative_to(root)),not tree.root_node.has_error,'Syntax parsing only; no type checking or Razor compilation.')
for p in (web/'wwwroot/js').glob('*.js'):
 proc=subprocess.run(['node','--check',str(p)],capture_output=True,text=True);check('JS syntax '+p.name,proc.returncode==0,proc.stderr)
css=tinycss2.parse_stylesheet((web/'wwwroot/css/busgo.css').read_text(),skip_comments=True,skip_whitespace=True)
check('BusGo CSS top-level syntax',not any(x.type=='error' for x in css))
for p in (web/'Views').rglob('*.cshtml'):
 text=p.read_text();check('No old brand/academic UI '+str(p.relative_to(web)),not re.search(r'véxe|v[ée]x[eé]\.|học tập|mô phỏng|snapshot|SQL Server|420 phút',text,re.I))
 for asset in re.findall(r'(?:src|href)="~/([^"?]+)"',text):check('Local asset '+asset,(web/'wwwroot'/asset).is_file())
for layout in ['_Layout.cshtml','_AdminLayout.cshtml']:
 s=(web/'Views/Shared'/layout).read_text();check('Shared BusGo styling '+layout,'css/busgo.css' in s and 'BusGo' in s)
append=(web/'Services/TripStopAppendService.cs').read_text();planner=(web/'Services/StopAppendPlanner.cs').read_text()
check('Append trip gate before plan/persist',append.index('await gate.Trip(')<append.index('plan=requests==null')<append.index('await Persist('))
check('Append publishes after commit',append.index('await tx.CommitAsync')<append.index('await realtime.Publish'))
check('Append no hold/booking/removal writes',all(x not in append for x in ['SeatHolds','Bookings','RemoveRange','ExecuteSql','Thread.Sleep','Task.Delay']))
check('Planner pure and no renumber assignments',all(x not in planner for x in ['SaveChanges','DbContext','existing.Remove','existing.Add','existing.Clear']))
service=(web/'Services/TripStopService.cs').read_text()
check('Full editor still guards ALL historical holds','!await db.SeatHolds.AnyAsync(h=>h.TripId==trip.Id,ct)' in service)
check('Both new Admin POSTs explicitly antiforgery',(web/'Controllers/AdminController.cs').read_text().count('[HttpPost,ValidateAntiForgeryToken]')>=2)
check('Planner/new SQL regression source present',(root/'Tests/BusBooking.SqlTests/StopAppendPlannerTests.cs').is_file() and 'Append_with_active_expired_and_confirmed_holds' in (root/'Tests/BusBooking.SqlTests/Program.cs').read_text())
args=argparse.ArgumentParser();args.add_argument('--baseline');opt=args.parse_args()
if opt.baseline:
 with zipfile.ZipFile(opt.baseline) as z:
  original={n.split('/',1)[1]:z.read(n) for n in z.namelist() if n.startswith('BusBooking/') and not n.endswith('/')}
  check('All original files retained',all((root/n).is_file() for n in original))
  protected=[]
  for n,b in original.items():
   if n.endswith(('.csproj','.sln')) or n in ['global.json','.config/dotnet-tools.json','BusBooking/Program.cs']:
    if n=='BusBooking/Program.cs':continue
    protected.append(n)
   elif n.startswith(('BusBooking/Data/','BusBooking/Models/','BusBooking/Hubs/','Database/')) or n in ['BusBooking/Controllers/AccountController.cs','BusBooking/Services/Infrastructure.cs','BusBooking/Services/LoginDestination.cs','BusBooking/Services/RealtimePublisher.cs','BusBooking/Services/HoldExpiryWorker.cs'] or n.startswith(('BusBooking/appsettings','BusBooking/Properties/')) or n.endswith('.webp'):protected.append(n)
  for n in protected:check('Protected unchanged '+n,(root/n).read_bytes()==original[n])
  for name in ['Select','Resolve']:
   pattern=r'    public (?:static |async )?[^\n]* '+name+r'\(.*?(?=\n    (?:public|//)|\Z)'
   old=re.search(pattern,original['BusBooking/Services/TripStopService.cs'].decode(),re.S);new=re.search(pattern,service,re.S)
   check('Stop server validation unchanged '+name,bool(old and new and old.group()==new.group()))
  old=original['BusBooking/Services/BookingService.cs'].decode()
  replacements={"Giữ chỗ đã chuyển thành vé; không thể nhả bằng token cũ.":"Giữ chỗ đã được xác nhận thành vé; không thể nhả lượt giữ cũ.","Retry khác yêu cầu đổi vé ban đầu.":"Yêu cầu đổi vé khác với lần xác nhận trước.","Vé đã xác nhận với điểm đón/trả khác. Không thể ghi đè bằng retry.":"Vé đã được xác nhận với điểm đón/trả khác. Vui lòng kiểm tra thông tin trên vé."}
  for a,b in replacements.items():old=old.replace(a,b)
  check('BookingService only 3 error-copy edits',old==(web/'Services/BookingService.cs').read_text())
  for token in ['UserSecretsId','BusBooking.Auth','AddIdentity','AddAntiforgery']:
   oldprog=original['BusBooking/Program.cs'].decode();newprog=(web/'Program.cs').read_text()
   check('Auth identity contract retained '+token,[x.strip() for x in oldprog.splitlines() if token in x]==[x.strip() for x in newprog.splitlines() if token in x])
result=dict(utc=__import__('datetime').datetime.now(__import__('datetime').timezone.utc).isoformat(),scope=__doc__.strip(),checks=checks)
out=root/'Documentation/validation/busgo-source-review.json';out.write_text(json.dumps(result,ensure_ascii=False,indent=2))
fail=[x for x in checks if x['status']=='FAIL'];print(json.dumps(dict(total=len(checks),passed=len(checks)-len(fail),failed=fail),ensure_ascii=False,indent=2));raise SystemExit(bool(fail))

#!/usr/bin/env python3
"""Static source/packaging regression. Does not compile Razor/C#, execute SQL or test browser layout.
Usage: python Tests/buscenter-review.py --baseline INPUT_BUSGO.zip
Requires tree-sitter, tree-sitter-c-sharp, tinycss2 and Node.
"""
from pathlib import Path
import argparse,hashlib,json,re,subprocess,zipfile,datetime
from tree_sitter import Parser,Language
import tree_sitter_c_sharp,tinycss2
r=Path(__file__).resolve().parents[1];w=r/'BusBooking';checks=[]
def check(name,ok,detail=''):checks.append(dict(name=name,status='PASS' if ok else 'FAIL',detail=detail))
p=Parser(Language(tree_sitter_c_sharp.language()))
for f in r.rglob('*.cs'):check('C# syntax '+str(f.relative_to(r)),not p.parse(f.read_bytes()).root_node.has_error,'Parsing only; no type checking or Razor compilation.')
for f in (w/'wwwroot/js').glob('*.js'):
 proc=subprocess.run(['node','--check',str(f)],capture_output=True,text=True);check('JS syntax '+f.name,proc.returncode==0,proc.stderr)
for f in (w/'wwwroot/css').glob('*.css'):
 rules=tinycss2.parse_stylesheet(f.read_text(),skip_whitespace=True,skip_comments=True);check('CSS syntax '+f.name,not any(x.type=='error' for x in rules))
for f in (w/'Views').rglob('*.cshtml'):
 text=f.read_text();check('No old display brand '+str(f.relative_to(w)),not re.search(r'BusGo|BUSGO|véxe\.',text))
 if f.parent.name!='Photos':check('No image credit caption '+str(f.relative_to(w)),not re.search(r'photo-credit|figcaption|ẢNH MINH HỌA ĐIỂM ĐẾN',text))
 for asset in re.findall(r'(?:src|href)="~/([^"?]+)"',text):check('Local asset '+asset,(w/'wwwroot'/asset).is_file())
for f in ['_Layout.cshtml','_AdminLayout.cshtml']:
 text=(w/'Views/Shared'/f).read_text();check('Shared brand/Credits '+f,'Bus Center' in text and 'css/buscenter.css' in text and 'asp-controller="Photos"' in text)
photo=(w/'Views/Shared/_Photo.cshtml').read_text();home=(w/'Views/Home/Index.cshtml').read_text();css=(w/'wwwroot/css/buscenter.css').read_text()
check('No caption node in photo partial','figcaption' not in photo and 'Author' not in photo)
check('Credits page full metadata',all(s in (w/'Views/Photos/Index.cshtml').read_text() for s in ['OriginalTitle','photo.Author','photo.SourceUrl','photo.LicenseUrl','photo.License']))
check('Hero eager image + no reveal-only content','new PhotoDisplay(PhotoCatalog.CoachHero,false)' in home and 'buscenter-hero' in home and 'reveal' not in home and 'fetchpriority' in photo and 'width=' in photo and 'height=' in photo)
check('Hero mobile image explicit visible','.buscenter-hero .hero-art{display:block' in css and 'aspect-ratio:16/9' in css)
check('Search GET names unchanged',all(x in home for x in ['asp-controller="Trips" asp-action="Index" method="get"','name="origin"','name="destination"','name="date"']))
check('No credit height subtraction','calc(100% - 38px)' not in (w/'wwwroot/css/photos-contact.css').read_text())
check('Contact channels shared',(w/'Views/Shared/_SupportChannels.cshtml').exists() and '_SupportChannels' in (w/'Views/Contact/Index.cshtml').read_text() and '_SupportChannels' in (w/'Views/Shared/_Layout.cshtml').read_text())
service=(w/'Services/TripStopAppendService.cs').read_text();specific=service[service.index('    public async Task<StopAppendResult> Specific('):service.index('    private async Task<StopAppendResult> Apply(')]
check('Specific service gate before plan/persist',specific.index('await gate.Trip(')<specific.index('SpecificStopPlanner.Plan')<specific.index('await Persist('))
check('Specific service publishes after commit',specific.index('await tx.CommitAsync')<specific.index('await realtime.Publish'))
check('Specific service no hold/booking/point removal writes',all(x not in specific for x in ['SeatHolds','Bookings','Tickets','Payments','RemoveRange','ExecuteSql','Task.Delay']))
check('New points retain configured/illustrative provenance','IsIllustrative=true' in (w/'Services/SpecificStopPlanner.cs').read_text())
check('Reviewed source bounded to audit column','request.SourceReference.Trim().Length>500' in (w/'Services/SpecificStopPlanner.cs').read_text() and 'now,source.SourceReference.Trim()' in specific)
check('Admin new POST antiforgery/role','[HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> SpecificStop' in (w/'Controllers/AdminController.cs').read_text() and '[Authorize(Roles="Admin")]' in (w/'Controllers/AdminController.cs').read_text())
check('Full edit still protects ALL historical holds','!await db.SeatHolds.AnyAsync(h=>h.TripId==trip.Id,ct)' in (w/'Services/TripStopService.cs').read_text())
args=argparse.ArgumentParser();args.add_argument('--baseline',required=True);opt=args.parse_args()
with zipfile.ZipFile(opt.baseline) as z:
 old={n.split('/',1)[1]:z.read(n) for n in z.namelist() if n.startswith('BusBooking/') and not n.endswith('/')}
 check('All baseline files retained',all((r/n).is_file() for n in old))
 for n,b in old.items():
  protect=n.endswith(('.csproj','.sln','.webp')) or n=='global.json' or n.startswith(('BusBooking/Models/','BusBooking/Data/','BusBooking/Hubs/','BusBooking/Properties/','BusBooking/appsettings','Database/','.config/')) or n in ['BusBooking/Controllers/AccountController.cs','BusBooking/Controllers/ContactController.cs','BusBooking/Controllers/AdminContactsController.cs','BusBooking/Services/ContactService.cs','BusBooking/Services/ContactTokens.cs','BusBooking/Services/Infrastructure.cs','BusBooking/Services/RealtimePublisher.cs','BusBooking/Services/HoldExpiryWorker.cs','BusBooking/Services/LoginDestination.cs','BusBooking/Services/ReportService.cs'] or 'INPUT_' in n and 'REQUIREMENT' in n
  if protect:check('Protected unchanged '+n,(r/n).read_bytes()==b)
 # The complete booking service differs ONLY at point eligibility/snapshot composition.
 b=(w/'Services/BookingService.cs').read_text()
 a=b.index('        var route=await db.Routes.AsNoTracking()',b.index('public async Task<SeatSnapshot> Snapshot'))
 e=b.index('        var result=new SeatSnapshot(',a);b=b[:a]+b[e:]
 b=b.replace('visible,options.ArriveEarlyMinutes','stops.Select(s=>TripStopService.ToView(t,s)).ToList(),options.ArriveEarlyMinutes')
 b=b.replace('.ResolveNew(trip,form.PickupStopId,form.DropoffStopId,ct)','.Resolve(trip,form.PickupStopId,form.DropoffStopId,ct)').replace('.ResolveHeld(trip,hold,','.Resolve(trip,')
 check('Booking core unchanged outside point eligibility',b==old['BusBooking/Services/BookingService.cs'].decode())
 # Existing server validation method Select and Resolve retained exactly.
 oldStop=old['BusBooking/Services/TripStopService.cs'].decode();newStop=(w/'Services/TripStopService.cs').read_text()
 start='    public static (TripStop Pickup,TripStop Dropoff) Select(';end='    public async Task<(TripStop Pickup,TripStop Dropoff)> ResolveNew('
 original=oldStop[oldStop.index(start):oldStop.index('    // Caller owns the catalog/trip gate')]
 current=newStop[newStop.index(start):newStop.index(end)]
 check('Original stop ownership/order/time validation retained',original==current)
 for token in ['BusBooking.Auth','AddIdentity','AddAntiforgery','ConfigureApplicationCookie']:
  check('Auth config retained '+token,[x.strip() for x in old['BusBooking/Program.cs'].decode().splitlines() if token in x]==[x.strip() for x in (w/'Program.cs').read_text().splitlines() if token in x])
result=dict(utc=datetime.datetime.now(datetime.timezone.utc).isoformat(),scope=__doc__,checks=checks)
(r/'Documentation/validation/buscenter-source-review.json').write_text(json.dumps(result,ensure_ascii=False,indent=2));fails=[x for x in checks if x['status']=='FAIL'];print(json.dumps(dict(total=len(checks),failed=fails),ensure_ascii=False,indent=2));raise SystemExit(bool(fails))

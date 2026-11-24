#!/usr/bin/env python3
"""Source/package checks only. Never connects to SQL or starts the application."""
import argparse, hashlib, json, re, subprocess
from pathlib import Path
from zipfile import ZipFile

root=Path(__file__).resolve().parents[1]
web=root/'BusBooking'
parser=argparse.ArgumentParser()
parser.add_argument('--baseline',type=Path)
args=parser.parse_args()
checks=[]
def check(name,ok,detail=''):
    checks.append({'name':name,'status':'PASS' if ok else 'FAIL','detail':detail})

assets=json.loads((root/'Documentation/PHOTO_ASSETS.json').read_text())
check('six_real_photo_files',len(assets)==6)
actual=set((web/'wwwroot/images').rglob('*.webp'))
check('all_webp_files_documented',actual=={web/a['file'] for a in assets})
for a in assets:
    p=web/a['file'];data=p.read_bytes() if p.exists() else b''
    check('photo:'+a['key'],data[:4]==b'RIFF' and data[8:12]==b'WEBP' and hashlib.sha256(data).hexdigest()==a['sha256'])
    check('source:'+a['key'],a['source'].startswith('https://commons.wikimedia.org/wiki/File:') and a['license_url'].startswith('https://creativecommons.org/licenses/'))
    check('photo_size:'+a['key'],len(data)<450000)
for name in ['IMAGE_INTEGRATION_REPORT','CONTACT_PAGE_GUIDE','DATABASE_CHANGELOG']:
    check('documentation:'+name,(root/(name+'.md')).exists())
sql=(root/'Database/20261102000100_AddContactMessages.sql').read_text()
check('additive_sql_one_table',re.findall(r'CREATE TABLE\s+\[dbo\]\.\[([^]]+)\]',sql,re.I)==['ContactMessages'])
check('additive_sql_no_business_mutation',not re.search(r'\b(?:DELETE|UPDATE|DROP|TRUNCATE|ALTER)\s+(?:TABLE\s+)?\[?dbo',sql,re.I))
check('sql_initial_history_precondition',"MigrationId=N'20261029000100_InitialSqlServer'" in sql)
check('sql_only_new_history_insert',len(re.findall('INSERT INTO',sql,re.I))==1 and "VALUES (N'20261102000100_AddContactMessages',N'9.0.9')" in sql)
check('no_auto_migration_or_seed_added','Database.Migrate' not in (web/'Program.cs').read_text())
check('admin_contact_authorization','[Authorize(Roles="Admin")]' in (web/'Controllers/AdminContactsController.cs').read_text())
check('anti_forgery_contact','[ValidateAntiForgeryToken]' in (web/'Controllers/ContactController.cs').read_text())
check('anti_forgery_mark_read','[ValidateAntiForgeryToken]' in (web/'Controllers/AdminContactsController.cs').read_text())
check('no_raw_contact_html','Html.Raw' not in ''.join(p.read_text() for p in (web/'Views/AdminContacts').glob('*.cshtml')))
check('sql_replay_unique_index','IsUnique()' in (web/'Data/ContactModelV1.cs').read_text())
check('post_redirect_get','await service.Record' in (web/'Controllers/ContactController.cs').read_text() and 'return RedirectToAction(nameof(Index))' in (web/'Controllers/ContactController.cs').read_text())
check('no_email_delivery_code',all(x not in (web/'Services/ContactService.cs').read_text() for x in ['SmtpClient','SendMail','SendEmail']))
check('both_migration_files_present',len(list((web/'Data/Migrations').glob('[0-9]*.cs')))==2)
if args.baseline:
    with ZipFile(args.baseline) as z:
        protected=[n for n in z.namelist() if n.startswith('BusBooking/BusBooking/') and any(x in n for x in [
            '/Controllers/','/Services/BookingService.cs','/Services/CatalogService.cs','/Services/Infrastructure.cs',
            '/Services/LoginDestination.cs','/Services/ReportService.cs','/Services/Realtime',
            '/BackgroundServices/','/Hubs/','/Models/Entities.cs','/ViewModels/Forms.cs',
            '/Data/SeedData.cs','/Data/AppDbContextFactory.cs','/Data/Migrations/InitialModelV1.cs',
            '/Data/Migrations/20261029000100_InitialSqlServer.cs','/wwwroot/js/seats.js','/wwwroot/js/admin.js',
            '/wwwroot/js/dashboard.js','/Views/Trips/Details.cshtml','/Views/Bookings/',
            '/Views/Staff/','/Views/Driver/','/Properties/launchSettings.json','/appsettings', '/BusBooking.csproj'])]
        protected+=['BusBooking/global.json','BusBooking/BusBooking.sln','BusBooking/Documentation/INPUT_REQUIREMENTS_ORIGINAL.md','BusBooking/Documentation/INPUT_UI_REQUIREMENTS_ORIGINAL.md']
        for n in protected:
            p=root.parent/n
            check('unchanged:'+n,p.exists() and p.read_bytes()==z.read(n))
        for n in z.namelist():
            if not n.endswith('.cshtml'):continue
            before=z.read(n).decode('utf-8-sig');after=(root.parent/n).read_text(encoding='utf-8-sig')
            check('existing_form_contract:'+n,re.findall(r'<form\b[^>]*>',before)==re.findall(r'<form\b[^>]*>',after))
        original=z.read('BusBooking/BusBooking/Data/AppDbContext.cs').decode()
        current=(web/'Data/AppDbContext.cs').read_text().replace('    public DbSet<ContactMessage> ContactMessages=>Set<ContactMessage>();\n','').replace('        ContactModelV1.Build(b);\n','')
        check('existing_entity_mapping_unchanged',current==original)
for p in (web/'wwwroot/js').glob('*.js'):
    result=subprocess.run(['node','--check',str(p)],capture_output=True,text=True)
    check('javascript_syntax:'+p.name,result.returncode==0,result.stderr)
print(json.dumps({'kind':'STATIC_ONLY','checks':checks,'pass':sum(c['status']=='PASS' for c in checks),'fail':sum(c['status']=='FAIL' for c in checks),'runtime_status':'CHƯA KIỂM CHỨNG'},ensure_ascii=False,indent=2))
raise SystemExit(1 if any(c['status']=='FAIL' for c in checks) else 0)

import os,pathlib,tempfile,subprocess,shutil,json
source=pathlib.Path(__file__).resolve().parents[2]/'bin/deploy'
for scenario in ['ok','dirty','busy','restore-failure','migration-failure']:
 with tempfile.TemporaryDirectory() as tmp:
  root=pathlib.Path(tmp);(root/'web/deploy').mkdir(parents=True);(root/'bin').mkdir();shutil.copy(source,root/'bin/deploy')
  (root/'.cache/online').mkdir(parents=True);(root/'Builds/Server').mkdir(parents=True)
  for f in ['.cache/online/env','.cache/online/config.yml','Builds/Server/FrontierServer','compose.online.yml']:(root/f).write_text('fixture')
  mock=root/'mocks';mock.mkdir(); log=root/'calls'
  stub='''#!/usr/bin/env python3
import os,sys,json
name=os.path.basename(sys.argv[0]);args=sys.argv[1:];s=os.environ['SCENARIO']
with open(os.environ['CALL_LOG'],'a') as f:f.write(json.dumps([name]+args)+'\\n')
if name=='git':
 if args==['status','--porcelain']: print(' M changed' if s=='dirty' else '')
 elif args[:2]==['symbolic-ref','--quiet']:print('main')
 elif args[:2]==['rev-parse','--abbrev-ref']:print('origin/main')
 elif args==['rev-parse','HEAD']:print('abc123')
if name=='docker':
 if 'ps' in args and '-q' in args:print('container-'+args[-1])
 if 'inspect' in args:print('sha256:old')
 if 'psql' in args:print('1' if s=='busy' else '0')
 if 'pg_dump' in args:print('database-dump')
 if 'pg_restore' in args and s=='restore-failure':sys.exit(1)
 if 'run' in args and args[-1]=='migrate' and s=='migration-failure':sys.exit(1)
'''
  for name in ['git','docker','flock']:
   p=mock/name;p.write_text(stub);p.chmod(0o755)
  env=dict(os.environ,PATH=str(mock)+':'+os.environ['PATH'],SCENARIO=scenario,CALL_LOG=str(log))
  result=subprocess.run([str(root/'bin/deploy')],env=env,capture_output=True,text=True)
  calls=[json.loads(line) for line in log.read_text().splitlines()]
  migration=any('run' in c and c[-1]=='migrate' for c in calls)
  restart=any(c[:2]==['docker','start'] for c in calls)
  up=any('up' in c and 'nakama' in c for c in calls)
  assert (result.returncode==0)==(scenario=='ok'),(scenario,result.stderr)
  assert migration==(scenario in ['ok','migration-failure']),scenario
  assert restart==(scenario in ['busy','restore-failure']),scenario
  assert up==(scenario=='ok'),scenario
  if scenario=='dirty':assert not any(c[0]=='docker' for c in calls)
  print(scenario+': passed')

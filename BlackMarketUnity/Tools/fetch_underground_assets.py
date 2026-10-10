# Free CC0 assets used by UndergroundBuilder; run from project root.
import requests,pathlib,json,hashlib,concurrent.futures
root=pathlib.Path('Assets/BlackMarket/Art/Underground/Sources');root.mkdir(parents=True,exist_ok=True)
def fetch(name):
 j=requests.get('https://api.polyhaven.com/files/'+name,timeout=45).json();p=root/name;p.mkdir(exist_ok=True)
 files=[]
 if 'fbx' not in j:print('NO FBX',name,list(j));return
 f=j['fbx'];print(name, list(f),flush=True)
 f=f.get('1k',next(iter(f.values())));f=f.get('fbx',f);files.append(f)
 for k in ['Diffuse','nor_gl','Rough','Metal']:
  if k in j:
   variants=j[k]['1k'];files.append(variants.get('png',variants.get('jpg',next(iter(variants.values())))))
 manifest={'source':'https://polyhaven.com/a/'+name,'license':'CC0','license_url':'https://polyhaven.com/license','files':[]}
 for f in files:
  u=f['url'];r=requests.get(u,timeout=120);r.raise_for_status();dest=p/u.rsplit('/',1)[-1];dest.write_bytes(r.content);manifest['files'].append({'file':dest.name,'url':u,'sha256':hashlib.sha256(r.content).hexdigest()})
 (p/'source.json').write_text(json.dumps(manifest,indent=2));print('DONE',name,flush=True)
with concurrent.futures.ThreadPoolExecutor(max_workers=3) as e:list(e.map(fetch,['ammo_box','industrial_storage_cart','industrial_caged_sconce']))

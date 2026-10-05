"""Fetch the selected MIT Rocketbox roster at a pinned revision; preserve hashes and license."""
import concurrent.futures,hashlib,json,pathlib,subprocess,urllib.request
ROOT=pathlib.Path(__file__).resolve().parents[1]
REV='0943055db6ec570bcef9f2c8b41c9e5467c808f9'
ROSTER=['Professions/Military_Male_01','Professions/Military_Male_05','Professions/Police_Male_04','Professions/Police_Male_06','Adults/Male_Adult_04','Professions/Police_Male_02']

def main():
    # A blob-filtered checkout provides the file tree without downloading the entire library.
    checkout=pathlib.Path('/tmp/blackmarket-rocketbox')
    if not (checkout/'.git').exists(): subprocess.run(['git','clone','--depth','1','--filter=blob:none','--no-checkout','https://github.com/microsoft/Microsoft-Rocketbox.git',str(checkout)],check=True)
    if subprocess.run(['git','-C',str(checkout),'cat-file','-e',REV],stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL).returncode: subprocess.run(['git','-C',str(checkout),'fetch','--depth','1','origin',REV],check=True)
    tree=subprocess.check_output(['git','-C','/tmp/blackmarket-rocketbox','ls-tree','-r','--name-only',REV],text=True).splitlines()
    jobs=[]
    for actor in ROSTER:
        name=actor.split('/')[-1]; prefix='Assets/Avatars/'+actor+'/'
        jobs += [p for p in tree if p==prefix+'Export/'+name+'.fbx' or (p.startswith(prefix+'Textures/') and p.endswith('.tga') and ('_color' in p or '_normal' in p))]
    dest=ROOT/'Assets/BlackMarket/Resources/Rocketbox';dest.mkdir(parents=True,exist_ok=True)
    def fetch(path):
        url='https://raw.githubusercontent.com/microsoft/Microsoft-Rocketbox/'+REV+'/'+path
        target=dest/pathlib.Path(path).name
        if not target.exists():
            with urllib.request.urlopen(url,timeout=120) as response: data=response.read()
            target.write_bytes(data)
        data=target.read_bytes()
        return {'path':path,'url':url,'local':str(target.relative_to(ROOT)),'license':'MIT','sha256':hashlib.sha256(data).hexdigest(),'bytes':len(data)}
    with concurrent.futures.ThreadPoolExecutor(max_workers=6) as pool: records=list(pool.map(fetch,jobs))
    (ROOT/'Documentation/roster-manifest.json').write_text(json.dumps({'revision':REV,'files':records},indent=2))
    print('Verified',len(records),'files /',sum(r['bytes'] for r in records),'bytes',flush=True)
if __name__=='__main__':main()

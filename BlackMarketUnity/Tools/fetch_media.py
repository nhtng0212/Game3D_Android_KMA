"""Fetch attributed CC0 production media; originals stay outside the Godot asset tree."""
import concurrent.futures, hashlib, json, pathlib, urllib.request
from PIL import Image, ImageOps

ROOT = pathlib.Path(__file__).resolve().parents[1]
DEST = ROOT / 'Assets/BlackMarket/Resources/Media'
HEAD = {'User-Agent': 'BlackMarketUnity/0.2 (local educational game asset download)'}
MODELS = ['metal_office_desk', 'vintage_radio_transceiver', 'metal_tool_chest',
          'rollershutter_door', 'wooden_crate_01', 'metal_stool_01', 'Shelf_01', 'desk_lamp_arm_01', 'street_lamp_01', 'Television_01', 'television_02', 'cardboard_box_01']
SURFACES = ['painted_plaster_wall', 'concrete_floor_02', 'asphalt_02', 'brick_wall_001', 'metal_plate']

def get(url):
    with urllib.request.urlopen(urllib.request.Request(url, headers=HEAD), timeout=90) as r:
        return r.read()

def fetch(job):
    path, data = job
    path.parent.mkdir(parents=True, exist_ok=True)
    if path.exists() and hashlib.md5(path.read_bytes()).hexdigest() == data['md5']:
        return
    raw = get(data['url'])
    assert hashlib.md5(raw).hexdigest() == data['md5'], data['url']
    path.write_bytes(raw)

def main():
    records, jobs = [], []
    for aid in MODELS + SURFACES:
        info = json.loads(get('https://api.polyhaven.com/files/' + aid))
        meta = json.loads(get('https://api.polyhaven.com/info/' + aid))
        folder = DEST / aid
        record = {'id': aid, 'source': 'https://polyhaven.com/a/' + aid, 'license': 'CC0-1.0',
                  'authors': meta.get('authors', {}), 'files': {}}
        if aid in MODELS:
            item = info['fbx']['1k']['fbx']
            jobs.append((folder / (aid + '.fbx'), item))
            record['files'][aid+'.fbx'] = item
        for key, levels in info.items():
            lower = key.lower()
            if lower not in ['diffuse', 'diff', 'nor_gl', 'rough', 'metal', 'ao'] and not lower.startswith('accessories_'):
                continue
            if '1k' not in levels: continue
            versions = levels['1k']
            item = versions.get('jpg') or versions.get('png')
            if item is None: continue
            ext = pathlib.Path(item['url']).suffix
            name = lower + ext
            record['files'][name] = item
            jobs.append((folder / name, item))
        records.append(record)
    with concurrent.futures.ThreadPoolExecutor(max_workers=5) as pool:
        list(pool.map(fetch, jobs))
    # Unity URP Lit: metallic in R, smoothness (1 - roughness) in A.
    for aid in MODELS + SURFACES:
        folder = DEST / aid
        for prefix in ['', 'accessories_']:
            rough = next((p for p in folder.glob(prefix+'rough.*') if p.suffix in ('.jpg','.png')), None)
            if not rough: continue
            r = Image.open(rough).convert('L')
            metal = next((p for p in folder.glob(prefix+'metal.*') if p.suffix in ('.jpg','.png')), None)
            m = Image.open(metal).convert('L').resize(r.size) if metal else Image.new('L', r.size, 0)
            zero = Image.new('L', r.size, 0)
            Image.merge('RGBA', (m, zero, zero, ImageOps.invert(r))).save(folder/(prefix+'unity_mask.png'))
    (ROOT/'Documentation/media-manifest.json').write_text(json.dumps(records, indent=2))
    print(f'Downloaded/verified {len(jobs)} files from {len(records)} CC0 assets.', flush=True)

if __name__ == '__main__': main()

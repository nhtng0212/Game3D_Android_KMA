#!/usr/bin/env python3
"""Prepare CC0 location recordings. Requires numpy, soundfile, py7zr.
Usage: python Tools/prepare_combat_audio.py /path/to/Prepared_SFX_Library.7z
Only six trimmed game clips are imported; source archive is kept outside Assets.
"""
import sys, hashlib, json, tempfile
from pathlib import Path
import numpy as np
import soundfile as sf
import py7zr
ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'Assets/BlackMarket/Resources/FieldAudio'
RATE=44100
ARCHIVE=Path(sys.argv[1])
archive_hash=hashlib.sha256(ARCHIVE.read_bytes()).hexdigest()
records=[]
def save(name,x,source):
    x=x-np.mean(x);x=x/max(np.max(np.abs(x)),1e-6)*.84
    n=min(len(x)//2,220);x[:22]*=np.linspace(0,1,22);x[-n:]*=np.linspace(1,0,n)
    path=OUT/(name+'.wav');sf.write(path,x,RATE,subtype='PCM_16')
    records.append(dict(file=str(path.relative_to(ROOT)),source=source,seconds=round(len(x)/RATE,4),peak=float(np.max(abs(x))),rms=float(np.sqrt(np.mean(x*x))),sha256=hashlib.sha256(path.read_bytes()).hexdigest()))
    return x
with tempfile.TemporaryDirectory(prefix='north-point-audio-') as directory:
    names=['Prepared SFX Library/AK-47/C_28P.wav','Prepared SFX Library/Walther PPQ/X_39P.wav']
    with py7zr.SevenZipFile(ARCHIVE) as z:z.extract(directory,targets=names)
    for kind,member,onsets in [('rifle',names[0],[.611,3.257,6.021]),('pistol',names[1],[1.407,6.444,10.660])]:
        source=Path(directory)/member;x,rate=sf.read(source);x=x.mean(axis=1)
        source_hash=hashlib.sha256(source.read_bytes()).hexdigest()
        for i,onset in enumerate(onsets):
            start=max(0,int((onset-.006)*rate));clip=x[start:start+int(1.25*rate)].copy()
            # Remove subsonic rumble and ultrasonic content before resampling; keep the impulse.
            spectrum=np.fft.rfft(clip);f=np.fft.rfftfreq(len(clip),1/rate)
            spectrum*=f/np.sqrt(f*f+35**2);spectrum*=1/np.sqrt(1+(f/17000)**12)
            clip=np.fft.irfft(spectrum,n=len(clip));clip=np.interp(np.arange(int(1.25*RATE))/RATE,np.arange(len(clip))/rate,clip)
            # Low-level early room reflections, plus the recording's natural tail.
            dry=clip.copy()
            for delay,gain in [(0.027,.13),(0.049,.085),(0.081,.045)]:
                d=int(delay*RATE);clip[d:]+=dry[:-d]*gain
            fade=int(.3*RATE);clip[-fade:]*=np.linspace(1,0,fade)**1.5
            save(f'{kind}_{i}',clip,dict(member=member,sha256=source_hash,onset_seconds=onset))
# Door rollers: grain texture from existing CC0 metal foley plus a quiet original motor layer.
metal,rate=sf.read(ROOT/'Assets/BlackMarket/Resources/Foley/impactMetal_light_000.ogg');metal=metal.mean(axis=1)
t=np.arange(int(.75*RATE))/RATE;rng=np.random.default_rng(71)
move=np.zeros(len(t));grain=metal[:int(.075*RATE)]*np.hanning(int(.075*RATE))
for at in np.arange(0,.68,.065):
    a=int(at*RATE);count=min(len(grain),len(move)-a);move[a:a+count]+=grain[:count]*rng.uniform(.3,.5)
move+=(np.sin(2*np.pi*83*t)+.3*np.sin(2*np.pi*167*t))*.013
move*=np.sin(np.pi*np.arange(len(move))/len(move))**.4
save('door_move',move,dict(foley='Kenney impactMetal_light_000.ogg',motor='Original synthesized motor'))
manifest=dict(page='https://opengameart.org/node/21826',url='https://opengameart.org/sites/default/files/Prepared%20SFX%20Library.7z',archive_sha256=archive_hash,license='CC0',authors=['Ben Jaszczak','Brian Nelson','Kevin Heras','Matthew Nanney'],processing='Mono, 44.1 kHz PCM, rumble removal, early room reflections, fade, peak -1.5 dBFS; original firearm impulses retained',clips=records)
(ROOT/'Documentation/combat-audio-manifest.json').write_text(json.dumps(manifest,indent=2)+'\n')
# Audition: 3 pistol shots, then a 12-round AK burst at the game's cadence and gain.
demo=np.zeros(7*RATE)
for kind,at,index,gain in [('pistol',.2+i*.9,i,.62) for i in range(3)]+[('rifle',3.5+i*.11,i%3,.56) for i in range(12)]:
    clip,_=sf.read(OUT/f'{kind}_{index}.wav');a=int(at*RATE);demo[a:a+len(clip)]+=clip*gain
assert np.max(abs(demo))<1,'Audition clips at runtime gains'
sf.write(ROOT/'Documentation/Audio/pistol-and-ak.wav',demo,RATE,subtype='PCM_16')
print(json.dumps(dict(clips=len(records),audition_peak=float(np.max(abs(demo))),archive_sha256=archive_hash)))

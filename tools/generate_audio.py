"""Original deterministic synthesis; no runtime dependencies or network."""
import math, random, wave, struct, pathlib
random.seed(71)
rate=22050
for name,duration in {'shot':.3,'reload':1.2,'door':.8,'beep':.17,'alarm':1.5,'hit':.2,'step':.12,'ring':1.8,'ambient':8}.items():
    data=[]
    for i in range(int(rate*duration)):
        t=i/rate; n=random.uniform(-1,1); fade=min(1,t*100)*min(1,(duration-t)*30)
        if name=='shot': v=(n*.75+math.sin(2*math.pi*70*t)*.4)*math.exp(-t*26)
        elif name=='reload': v=n*.4*(math.exp(-abs(t-.1)*100)+math.exp(-abs(t-.65)*120)+math.exp(-abs(t-1)*90))
        elif name=='door': v=(n*.08+math.sin(2*math.pi*(95+t*50)*t)*.17)*math.sin(math.pi*t/duration)
        elif name=='beep': v=math.sin(2*math.pi*880*t)*.16
        elif name=='alarm': v=math.sin(2*math.pi*(500*t-50/math.pi*math.cos(2*math.pi*t)))*.14
        elif name=='hit': v=n*.45*math.exp(-t*20)
        elif name=='step': v=n*.3*math.exp(-t*45)
        elif name=='ring': v=(math.sin(2*math.pi*440*t)+math.sin(2*math.pi*480*t))*.12*(1 if t%1<.55 else 0)
        else: v=math.sin(2*math.pi*55*t)*.14+math.sin(2*math.pi*82.5*t)*.07+n*.015
        data.append(struct.pack('<h',int(max(-1,min(1,v*fade))*32767)))
    path=pathlib.Path('assets/audio',name+'.wav')
    with wave.open(str(path),'wb') as f: f.setnchannels(1);f.setsampwidth(2);f.setframerate(rate);f.writeframes(b''.join(data))
    if name=='ambient':
        pathlib.Path(str(path)+'.import').write_text('[remap]\nimporter="wav"\ntype="AudioStreamWAV"\n\n[params]\nedit/loop_mode=1\ncompress/mode=0\n')

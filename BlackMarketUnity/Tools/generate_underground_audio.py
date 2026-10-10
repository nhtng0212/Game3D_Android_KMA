import wave,random,math,struct,pathlib
random.seed(71)
p=pathlib.Path('Assets/BlackMarket/Resources/Audio')
for name,duration in [('explosion',3),('power_down',2)]:
 rate=22050;prev=0;samples=[]
 for i in range(rate*duration):
  t=i/rate;prev=.9*prev+.1*random.uniform(-1,1)
  if name=='explosion':v=(prev*3+math.sin(t*math.pi*2*(45-5*t))*.22)*math.exp(-t*2.1)*min(1,t*300)
  else:v=(math.sin(2*math.pi*(500*t-100*t*t))*.25+prev*.2)*max(0,1-t/2)
  samples.append(struct.pack('<h',int(max(-1,min(1,v))*.85*32767)))
 with wave.open(str(p/(name+'.wav')),'wb') as f:f.setparams((1,2,rate,0,'NONE','not compressed'));f.writeframes(b''.join(samples))

from PIL import Image,ImageDraw,ImageFilter
import random
random.seed(71)
for kind in ['concrete','floor']:
    n=512
    img=Image.new('RGB',(n,n))
    pix=img.load()
    for y in range(n):
        for x in range(n):
            noise=random.randrange(-16,17)
            base=168 if kind=='concrete' else 152
            pix[x,y]=(base+noise,base+noise,base+noise)
    d=ImageDraw.Draw(img)
    if kind=='floor':
        for t in range(0,n,128):
            d.line((t,0,t,n),fill=(92,92,92),width=2)
            d.line((0,t,n,t),fill=(92,92,92),width=2)
    else:
        for i in range(140):
            x=random.randrange(n);y=random.randrange(n); r=random.randrange(1,4)
            d.ellipse((x,y,x+r,y+r),fill=(125,125,125))
        d.line((0,0,512,0),fill=(110,110,110),width=2)
        d.line((0,0,0,512),fill=(110,110,110),width=2)
    img.save('assets/textures/'+kind+'.png')

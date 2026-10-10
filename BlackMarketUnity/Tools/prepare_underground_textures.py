# Pack URP masks and generate the control center display from downloaded source textures.
from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
root=Path('Assets/BlackMarket/Art/Underground')
for name in ['ammo_box','industrial_storage_cart','industrial_caged_sconce']:
 d=root/'Sources'/name;rough=Image.open(next(d.glob('*rough*.png'))).convert('L');metal=Image.open(next(d.glob('*metal*.png'))).convert('L').resize(rough.size);zero=Image.new('L',rough.size);smooth=rough.point(lambda p:255-p);Image.merge('RGBA',(metal,zero,zero,smooth)).save(d/(name+'_metallic_smoothness.png'))
im=Image.new('RGB',(1536,384),(4,17,26));draw=ImageDraw.Draw(im);font=ImageFont.truetype('/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf',21);small=ImageFont.truetype('/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf',16)
for x in range(0,1536,32):draw.line((x,0,x,384),fill=(9,31,42))
for y in range(0,384,32):draw.line((0,y,1536,y),fill=(9,31,42))
for i in range(3):
 x=20+i*510;draw.rounded_rectangle((x,16,x+488,365),radius=8,outline=(33,126,148),width=2)
 draw.text((x+18,29),['HỆ THỐNG CĂN CỨ','MẠNG ĐIỀU PHỐI','LƯU TRỮ BẢO MẬT'][i],font=font,fill=(91,204,220))
if True:
 for i,label in enumerate(['B1 / QUÂN GIỚI','B2 / ĐIỀU PHỐI','006 / AN NINH','NGUỒN DỰ PHÒNG']):
  y=88+i*65;draw.text((42,y),label,font=small,fill=(150,187,191));draw.rectangle((275,y+2,450,y+18),outline=(37,84,93));draw.rectangle((278,y+5,300+i*42,y+15),fill=(44,173,156))
 points=[(600,150),(750,100),(910,150),(750,240),(610,300),(900,300)]
 for a,b in [(0,1),(1,2),(0,3),(2,3),(3,4),(3,5)]:draw.line((*points[a],*points[b]),fill=(46,152,174),width=3)
 for x,y in points:draw.ellipse((x-12,y-12,x+12,y+12),fill=(100,220,211))
 for i,line in enumerate(['ORDER 71 / PHỤ LỤC MẬT','USB: YÊU CẦU XÁC THỰC','MÃ: ••••••••••','NGƯỜI QUẢN LÝ: MARCUS','BẢN SAO: NGOẠI TUYẾN']):draw.text((1060,100+i*43),line,font=small,fill=(145,202,204))
im.save(root/'control_display.png')

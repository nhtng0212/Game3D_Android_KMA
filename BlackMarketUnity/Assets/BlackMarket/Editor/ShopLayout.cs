using UnityEngine;
namespace BlackMarket.Editor {
    public static partial class NorthPointBuilder {
        // All dimensions in metres. Openings form two independent loops around the service corridor.
        static void WallRun(bool alongX,float fixedAt,float from,float to,params float[] doors){
            float cursor=from;System.Array.Sort(doors);
            foreach(float door in doors){WallPiece(alongX,fixedAt,cursor,door-1.1f);var p=alongX?new Vector3(door,3.1f,fixedAt):new Vector3(fixedAt,3.1f,door);Box("Door lintel",p,alongX?new Vector3(2.2f,1,.2f):new Vector3(.2f,1,2.2f),"painted_plaster_wall");cursor=door+1.1f;}
            WallPiece(alongX,fixedAt,cursor,to);
        }
        static void WallPiece(bool x,float fixedAt,float a,float b){if(b<=a)return;Box("Room partition",x?new Vector3((a+b)/2,1.8f,fixedAt):new Vector3(fixedAt,1.8f,(a+b)/2),x?new Vector3(b-a,3.6f,.2f):new Vector3(.2f,3.6f,b-a),"painted_plaster_wall");}
        static int cartonNumber;
        static void Carton(Vector3 p,Vector3 size,bool solid=true){
            if(cartonNumber++%3==0){var box=Model("cardboard_box_01",p,size.x,0,solid);var bounds=box.GetComponentInChildren<Renderer>().bounds;foreach(var r in box.GetComponentsInChildren<Renderer>())bounds.Encapsulate(r.bounds);box.transform.localScale=new Vector3(size.x/bounds.size.x,size.y/bounds.size.y,size.z/bounds.size.z);}
            else {Box("Stock / sealed carton",p+Vector3.up*size.y/2,size,"Cardboard",solid);Box("Packing tape",p+new Vector3(0,size.y+.003f,0),new Vector3(.065f,.006f,size.z),"PackingTape",false);}
            Box("Shipping label",p+new Vector3(size.x*.15f,size.y*.55f,-size.z*.5f-.004f),new Vector3(size.x*.35f,size.y*.22f,.006f),"ShippingLabel",false);
        }
        static void RetailShelf(Vector3 p,bool loaded){
            // Open rack: only actual posts, shelves and merchandise occlude vision.
            for(int i=0;i<4;i++)Box("Rack steel upright",p+new Vector3(i%2==0?-1.05f:1.05f,1.1f,i<2?-.38f:.38f),new Vector3(.065f,2.2f,.065f),"Steel");
            for(int tier=0;tier<4;tier++){float y=.18f+tier*.59f;Box("Rack shelf",p+new Vector3(0,y,0),new Vector3(2.2f,.06f,.85f),"Steel");
                for(int j=0;j<(loaded?4:2);j++){float x=-.8f+j*.52f;if(loaded || tier%2==0)Carton(p+new Vector3(x,y+.03f,0),new Vector3(.44f,.42f,.63f));else Speaker(p+new Vector3(x,y+.03f,0));}}
        }
        static void Speaker(Vector3 p){Box("Audio speaker cabinet",p+Vector3.up*.2f,new Vector3(.27f,.4f,.23f),"Black",false);for(int i=0;i<2;i++){var g=GameObject.CreatePrimitive(PrimitiveType.Cylinder);g.name="Speaker driver";g.transform.SetParent(parent);g.transform.position=p+new Vector3(0,.12f+i*.18f,-.12f);g.transform.rotation=Quaternion.Euler(90,0,0);g.transform.localScale=new Vector3(.16f,.012f,.16f);g.GetComponent<Renderer>().sharedMaterial=mats["Steel"];UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());}}
        static void Workbench(Vector3 p,bool radio=false){
            Model("metal_office_desk",p,2);Model("metal_stool_01",p+new Vector3(0,0,-1),.5f);
            Model("desk_lamp_arm_01",p+new Vector3(-.6f,.85f,.15f),.35f,180,false);
            if(radio)Model("vintage_radio_transceiver",p+new Vector3(.3f,.85f,0),.55f,0,false);else Monitor(p+Vector3.up*1.24f,"NORTH POINT\nDIAGNOSTICS");
            Box("Keyboard",p+new Vector3(0,.875f,-.3f),new Vector3(.45f,.04f,.17f),"Black",false);
            for(int k=0;k<5;k++){Box("Service paperwork",p+new Vector3(.55f,.86f+k*.004f,-.18f),new Vector3(.23f,.003f,.3f),"Paper",false);Box("Screwdriver handle",p+new Vector3(-.4f+k*.09f,.88f,-.3f),new Vector3(.035f,.035f,.1f),"PackingTape",false);Box("Screwdriver shaft",p+new Vector3(-.4f+k*.09f,.88f,-.2f),new Vector3(.012f,.012f,.12f),"Steel",false);}
        }
        static void Route(string id,params Vector3[] points){for(int i=0;i<points.Length;i++)Mark("patrol_"+id+"_"+i,points[i]);}
        static void Shop(){
            cartonNumber=0;
            Floor(32,20,-20,"asphalt_02");Floor(28,36,0,"concrete_floor_02");
            Box("West facade",new Vector3(-14,1.85f,18),new Vector3(.3f,3.7f,36),"brick_wall_001");Box("East facade",new Vector3(14,1.85f,18),new Vector3(.3f,3.7f,36),"brick_wall_001");
            Box("Rear wall",new Vector3(0,1.85f,36),new Vector3(28,3.7f,.3f),"brick_wall_001");Box("Ceiling",new Vector3(0,3.8f,18),new Vector3(28,.2f,36),"painted_plaster_wall");
            WallRun(true,0,-14,14,0);WallRun(true,12,-14,14,-9,0,9);WallRun(true,24,-14,14,-9,0,9);
            WallRun(false,-3,12,24,15,21);WallRun(false,3,12,24,15,21);
            WallRun(true,18,-14,-3,-9);WallRun(true,19,3,14,9);
            WallRun(false,3,24,36,27,33);WallRun(true,30,-14,3,-9);WallRun(true,30,3,14,9);
            // Entrance sales floor, checkout, showroom and repair drop-off.
            Box("Shop fascia",new Vector3(0,4.15f,-.1f),new Vector3(28,1,.3f),"Steel");Sign("NORTH POINT  /  ELECTRONICS & REPAIR",new Vector3(0,4.15f,-.28f),.16f);
            foreach(float x in new[]{-9f,9f}){Box("Display window",new Vector3(x,1.75f,-.15f),new Vector3(7,2.2f,.035f),"Glass");Sign(x<0?"AUDIO / COMPUTING":"REPAIRS / COLLECTION",new Vector3(x,1.8f,-.19f),.065f);}
            for(int z=3;z<=9;z+=3){RetailShelf(new Vector3(-6,0,z),true);RetailShelf(new Vector3(6,0,z),z!=6);}
            for(int i=0;i<3;i++){Workbench(new Vector3(-11,0,3+i*3));Model(i%2==0?"Television_01":"television_02",new Vector3(11,.85f,3+i*3),.65f,0,false);Model("metal_office_desk",new Vector3(11,0,3+i*3),2);}
            Workbench(new Vector3(-2,0,9.5f));Item("note","GHI CHÚ MARCUS",new Vector3(-2,.9f,8.9f));
            Sign("SERVICE CORRIDOR",new Vector3(0,2.8f,11.86f),.07f);Sign("M. CARTER / OFFICE",new Vector3(-9,2.8f,11.86f),.065f);Sign("REPAIR INTAKE",new Vector3(9,2.8f,11.86f),.065f);
            // Marcus office and records/staff room, with two routes into the rear corridor.
            Workbench(new Vector3(-10,0,16.7f));Monitor(new Vector3(-10,1.24f,16.7f),"M. CARTER\nORDER #071");
            Box("Marcus keycard",new Vector3(-10.65f,.86f,16.3f),new Vector3(.16f,.008f,.1f),"Paper",false);Item("keycard","THẺ MARCUS",new Vector3(-10.65f,.9f,16.05f));Item("computer","TERMINAL MARCUS",new Vector3(-9.5f,1.1f,16.1f));
            Model("Shelf_01",new Vector3(-12.8f,0,14.5f),2,90);Model("metal_tool_chest",new Vector3(-5,0,17),1.4f);
            Workbench(new Vector3(-11,0,22.5f));Model("Shelf_01",new Vector3(-5,0,22.5f),2);Carton(new Vector3(-6,0,20),new Vector3(1.2f,1.15f,.9f));
            Sign("RECORDS / STAFF",new Vector3(-9,2.8f,17.87f),.065f);
            // Workshop: repair benches, parts stock and the radio distraction.
            Workbench(new Vector3(6,0,16.7f));Workbench(new Vector3(12,0,16.7f));Model("television_02",new Vector3(12.5f,.85f,16.7f),.45f,160,false);Workbench(new Vector3(10,0,22.5f),true);Item("radio","RADIO / ĐÁNH LẠC HƯỚNG",new Vector3(10.3f,1,21.8f));
            Model("metal_tool_chest",new Vector3(5,0,22.5f),1.5f);RetailShelf(new Vector3(12,0,20.5f),true);Sign("WORKSHOP / TEST BENCH",new Vector3(9,2.8f,18.87f),.065f);
            // Warehouse and dispatch: staggered full-height stock, low cover, packing tables.
            foreach(float z in new[]{26.5f,28.5f,33.5f}){RetailShelf(new Vector3(-5.5f,0,z),true);RetailShelf(new Vector3(-12,0,z),true);}
            for(int i=0;i<5;i++){var p=new Vector3(5+i%2*2.1f,0,25.5f+i/2*3.5f);Model("wooden_crate_01",p,1.25f);if(i%2==0)Carton(p+Vector3.up*.75f,new Vector3(.8f,.6f,.75f));}
            Workbench(new Vector3(12,0,34));Carton(new Vector3(-2,0,26),new Vector3(1.5f,1.05f,1.4f));Carton(new Vector3(1,0,28.5f),new Vector3(1.2f,1.6f,1));
            Sign("WAREHOUSE / STOCK",new Vector3(-9,2.8f,23.87f),.065f);Sign("DISPATCH / RECEIVING",new Vector3(9,2.8f,23.87f),.065f);
            Model("rollershutter_door",new Vector3(0,0,35.6f),2.7f);Sign("06",new Vector3(0,2.4f,35.25f),.22f);Box("Biometric panel",new Vector3(1.65f,1.3f,35.3f),new Vector3(.22f,.4f,.15f),"Screen",false);Item("door06","DOOR 06 / SCANNER",new Vector3(0,1.1f,34.8f));
            foreach(float z in new[]{3f,9f,15f,21f,27f,33f})foreach(float x in new[]{-9f,0f,9f})Fixture(new Vector3(x,3.55f,z),x<0);
            foreach(float z in new[]{12f,24f,34f})Light("Emergency route",new Vector3(0,2.7f,z),new Color(.3f,.55f,.65f),.8f,6);
            for(int z=2;z<36;z+=4){Box("Service duct",new Vector3(2.5f,3.4f,z),new Vector3(.35f,.3f,4),"metal_plate",false);Box("Structural beam",new Vector3(0,3.6f,z),new Vector3(28,.18f,.12f),"Steel",false);}
            foreach(float x in new[]{-12f,12f}){Model("street_lamp_01",new Vector3(x,0,-8),1.2f);Light("Emergency street",new Vector3(x,5,-8),new Color(1,.65f,.3f),3,14);}
            foreach(var practical in parent.GetComponentsInChildren<Light>())if(practical.name=="Ceiling practical")practical.shadows=LightShadows.None;
            Mark("spawn",new Vector3(0,.1f,-7));Mark("enemy_a",new Vector3(-9,.1f,27),180);Mark("enemy_b",new Vector3(9,.1f,32),180);
            Route("enemy_a",new Vector3(-9,0,27),new Vector3(-9,0,21),new Vector3(0,0,21),new Vector3(0,0,15),new Vector3(-9,0,15),new Vector3(-9,0,21));
            Route("enemy_b",new Vector3(9,0,32),new Vector3(9,0,27),new Vector3(9,0,21),new Vector3(0,0,21),new Vector3(0,0,27),new Vector3(9,0,27));
            Mark("shutter",new Vector3(0,0,24));Mark("alarm",new Vector3(10,1,22));for(int i=0;i<3;i++){var c=Mark("camera_"+i,new Vector3(-2,3,6+i*12));c.LookAt(new Vector3(0,1,10+i*10));}
        }
    }
}

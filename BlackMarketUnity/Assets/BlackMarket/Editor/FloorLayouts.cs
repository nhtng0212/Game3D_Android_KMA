using UnityEngine;
using UnityEngine.AI;

namespace BlackMarket.Editor {
    public static partial class NorthPointBuilder {
        // Seven authored floors. Coordinates are metres; each entrance stair is playable geometry.
        static void CampaignFloor(int stage) {
            cartonNumber=0;
            if(stage==0){Shop();ShopUpgrade();DenseDressing(0);RoomExpansion(0);return;}
            float width=stage==1?26:stage==2?30:stage==3?34:stage==4?30:stage==5?28:36;
            float depth=stage==1?32:stage==2?30:stage==3?30:stage==4?40:stage==5?38:32;
            Shell(width,depth,stage);
            switch(stage){case 1:UtilityFloor();break;case 2:ArchiveFloor();break;case 3:SecurityFloor();break;case 4:MedicalFloor();break;case 5:ServerFloor();break;case 6:VictorRooms();break;}
            ExtraPatrols(stage);DenseDressing(stage);RoomExpansion(stage);PatrolWindows(stage);
            EntranceStairs();
            Mark("safe",new Vector3(0,.1f,3));
            for(int i=0;i<3;i++){var cam=Mark("camera_"+i,new Vector3(i==1?width/2-1:-width/2+1,3.2f,5+i*(depth-9)/3));cam.LookAt(new Vector3(0,1,8+i*(depth-9)/3));Box("CCTV",cam.position,new Vector3(.3f,.2f,.4f),"Paper",false);}
            Mark("alarm",new Vector3(width/2-3,1,depth-6));
            if(stage<3)Mark("shutter",new Vector3(0,4,depth-2));
            Supply(new Vector3(-width/2+2,0,4),"supply_entry");
            Sign(StoryData.Locations[stage],new Vector3(0,2.7f,2.8f),.065f);
        }
        static void Shell(float w,float d,int stage){
            string floorMat="FloorSurface"+stage,wallMat="WallSurface"+stage;
            Color[] wallColors={Color.white,new Color(.55f,.5f,.37f),new Color(.4f,.43f,.38f),new Color(.32f,.49f,.6f),new Color(.66f,.8f,.73f),new Color(.3f,.36f,.5f),new Color(.45f,.29f,.25f)};
            var surface=new Material(mats["concrete_floor_02"]);surface.SetColor("_BaseColor",stage==4?new Color(.72f,.85f,.8f):stage==3?new Color(.53f,.65f,.72f):stage==6?new Color(.58f,.43f,.36f):new Color(.7f,.7f,.7f));SaveMat(floorMat,surface);
            var wall=new Material(mats["painted_plaster_wall"]);wall.SetColor("_BaseColor",wallColors[stage]);SaveMat(wallMat,wall);
            Floor(w,d,0,floorMat);
            Box("West wall",new Vector3(-w/2,2,d/2),new Vector3(.3f,4,d),wallMat);
            Box("East wall",new Vector3(w/2,2,d/2),new Vector3(.3f,4,d),wallMat);
            Box("Rear wall",new Vector3(0,2,d),new Vector3(w,4,.3f),wallMat);
            WallPiece(true,0,-w/2,-1.65f);WallPiece(true,0,1.65f,w/2);
            Box("Ceiling",new Vector3(0,4.1f,d/2),new Vector3(w,.2f,d),"metal_plate");
            Color code=stage==1?new Color(.95f,.5f,.12f):stage==2?new Color(.65f,.55f,.3f):stage==3?new Color(.22f,.65f,.8f):stage==4?new Color(.35f,.8f,.65f):stage==5?new Color(.35f,.5f,1):new Color(.95f,.2f,.12f);
            Plain("FloorAccent"+stage,code,.1f,.3f,1);
            for(int z=4;z<d;z+=6){Fixture(new Vector3(-w/2+5,3.7f,z),stage==1||stage==2);Fixture(new Vector3(w/2-5,3.7f,z),stage==6);Box("Floor route stripe",new Vector3(-1.5f,.015f,z),new Vector3(.09f,.02f,2.5f),"FloorAccent"+stage,false);}
            Light("Emergency stair",new Vector3(0,3,2),code,2.2f,9);
        }
        static void EntranceStairs(){
            // A six metre run descends 3 m, with .2 m risers below the controller step offset.
            Box("Upper stair landing",new Vector3(0,2.85f,-5.5f),new Vector3(3,.3f,3),"concrete_floor_02");
            for(int i=0;i<15;i++){float h=3-(i+1)*.2f;Box("Stair tread "+i,new Vector3(0,h/2-.05f,-3.8f+i*.4f),new Vector3(3,Mathf.Max(.1f,h+.1f),.4f),"concrete_floor_02");Box("Stair nosing",new Vector3(0,h+.012f,-3.61f+i*.4f),new Vector3(2.9f,.018f,.035f),"Paper",false);}
            Box("Stairwell base",new Vector3(0,-.25f,-2.5f),new Vector3(3,.3f,9),"concrete_floor_02");
            foreach(float x in new[]{-1.65f,1.65f}){Box("Stairwell side wall",new Vector3(x,2.8f,-2.5f),new Vector3(.25f,5.6f,9),"painted_plaster_wall");for(int i=0;i<8;i++)Box("Stair rail post",new Vector3(x*.88f,3.5f-i*.4f,-4+i*.8f),new Vector3(.055f,1,.055f),"Steel");var rail=Box("Stair handrail",new Vector3(x*.88f,2.5f,-1),new Vector3(.07f,.07f,6.7f),"Steel");rail.transform.rotation=Quaternion.Euler(26.565f,0,0);}
            Box("Sealed upper fire door",new Vector3(0,4.25f,-6.9f),new Vector3(3,2.5f,.2f),"Steel");
            Box("Stairwell upper header",new Vector3(0,4.8f,0),new Vector3(3.5f,1.4f,.2f),"painted_plaster_wall");
            Box("Stairwell ceiling",new Vector3(0,5.5f,-2.5f),new Vector3(3.5f,.2f,9),"metal_plate");
            Light("Emergency landing",new Vector3(0,5,-5),new Color(.75f,.83f,1),2,6);
            Mark("spawn",new Vector3(0,3.08f,-5.6f));Mark("stair_bottom",new Vector3(0,.1f,3));
        }
        static void StairExit(Vector3 p,string id,string label){
            Box("Stair access frame",p+new Vector3(-1.3f,1.4f,0),new Vector3(.15f,2.8f,.4f),"Steel");Box("Stair access frame",p+new Vector3(1.3f,1.4f,0),new Vector3(.15f,2.8f,.4f),"Steel");
            Box("Stair fire door",p+Vector3.up*1.35f,new Vector3(2.4f,2.7f,.18f),"metal_plate");Sign(label+"\nSTAIRS  /  DOWN",p+new Vector3(0,2.2f,-.12f),.055f);Item(id,label+" / MỞ CỬA CẦU THANG",p+new Vector3(0,.8f,-1));
            Light("Emergency exit",p+new Vector3(0,2.8f,-.4f),new Color(.3f,.9f,.65f),1.5f,5);
        }
        static void RoomSign(string name,float x,float z){Sign(name,new Vector3(x,2.65f,z-.14f),.055f);}
        static void Electrical(Vector3 p){
            for(int i=0;i<3;i++){Box("Electrical switchgear",p+new Vector3(i*1.15f,1.1f,0),new Vector3(.9f,2.2f,.6f),"Steel");for(int j=0;j<4;j++){Box("Breaker",p+new Vector3(i*1.15f,.55f+j*.35f,-.32f),new Vector3(.55f,.15f,.035f),"Black",false);Box("Breaker status",p+new Vector3(i*1.15f+.3f,.55f+j*.35f,-.35f),new Vector3(.05f,.06f,.02f),"Amber",false);}}
        }
        static void Toilet(Vector3 p){
            Box("WC cistern",p+new Vector3(0,.75f,.18f),new Vector3(.55f,.7f,.25f),"Paper");Box("WC pedestal",p+new Vector3(0,.25f,-.15f),new Vector3(.38f,.5f,.55f),"Paper");Box("WC seat",p+new Vector3(0,.52f,-.2f),new Vector3(.52f,.09f,.62f),"Paper");
            Box("Wash basin",p+new Vector3(1.3f,.8f,0),new Vector3(.7f,.18f,.55f),"Paper");Box("Mirror",p+new Vector3(1.3f,1.5f,.27f),new Vector3(.65f,.65f,.04f),"Glass",false);
        }
        static void Bed(Vector3 p){Box("Medical bed",p+Vector3.up*.5f,new Vector3(1.1f,.3f,2.1f),"Steel");Box("Mattress",p+Vector3.up*.72f,new Vector3(1.05f,.2f,2),"Paper");Box("Pillow",p+new Vector3(0,.87f,.65f),new Vector3(.7f,.15f,.4f),"Paper",false);foreach(float x in new[]{-.48f,.48f})Box("Bed guard",p+new Vector3(x,.94f,0),new Vector3(.05f,.12f,1.3f),"Steel");}
        static void SecurityDoor(float x,float z){
            var marker=Mark("shutter",new Vector3(x,3.5f,z));var door=Box("Security shutter B",new Vector3(x,5,z),new Vector3(2.05f,3,.2f),"metal_plate");door.isStatic=false;door.transform.SetParent(marker,true);var obstacle=door.AddComponent<NavMeshObstacle>();obstacle.shape=NavMeshObstacleShape.Box;obstacle.size=Vector3.one;obstacle.carving=true;
        }
        static void ShopUpgrade(){
            // Replace the opaque front wall with a glazed storefront and a locked glass entrance.
            var remove=new System.Collections.Generic.List<GameObject>();foreach(Transform t in parent)if((t.name=="Room partition" && Mathf.Abs(t.position.z)<.01f)||t.name=="Display window")remove.Add(t.gameObject);foreach(var g in remove)UnityEngine.Object.DestroyImmediate(g);
            var glass=Plain("StorefrontGlass",new Color(.24f,.45f,.52f,.14f),0,.85f);glass.SetFloat("_Surface",1);glass.SetFloat("_SrcBlend",5);glass.SetFloat("_DstBlend",10);glass.SetFloat("_ZWrite",0);glass.SetFloat("_Cull",0);glass.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");glass.renderQueue=3000;
            Box("Glazed storefront boundary",new Vector3(0,1.8f,0),new Vector3(27.7f,3.5f,.09f),"StorefrontGlass");for(int x=-14;x<=14;x+=2)Box("Storefront mullion",new Vector3(x,1.8f,-.07f),new Vector3(.06f,3.6f,.09f),"Steel");
            Box("Front door handle",new Vector3(.7f,1.2f,.12f),new Vector3(.035f,.5f,.04f),"Steel");Sign("CLOSED  /  21:30",new Vector3(0,1.8f,.16f),.06f,null,180);
            foreach(var m in parent.GetComponentsInChildren<WorldMarker>())if(m.id=="spawn")m.transform.position=new Vector3(0,.1f,2);
            Mark("safe",new Vector3(0,.1f,2));
            foreach(var marker in parent.GetComponentsInChildren<WorldMarker>())if(marker.id.StartsWith("patrol_") || marker.id.StartsWith("enemy_"))UnityEngine.Object.DestroyImmediate(marker.gameObject);
            // Subdivide the former dispatch stock area into WC and electrical rooms.
            remove.Clear();foreach(Transform t in parent)if(t.position.x>3.5f && t.position.z>24.5f && t.position.z<35 && t.position.y<2.5f && !t.GetComponent<WorldMarker>() && t.name!="Room partition" && t.name!="Floor tile")remove.Add(t.gameObject);foreach(var g in remove)UnityEngine.Object.DestroyImmediate(g);
            WallRun(false,8,24,36,27,33);Toilet(new Vector3(11,0,28.8f));Electrical(new Vector3(9,0,35));RoomSign("WC / RESTROOM",9,24);RoomSign("ELECTRICAL / STAFF ONLY",9,30);
            Item("frontdoor","CỬA TRƯỚC / ĐÃ KHÓA",new Vector3(0,1,.6f));
            // Door 06 retains its story identity, but leads to the first basement stair.
            foreach(var p in parent.GetComponentsInChildren<Interaction>())if(p.id=="door06")p.title="DOOR 06 / CẦU THANG NHÂN VIÊN";
            RoomSign("06 / STAFF STAIRS",0,35.4f);
        }
        static void UtilityFloor(){
            WallRun(true,9,-13,13,-8,6);WallRun(false,-3,9,25,13,22);WallRun(false,3,9,25,13,22);WallRun(true,18,-13,-3,-8);WallRun(true,18,3,13,8);WallRun(true,25,-13,13,0,8);
            RoomSign("B1 / MAINTENANCE",-8,9);RoomSign("WC / LOCKERS",8,9);RoomSign("GENERATOR",-8,18);RoomSign("RADIO / WORKSHOP",8,18);
            for(int i=0;i<4;i++){Carton(new Vector3(-11.5f,0,10+i*1.5f),new Vector3(.9f,1.1f,1));Box("Overhead service pipe",new Vector3(-5+i*.22f,3.25f,16),new Vector3(.12f,.12f,28),"Steel",false);}
            Electrical(new Vector3(-11,0,16.5f));Electrical(new Vector3(-11,0,23.5f));Toilet(new Vector3(9,0,16.7f));Workbench(new Vector3(9,0,22.8f),true);Item("radio","RADIO / TẠO TIẾNG ĐỘNG",new Vector3(9,.9f,21.8f));
            for(int i=0;i<4;i++){Box("Generator engine",new Vector3(-10+i*2.4f,.65f,6),new Vector3(1.4f,1.3f,2),"Steel");Box("Generator heat grille",new Vector3(-10+i*2.4f,1.4f,6),new Vector3(1.25f,.15f,1.8f),"Black",false);}
            for(int i=0;i<3;i++){RetailShelf(new Vector3(6+i*2.6f,0,4),true);Carton(new Vector3(-7+i*3,0,28),new Vector3(1.4f,1.1f,1.2f));}
            Box("Maintenance cover",new Vector3(0,.55f,16),new Vector3(1.2f,1.1f,1.2f),"Steel");
            StairExit(new Vector3(0,0,31.6f),"door06","B2 / CARTER SCANNER");
            Mark("enemy_a",new Vector3(-8,.1f,22));Mark("enemy_b",new Vector3(8,.1f,27));
            Route("enemy_a",new Vector3(-8,0,22),new Vector3(0,0,22),new Vector3(0,0,13),new Vector3(-8,0,13));
            Route("enemy_b",new Vector3(8,0,27),new Vector3(8,0,21),new Vector3(4.5f,0,21),new Vector3(0,0,22),new Vector3(0,0,28));
        }
        static void ArchiveFloor(){
            WallRun(false,-4,0,30,6,23);WallRun(false,4,0,30,6,23);WallRun(true,13,-15,-4,-9);WallRun(true,16,4,15,10);WallRun(true,19,-4,4,0);
            RoomSign("ARMORY / PERSONAL DEFENCE",-9,13);RoomSign("ORDER 071 / EVIDENCE",10,16);RoomSign("FOR_ALEX / PRIVATE ARCHIVE",0,19);
            Workbench(new Vector3(-10,0,10));var gun=UnityEngine.Object.Instantiate(AssetDatabaseWeapon(),parent);gun.transform.position=new Vector3(-10,.92f,10);Item("pistol","PISTOL / TỰ VỆ",new Vector3(-10,.9f,9));
            for(int i=0;i<4;i++){Model("metal_tool_chest",new Vector3(-13,0,16+i*3),1.5f);RetailShelf(new Vector3(7+i%2*4,0,4+i/2*5),true);}
            Model("metal_tool_chest",new Vector3(11,0,24),2);Item("order","LOCKER 071 / BẰNG CHỨNG",new Vector3(11,.9f,22.8f));RoomSign("071",11,24);
            Workbench(new Vector3(0,0,26));Item("recording","FOR_ALEX / ĐỌC BẢN GHI",new Vector3(0,1,24.9f));
            for(int i=0;i<5;i++)RetailShelf(new Vector3(-10,0,16+i*2.5f),true);
            for(int i=0;i<4;i++){Model("metal_tool_chest",new Vector3(13,0,17+i*3),1.1f);Carton(new Vector3(6,0,18+i*2.5f),new Vector3(1.1f,1.3f,1.3f));}
            StairExit(new Vector3(10,0,29.6f),"exit","B3 / SECURITY");
        }
        static GameObject AssetDatabaseWeapon(){return UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Actors/Pistol.prefab");}
        static void SecurityFloor(){
            // Central observation room, two outer circulation routes, south interview rooms.
            WallRun(false,-5,8,24,12,21);WallRun(false,5,8,24,12,21);WallRun(true,8,-5,5,0);WallRun(true,24,-17,17,-10,0,10);WallRun(true,16,-17,-5,-10);WallRun(true,16,5,17,11);
            RoomSign("SURVEILLANCE / KEEPERS",0,8);RoomSign("INTERVIEW 01",-10,16);RoomSign("ACCESS CONTROL",11,16);
            for(int i=0;i<3;i++){Workbench(new Vector3(-2+i*2,0,18));Monitor(new Vector3(-3+i*3,2.4f,23.6f),"CCTV / LIVE");}
            for(int i=0;i<3;i++){Workbench(new Vector3(-13,0,4+i*4));Model("metal_tool_chest",new Vector3(14,0,6+i*5),1.8f);}
            for(int i=0;i<3;i++){Box("Interview screen",new Vector3(-8,1.1f,5+i*3),new Vector3(.18f,2.2f,1.7f),"Steel");Carton(new Vector3(8,0,5+i*3),new Vector3(1.4f,1.2f,1.5f));}
            Electrical(new Vector3(9,0,22.6f));Carton(new Vector3(-8,0,20),new Vector3(1.4f,1.1f,1.3f));Carton(new Vector3(8,0,10),new Vector3(1.4f,1.1f,1.3f));
            SecurityDoor(0,24);StairExit(new Vector3(0,0,29.6f),"exit","B4 / MEDICAL & STORAGE");
            Mark("security",new Vector3(0,1,12));Mark("enemy_a",new Vector3(-10,.1f,21));Mark("enemy_b",new Vector3(11,.1f,21));
            Route("enemy_a",new Vector3(-10,0,21),new Vector3(-10,0,11),new Vector3(0,0,12),new Vector3(0,0,21));Route("enemy_b",new Vector3(11,0,21),new Vector3(11,0,11),new Vector3(6.5f,0,12),new Vector3(11,0,27));
        }
        static void MedicalFloor(){
            // Long ward to the west; cramped staggered storage aisles to the east.
            WallRun(false,-3,6,35,10,29);WallRun(true,16,-15,-3,-9);WallRun(true,26,-15,-3,-9);WallRun(true,35,-15,15,0,10);
            RoomSign("TRIAGE / MEDICAL",-9,16);RoomSign("ISOLATION",-9,26);RoomSign("FREIGHT / STORAGE",8,35);
            for(int i=0;i<5;i++)Bed(new Vector3(-12,0,8+i*5));foreach(float z in new[]{8f,14f,20f,29f})Bed(new Vector3(-5.5f,0,z));Toilet(new Vector3(-6,0,33));Workbench(new Vector3(-7,0,23.8f));Supply(new Vector3(-5,0,14),"supply_medical");
            foreach(float z in new[]{8f,9.2f,10.4f,15f,16.2f,17.4f,20f,21.2f,22.4f,27f,28.2f,29.4f}){RetailShelf(new Vector3(3,0,z),true);RetailShelf(new Vector3(7,0,z),true);RetailShelf(new Vector3(12,0,z),true);if(z==15||z==27)Carton(new Vector3(1,0,z+2),new Vector3(1.5f,1.15f,1.2f));}
            SecurityDoor(0,35);StairExit(new Vector3(0,0,39.6f),"exit","B5 / UPLINK");Mark("security",new Vector3(-7,1,23));
            Mark("enemy_a",new Vector3(0,.1f,18));Mark("enemy_b",new Vector3(9.5f,.1f,25));Mark("enemy_c",new Vector3(-9,.1f,30));
            Route("enemy_a",new Vector3(0,0,18),new Vector3(0,0,10),new Vector3(-9,0,10),new Vector3(-9,0,20));Route("enemy_b",new Vector3(9.5f,0,25),new Vector3(9.5f,0,13),new Vector3(5,0,12),new Vector3(5,0,24));Route("enemy_c",new Vector3(-9,0,30),new Vector3(0,0,29),new Vector3(0,0,33),new Vector3(-9,0,29));
        }
        static void ServerFloor(){
            WallRun(true,10,-14,14,-9,9);WallRun(true,29,-14,14,0,9);WallRun(false,-4,10,29,14,25);WallRun(false,4,10,29,14,25);
            RoomSign("UPS / COOLING",-9,10);RoomSign("DATA HALL",9,10);RoomSign("EXTERNAL UPLINK",0,29);
            Electrical(new Vector3(-12,0,7));Electrical(new Vector3(6,0,7));
            foreach(float x in new[]{-11f,-7f,7f,11f})for(int i=0;i<5;i++)Rack(new Vector3(x,0,12+i*3.3f));
            for(int i=0;i<4;i++){Box("Central coolant unit",new Vector3(i%2==0?-1.8f:1.8f,.65f,13+i*4),new Vector3(1.6f,1.3f,1.4f),"Steel");Box("Overhead cable tray",new Vector3(0,3.4f,13+i*4),new Vector3(6,.2f,.6f),"Black",false);}
            Workbench(new Vector3(-7,0,35.5f));Item("upload","UPLINK / GỬI ORDER 071",new Vector3(-7,1,34.3f));Supply(new Vector3(-11,0,32),"supply_uplink");SecurityDoor(0,29);StairExit(new Vector3(9,0,37.6f),"exit","B6 / CONTROL ROOM");
            Mark("enemy_a",new Vector3(-9,.1f,6));Mark("enemy_b",new Vector3(9,.1f,6));Mark("enemy_c",new Vector3(0,.1f,25));
            Route("enemy_a",new Vector3(-9,0,6),new Vector3(-9,0,14),new Vector3(0,0,14));Route("enemy_b",new Vector3(9,0,6),new Vector3(9,0,25),new Vector3(0,0,25));Route("enemy_c",new Vector3(0,0,25),new Vector3(0,0,28),new Vector3(9,0,31));
        }
        static void CommandFloor(){
            // Broad command chamber with asymmetric cover, side service rooms and a raised visual centre.
            WallRun(true,8,-18,18,-11,0,11);WallRun(false,-12,8,26,13,23);WallRun(false,12,8,26,13,23);
            RoomSign("VICTOR HALE / COMMAND",0,8);RoomSign("LOCAL OVERRIDE",-15,8);RoomSign("KEEPER ARCHIVE",15,8);
            for(int i=0;i<7;i++)Monitor(new Vector3(-9+i*3,2.5f,31.6f),"THE MARKET\nKEEPER NETWORK");
            foreach(float x in new[]{-7f,7f})for(int i=0;i<3;i++){Workbench(new Vector3(x,0,12+i*6));Carton(new Vector3(x+(x<0?2:-2),0,14+i*6),new Vector3(1.3f,1.1f,.9f));}
            for(int i=0;i<3;i++){Rack(new Vector3(15,0,12+i*5));Model("metal_tool_chest",new Vector3(-15,0,18+i*4),1.5f);}
            Workbench(new Vector3(-15,0,11));Item("override","LOCAL OVERRIDE / KHÔI PHỤC QUYỀN",new Vector3(-15,1,10));
            Workbench(new Vector3(0,0,29));Item("final","SUCCESSOR / QUYẾT ĐỊNH",new Vector3(0,1,27.8f));
            foreach(float z in new[]{14f,23f})foreach(float x in new[]{-10f,10f}){Box("Command support pillar",new Vector3(x,2,z),new Vector3(.9f,4,.9f),"Steel");Box("Pillar status band",new Vector3(x,1.3f,z),new Vector3(.94f,.15f,.94f),"Red",false);}
            Mark("boss",new Vector3(0,.1f,23));SecurityDoor(0,8);Supply(new Vector3(15,0,28),"supply_boss");
            Route("boss",new Vector3(0,0,23),new Vector3(-3,0,20),new Vector3(0,0,12),new Vector3(3,0,20));
        }
    }
}

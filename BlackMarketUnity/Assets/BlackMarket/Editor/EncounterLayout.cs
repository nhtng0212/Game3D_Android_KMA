using UnityEngine;
namespace BlackMarket.Editor {
    public static partial class NorthPointBuilder {
        static void ExtraPatrols(int stage){
            if(stage==1){Patrol("enemy_c",new Vector3(-8,0,12),new Vector3(-8,0,21),new Vector3(0,0,22));Patrol("enemy_d",new Vector3(7,0,12),new Vector3(8,0,21),new Vector3(8,0,28));}
            if(stage==3){Patrol("enemy_c",new Vector3(-10,0,10),new Vector3(-10,0,21),new Vector3(-10,0,27));Patrol("enemy_d",new Vector3(11,0,10),new Vector3(11,0,21),new Vector3(0,0,21));}
            if(stage==4){Patrol("enemy_d",new Vector3(-9,0,13),new Vector3(-9,0,20),new Vector3(-9,0,30));Patrol("enemy_e",new Vector3(5,0,18),new Vector3(5,0,12),new Vector3(5,0,25));Patrol("enemy_f",new Vector3(9.5f,0,32),new Vector3(9.5f,0,24),new Vector3(9.5f,0,12));}
            if(stage==5){Patrol("enemy_d",new Vector3(-9,0,22),new Vector3(-9,0,14),new Vector3(0,0,14));Patrol("enemy_e",new Vector3(9,0,22),new Vector3(9,0,25),new Vector3(9,0,31));Patrol("enemy_f",new Vector3(0,0,18),new Vector3(0,0,14),new Vector3(0,0,25));}
        }
        static void Patrol(string id,params Vector3[] points){Mark(id,points[0]+Vector3.up*.1f);Route(id,points);}
        static void Chair(Vector3 p,float yaw=0){var save=parent;var root=new GameObject("Office chair");root.transform.SetParent(parent);root.transform.position=p;parent=root.transform;Box("Padded seat",new Vector3(0,.47f,0),new Vector3(.52f,.11f,.52f),"Black");Box("Chair back",new Vector3(0,.85f,.22f),new Vector3(.52f,.65f,.09f),"Black");for(int i=0;i<4;i++)Box("Chair leg",new Vector3(i%2==0?-.21f:.21f,.23f,i<2?-.21f:.21f),new Vector3(.04f,.46f,.04f),"Steel");parent=save;root.transform.rotation=Quaternion.Euler(0,yaw,0);}
        static void MeetingTable(Vector3 p){Box("Meeting tabletop",p+Vector3.up*.78f,new Vector3(2.7f,.12f,1.2f),"Walnut");foreach(float x in new[]{-1f,1f})Box("Table pedestal",p+new Vector3(x,.36f,0),new Vector3(.18f,.72f,.65f),"Steel");Chair(p+new Vector3(-.7f,0,-1));Chair(p+new Vector3(.7f,0,-1));Chair(p+new Vector3(-.7f,0,1),180);Chair(p+new Vector3(.7f,0,1),180);}
        static void LockerBank(Vector3 p,int count=3){for(int i=0;i<count;i++){Box("Staff locker",p+new Vector3(i*.62f,1,0),new Vector3(.58f,2,.55f),"Steel");Box("Locker label",p+new Vector3(i*.62f,1.6f,-.29f),new Vector3(.2f,.08f,.015f),"Paper",false);Box("Locker handle",p+new Vector3(i*.62f+.18f,1,-.31f),new Vector3(.035f,.2f,.025f),"Black",false);}}
        static void DenseDressing(int stage){
            if(stage==0){MeetingTable(new Vector3(-9,0,20));LockerBank(new Vector3(4.5f,0,28));Chair(new Vector3(-8.6f,0,15),90);}
            if(stage==1){LockerBank(new Vector3(5,0,16.9f),4);MeetingTable(new Vector3(-8,0,3));Carton(new Vector3(1.6f,0,20),new Vector3(.8f,1.2f,2));}
            if(stage==2){LockerBank(new Vector3(-13.5f,0,3),4);MeetingTable(new Vector3(9,0,13));Chair(new Vector3(2,0,25),90);}
            if(stage==3){MeetingTable(new Vector3(-13,0,26));LockerBank(new Vector3(12,0,28.8f),5);Toilet(new Vector3(-15,0,14.4f));}
            if(stage==4){LockerBank(new Vector3(-13,0,37),5);MeetingTable(new Vector3(9,0,4));Carton(new Vector3(-5,0,19),new Vector3(1,1.3f,1.2f));}
            if(stage==5){MeetingTable(new Vector3(7,0,33));LockerBank(new Vector3(-3,0,7.8f),3);Toilet(new Vector3(-12,0,35.5f));}
        }
        static void Room(string id,string label,Vector3 p){Mark("room_"+id,p);RoomSign(label,p.x,p.z);}
        static void VictorRooms(){
            // Eight enclosed functional rooms, joined by a central vestibule and three cross routes.
            WallRun(true,8,-18,18,-15,-8,0,8,15);
            WallRun(false,-4,0,8,4);WallRun(false,4,0,8,4);
            WallRun(false,-12,8,32,13,23,29);WallRun(false,12,8,32,13,23,29);
            WallRun(true,18,-18,-12,-15);WallRun(true,18,12,18,15);WallRun(true,26,-12,12,-8,0,8);
            Room("armory","QUÂN NHU / AK",new Vector3(-9,0,7.8f));Room("optics","QUANG HỌC / KÍNH ĐÊM",new Vector3(9,0,7.8f));
            Room("command","VICTOR / COMMAND",new Vector3(0,0,25.8f));Room("override","LOCAL OVERRIDE",new Vector3(-15,0,17.8f));Room("restroom","WC / STAFF",new Vector3(-15,0,31.7f));Room("archives","KEEPER ARCHIVE",new Vector3(15,0,17.8f));Room("electrical","ELECTRICAL",new Vector3(15,0,31.7f));Room("successor","SUCCESSOR TERMINAL",new Vector3(0,0,31.7f));
            Workbench(new Vector3(-9,0,6));EquipmentPickup("ak","AK","NHẶT AK / 30 + 90 VIÊN",new Vector3(-9,.96f,5.6f));LockerBank(new Vector3(-17,0,6.8f),5);MeetingTable(new Vector3(-13,0,3));
            Workbench(new Vector3(9,0,6));EquipmentPickup("nightvision","NightVisionGoggles","NHẶT KÍNH NHÌN ĐÊM",new Vector3(9,1,5.6f));MeetingTable(new Vector3(13,0,3));LockerBank(new Vector3(5,0,6.8f),4);
            Workbench(new Vector3(-15,0,11));Item("override","LOCAL OVERRIDE / KHÔI PHỤC QUYỀN",new Vector3(-15,1,10));Toilet(new Vector3(-16,0,30.5f));LockerBank(new Vector3(-17,0,20),3);
            for(int i=0;i<3;i++)Rack(new Vector3(16,0,10+i*2.5f));Electrical(new Vector3(13,0,30.7f));Model("metal_tool_chest",new Vector3(16,0,21),1.6f);
            // Fourteen primary cover islands vs the previous seven desks, plus crates/columns.
            foreach(float x in new[]{-8f,8f})foreach(float z in new[]{10.5f,15.5f,21f}){Workbench(new Vector3(x,0,z));Carton(new Vector3(x+(x<0?2:-2),0,z+1.5f),new Vector3(1.3f,1.2f,1.1f));}
            foreach(float x in new[]{-4.8f,4.8f})foreach(float z in new[]{12f,18f,23f}){Box("Command barricade",new Vector3(x,.6f,z),new Vector3(1.5f,1.2f,1.4f),"Steel");Chair(new Vector3(x+(x<0?1:-1),0,z+.8f),x<0?90:-90);}
            foreach(float x in new[]{-10.5f,10.5f})foreach(float z in new[]{11.5f,16f,22f,24.8f}){Box("Command support pillar",new Vector3(x,2,z),new Vector3(.75f,4,.75f),"Steel");Box("Pillar status band",new Vector3(x,1.3f,z),new Vector3(.78f,.14f,.78f),"Red",false);}
            Workbench(new Vector3(0,0,29.7f));Item("final","SUCCESSOR / QUYẾT ĐỊNH",new Vector3(0,1,28.5f));for(int i=0;i<7;i++)Monitor(new Vector3(-9+i*3,2.5f,31.6f),"THE MARKET\nKEEPER NETWORK");
            Mark("boss",new Vector3(0,.1f,22));Route("boss",new Vector3(0,0,22),new Vector3(-2.5f,0,20),new Vector3(0,0,12),new Vector3(2.5f,0,20));Patrol("enemy_a",new Vector3(15,0,14),new Vector3(10,0,13),new Vector3(2,0,14),new Vector3(2,0,23));
            SecurityDoor(0,8);Supply(new Vector3(-15,0,25),"supply_boss");
        }
    }
}

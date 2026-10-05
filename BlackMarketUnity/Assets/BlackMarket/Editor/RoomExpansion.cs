using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
namespace BlackMarket.Editor {
    public static partial class NorthPointBuilder {
        static void PocketDoor(string id,string label,float x,float z,float yaw=0){
            var save=parent;var root=new GameObject("Room door / "+label);root.transform.SetParent(parent);root.transform.localPosition=new Vector3(x,0,z);root.transform.localRotation=Quaternion.Euler(0,yaw,0);parent=root.transform;
            var leaf=Box("Sliding door leaf",new Vector3(0,1.25f,0),new Vector3(2.12f,2.5f,.12f),"Steel");leaf.isStatic=false;
            var modifier=leaf.AddComponent<NavMeshModifier>();modifier.ignoreFromBuild=true;
            var obstacle=leaf.AddComponent<NavMeshObstacle>();obstacle.shape=NavMeshObstacleShape.Box;obstacle.size=Vector3.one;obstacle.carving=true;obstacle.enabled=false;
            var door=root.AddComponent<RoomDoor>();door.leaf=leaf.transform;door.roomName=label;
            var item=root.AddComponent<Interaction>();item.id="roomdoor_"+id;item.title="MỞ CỬA / "+label;
            Box("Door rail",new Vector3(1,2.57f,0),new Vector3(4.4f,.09f,.2f),"Steel",false);
            var status=Box("Door status lamp",new Vector3(0,2.7f,-.12f),new Vector3(.28f,.08f,.04f),"WhiteLight",false);status.isStatic=false;
            Light("Emergency door marker",root.transform.TransformPoint(new Vector3(0,2.75f,-.25f)),new Color(.5f,.72f,.6f),.5f,2.5f);
            parent=save;
        }
        static void RoomExpansion(int stage){
            if(stage==0){
                // Four sales departments instead of an open retail hall; retain a central service aisle.
                WallRun(false,-3,0,12,4);WallRun(false,3,0,12,4);
                WallRun(true,7.5f,-14,-3,-9);WallRun(true,7.5f,3,14,9);
                RoomSign("AUDIO / SHOWROOM",-9,7.5f);RoomSign("COMPUTING / TEST",9,7.5f);
                PocketDoor("audio","PHÒNG AUDIO",-9,7.5f);PocketDoor("computing","PHÒNG MÁY TÍNH",9,7.5f);
                WallRun(false,-7,12,24,15,21);WallRun(false,8,12,24,15,21);WallRun(false,-3,24,36,27,33);
                RoomSign("FILES / PARTS",-5,18);PocketDoor("repair","PHÒNG SỬA CHỮA",8,15,90);
                Workbench(new Vector3(-5,0,14));LockerBank(new Vector3(4.2f,0,18.3f),4);
                MeetingTable(new Vector3(-9,0,26.5f));Workbench(new Vector3(-9,0,34.8f));
                LockerBank(new Vector3(9,0,32),4);Model("metal_tool_chest",new Vector3(11,0,25.8f),1.6f);
                PocketDoor("marcus","VĂN PHÒNG MARCUS",-9,12);PocketDoor("wc","WC",9,24);PocketDoor("power","PHÒNG ĐIỆN",9,30);
                foreach(float z in new[]{2f,10.5f}){RetailShelf(new Vector3(-8.4f,0,z),true);RetailShelf(new Vector3(8.4f,0,z),true);}
                LockerBank(new Vector3(-13,0,11),4);LockerBank(new Vector3(10,0,11),4);
                foreach(float z in new[]{25.7f,28.3f,32f,34.5f})Carton(new Vector3(-3.9f,0,z),new Vector3(1.1f,1.4f,1));
                Workbench(new Vector3(-5.5f,0,32.7f));LockerBank(new Vector3(4.5f,0,34.7f),4);
                foreach(float z in new[]{13.5f,19.5f,23f})Carton(new Vector3(1.9f,0,z),new Vector3(.9f,1.25f,1));
            }
            if(stage==1){
                // Service passage behind the lockers; an alternate doorway avoids the watched workshop entrance.
                foreach(Transform t in parent.GetComponentsInChildren<Transform>())if(t.parent==parent && t.name=="Room partition" && (((Mathf.Abs(t.position.z-18)<.01f || Mathf.Abs(t.position.z-25)<.01f) && t.position.x>9) || (Mathf.Abs(t.position.z-9)<.01f && t.position.x>7.1f)))UnityEngine.Object.DestroyImmediate(t.gameObject);
                WallPiece(true,9,7.1f,10.4f);WallPiece(true,9,12.6f,13);
                WallPiece(true,25,9.1f,10.4f);WallPiece(true,25,12.6f,13);
                WallPiece(false,10.5f,18,19.7f);WallPiece(false,10.5f,22.2f,29);
                WallPiece(true,18,9.1f,10.4f);WallPiece(true,18,12.6f,13);
                WallRun(false,10.5f,9,17,10.5f);RoomSign("SERVICE / RADIO",11.5f,18);
                WallRun(true,5,-13,-3,-11.7f);RoomSign("STAFF BREAK ROOM",-11.7f,5);PocketDoor("staff","PHÒNG NGHỈ",-11.7f,5);
                LockerBank(new Vector3(4.5f,0,7.7f),5);Carton(new Vector3(-4.6f,0,11),new Vector3(1,1.5f,2));
                Carton(new Vector3(10.1f,0,25.8f),new Vector3(1.1f,1.6f,1.8f));LockerBank(new Vector3(-12,0,30),6);
                Workbench(new Vector3(-8,0,11));Carton(new Vector3(-11,0,20),new Vector3(1.4f,1.6f,1.4f));
                Workbench(new Vector3(5.5f,0,23.6f));LockerBank(new Vector3(4.2f,0,19.2f),4);
                Carton(new Vector3(-5,0,27),new Vector3(1.4f,1.2f,1.5f));
                Carton(new Vector3(8,0,29.2f),new Vector3(2.4f,1.7f,.7f));
                Mark("stealth_wait",new Vector3(11.7f,0,17));Mark("stealth_goal",new Vector3(10.7f,0,21.5f));
                Sign("CHỜ ĐÈN PIN QUAY ĐI → RADIO",new Vector3(11.7f,2.3f,17.8f),.034f);
            }
            if(stage==2){
                WallRun(true,7,-15,-4,-8);RoomSign("ARMORY / LOCKERS",-8,7);PocketDoor("armory","TỦ QUÂN NHU",-8,7);
                WallRun(true,9,4,15,10);RoomSign("RETURNS / INVENTORY",10,9);PocketDoor("returns","KHO HÀNG TRẢ",10,9);
                foreach(float z in new[]{3f,7f,11f,15f})Carton(new Vector3(2.4f,0,z),new Vector3(1,1.4f,1.2f));
                LockerBank(new Vector3(-2.8f,0,28),4);Workbench(new Vector3(-6,0,27));
            }
            if(stage==3){
                WallRun(true,7,-17,-5,-10);PocketDoor("interview","PHÒNG THẨM VẤN",-10,16);
                WallRun(true,7,5,17,11);RoomSign("EQUIPMENT CHECK",11,7);
                LockerBank(new Vector3(6,0,28.6f),5);LockerBank(new Vector3(-4,0,28.5f),4);
                foreach(float z in new[]{10f,14f,22f})Carton(new Vector3(-15.5f,0,z),new Vector3(1.1f,1.4f,1.3f));
            }
            if(stage==4){
                WallRun(true,21,-15,-3,-9);RoomSign("PHARMACY / WARD",-9,21);PocketDoor("isolation","PHÒNG CÁCH LY",-9,26);
                LockerBank(new Vector3(-13.5f,0,24.7f),4);Workbench(new Vector3(5,0,37.5f));
                Carton(new Vector3(-5,0,7),new Vector3(1.2f,1.3f,1.1f));
            }
            if(stage==5){
                WallRun(true,19,-14,-4,-9);RoomSign("BACKUP / STORAGE",-9,19);PocketDoor("backup","PHÒNG BACKUP",-9,19);
                WallRun(true,19,4,14,9);RoomSign("NETWORK / ROUTING",9,19);
                WallRun(false,-10,30,38,32);RoomSign("WC / STAFF",-12,30);
                foreach(float z in new[]{31f,34f,36.7f})Carton(new Vector3(-2,0,z),new Vector3(1.3f,1.5f,1.2f));
            }
            if(stage==6){PocketDoor("optics","PHÒNG KÍNH ĐÊM",4,4,90);PocketDoor("supply","PHÒNG AK",-4,4,90);PocketDoor("archive","HỒ SƠ KEEPER",15,18);}
        }
        static void PatrolWindows(int stage){
            if(stage==1){
                var remove=new System.Collections.Generic.List<GameObject>();foreach(var m in parent.GetComponentsInChildren<WorldMarker>())if(m.id.StartsWith("patrol_") || m.id.StartsWith("enemy_"))remove.Add(m.gameObject);foreach(var g in remove)UnityEngine.Object.DestroyImmediate(g);
                Patrol("enemy_a",new Vector3(-8,0,22),new Vector3(-8,0,13));
                Patrol("enemy_c",new Vector3(-5,0,23),new Vector3(-5,0,20));
                Patrol("enemy_d",new Vector3(5,0,12),new Vector3(5,0,15));
                Patrol("enemy_b",new Vector3(8,0,21),new Vector3(8,0,28));
            }
            if(stage==1)foreach(var m in parent.GetComponentsInChildren<WorldMarker>())if(m.id=="patrol_enemy_d_0"){m.patrolWait=3;m.transform.rotation=Quaternion.Euler(0,270,0);}
            foreach(var m in parent.GetComponentsInChildren<WorldMarker>())if(m.id.StartsWith("patrol_") && m.id.EndsWith("_1")){m.patrolWait=stage==1?9:5;m.transform.rotation=Quaternion.Euler(0,stage==1 && m.id.Contains("enemy_d")?270:0,0);}
        }
    }
}

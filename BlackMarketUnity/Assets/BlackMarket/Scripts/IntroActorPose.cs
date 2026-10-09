using UnityEngine;
using System.Collections.Generic;
namespace BlackMarket {
    // Cinematic-only IK. Restores the sampled skeleton before Animation evaluates.
    public class IntroActorPose:MonoBehaviour {
        public enum Pose { Desk, Phone, Ride }
        public Pose pose;
        public float weight=1,phoneLift=1;
        public Transform phone,leftGrip,rightGrip;
        Transform pelvis,head;Transform[] bones;Vector3[] positions;Quaternion[] rotations;bool applied;
        readonly Dictionary<string,Transform> lookup=new Dictionary<string,Transform>();
        Transform Bone(string name)=>lookup.TryGetValue("Bip01 "+name,out var bone)?bone:null;
        void Awake(){bones=GetComponentsInChildren<Transform>();foreach(var bone in bones)lookup[bone.name]=bone;pelvis=Bone("Pelvis");head=Bone("Head");positions=new Vector3[bones.Length];rotations=new Quaternion[bones.Length];}
        void Restore(){if(!applied)return;for(int i=0;i<bones.Length;i++){bones[i].localPosition=positions[i];bones[i].localRotation=rotations[i];}applied=false;}
        void Update(){Restore();}
        void LateUpdate(){
            if(!pelvis || weight<=0)return;for(int i=0;i<bones.Length;i++){positions[i]=bones[i].localPosition;rotations[i]=bones[i].localRotation;}applied=true;
            bool bike=pose==Pose.Ride;float height=bike?.81f:.57f;pelvis.position=Vector3.Lerp(pelvis.position,transform.TransformPoint(new Vector3(0,height,0)),weight);
            var leftFoot=Bone("L Foot");var rightFoot=Bone("R Foot");var leftFootRotation=leftFoot.rotation;var rightFootRotation=rightFoot.rotation;
            Limb("L Thigh","L Calf","L Foot",new Vector3(-.20f,bike?.26f:.08f,bike?.15f:.40f),transform.forward);
            Limb("R Thigh","R Calf","R Foot",new Vector3(.20f,bike?.26f:.08f,bike?.15f:.40f),transform.forward);
            leftFoot.rotation=leftFootRotation;rightFoot.rotation=rightFootRotation;
            Limb("R UpperArm","R Forearm","R Hand",new Vector3(bike?.32f:.30f,bike?1.16f:.85f,bike?.51f:.57f),-transform.up+transform.right);
            Vector3 left=new Vector3(bike?-.32f:-.20f,bike?1.16f:.85f,bike?.51f:.57f);
            if(pose==Pose.Phone && head)left=Vector3.Lerp(left,transform.InverseTransformPoint(head.position)+new Vector3(-.13f,.09f,.025f),phoneLift);
            Limb("L UpperArm","L Forearm","L Hand",left,pose==Pose.Phone?-transform.up+transform.forward*1.5f-transform.right*.2f:-transform.up-transform.right);
            if(bike && leftGrip && rightGrip){Limb("L UpperArm","L Forearm","L Hand",transform.InverseTransformPoint(leftGrip.position),-transform.up-transform.right);Limb("R UpperArm","R Forearm","R Hand",transform.InverseTransformPoint(rightGrip.position),-transform.up+transform.right);}
            if(phone && pose==Pose.Phone){phone.position=Bone("L Hand").position+transform.up*.045f;phone.rotation=transform.rotation*Quaternion.Euler(0,90,0);}

        }
        void Limb(string a,string b,string c,Vector3 local,Vector3 bend){var upper=Bone(a);var mid=Bone(b);var end=Bone(c);if(!upper||!mid||!end)return;Solve(upper,mid,end,Vector3.Lerp(end.position,transform.TransformPoint(local),weight),bend);}
        public static void Solve(Transform upper,Transform mid,Transform end,Vector3 target,Vector3 pole){float a=Vector3.Distance(upper.position,mid.position),b=Vector3.Distance(mid.position,end.position);Vector3 delta=target-upper.position;float d=Mathf.Clamp(delta.magnitude,Mathf.Abs(a-b)+.001f,a+b-.001f);if(a<.001f||b<.001f||delta.sqrMagnitude<.00001f)return;Vector3 axis=delta.normalized,bend=Vector3.ProjectOnPlane(pole,axis).normalized;float cos=Mathf.Clamp((a*a+d*d-b*b)/(2*a*d),-1,1);Vector3 joint=upper.position+axis*a*cos+bend*a*Mathf.Sqrt(1-cos*cos);upper.rotation=Quaternion.FromToRotation(mid.position-upper.position,joint-upper.position)*upper.rotation;mid.rotation=Quaternion.FromToRotation(end.position-mid.position,target-mid.position)*mid.rotation;}
        void OnDisable(){Restore();}
    }
}

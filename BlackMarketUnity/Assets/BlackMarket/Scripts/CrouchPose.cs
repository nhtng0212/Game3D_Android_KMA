using UnityEngine;
namespace BlackMarket {
    // Restore before legacy Animation evaluates, then apply a forward-leaning sneak pose.
    // Shortened animated strides and two-bone IK retain foot lift instead of a seated walk.
    [DefaultExecutionOrder(-20)]
    public class CrouchPose:MonoBehaviour {
        public PlayerMotor player;
        Transform[] bones;Vector3[] positions;Quaternion[] rotations;bool applied;float amount;
        public float Amount=>amount;
        Transform Bone(string name){foreach(var t in GetComponentsInChildren<Transform>())if(t.name==name)return t;return null;}
        void Start(){bones=new[]{Bone("Bip01 Pelvis"),Bone("Bip01 L Thigh"),Bone("Bip01 L Calf"),Bone("Bip01 L Foot"),Bone("Bip01 R Thigh"),Bone("Bip01 R Calf"),Bone("Bip01 R Foot"),Bone("Bip01 Spine"),Bone("Bip01 Spine1"),Bone("Bip01 Spine2"),Bone("Bip01 Head"),Bone("Bip01 L UpperArm"),Bone("Bip01 L Forearm"),Bone("Bip01 L Hand"),Bone("Bip01 R UpperArm"),Bone("Bip01 R Forearm"),Bone("Bip01 R Hand")};foreach(var t in bones)if(!t)Debug.LogError("Missing crouch bone");positions=new Vector3[bones.Length];rotations=new Quaternion[bones.Length];}
        void Restore(){if(!applied)return;for(int i=0;i<bones.Length;i++)if(bones[i]){bones[i].localPosition=positions[i];bones[i].localRotation=rotations[i];}applied=false;}
        void Update(){Restore();}
        void LateUpdate(){
            if(!player || bones==null || System.Array.Exists(bones,x=>!x))return;
            amount=Mathf.MoveTowards(amount,player.crouch?1:0,Time.deltaTime*5);if(amount<=0)return;
            for(int i=0;i<bones.Length;i++){positions[i]=bones[i].localPosition;rotations[i]=bones[i].localRotation;}applied=true;
            Vector3 left=FootTarget(bones[3]),right=FootTarget(bones[6]);Quaternion lrot=bones[3].rotation,rrot=bones[6].rotation;
            // Moderate knee bend, hinge at the hip and distribute curvature through the back.
            bones[0].position+=(-Vector3.up*.20f+transform.forward*.035f)*amount;
            bones[7].rotation=Quaternion.AngleAxis(amount*30,transform.right)*bones[7].rotation;
            bones[8].rotation=Quaternion.AngleAxis(amount*12,transform.right)*bones[8].rotation;
            bones[9].rotation=Quaternion.AngleAxis(amount*6,transform.right)*bones[9].rotation;
            bones[10].rotation=Quaternion.AngleAxis(amount*-20,transform.right)*bones[10].rotation;
            RelaxArm(11,-1);if(!player.campaign.state.armed)RelaxArm(14,1);
            Solve(bones[1],bones[2],bones[3],left);Solve(bones[4],bones[5],bones[6],right);bones[3].rotation=lrot;bones[6].rotation=rrot;
        }
        void RelaxArm(int index,float side){
            var upper=bones[index];var elbow=bones[index+1];var hand=bones[index+2];
            var upperDirection=transform.forward*.25f-Vector3.up*.8f+transform.right*(side*.12f);
            upper.rotation=Quaternion.Slerp(upper.rotation,Quaternion.FromToRotation(elbow.position-upper.position,upperDirection)*upper.rotation,amount*.85f);
            var foreDirection=transform.forward*.65f-Vector3.up*.2f;
            elbow.rotation=Quaternion.Slerp(elbow.rotation,Quaternion.FromToRotation(hand.position-elbow.position,foreDirection)*elbow.rotation,amount*.85f);
        }
        Vector3 FootTarget(Transform foot){
            var local=transform.InverseTransformPoint(foot.position);
            local.z=Mathf.Lerp(local.z,local.z*.55f,amount);local.x*=Mathf.Lerp(1,1.08f,amount);
            return transform.TransformPoint(local);
        }
        void Solve(Transform thigh,Transform calf,Transform foot,Vector3 target){
            float a=Vector3.Distance(thigh.position,calf.position),b=Vector3.Distance(calf.position,foot.position);Vector3 delta=target-thigh.position;
            if(a<.001f || b<.001f || delta.sqrMagnitude<.000001f)return;
            float d=Mathf.Clamp(delta.magnitude,Mathf.Abs(a-b)+.001f,a+b-.001f);Vector3 axis=delta.normalized;
            Vector3 bend=Vector3.ProjectOnPlane(transform.forward,axis).normalized;float cosine=Mathf.Clamp((a*a+d*d-b*b)/(2*a*d),-1,1);
            Vector3 knee=thigh.position+axis*(a*cosine)+bend*(a*Mathf.Sqrt(1-cosine*cosine));thigh.rotation=Quaternion.FromToRotation(calf.position-thigh.position,knee-thigh.position)*thigh.rotation;calf.rotation=Quaternion.FromToRotation(foot.position-calf.position,target-calf.position)*calf.rotation;
        }
        void OnDisable(){Restore();}
    }
}

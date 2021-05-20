using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MMIUnity.Retargeting;

public class JointAlignment : IJointAlignment
{
    ISVisualizationJoint rootVisualization;
    // Start is called before the first frame update

    private void AlignJoint(ISVisualizationJoint start, ISVisualizationJoint end)
    {
        JointController jc = start.reference.GetComponent<JointController>();
        if (jc != null && jc.wasChanged)
        {
            return;
        }
        Vector3 source_dir = getJointPosition(end.reference) - getJointPosition(start.reference);
        Vector3 endPos = end.GetGlobalMatrix().GetColumn(3);//end.gameJoint.transform.position;
        Vector3 target_dir = endPos - getJointPosition(start.reference);
        Quaternion q = Quaternion.FromToRotation(source_dir, target_dir);

        if (start.reference != null)
        {
            start.reference.rotation = q * start.reference.rotation;
        }


        //this.anim.GetBoneTransform (start.referenceBone).rotation = q * this.anim.GetBoneTransform(start.referenceBone).rotation;
    }

    /// <summary>
    /// This function aligns the target avatar to the intermediate skeleton posture. It assumes, that both are in T-Pose and reduces the residual error due to small misalignments. 
    /// This function may have to be adapted to incorporate / load manually pre-aligend configurations
    /// </summary>
    /// <param name="anim"></param>
    public override void AlignAvatar(ISVisualizationJoint root)
    {
        this.rootVisualization = root;
        //this.anim = anim;
        //AlignJoint(rootVisualization.GetJointByName("LeftHip"), rootVisualization.GetJointByName("LeftAnkle"));
        //AlignJoint(rootVisualization.GetJointByName("RightHip"), rootVisualization.GetJointByName("RightAnkle"));
        AlignJoint(rootVisualization.GetJointByName("LeftHip"), rootVisualization.GetJointByName("LeftKnee"));
        AlignJoint(rootVisualization.GetJointByName("LeftKnee"), rootVisualization.GetJointByName("LeftAnkle"));
        AlignJoint(rootVisualization.GetJointByName("RightHip"), rootVisualization.GetJointByName("RightKnee"));
        AlignJoint(rootVisualization.GetJointByName("RightKnee"), rootVisualization.GetJointByName("RightAnkle"));
        //TryMoveShoulder(rootVisualization.GetJointByName("LeftShoulder"));
        //TryMoveShoulder(rootVisualization.GetJointByName("RightShoulder"));
        //AlignJoint(rootVisualization.GetJointByName("LeftShoulder"), rootVisualization.GetJointByName("LeftWrist"));
        AlignJoint(rootVisualization.GetJointByName("LeftShoulder"), rootVisualization.GetJointByName("LeftElbow"));
        AlignJoint(rootVisualization.GetJointByName("LeftElbow"), rootVisualization.GetJointByName("LeftWrist"));
        //AlignJoint(rootVisualization.GetJointByName("RightShoulder"), rootVisualization.GetJointByName("RightWrist"));
        AlignJoint(rootVisualization.GetJointByName("RightShoulder"), rootVisualization.GetJointByName("RightElbow"));
        AlignJoint(rootVisualization.GetJointByName("RightElbow"), rootVisualization.GetJointByName("RightWrist"));

        AlignJoint(rootVisualization.GetJointByName("LeftWrist"), rootVisualization.GetJointByName("LeftMiddleProximal"));
        AlignJoint(rootVisualization.GetJointByName("RightWrist"), rootVisualization.GetJointByName("RightMiddleProximal"));


        AlignHand("Left");
        AlignHand("Right");
    }

    public void TryMoveShoulder(ISVisualizationJoint shoulder)
    {
        if (shoulder.reference.parent != shoulder.parent.reference)
        {
            // there is a clavicle joint
            Transform clavicle = shoulder.reference.parent;
            Vector3 current_dir = shoulder.reference.transform.position - clavicle.position;
            Vector3 target_dir = (Vector3)shoulder.GetGlobalMatrix().GetColumn(3) - clavicle.position;
            clavicle.rotation = Quaternion.FromToRotation(current_dir, target_dir) * clavicle.rotation;
        }
    }



    private void AlignHand(string hand)
    {
        // under-aligning the fingers results in better visual quality
        AlignJoint(rootVisualization.GetJointByName(hand + "ThumbMid"), rootVisualization.GetJointByName(hand + "ThumbCarpal"));
        AlignJoint(rootVisualization.GetJointByName(hand + "IndexProximal"), rootVisualization.GetJointByName(hand + "IndexDistal"));
        AlignJoint(rootVisualization.GetJointByName(hand + "MiddleProximal"), rootVisualization.GetJointByName(hand + "MiddleDistal"));
        AlignJoint(rootVisualization.GetJointByName(hand + "RingProximal"), rootVisualization.GetJointByName(hand + "RingDistal"));
        AlignJoint(rootVisualization.GetJointByName(hand + "LittleProximal"), rootVisualization.GetJointByName(hand + "LittleDistal"));

        // equal numerical poses do not necessarily mean equal geometrical / meshed poses

        /*
        SetFingerJoints(this.GetJointByName(hand + "ThumbMid"));
        SetFingerJoints(this.GetJointByName(hand + "IndexProximal"));
        SetFingerJoints(this.GetJointByName(hand + "MiddleProximal"));
        SetFingerJoints(this.GetJointByName(hand + "RingProximal"));
        SetFingerJoints(this.GetJointByName(hand + "LittleProximal"));

        AlignJoint(this.GetJointByName(hand + "ThumbMid"), this.GetJointByName(hand + "ThumbMeta"));
        AlignJoint(this.GetJointByName(hand + "ThumbMeta"), this.GetJointByName(hand + "ThumbCarpal"));

        AlignJoint(this.GetJointByName(hand + "IndexProximal"), this.GetJointByName(hand + "IndexMeta"));
        AlignJoint(this.GetJointByName(hand + "IndexMeta"), this.GetJointByName(hand + "IndexDistal"));

        AlignJoint(this.GetJointByName(hand + "MiddleProximal"), this.GetJointByName(hand + "MiddleMeta"));
        AlignJoint(this.GetJointByName(hand + "MiddleMeta"), this.GetJointByName(hand + "MiddleDistal"));

        AlignJoint(this.GetJointByName(hand + "RingProximal"), this.GetJointByName(hand + "RingMeta"));
        AlignJoint(this.GetJointByName(hand + "RingMeta"), this.GetJointByName(hand + "RingDistal"));

        AlignJoint(this.GetJointByName(hand + "LittleProximal"), this.GetJointByName(hand + "LittleMeta"));
        AlignJoint(this.GetJointByName(hand + "LittleMeta"), this.GetJointByName(hand + "LittleDistal"));
        */
    }

    private void SetFingerJoints(ISVisualizationJoint j)
    {

        //set index joint to target index joint position
        Vector3 targetPos = getJointPosition(j.reference);
        Vector3 newOffset = j.parent.GetGlobalMatrix().inverse.MultiplyPoint3x4(targetPos);

        j.reference.position = new Vector3(j.GetGlobalMatrix().GetColumn(3).x, j.GetGlobalMatrix().GetColumn(3).y, j.GetGlobalMatrix().GetColumn(3).z);
    }

    private static Vector3 getJointPosition(Transform t)
    {
        if (t == null)
        {
            return Vector3.zero;
        }
        else
        {
            return new Vector3(t.position.x, t.position.y, t.position.z);
        }
        /*
        if (bone != HumanBodyBones.LastBone && anim.GetBoneTransform(bone) != null)
        {
            Vector3 v = anim.GetBoneTransform(bone).position;
            return new Vector3(v.x, v.y, v.z);
        }
        else
        {
            return Vector3.zero;
        }*/
    }




}

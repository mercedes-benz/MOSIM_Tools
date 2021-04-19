using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Class for reading symmetric joint pairs from an Animator</summary>
public class JointPairsReaderFromAnimator : JointPairsReader
{
    public Animator animator;
    private static (HumanBodyBones, HumanBodyBones)[] SymmetricBones = new(HumanBodyBones, HumanBodyBones)[]
    {
        ( HumanBodyBones.LeftLowerLeg, HumanBodyBones.RightLowerLeg ),
        ( HumanBodyBones.LeftUpperLeg, HumanBodyBones.RightUpperLeg ),
        ( HumanBodyBones.LeftFoot, HumanBodyBones.RightFoot ),
        ( HumanBodyBones.LeftShoulder, HumanBodyBones.RightShoulder ),
        ( HumanBodyBones.LeftUpperArm, HumanBodyBones.RightUpperArm ),
        ( HumanBodyBones.LeftLowerArm, HumanBodyBones.RightLowerArm ),
        ( HumanBodyBones.LeftHand, HumanBodyBones.RightHand ),
        ( HumanBodyBones.LeftToes, HumanBodyBones.RightToes ),
        ( HumanBodyBones.LeftEye, HumanBodyBones.RightEye ),
        ( HumanBodyBones.LeftThumbProximal, HumanBodyBones.RightThumbProximal ),
        ( HumanBodyBones.LeftThumbIntermediate, HumanBodyBones.RightThumbIntermediate ),
        ( HumanBodyBones.LeftThumbDistal, HumanBodyBones.RightThumbDistal ),
        ( HumanBodyBones.LeftIndexProximal, HumanBodyBones.RightIndexProximal ),
        ( HumanBodyBones.LeftIndexIntermediate, HumanBodyBones.RightIndexIntermediate ),
        ( HumanBodyBones.LeftIndexDistal, HumanBodyBones.RightIndexDistal ),
        ( HumanBodyBones.LeftMiddleProximal, HumanBodyBones.RightMiddleProximal ),
        ( HumanBodyBones.LeftMiddleIntermediate, HumanBodyBones.RightMiddleIntermediate ),
        ( HumanBodyBones.LeftMiddleDistal, HumanBodyBones.RightMiddleDistal ),
        ( HumanBodyBones.LeftRingProximal, HumanBodyBones.RightRingProximal ),
        ( HumanBodyBones.LeftRingIntermediate, HumanBodyBones.RightRingIntermediate ),
        ( HumanBodyBones.LeftRingDistal, HumanBodyBones.RightRingDistal ),
        ( HumanBodyBones.LeftLittleProximal, HumanBodyBones.RightLittleProximal ),
        ( HumanBodyBones.LeftLittleIntermediate, HumanBodyBones.RightLittleIntermediate ),
        ( HumanBodyBones.LeftLittleDistal, HumanBodyBones.RightLittleDistal )
    };
    private void FindAnimator()
    {
        animator = FindObjectOfType<Animator>();
    }
    public override Tuple<Transform, Transform>[] ReadJointPairs()
    {
        List<Tuple<Transform, Transform>> jointPairsList = new List<Tuple<Transform, Transform>>();
        if(animator == null)
            animator = FindObjectOfType<Animator>();
        if(animator == null)
            return null;
        foreach(var pair in SymmetricBones)
        {
            var t1 = animator.GetBoneTransform(pair.Item1);
            var t2 = animator.GetBoneTransform(pair.Item2);
            if(t1 != null && t2 != null)
                jointPairsList.Add(new Tuple<Transform, Transform>(t1, t2));
        }
        return jointPairsList.ToArray();
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MMIStandard;
using UnityEngine;

public class JointMap
{
    private Dictionary<MJointType, Transform> _jointMap;

    private static List<MJointType> types = new List<MJointType>()
    {
        MJointType.Undefined,

        MJointType.PelvisCentre,
        MJointType.S1L5Joint,
        MJointType.T12L1Joint,
        MJointType.T1T2Joint,
        MJointType.C4C5Joint,
        MJointType.HeadJoint,
        MJointType.HeadTip,
        MJointType.MidEye,

        MJointType.LeftHip,
        MJointType.LeftKnee,
        MJointType.LeftAnkle,
        MJointType.LeftBall,
        MJointType.LeftBallTip,

        MJointType.RightHip,
        MJointType.RightKnee,
        MJointType.RightAnkle,
        MJointType.RightBall,
        MJointType.RightBallTip,


        MJointType.LeftShoulder,
        MJointType.LeftElbow,
        MJointType.LeftWrist,
        MJointType.RightShoulder,
        MJointType.RightElbow,
        MJointType.RightWrist,

        MJointType.LeftThumbMid,
        MJointType.LeftThumbMeta,
        MJointType.LeftThumbCarpal,
        MJointType.LeftThumbTip,
        MJointType.LeftIndexMeta,
        MJointType.LeftIndexProximal,
        MJointType.LeftIndexDistal,
        MJointType.LeftIndexTip,
        MJointType.LeftMiddleMeta,
        MJointType.LeftMiddleProximal,
        MJointType.LeftMiddleDistal,
        MJointType.LeftMiddleTip,
        MJointType.LeftRingMeta,
        MJointType.LeftRingProximal,
        MJointType.LeftRingDistal,
        MJointType.LeftRingTip,
        MJointType.LeftLittleMeta,
        MJointType.LeftLittleProximal,
        MJointType.LeftLittleDistal,
        MJointType.LeftLittleTip,
        MJointType.RightThumbMid,
        MJointType.RightThumbMeta,
        MJointType.RightThumbCarpal,
        MJointType.RightThumbTip,
        MJointType.RightIndexMeta,
        MJointType.RightIndexProximal,
        MJointType.RightIndexDistal,
        MJointType.RightIndexTip,
        MJointType.RightMiddleMeta,
        MJointType.RightMiddleProximal,
        MJointType.RightMiddleDistal,
        MJointType.RightMiddleTip,
        MJointType.RightRingMeta,
        MJointType.RightRingProximal,
        MJointType.RightRingDistal,
        MJointType.RightRingTip,
        MJointType.RightLittleMeta,
        MJointType.RightLittleProximal,
        MJointType.RightLittleDistal,
        MJointType.RightLittleTip
    };

    public bool allowDuplicated;  //whether allow different keys have same value
    public JointMap(bool allowDuplicated=false)
    {
        _jointMap = new Dictionary<MJointType, Transform>();
        foreach(MJointType jt in types)
        {
            if(jt != MJointType.Undefined)
                _jointMap.Add(jt, null);
        }
        this.allowDuplicated = allowDuplicated;
    }

    // isSwap: if true, when the transform is in the dict, swap two transform; 
    // if false, set the entry whoes value is transform to null and set the _jointMap[jointType]=transform
    // only works when allowDuplicated is false
    public void SetTransform(MJointType jointType, Transform transform, bool isSwap=false)
    {
        if(jointType == MJointType.Undefined)
        {
            return;
        }
        if(_jointMap[jointType] == transform)
            return;
        if(!allowDuplicated && transform != null)
        {
            MJointType DuplicatedEntry = MJointType.Undefined;
            foreach(var key in _jointMap.Keys)
            {
                if(_jointMap[key] == transform)
                {
                    DuplicatedEntry = key;
                    break;
                }
            }
            if(DuplicatedEntry != MJointType.Undefined)
            {
                if(isSwap)
                    _jointMap[DuplicatedEntry] = _jointMap[jointType];
                else
                    _jointMap[DuplicatedEntry] = null;
            }
        }
        _jointMap[jointType] = transform;
        //Debug.Log(jointType);
        //Debug.Log(transform);
    }

    public ReadOnlyDictionary<MJointType, Transform> GetJointMap()
    {
        return new ReadOnlyDictionary<MJointType, Transform>(_jointMap);
    }
}
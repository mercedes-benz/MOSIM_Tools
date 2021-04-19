using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Set mirroredJoint in JointController</summary>
public class MirroredJointsLoader : MonoBehaviour
{
    public bool isLoaded = false;  // By setting this to false from outside, we can reload the file at next update

    /// <summary>Check if given <c>jointPairs</c> is valid</summary>
    /// <param name"jointPairs"></param>
    public bool CheckJointControler(Tuple<Transform, Transform>[] jointPairs)
    {
        foreach(var pair in jointPairs)
        {
            if(pair.Item1.GetComponent<JointController>() == null)
            {
                Debug.LogWarning("Joint controler is missing on GameObject " + pair.Item1.gameObject);
                return false;
            }
            if(pair.Item2.GetComponent<JointController>() == null)
            {
                Debug.LogWarning("Joint controler is missing on GameObject " + pair.Item2.gameObject);
                return false;
            }
        }
        return true;
    }
    /// <summary>Assign mirroredJoint</summary>
    public bool ApplySymmetricControl(Tuple<Transform, Transform>[] jointPairs)
    {
        bool isValid = CheckJointControler(jointPairs);
        if(isValid)
        foreach(var pair in jointPairs)
        {
            var jc1 = pair.Item1.GetComponent<JointController>();
            var jc2 = pair.Item2.GetComponent<JointController>();
            jc1.mirroredJoint = jc2.transform;
            jc2.mirroredJoint = jc1.transform;
        }
        return isValid;
    }

    public void UpdateMirrors(JointPairsReader reader)
    {
        Tuple<Transform, Transform>[] jointPairs = reader.ReadJointPairs();
        if (jointPairs != null)
        {
            if (ApplySymmetricControl(jointPairs))
                isLoaded = true;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (!isLoaded)
        {
            JointPairsReader jointPairsReader = GetComponent<JointPairsReader>();
            if (jointPairsReader != null)
            {
                Tuple<Transform, Transform>[] jointPairs = jointPairsReader.ReadJointPairs();
                if (jointPairs != null)
                {
                    if(ApplySymmetricControl(jointPairs))
                        isLoaded = true;
                }
            }
        }
    }
}

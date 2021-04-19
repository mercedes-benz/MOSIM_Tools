using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>Attatch JointController on Joint</summary>
public class JointControllerManager : MonoBehaviour
{
    public Transform root;

    void AddJointController(Transform joint)
    {
        if(joint != null && joint != root)
        {
            var jointController = joint.gameObject.GetComponent<JointController>();
            if(jointController == null)
            {
                joint.gameObject.AddComponent<JointController>();
            }
        }
        if (joint != null)
        {
            JointInfo jointInfo = joint.GetComponent<JointInfo>();
            if (jointInfo != null)
                foreach (JointInfo item in jointInfo.childJoints)
                    AddJointController(item.transform);
        }
    }

    void Awake()
    {
        AddJointController(root);
    }

    void Update()
    {
        AddJointController(root);
    }

}

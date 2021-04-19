using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneController : MonoBehaviour
{
    public Transform endJoint;
    public Vector3 endPoint;
    void Start()
    {
        SetLocalTransformationFromJointInitConfig();
    }

    void LateUpdate()
    {
        transform.localPosition = Vector3.zero;
        if (endJoint != null)
        {
            transform.localRotation = Quaternion.FromToRotation(Vector3.up, endJoint.localPosition);
        }
        else if (endPoint != Vector3.zero)
        {
            transform.localRotation = Quaternion.FromToRotation(Vector3.up, endPoint);
        }
    }

     private void SetLocalTransformationFromJointInitConfig()
    {
        transform.localPosition = Vector3.zero;
        if(endJoint != null)
        {
            transform.localRotation = Quaternion.FromToRotation(Vector3.up, endJoint.localPosition);
            float boneLength = Vector3.Magnitude(endJoint.localPosition);
            transform.localScale = transform.localScale * boneLength;
        }
        else if(endPoint != Vector3.zero)
        {
            transform.localRotation = Quaternion.FromToRotation(Vector3.up, endPoint);
            transform.localScale = transform.localScale * endPoint.magnitude;
        } else
        {
            transform.rotation = transform.parent.parent.GetChild(1).rotation;//Quaternion.FromToRotation(Vector3.up, endPoint);
            transform.localScale = 0.9f * transform.parent.parent.GetChild(1).localScale; // transform.localScale * endPoint.magnitude;
        }
        
    }

    
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Control joint rotation</summary>
public class JointController : MonoBehaviour
{
    public Transform mirroredJoint;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>Rotate joint and mirroredJoint</summary>
    /// <param name="axis">rotate around <c>axis</c></param>
    /// <param name="angle">rotate <c>angle</c> degree</param>
    /// <param name="relativeTo">The coordinate space in which to operate</param>
    public void Rotate(Vector3 axis, float angle, Space relativeTo)
    {
        transform.Rotate(axis, angle, relativeTo);
        if(mirroredJoint != null)
        {
            Vector3 tmpEulerAngles = transform.localEulerAngles;
            tmpEulerAngles.y = -tmpEulerAngles.y;
            tmpEulerAngles.z = -tmpEulerAngles.z;
            mirroredJoint.localEulerAngles = tmpEulerAngles;
        }
    }
}

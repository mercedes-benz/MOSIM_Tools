using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Control joint rotation</summary>
public class JointController : MonoBehaviour
{
    public Transform mirroredJoint;
    public bool wasChanged = false;
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
        Debug.Log("rotate " + axis + " " + angle + " " + relativeTo);
        this.wasChanged = true;
        if(mirroredJoint != null)
        {
            Vector3 axis2 = new Vector3(-axis.x, axis.y, axis.z);
            float angle2 = -angle;
            mirroredJoint.Rotate(axis2, angle2, relativeTo);
            mirroredJoint.GetComponent<JointController>().wasChanged = true;
            /**
            Vector3 tmpEulerAngles = transform.localEulerAngles;
            tmpEulerAngles.y = -tmpEulerAngles.y;
            tmpEulerAngles.z = -tmpEulerAngles.z;
            mirroredJoint.localEulerAngles = tmpEulerAngles;
            */
        }
    }
}

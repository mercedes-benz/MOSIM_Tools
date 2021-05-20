using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointInfo : MonoBehaviour
{
    public GameObject connectedBone;
    public List<GameObject> attachedBones = new List<GameObject>();
    public List<JointInfo> childJoints = new List<JointInfo>();
    public List<Vector3> endPoints = new List<Vector3>();
    public int totalChildrenCount {get{return childJoints.Count + endPoints.Count;}}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attatch JointInfo onto joint gameobject and generate bones
/// </summary> 
public class CharacterBonesBuilder : MonoBehaviour
{
    /// <summary> The root transform of character </summary>
    public Transform root;
    /// <summary> Prefab that represents a bone </summary>
    public GameObject bonePrefab;
    /// <summary> Prefab that represent a bone with unknown end point </summary>
    public GameObject endPrefab;
    public float boneScale = 100;
    public bool IsInit{private set; get;}
    /// <summary> All generated bones </summary>
    public List<GameObject> boneList = new List<GameObject>();
    /// <summary> Whether add JointInfo to each GameObject before creating bones </summary>
    public bool addJointInfo = false;
    void Awake()
    {
        IsInit = false;
        if(root != null && bonePrefab != null)
        {
            if(addJointInfo)
                AddJointInfo(root);
            BuildBones(root.GetComponent<JointInfo>());
            IsInit = true;
        }
    }

    void Update()
    {
        if(root != null && bonePrefab != null && !IsInit)
        {
            if(addJointInfo)
                AddJointInfo(root);
            BuildBones(root.GetComponent<JointInfo>());
            IsInit = true;
        }
        
    }

    /// <summary>
    /// Attatch JointInfo onto GameObject which represent joint, recursively
    /// </summary>
    /// <param name="joint">joint that JointInfo is attached to</param>
    private void AddJointInfo(Transform joint)
    {
        JointInfo jointInfo = joint.gameObject.AddComponent<JointInfo>();
        if(joint.parent != null)
        {
            JointInfo parentJointInfo = joint.transform.parent.GetComponent<JointInfo>();
            if (parentJointInfo != null)
            {
                parentJointInfo.childJoints.Add(jointInfo);
            }
        }
        foreach(Transform childJoint in joint)
            AddJointInfo(childJoint);
    }

    /// <summary>
    /// Create GameObject to represent bones between two joints
    /// </summary>
    /// <param name="jointInfo"/>
    private void BuildBones(JointInfo jointInfo)
    {
        Transform parentJoint = jointInfo.transform;

        foreach(JointInfo item in jointInfo.childJoints)
        {
            Transform childJoint = item.transform;
            GameObject bone = Instantiate<GameObject>(bonePrefab, parentJoint);
            bone.name = "vis123bone_" + parentJoint.name + "-" + childJoint.name;
            bone.transform.localScale = Vector3.one * boneScale;

            jointInfo.attachedBones.Add(bone);
            childJoint.GetComponent<JointInfo>().connectedBone = bone;

            BoneController boneController = bone.AddComponent<BoneController>();
            boneController.endJoint = childJoint;
            boneList.Add(bone);
        }
        foreach (Vector3 endPoint in jointInfo.endPoints)
        {
            GameObject bone = Instantiate<GameObject>(bonePrefab, parentJoint);
            bone.name = "vis123bone_" + parentJoint.name + "-" + "end";
            bone.transform.localScale = Vector3.one * boneScale;// / 100;

            jointInfo.attachedBones.Add(bone);
            BoneController boneController = bone.AddComponent<BoneController>();
            boneController.endPoint = endPoint;
            boneList.Add(bone);
        }

        if(jointInfo.childJoints.Count == 0 && jointInfo.endPoints.Count == 0)
        {
            GameObject bone = Instantiate<GameObject>(endPrefab, parentJoint);
            bone.name = "vis123bone_" + parentJoint.name + "-" + "end";
            bone.transform.localScale = Vector3.one * boneScale;
            jointInfo.attachedBones.Add(bone);
            BoneController boneController = bone.AddComponent<BoneController>();
            boneList.Add(bone);
        }

        foreach(JointInfo item in jointInfo.childJoints)
        {
            BuildBones(item);
        }
    }

}

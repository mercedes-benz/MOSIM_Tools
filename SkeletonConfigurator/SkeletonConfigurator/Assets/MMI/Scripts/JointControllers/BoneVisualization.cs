using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> Central Behavior to add and controll the runtime visualization of Bones. Add this behavior to the avatar. </summary>
public class BoneVisualization : MonoBehaviour
{
    /// <summary> First joint of the hierarchy.</summary>
    public Transform Root;

    /// <summary> Prefab that represents a bone </summary>
    public GameObject bonePrefab;

    /// <summary> Prefab that represent a bone with unknown end point </summary>
    public GameObject endPrefab;

    /// <summary> Scale of the bones </summary>
    public float boneScale = 100;

    //private bool IsInit = false;

    /// <summary> All generated bones </summary>
    public List<GameObject> boneList = new List<GameObject>();

    // Render Opacity controllers to change ht ebone opacity. 
    private List<RendererOpacityController> rendererOpacityControllers;

    /// <summary> Alpha of the visualization </summary>
    private float alpha = 1.0f;


    // Start is called before the first frame update
    void Start()
    {
        this.alpha = 1.0f;
        rendererOpacityControllers = new List<RendererOpacityController>();

        BuildBones(this.Root);
    }

    void BuildBones(Transform bone)
    {
        if (bone.GetComponent<ExcludeBone>() != null)
        {
            return;
        }
        if (bone.childCount > 0)
        {
            Transform[] children = new Transform[bone.childCount];
            for (int i = 0; i < bone.childCount; i++)
            {
                children[i] = bone.GetChild(i);
            }
            for (int i = 0; i < children.Length; i++)
            {
                Transform child = children[i];
                GameObject visBone = Instantiate<GameObject>(this.bonePrefab, bone);
                visBone.name = "vis123bone_" + bone.name + "-" + child.name;
                visBone.transform.localScale = Vector3.one * this.boneScale;

                visBone.transform.localRotation = Quaternion.FromToRotation(Vector3.up, child.localPosition);
                float bonelength = Vector3.Magnitude(child.localPosition);
                visBone.transform.localScale = visBone.transform.localScale * bonelength;

                boneList.Add(visBone);

                MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
                foreach (var renderer in renderers)
                {
                    rendererOpacityControllers.Add(new RendererOpacityController(renderer));
                }

                BuildBones(child);
            }
            bone.gameObject.AddComponent<JointController>();

        }
    }

    public float GetAlpha()
    {
        return alpha;
    }

    public void SetAlpha(float alpha)
    {
        this.alpha = alpha;
        foreach (var opacityController in this.rendererOpacityControllers)
        {
            opacityController.Alpha = alpha;
        }

    }


    // Update is called once per frame
    void Update()
    {
        
    }
}

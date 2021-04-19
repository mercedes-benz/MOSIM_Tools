using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterBonesBuilder))]
/// <summary> Change opacity of bone's mesh </summary>
public class CharactorBonesVisController : MonoBehaviour
{
    private CharacterBonesBuilder characterBonesBuilder;
    private List<RendererOpacityController> rendererOpacityControllers;
    public bool isInit {get; private set;}
    public float alpha
    {
        get 
        {
            if(!isInit)
                return 1;
            if(rendererOpacityControllers.Count == 0)
                return 1;
            return rendererOpacityControllers[0].Alpha;
        }
        set
        {
            if(isInit)
                foreach(var opacityController in rendererOpacityControllers)
                {
                    opacityController.Alpha = value;
                }
        }
    }
    void Awake()
    {
        isInit = Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        if(!isInit)
            isInit = Initialize();
    }

    bool Initialize()
    {
        characterBonesBuilder = GetComponent<CharacterBonesBuilder>();
        if(!characterBonesBuilder)
            return false;
        if(!characterBonesBuilder.IsInit)
            return false;
        rendererOpacityControllers = new List<RendererOpacityController>();
        foreach(var bone in characterBonesBuilder.boneList)
        {
            MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
            foreach(var renderer in renderers)
            {
                rendererOpacityControllers.Add(new RendererOpacityController(renderer));
            }
        }
        return true;
    }


}

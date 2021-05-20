using UnityEngine;
using System.Collections.Generic;

/// <summary> Change opacity of character's mesh </summary>
public class CharacterMeshRendererController : MonoBehaviour
{
    public bool isInit {get; private set;} = false;
    public float alpha
    {
        get 
        {
            if(!isInit)
                return 1;
            if(skinnedMeshRendererControllers.Count == 0)
                return 1;
            return skinnedMeshRendererControllers[0].Alpha;
        }
        set
        {
            if(isInit)
                foreach(var opacityController in skinnedMeshRendererControllers)
                {
                    opacityController.Alpha = value;
                }
        }
    }

    private List<RendererOpacityController> skinnedMeshRendererControllers;
    private bool Initialize()
    {
        SkinnedMeshRenderer[] skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        skinnedMeshRendererControllers = new List<RendererOpacityController>();
        for(int i = 0; i < skinnedMeshRenderers.Length; ++i)
        {
            if (!skinnedMeshRenderers[i].name.Contains("_virtual"))
            {
                skinnedMeshRendererControllers.Add(new RendererOpacityController(skinnedMeshRenderers[i]));
            }
            //skinnedMeshRendererControllers.Add(new RendererOpacityController(skinnedMeshRenderers[i]));
        }
        return true;
    }

    void Awake()
    {
            isInit = Initialize();
    }

    void Update()
    {
        if(!isInit)
            isInit = Initialize();
    }
}
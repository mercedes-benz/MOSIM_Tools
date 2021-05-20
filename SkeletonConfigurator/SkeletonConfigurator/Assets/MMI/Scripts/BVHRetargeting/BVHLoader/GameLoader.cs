using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MMICSharp.Common;
using MMIUnity;

public class GameLoader : MonoBehaviour
{
    bool isLoad = false;
    ApplyBVH bvhLoader;
    GameObject mainCam;
    GameObject bonePrefab;

    // Start is called before the first frame update
    void Start()
    {
        bvhLoader = GetComponent<ApplyBVH>();
        mainCam = GameObject.FindGameObjectWithTag("MainCamera");
        bonePrefab = Resources.Load<GameObject>("Prefab/SingleBone");
    }

    // Update is called once per frame
    void Update()
    {
        if(bvhLoader.Isinit && ! isLoad)
        {
            // Load FlyCam
            mainCam.AddComponent<FlyCamController>();
            FlyCam flyCam = mainCam.GetComponent<FlyCam>();
            flyCam.target = bvhLoader.Root.jointObj.transform;

            isLoad = true;
        }

    }
}

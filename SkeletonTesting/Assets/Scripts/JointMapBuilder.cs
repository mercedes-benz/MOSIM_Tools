using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointMapBuilder : MonoBehaviour
{
    public bool isRestart = false;
    public bool isSeccess {get; private set;} = false;
    public JointMap JointMap {get; private set;}
    // Start is called before the first frame update
    public Transform root = null;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(isRestart)
        {
            Debug.Log("aaaa");
            isSeccess = Init();
            if(isSeccess)
            {
                isRestart = false;
                Debug.Log("seccess");
            }
        }
        
    }

    public bool Init()
    {
        JointMapper jointMapper = new JointMapper();
        var ret = jointMapper.BuildJointMap(root);
        JointMap = jointMapper.jointMap;
        return ret;
    }
}

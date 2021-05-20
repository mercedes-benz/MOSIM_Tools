using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using MMIStandard;

public class ControlPanelInitializer : MonoBehaviour
{
    public ApplyBVH bvhLoader;
    public FlyCamViewButton frontViewButton;
    public FlyCamViewButton sideViewButton;
    public FlyCamViewButton handViewButton;
   // public JointMapBuilder jointMapBuilder;

    bool IsInit = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if(bvhLoader.Isinit && !IsInit && jointMapBuilder.JointMap != null)
        {
            var jointMap = jointMapBuilder.JointMap.GetJointMap();
            if(frontViewButton && jointMap[MJointType.PelvisCentre])
            {
                frontViewButton.target = jointMap[MJointType.PelvisCentre];
            }
            if(sideViewButton && jointMap[MJointType.PelvisCentre])
            {
                sideViewButton.target = jointMap[MJointType.PelvisCentre];
            }
            if(handViewButton && jointMap[MJointType.LeftWrist])
            {
                handViewButton.target = jointMap[MJointType.LeftWrist];
            }
        }*/
    }
}

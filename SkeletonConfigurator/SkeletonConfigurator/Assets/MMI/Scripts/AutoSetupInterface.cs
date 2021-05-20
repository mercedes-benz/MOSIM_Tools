using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MMIUnity;

public class AutoSetupInterface : MonoBehaviour
{

    public string ConfigFilePath = "";
    // Start is called before the first frame update
    void Start()
    {
        TestIS testis = this.GetComponent<TestIS>();
        var resetting = GameObject.Find("Resetting").transform;
        JointMapper2 mapper = this.GetComponent<JointMapper2>();


        resetting.Find("RealignButton").GetComponent<Button>().onClick.AddListener(delegate { testis.RealignSkeletons(); });
        resetting.Find("ResetPose").GetComponent<Button>().onClick.AddListener(delegate { testis.ResetBasePosture(); });
        resetting.Find("SaveButton").GetComponent<Button>().onClick.AddListener(delegate { testis.SaveConfig(); });
        resetting.Find("LoadButton").GetComponent<Button>().onClick.AddListener(delegate { testis.LoadConfig(); mapper.SetJointMap(testis.jointMap);  mapper.UpdateJointMap(); });
        resetting.Find("PlayButton").GetComponent<Button>().onClick.AddListener(delegate { testis.PlayExampleClip(); });

        var toggle = GameObject.Find("SwitchRetargeting").GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(delegate { testis.ToggleAvatar2ISRetargeting(toggle); });

        var BoneSlider = GameObject.Find("BoneMeshOpacity").transform.Find("Slider").GetComponent<Slider>();
        BoneSlider.onValueChanged.AddListener(delegate { this.GetComponent<BoneVisualization>().SetAlpha(BoneSlider.value); });

        var SkinSlider = GameObject.Find("SkinMeshOpacity").transform.Find("Slider").GetComponent<Slider>();
        SkinSlider.onValueChanged.AddListener(delegate { this.GetComponent<CharacterMeshRendererController>().alpha = SkinSlider.value; });

        var ReMap = GameObject.Find("ReMap").GetComponent<Button>();
        ReMap.onClick.AddListener(delegate { mapper.AutoRemap(); });

        var ClearMap = GameObject.Find("ClearMap").GetComponent<Button>();
        ClearMap.onClick.AddListener(delegate { mapper.ClearMap(); });

        var  ApplyRetareting = GameObject.Find("ApplyRetargeting").GetComponent<Button>();
        ApplyRetareting.onClick.AddListener(delegate { testis.ResetBoneMap2(); });

        var cam = GameObject.Find("Main Camera");
        if(cam != null)
        {
            var flyCam = cam.GetComponent<FlyCam>();
            if(flyCam != null)
            {
                flyCam.target = testis.gameObject.transform;
            }
        }


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

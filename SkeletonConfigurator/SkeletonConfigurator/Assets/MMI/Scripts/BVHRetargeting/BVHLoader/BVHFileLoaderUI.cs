using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;

public class BVHFileLoaderUI : MonoBehaviour
{
    public GameObject fileBrowserPref;
    private GameObject fileBrowserUI;
    private string path;
    private ApplyBVH bvhReader;
    private MMIUnity.TestIS tis;

    // Start is called before the first frame update
    void Start()
    {
        GameObject canvas = GameObject.Find("Canvas");
        bvhReader = GetComponent<ApplyBVH>();
        if(canvas)
        {
            fileBrowserUI = Instantiate<GameObject>(fileBrowserPref, canvas.transform);
            Button browseButton = fileBrowserUI.transform.GetChild(1).GetComponent<Button>();
            browseButton.onClick.AddListener(delegate {StartCoroutine(ShowLoadDialogCoroutine());});
            Button playButton = fileBrowserUI.transform.GetChild(2).GetComponent<Button>();
            playButton.interactable = false;
            playButton.onClick.AddListener(delegate {TogglePlay();});
        }
    }

    // Update is called once per frame
    void Update()
    {
        fileBrowserUI.transform.GetChild(0).GetComponent<Text>().text = path;
        Button playButton = fileBrowserUI.transform.GetChild(2).GetComponent<Button>();
        if(bvhReader.Isinit)
        {
            playButton.interactable = true;
            if(bvhReader.start)
                playButton.transform.GetChild(0).GetComponent<Text>().text = "Pause";
            else
                playButton.transform.GetChild(0).GetComponent<Text>().text = "Play";
        }
    }

    IEnumerator ShowLoadDialogCoroutine()
	{
		// Show a load file dialog and wait for a response from user
		// Load file/folder: file, Initial path: default (Documents), Title: "Load File", submit button text: "Load"
		yield return FileBrowser.WaitForLoadDialog( false, null, "Load File", "Load" );

		// Dialog is closed
		// Print whether a file is chosen (FileBrowser.Success)
		// and the path to the selected file (FileBrowser.Result) (null, if FileBrowser.Success is false)
		Debug.Log( FileBrowser.Success + " " + FileBrowser.Result );
		
		if( FileBrowser.Success )
		{
			path = FileBrowser.Result;
            GameObject root = bvhReader.Init(path);
            //GameObject jmUI = GameObject.FindGameObjectWithTag("JointMapUI");

            //jmUI.GetComponent<JointMapUI>().jointMapBuilder.Init();
            //jmUI.GetComponent<JointMapUI>().jointMapBuilder.isRestart = true;



            root.AddComponent<MMIUnity.TestIS>();
            tis = root.GetComponent<MMIUnity.TestIS>();
            //tis.jointMapUI = jmUI.GetComponent<JointMapUI>();
            //jmUI.GetComponent<JointMapUI>().IntermediateSkeleton = tis;


            tis.Pelvis = root.transform;
            tis.RootTransform = root.transform;
            tis.UseVirtualRoot = false;
            tis.ConfigurationFilePath = System.IO.Path.Combine(System.IO.Path.GetFullPath(System.IO.Path.GetDirectoryName(FileBrowser.Result)), System.IO.Path.GetFileNameWithoutExtension(FileBrowser.Result) + "_configuration.mos");
            tis.gameJointPrefab = Resources.Load("RuntimeIntermediateBone") as GameObject;
            tis.UseSkeletonVisualization = true;
            //tis.SetBoneMap(jmUI.GetComponent<JointMapUI>().jointMapBuilder.JointMap);

            root.AddComponent<JointMapper2>();
            JointMapper2 mapper = root.GetComponent<JointMapper2>();
            mapper.Root = root.transform;
            

            Button[] buttons = GameObject.FindObjectsOfType<Button>();
            foreach(Button b in buttons)
            {
                if(b.name == "RealignButton")
                {
                    b.onClick.AddListener(delegate { tis.RealignSkeletons(); });
                } else if(b.name == "ResetPose")
                {
                    b.onClick.AddListener(delegate { tis.ResetBasePosture(); });
                } else if (b.name == "SaveButton")
                {
                    //b.onClick.AddListener(delegate { tis.SaveConfig("BVH_config_file.retargeting", this.bvhReader.base_rotations); });
                    b.onClick.AddListener(delegate { tis.SaveConfig(); });
                } else if(b.name == "LoadButton")
                {
                    // b.onClick.AddListener(delegate { tis.LoadConfig("BVH_config_file.retargeting"); });
                    b.onClick.AddListener(delegate { tis.LoadConfig(); });
                } else if(b.name == "PlayButton")
                {
                    b.onClick.AddListener(delegate { tis.PlayExampleClip(); });
                }
            }
            var toggle = GameObject.Find("SwitchRetargeting").GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(delegate { tis.ToggleAvatar2ISRetargeting(toggle); });

            var BoneSlider = GameObject.Find("BoneMeshOpacity").transform.Find("Slider").GetComponent<Slider>();
            //BoneSlider.onValueChanged.AddListener(delegate { this.GetComponent<BoneVisualization>().SetAlpha(BoneSlider.value); });

            var SkinSlider = GameObject.Find("SkinMeshOpacity").transform.Find("Slider").GetComponent<Slider>();
            //SkinSlider.onValueChanged.AddListener(delegate { this.GetComponent<CharacterMeshRendererController>().alpha = SkinSlider.value; });

            var ReMap = GameObject.Find("ReMap").GetComponent<Button>();
            ReMap.onClick.AddListener(delegate { mapper.AutoRemap(); });

            var ClearMap = GameObject.Find("ClearMap").GetComponent<Button>();
            ClearMap.onClick.AddListener(delegate { mapper.ClearMap(); });

            var ApplyRetareting = GameObject.Find("ApplyRetargeting").GetComponent<Button>();
            ApplyRetareting.onClick.AddListener(delegate { tis.ResetBoneMap2(); });

            var flyCam = Camera.main.gameObject.GetComponent<FlyCam>();
            var flyCamController = Camera.main.gameObject.GetComponent<FlyCamController>();
            if(flyCam != null)
            {
                flyCam.target = root.transform;
                flyCam.enabled = true;
                if(flyCamController != null)
                {
                    flyCamController.enabled = true;
                }
            }

        }
    }

    void TogglePlay()
    {
        tis.Avatar2IS2Avatar = false;
        tis.IS2Avatar = false;
        tis.Avatar2IS = true;
        bvhReader.start = !bvhReader.start;
        if(!bvhReader.start)
        {
            tis.Avatar2IS = false;
        }
    }
}

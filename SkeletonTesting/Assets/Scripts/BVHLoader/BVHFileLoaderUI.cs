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
            GameObject jmUI = GameObject.FindGameObjectWithTag("JointMapUI");

            jmUI.GetComponent<JointMapUI>().jointMapBuilder.Init();


            root.AddComponent<MMIUnity.TestIS>();
            tis = root.GetComponent<MMIUnity.TestIS>();
            tis.jointMapUI = jmUI.GetComponent<JointMapUI>();
            jmUI.GetComponent<JointMapUI>().IntermediateSkeleton = tis;


            tis.RootBone = root.transform;
            tis.RootTransform = root.transform;

            tis.gameJointPrefab = Resources.Load("Prefab/IntermediatSkeletonBone") as GameObject;
            tis.UseSkeletonVisualization = true;
            tis.SetBoneMap(jmUI.GetComponent<JointMapUI>().jointMapBuilder.JointMap);


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
                    b.onClick.AddListener(delegate { tis.SaveConfig("BVH_config_file.retargeting", this.bvhReader.base_rotations); });
                } else if(b.name == "LoadButton")
                {
                    b.onClick.AddListener(delegate { tis.LoadConfig("BVH_config_file.retargeting"); });
                } else if(b.name == "PlayMOSIM")
                {
                    b.onClick.AddListener(delegate { tis.PlayExampleClip(); });
                }
            }

            Toggle[] toggles = GameObject.FindObjectsOfType<Toggle>();
            foreach(Toggle t in toggles)
            {
                if(t.name == "Toggle")
                {
                    t.onValueChanged.AddListener(delegate { tis.ToggleAvatar2ISRetargeting(t); });
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using MMIStandard;
using MMIUnity;

public class JointMapUI : MonoBehaviour
{
    public RectTransform container;
    public GameObject jointMapItemPrefab;
    public JointMapBuilder jointMapBuilder;
    public JointMap jointMap { get; set; }
    private JointMapper jointMapper;
    private List<Transform> jointList;
    private List<GameObject> itemUIList = new List<GameObject>();
    public Transform root;
    public CharacterBonesBuilder characterBonesBuilder;
    public Camera mainCamera;
    public TestIS IntermediateSkeleton;
    public bool IsInit = false;

    void Awake()
    {
        if(container == null)
            container = (RectTransform)transform;
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    public void OnChange()
    {
        if (IsInit)
        {
            IntermediateSkeleton.SetBoneMap(jointMap);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(jointMapBuilder != null && jointMap == null)
        {
            jointMap = jointMapBuilder.JointMap;
        }
        if(characterBonesBuilder && characterBonesBuilder.IsInit && jointMapBuilder.isSeccess && !IsInit)
        {
            init();
            IsInit = true;
        }
        bool change = false;
        foreach (var itemUI in itemUIList)
        {
            string jointTypeString = null;
            foreach (RectTransform child in itemUI.transform)
            {
                if (child.name == "JointType" && child.GetComponent<Text>())
                {
                    jointTypeString = child.GetComponent<Text>().text;
                }
                if (child.name == "JointName" && child.GetComponent<Dropdown>())
                {
                    var jointNameDropdown = child.GetComponent<Dropdown>();
                    MJointType jointType = (MJointType)Enum.Parse(typeof(MJointType), jointTypeString);
                    jointNameDropdown.value = jointList.FindIndex(x => x == jointMap.GetJointMap()[jointType]);
                }
            }
            change = change || IntermediateSkeleton.SetBoneMap(jointMap);
        }
    }


    void init()
    {
        // jointMapper = new JointMapper(root);
        // jointMap = jointMapper.jointMap;
        
        BuildJointList();
        ReadOnlyDictionary<MJointType, Transform> dict = jointMap.GetJointMap();
        itemUIList = new List<GameObject>();
        foreach(var item in dict)
        {
            GameObject itemUI = Instantiate(jointMapItemPrefab, container);
            itemUIList.Add(itemUI);
            foreach(RectTransform child in itemUI.transform)
            {
                if(child.name == "JointType" && child.GetComponent<Text>())
                {
                    var jointTypeText = child.GetComponent<Text>();
                    jointTypeText.text = item.Key.ToString("g");
                }
                if(child.name == "JointName" && child.GetComponent<JointNameDropDown>())
                {
                    var jointNameDropdown = child.GetComponent<JointNameDropDown>();
                    foreach(Transform joint in jointList)
                    {
                        Dropdown.OptionData optionData = new Dropdown.OptionData();
                        if(joint == null)
                        {
                            optionData.text = "null";
                        }
                        else
                        {
                            optionData.text = joint.name;
                        }
                        jointNameDropdown.options.Add(optionData);
                    }
                    jointNameDropdown.mainCamera = mainCamera;
                    jointNameDropdown.RefreshShownValue();
                    int idx = jointList.FindIndex(x => x == item.Value);
                    if(idx != -1)
                        jointNameDropdown.value = idx;
                    jointNameDropdown.onValueChanged.AddListener(x => jointMap.SetTransform(item.Key, jointList[x]));
                }
            }
        }
    }

    void UpdateJointNameDropDownValue()
    {
        foreach(var itemUI in itemUIList)
        {
            string jointTypeString=null;
            foreach(RectTransform child in itemUI.transform)
            {
                if(child.name == "JointType" && child.GetComponent<Text>())
                {
                    jointTypeString = child.GetComponent<Text>().text;
                }
                if(child.name == "JointName" && child.GetComponent<Dropdown>())
                {
                    var jointNameDropdown = child.GetComponent<Dropdown>();
                    MJointType jointType = (MJointType)Enum.Parse(typeof(MJointType), jointTypeString);
                    jointNameDropdown.value = jointList.FindIndex(x => x == jointMap.GetJointMap()[jointType]);
                }
            }
        }
    }

    void BuildJointList()
    {
        Scene scene = SceneManager.GetActiveScene();
        GameObject[] rootGameObjects = scene.GetRootGameObjects();
        jointList = new List<Transform>();
        jointList.Add(null);
        foreach(var go in rootGameObjects)
        {
            JointInfo[] jointInfoArray = go.GetComponentsInChildren<JointInfo>();
            foreach(var jointInfo in jointInfoArray)
                jointList.Add(jointInfo.transform);
        }
    }

    Transform FindRootJoint(Transform transform)
    {
        if(transform.GetComponent<JointInfo>() != null)
            return transform;
        foreach(Transform child in transform)
        {
            Transform rootJoint = FindRootJoint(child);
            if(rootJoint != null)
                return rootJoint;
        }
        return null;
    }
}

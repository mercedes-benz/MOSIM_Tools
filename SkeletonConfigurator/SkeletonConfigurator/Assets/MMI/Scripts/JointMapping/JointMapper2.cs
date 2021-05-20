using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MMIStandard;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class JointMapper2 : MonoBehaviour
{
    private class JointMapEntry
    {
        public JointMapEntry(Transform t)
        {
            this.t = t;
        }
        public Transform t { get; set;  }
        public bool manual { get; set; } = false;
    }
    /// <summary>
    /// First transform of the joint hierarchy.  
    /// </summary>
    public Transform Root;

    public string LastAutoMapMessage { get; } = "";

    private string visualizationString = "vis123bone_";


    private RectTransform ItemListContainer;
    private List<GameObject> itemUIList;
    private GameObject jointMapItemPrefab;
    private List<Transform> jointList;
    private List<MJointType> joints;



    // Start is called before the first frame update
    void Start()
    {
        AutoMap();

        ItemListContainer = GameObject.Find("JointMap/Viewport/Content").GetComponent<RectTransform>();
        jointMapItemPrefab = Resources.Load<GameObject>("UI/JointMapItem");
        itemUIList = new List<GameObject>();
        jointList = new List<Transform>();
        jointList.Add(null);
        BuildJointList(this.Root);
        BuildJoints();


        foreach (MJointType mjoint in joints)
        {
            GameObject itemUI = Instantiate(jointMapItemPrefab, ItemListContainer);
            itemUIList.Add(itemUI);
            Transform mappedJoint = null;
            if(this.jointMap.ContainsKey(mjoint))
            {
                mappedJoint = this.jointMap[mjoint].t;
            }
            foreach (RectTransform child in itemUI.transform)
            {
                if (child.name == "JointType" && child.GetComponent<Text>())
                {
                    var jointTypeText = child.GetComponent<Text>();
                    jointTypeText.text = mjoint.ToString("g");
                }
                if (child.name == "JointName" && child.GetComponent<JointNameDropDown>())
                {
                    var jointNameDropdown = child.GetComponent<JointNameDropDown>();
                    foreach (Transform joint in jointList)
                    {
                        Dropdown.OptionData optionData = new Dropdown.OptionData();
                        if (joint == null)
                        {
                            optionData.text = "null";
                        }
                        else
                        {
                            optionData.text = joint.name;
                        }
                        jointNameDropdown.options.Add(optionData);
                    }
                    //jointNameDropdown.mainCamera = mainCamera;
                    jointNameDropdown.RefreshShownValue();
                    int idx = jointList.FindIndex(x => x == mappedJoint);
                    if (idx != -1)
                        jointNameDropdown.value = idx;
                    //jointNameDropdown.onValueChanged.AddListener(x => jointMap.SetTransform(item.Key, jointList[x]));
                }
            }
        }
        UpdateJointMap();

    }

    public void UpdateJointMap()
    {
        foreach (var ItemUI in this.itemUIList)
        {
            var typetext = ItemUI.transform.Find("JointType").GetComponent<Text>();
            if (typetext == null)
            {
                Debug.Log("Error: text not found");
            }
            MJointType type = (MJointType)Enum.Parse(typeof(MJointType), typetext.text, true);
            var dropdown = ItemUI.GetComponentInChildren<JointNameDropDown>();
            string name = dropdown.options[dropdown.value].text;

            if (this.jointMap.ContainsKey(type) && this.jointMap[type].t.name != name)
            {
                int idx = jointList.FindIndex(x => x == this.jointMap[type].t);
                if (idx != -1)
                    dropdown.value = idx;
            } else if(!this.jointMap.ContainsKey(type))
            {
                dropdown.value = 0;
            }
        }

        foreach(var j in this.jointList)
        {
            if(j == null)
            {
                continue;
            }
            JointController jc = j.GetComponent<JointController>();
            if(jc != null)
            {
                MJointType type = ReverseMap(j);
                if (type != MJointType.Undefined)
                {
                    string typestr = type.ToString();
                    if(typestr.Contains("Left"))
                    {
                        typestr = typestr.Replace("Left", "Right");
                    } else if(typestr.Contains("Left"))
                    {
                        typestr = typestr.Replace("Right", "Left");
                    } else
                    {
                        typestr = "";
                    }

                    if(typestr != "")
                    {
                        MJointType newType = (MJointType)Enum.Parse(typeof(MJointType), typestr);
                        if (this.jointMap.ContainsKey(newType))
                        {
                            jc.mirroredJoint = this.jointMap[newType].t;
                            JointController mjc = this.jointMap[newType].t.gameObject.GetComponent<JointController>();
                            mjc.mirroredJoint = this.transform;
                        }
                        else
                            jc.mirroredJoint = null;
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void BuildJointList(Transform t)
    {
        this.jointList.Add(t);
        foreach(var child in RealChildren(t))
        {
            BuildJointList(child);
        }
    }

    private Dictionary<MJointType, JointMapEntry> jointMap = new Dictionary<MJointType, JointMapEntry>();

    public void SetJointMap(JointMap jm)
    {
        foreach(KeyValuePair<MJointType, Transform> entry in jm.GetJointMap())
        {
            if(entry.Value != null)
            {
                if (!jointMap.ContainsKey(entry.Key))
                {
                    jointMap[entry.Key] = new JointMapEntry(entry.Value);
                }
                jointMap[entry.Key].t = entry.Value;
                jointMap[entry.Key].manual = true;
            }
        }
    }

    public void AddJointMapEntryAuto(MJointType type, Transform t)
    {
        if (jointMap.ContainsKey(type))
        {
            if(!(jointMap[type]).manual)
            {
                jointMap[type] = new JointMapEntry(t);
            }
        } else
        {
            jointMap.Add(type, new JointMapEntry(t));
        }
    }

    public Dictionary<string, MJointType> GetJointMap()
    {
        Dictionary<string, MJointType> namemap = new Dictionary<string, MJointType>();
        foreach(var e in this.jointMap)
        {
            if (e.Value.t != null)
            {
                namemap.Add(e.Value.t.name, e.Key);
            }
        }
        return namemap;
    }

    public bool IsMapped(MJointType type, bool manual = false)
    {
        if (this.jointMap.ContainsKey(type))
        {
            if((!manual) || this.jointMap[type].manual)
            {
                return true;
            }
        }
        return false;
    }

    public bool IsMapped(Transform t, bool manual = false)
    {
        foreach(JointMapEntry e in this.jointMap.Values)
        {
            if (e.t == t)
            {
                if( (!manual) || e.manual)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public MJointType ReverseMap(Transform t)
    {
        foreach (var e in this.jointMap)
        {
            if (e.Value.t == t)
            {
                return e.Key;
            }
        }
        return MJointType.Undefined;

    }


    (MJointType, MJointType)[] legJointTypePairsArray =
    {
                (MJointType.LeftHip, MJointType.RightHip),
                (MJointType.LeftKnee, MJointType.RightKnee),
                (MJointType.LeftAnkle, MJointType.RightAnkle),
                (MJointType.LeftBall, MJointType.RightBall),
                (MJointType.LeftBallTip, MJointType.RightBallTip)
            };
    (MJointType, MJointType)[] armJointPairsArray =
        {
                (MJointType.LeftShoulder, MJointType.RightShoulder),
                (MJointType.LeftElbow, MJointType.RightElbow),
                (MJointType.LeftWrist, MJointType.RightWrist)
            };

    public void AutoRemap()
    {
        foreach(var ItemUI in this.itemUIList)
        {
            var typetext = ItemUI.transform.Find("JointType").GetComponent<Text>();
            if(typetext == null)
            {
                Debug.Log("Error: text not found");
            }
            MJointType type = (MJointType)Enum.Parse(typeof(MJointType), typetext.text, true);
            var dropdown = ItemUI.GetComponentInChildren<JointNameDropDown>();
            string name = dropdown.options[dropdown.value].text;

            if(this.jointMap.ContainsKey(type) && this.jointMap[type].t.name == null)
            {
                this.jointMap.Remove(type);
            }
            else if(this.jointMap.ContainsKey(type) && this.jointMap[type].t.name != name)
            {
                this.jointMap[type].t = FindRecursive(this.Root, name);
                this.jointMap[type].manual = true;
            } else if(!this.jointMap.ContainsKey(type) && name != "null")
            {
                this.jointMap[type] = new JointMapEntry(FindRecursive(this.Root, name));
                this.jointMap[type].manual = true;
            }
        }
        AutoMap();
        UpdateJointMap();
    }

    public void ClearMap()
    {
        this.jointMap.Clear();
        UpdateJointMap();
    }


    public Transform FindRecursive(Transform t, string name)
    {
        if (t.name == name) return t;
        Transform tn = t.Find(name);
        if (tn != null) return tn;
        foreach(Transform child in RealChildren(t))
        {
            tn = FindRecursive(child, name);
            if (tn != null) return tn;
        }
        return null;
    }

    public bool AutoMap()
    {
        // finding legs
        bool legs = false;
        if (IsMapped(MJointType.LeftHip) && IsMapped(MJointType.RightHip))
        {
            Transform[] candidates = new Transform[] { this.jointMap[MJointType.LeftHip].t, this.jointMap[MJointType.RightHip].t };
            legs = AutoMapLegs(candidates[0].parent, candidates);
        } else
        {
            legs = AutoMapLegs(this.Root);
        }

        // find and map spine:

        var spine = AutoMapSpine(this.Root, new List<Transform>());

        // if there are already candidates, map arms:
        bool arms = false;
        if(IsMapped(MJointType.LeftShoulder) && IsMapped(MJointType.RightShoulder))
        {
            arms = AutoMapArms(new Transform[] { this.jointMap[MJointType.LeftShoulder].t, this.jointMap[MJointType.RightShoulder].t });  
        } else
        {
            // find and map arms:
            arms = AutoMapFindArms(this.Root, new List<Transform[]>());
        }

        bool fingers = false;
        if (IsMapped(MJointType.LeftWrist) && IsMapped(MJointType.RightWrist))
        {
            fingers = AutoMapFingers(this.jointMap[MJointType.LeftWrist].t, this.jointMap[MJointType.RightWrist].t);
        } else if(IsMapped(MJointType.LeftWrist))
        {
            fingers = AutoMapFingers(this.jointMap[MJointType.LeftWrist].t, null);
        } else if(IsMapped(MJointType.RightWrist))
        {
            fingers = AutoMapFingers(null, this.jointMap[MJointType.RightWrist].t);
        }
        

        Debug.Log("legs: " + legs + " spine: " + spine + " arms: " + arms + " fingers: " + fingers);
        return true;
    }

    private bool AutoMapSpine(Transform t, List<Transform> candidates)
    {
        // first go to top: 
        var children = RealChildren(t);
        if (children.Length > 1)
        {
            float min_height = t.position.y;
            Transform min_candidate = null;
            foreach(var child in children)
            {
                float[] minmax = minMaxHeight(child);
                if(minmax[1] > min_height)
                {
                    min_candidate = child;
                    min_height = minmax[1];
                }
            }
            candidates.Add(min_candidate);
            return AutoMapSpine(min_candidate, candidates);
        } else if(children.Length == 1)
        {
            candidates.Add(children[0]);
            return AutoMapSpine(children[0], candidates);
        } else
        {

            if (!IsMapped(candidates[0], true)) {

                // found the head. Reverse List
                candidates.Reverse();
                if (this.jointMap.ContainsKey(MJointType.HeadTip) && this.jointMap[MJointType.HeadTip].t == candidates[0])
                {
                    candidates.Remove(candidates[0]);
                }
                if (candidates.Count > 0)
                    this.AddJointMapEntryAuto(MJointType.HeadJoint, candidates[0]);
                if (candidates.Count > 1)
                    this.AddJointMapEntryAuto(MJointType.C4C5Joint, candidates[1]);
                if (candidates.Count > 2)
                    this.AddJointMapEntryAuto(MJointType.T1T2Joint, candidates[2]);
                if (candidates.Count > 3)
                    this.AddJointMapEntryAuto(MJointType.T12L1Joint, candidates[3]);
                if (candidates.Count > 4)
                    this.AddJointMapEntryAuto(MJointType.S1L5Joint, candidates[4]);
                if (candidates.Count > 5)
                    this.AddJointMapEntryAuto(MJointType.PelvisCentre, candidates[5]);

                return true;
            } else
            {
                return false;
            }
        }
    }

    private bool AutoMapFindArms(Transform t, List<Transform[]> candidates)
    {
        // first go to up and find all candidates: 
        var children = RealChildren(t);
        if (children.Length > 1)
        {
            float min_height = t.position.y;
            Transform min_candidate = null;
            foreach (var child in children)
            {
                float[] minmax = minMaxHeight(child);
                if (minmax[1] > min_height)
                {
                    min_candidate = child;
                    min_height = minmax[1];
                }
            }
            if(children.Length> 2)
            {
                // If there are more than 2 children, we have possible shoulder candidates. 
                List<Transform> shoulders = new List<Transform>();
                foreach(var child in children)
                {
                    if(child != min_candidate && !IsMapped(child))
                    {
                        shoulders.Add(child);
                    }
                }
                if(shoulders.Count == 2)
                {
                    candidates.Add(shoulders.ToArray());
                }
            }
            
            return AutoMapFindArms(min_candidate, candidates);
        }
        else if (children.Length == 1)
        {
            return AutoMapFindArms(children[0], candidates);
        }
        else
        {
            // from all candidates, decide which one goes more to the outside. 
            float max_x = this.Root.position.x;
            Transform[] max_candidates = null;
            foreach(Transform[] c in candidates)
            {
                float[] minmax = minMaxSpan(c[0]);
                if(minmax[1] > max_x)
                {
                    max_x = minmax[1];
                    max_candidates = c;
                }
            }
            if(max_candidates != null)
            {
                return AutoMapArms(max_candidates);
            } else
            {
                return false;
            }
            
        }
    }

    private bool AutoMapArms(Transform[] candidates)
    {
        candidates = LeftRight(candidates[0], candidates[1]);
        AddJointMapEntryAuto(MJointType.LeftShoulder, candidates[0]);
        AddJointMapEntryAuto(MJointType.RightShoulder, candidates[1]);
        bool check1 = false, check2 = false;
        // Map elbow and wrist by going to the dominant x children. 
        // Map left arm
        if (RealChildCount(candidates[0]) > 0)
        {
            AddJointMapEntryAuto(MJointType.LeftElbow, dominantXChild(candidates[0]));
            if (RealChildCount(dominantXChild(candidates[0])) > 0)
            {
                AddJointMapEntryAuto(MJointType.LeftWrist, dominantXChild(dominantXChild(candidates[0])));
                check1 = true;
            }
                
        }

        // map right arm
        if (RealChildCount(candidates[1]) > 0)
        {
            AddJointMapEntryAuto(MJointType.RightElbow, dominantXChild(candidates[1]));
            if (RealChildCount(dominantXChild(candidates[1])) > 0)
            {
                AddJointMapEntryAuto(MJointType.RightWrist, dominantXChild(dominantXChild(candidates[1])));
                check2 = true;
            }
        }
        return check1 && check2;
    }

    
    /// <summary>
    /// Automatically map the fingers.
    /// </summary>
    /// <param name="leftWrist"></param>
    /// <param name="rightWrist"></param>
    /// <returns></returns>
    private bool AutoMapFingers(Transform leftWrist, Transform rightWrist)
    {
        // building finger descriptions and mappings from left to right hand. 
        (MJointType, MJointType)[][] fingersJointTypePairs = new (MJointType, MJointType)[5][];
        (MJointType, MJointType)[] thumbJointTypePairs =
            {
                (MJointType.LeftThumbMid, MJointType.RightThumbMid),
                (MJointType.LeftThumbMeta, MJointType.RightThumbMeta),
                (MJointType.LeftThumbCarpal, MJointType.RightThumbCarpal),
                (MJointType.LeftThumbTip, MJointType.LeftThumbTip)
            };
        fingersJointTypePairs[0] = thumbJointTypePairs;
        string[] fingerStr = { "Index", "Middle", "Ring", "Little" };
        string[] jointStr = { "Proximal", "Meta", "Distal", "Tip" }; // TODO This is factually wrong, but requires a renaming of meta to mid or an additional joint. 
        (MJointType, MJointType)[] indexFingerPairs = new (MJointType, MJointType)[4];
        for (int i = 0; i < fingerStr.Length; ++i)
        {
            (MJointType, MJointType)[] fingerJointTypePairs = new (MJointType, MJointType)[jointStr.Length];
            for (int j = 0; j < jointStr.Length; ++j)
            {
                MJointType leftJoint = (MJointType)Enum.Parse(typeof(MJointType), "Left" + fingerStr[i] + jointStr[j]);
                MJointType rightJoint = (MJointType)Enum.Parse(typeof(MJointType), "Right" + fingerStr[i] + jointStr[j]);
                fingerJointTypePairs[j] = (leftJoint, rightJoint);
            }
            fingersJointTypePairs[i + 1] = fingerJointTypePairs;
        }


        bool left = false;
        if(leftWrist != null && RealChildCount(leftWrist) == 5)
        {
            List<Transform> leftFirstJoints = new List<Transform>(RealChildren(leftWrist));
            leftFirstJoints = sortAlongZ(leftFirstJoints);
            TryReplaceJoint(leftFirstJoints, MJointType.LeftThumbMid, 0);
            TryReplaceJoint(leftFirstJoints, MJointType.LeftIndexProximal, 1);
            TryReplaceJoint(leftFirstJoints, MJointType.LeftMiddleProximal, 2);
            TryReplaceJoint(leftFirstJoints, MJointType.LeftRingProximal, 3);
            TryReplaceJoint(leftFirstJoints, MJointType.LeftLittleProximal, 4);

            // Todo do for all fingers
            int fingerCount = Mathf.Min(fingersJointTypePairs.Length, leftFirstJoints.Count);
            for (int i = 0; i < fingerCount; ++i)
            {
                var leftFingerJoint = leftFirstJoints[i];

                AddJointMapEntryAuto(fingersJointTypePairs[i][0].Item1, leftFingerJoint);

                for (int j = 1; j < fingersJointTypePairs[i].Length; j++)
                {
                    if (RealChildCount(leftFingerJoint) > 0)
                    {
                        leftFingerJoint = RealChildren(leftFingerJoint)[0];
                        AddJointMapEntryAuto(fingersJointTypePairs[i][j].Item1, leftFingerJoint);
                    }
                }
            }
            left = true;
        }

        bool right = false;
        if(rightWrist != null && RealChildCount(rightWrist) == 5)
        {
            List<Transform> rightFirstJoints = new List<Transform>(RealChildren(rightWrist));
            rightFirstJoints = sortAlongZ(rightFirstJoints);
            TryReplaceJoint(rightFirstJoints, MJointType.RightThumbMid, 0);
            TryReplaceJoint(rightFirstJoints, MJointType.RightIndexProximal, 1);
            TryReplaceJoint(rightFirstJoints, MJointType.RightMiddleProximal, 2);
            TryReplaceJoint(rightFirstJoints, MJointType.RightRingProximal, 3);
            TryReplaceJoint(rightFirstJoints, MJointType.RightLittleProximal, 4);


            int fingerCount = Mathf.Min(fingersJointTypePairs.Length, rightFirstJoints.Count);

            for (int i = 0; i < fingerCount; ++i)
            {
                var rightFingerJoint = rightFirstJoints[i];

                AddJointMapEntryAuto(fingersJointTypePairs[i][0].Item2, rightFingerJoint);

                for (int j = 1; j < fingersJointTypePairs[i].Length; j++)
                {
                    if (RealChildCount(rightFingerJoint) > 0)
                    {
                        rightFingerJoint = RealChildren(rightFingerJoint)[0];
                        AddJointMapEntryAuto(fingersJointTypePairs[i][j].Item2, rightFingerJoint);
                    }
                }
            }
            right = true;
        }


        return left && right;
    }



    /// <summary>
    /// Trys to replace an entry in the transform list with a manual set transform at the id position in the list (e.g. thumb at position 0). 
    /// </summary>
    /// <param name="joints"></param>
    /// <param name="type"></param>
    /// <param name="id"></param>
    private void TryReplaceJoint(List<Transform> joints, MJointType type, int id)
    {
        if (this.jointMap.ContainsKey(type) && this.jointMap[type].manual && this.jointMap[type].t != null)
        {
            joints.Remove(this.jointMap[type].t);
            joints.Insert(id, this.jointMap[type].t);
        }
    }

    private List<Transform> sortAlongZ(List<Transform> joints)
    {
        List<Transform> ret = new List<Transform>();
        var minjoint = joints[0];

        float minz = lastJoint(joints[0]).transform.position.z;
        foreach (var joint in joints)
        {
            float min = lastJoint(joint).transform.position.z;
            if (min > minz)
            {
                minjoint = joint;
            }
        }
        ret.Add(minjoint);
        joints.Remove(minjoint);
        if (joints.Count > 0)
        {
            ret.AddRange(sortAlongZ(joints));
        }
        return ret;
    }
    private Transform lastJoint(Transform joint)
    {
        if (RealChildCount(joint) == 0)
        {
            return joint;
        }
        else
        {
            return lastJoint(RealChildren(joint)[0]);
        }
    }



    private bool AutoMapLegs(Transform t)
    {
        Transform[] children = RealChildren(t);
        bool check = false;
        if(children.Length > 1)
        {
            check = AutoMapLegs(t, children);
        }

        if (check) return check;
        
        if(children.Length == 0)
        {
            return false;
        }
        else
        {
            return AutoMapLegs(children[0]);
        }
    }

    private bool AutoMapLegs(Transform parent, Transform[] LegCandidates)
    {
        // filter out all visualization bones and all bones going upwards. 
        if (LegCandidates.Length > 2)
        {
            List<Transform> newCandidates = new List<Transform>();
            foreach (Transform t in LegCandidates)
            {
                var minmax = minMaxHeight(t);
                if(! (t.name.StartsWith(this.visualizationString) || minmax[1] - parent.position.y > 0.5))
                {
                    newCandidates.Add(t);
                }
            }
            LegCandidates = newCandidates.ToArray();
        }
        // If less than two candidates are there, return false. 
        if (LegCandidates.Length < 2)
        {
            Debug.Log("Auto Mapping Legs failed");
            return false;
        }
        float[] minmax0 = minMaxHeight(LegCandidates[0]);
        float[] minmax1 = minMaxHeight(LegCandidates[1]);

        // assumption for auto decision: both should have the same lower height (assuming both legs have the same length)
        // and the feet should be below the waist. 
        /*if (Mathf.Abs(minmax0[0] - minmax1[0]) < 0.01 && parent.position.y - minmax0[0] > 0.5)
        {*/
            var lr = LeftRight(LegCandidates[0], LegCandidates[1]);
            this.AddJointMapEntryAuto(MJointType.LeftHip, lr[0]);
            var lk = dominantYChild(lr[0]);
            if(lk != null)
            {
                this.AddJointMapEntryAuto(MJointType.LeftKnee, lk);
                var la = dominantYChild(lk);
                if(la != null)
                {
                    this.AddJointMapEntryAuto(MJointType.LeftAnkle, la);
                    var lt = dominantYChild(la);
                    if(lt != null)
                    {
                        this.AddJointMapEntryAuto(MJointType.LeftBall, lt);
                    }
                }
            }

            this.AddJointMapEntryAuto(MJointType.RightHip, lr[1]);
            var rk = dominantYChild(lr[1]);
            if (rk != null)
            {
                this.AddJointMapEntryAuto(MJointType.RightKnee, rk);
                var ra = dominantYChild(rk);
                if (ra != null)
                {
                    this.AddJointMapEntryAuto(MJointType.RightAnkle, ra);
                    var rt = dominantYChild(ra);
                    if (rt != null)
                    {
                        this.AddJointMapEntryAuto(MJointType.RightBall, rt);
                    }

                }
            }
            return true;
        /*
        } else
        {
            return false;
        }*/
    }

    private bool AutoMapArm()
    {
        return true;

    }

    private bool AutoMapFingers()
    {
        return true;
    }

    private int RealChildCount(Transform t)
    {
        int cc = 0;
        for(int i = 0; i < t.childCount; i++)
        {
            if(!t.GetChild(i).name.StartsWith(this.visualizationString))
            {
                cc++;
            }
        }
        return cc;
    }

    private Transform[] RealChildren(Transform t)
    {
        List<Transform> lt = new List<Transform>();
        for (int i = 0; i < t.childCount; i++)
        {
            if (!t.GetChild(i).name.StartsWith(this.visualizationString))
            {
                lt.Add(t.GetChild(i));
            }
        }
        return lt.ToArray();
    }


    /// <summary>
    /// Computes the minimum and maximum height of all children. 
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    private float[] minMaxHeight(Transform t)
    {
        float min, max;
        min = max = t.position.y;
        for (int i = 0; i < t.childCount; i++)
        {
            Transform child = t.GetChild(i);
            //if (Mathf.Abs(child.transform.position.y - t.position.y) < 1)
            //{
            float[] minmax = minMaxHeight(child);

            min = Mathf.Min(minmax[0], min);
            max = Mathf.Max(minmax[1], max);
            //}
        }
        return new float[] { min, max };
    }

    /// <summary>
    /// Computes the minimum and maximum span in x or z direction of all children. 
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    private float[] minMaxSpan(Transform t)
    {
        float min, max;
        max = Mathf.Max(t.position.z, t.position.x);
        min = Mathf.Min(t.position.z, t.position.x);
        for (int i = 0; i < t.childCount; i++)
        {
            Transform child = t.GetChild(i);
            float[] minmax = minMaxSpan(child);

            min = Mathf.Min(minmax[0], min);
            max = Mathf.Max(minmax[1], max);
        }
        return new float[] { min, max };
    }




    /// <summary>
    /// Returns the child most likely pointing in X direction, regardless of direction (left / right).
    /// </summary>
    /// <param name="joint"></param>
    /// <returns></returns>
    private Transform dominantXChild(Transform joint)
    {
        if (joint.childCount == 0)
        {
            return null;
        }
        Transform dominant = null;
        float dominant_x = 0; // Mathf.Abs(dominant.transform.position.x);
        for(int i = 0; i< joint.childCount; i++)
        {
            if (!joint.name.StartsWith(this.visualizationString))
            {
                Transform child = joint.GetChild(i);
                float child_x = Mathf.Abs(child.transform.position.x);
                if (child_x > dominant_x)
                {
                    dominant = child;
                    dominant_x = child_x;
                }
            }
        }
        return dominant;
    }

    /// <summary>
    /// Returns the child most likely pointing along y direction, regardless of direction (up / down). 
    /// </summary>
    /// <param name="joint"></param>
    /// <returns></returns>
    private Transform dominantYChild(Transform joint)
    {
        if (joint.childCount == 0)
        {
            return null;
        }
        Transform dominant = null; // joint.GetChild(0);
        float dominant_x = 0;//Mathf.Abs(dominant.transform.position.y - joint.transform.position.y);
        for (int i = 0; i < joint.childCount; i++)
        {
            if(!joint.name.StartsWith(this.visualizationString))
            {
                Transform child = joint.GetChild(i);
                float child_x = Mathf.Abs(child.transform.position.y - joint.transform.position.y);
                if (child_x > dominant_x)
                {
                    dominant = child;
                    dominant_x = child_x;
                }
            }
        }
        return dominant;
    }


    /// <summary>
    /// Orders t1 and t2 in left right order. 
    /// </summary>
    /// <param name="t1"></param>
    /// <param name="t2"></param>
    private Transform[] LeftRight(Transform t1, Transform t2)
    {
        Transform tmp;
        if (t1 && t2 && Vector3.Dot((t2.position - t1.position), Vector3.right) < 0)
        {
            tmp = t1;
            t1 = t2;
            t2 = tmp;
        }
        return new Transform[] { t1, t2 };
    }


    private void BuildJoints()
    {
        this.joints = new List<MJointType>();
        this.joints.Add(MJointType.Root);
        this.joints.Add(MJointType.PelvisCentre);
        this.joints.Add(MJointType.LeftHip);
        this.joints.Add(MJointType.RightHip);
        this.joints.Add(MJointType.LeftShoulder);
        this.joints.Add(MJointType.RightShoulder);
        this.joints.Add(MJointType.LeftWrist);
        this.joints.Add(MJointType.RightWrist);

        this.joints.Add(MJointType.S1L5Joint);
        this.joints.Add(MJointType.T12L1Joint);
        this.joints.Add(MJointType.T1T2Joint);
        this.joints.Add(MJointType.C4C5Joint);
        this.joints.Add(MJointType.HeadJoint);
        this.joints.Add(MJointType.HeadTip);

        this.joints.Add(MJointType.LeftKnee);
        this.joints.Add(MJointType.LeftAnkle);
        this.joints.Add(MJointType.LeftBall);
        this.joints.Add(MJointType.LeftBallTip);

        this.joints.Add(MJointType.RightKnee);
        this.joints.Add(MJointType.RightAnkle);
        this.joints.Add(MJointType.RightBall);
        this.joints.Add(MJointType.RightBallTip);


        this.joints.Add(MJointType.LeftElbow);
        this.joints.Add(MJointType.LeftThumbMid);
        this.joints.Add(MJointType.LeftThumbMeta);
        this.joints.Add(MJointType.LeftThumbCarpal);
        this.joints.Add(MJointType.LeftThumbTip);

        this.joints.Add(MJointType.LeftIndexProximal);
        this.joints.Add(MJointType.LeftIndexMeta);
        this.joints.Add(MJointType.LeftIndexDistal);
        this.joints.Add(MJointType.LeftIndexTip);

        this.joints.Add(MJointType.LeftMiddleProximal);
        this.joints.Add(MJointType.LeftMiddleMeta);
        this.joints.Add(MJointType.LeftMiddleDistal);
        this.joints.Add(MJointType.LeftMiddleTip);

        this.joints.Add(MJointType.LeftRingProximal);
        this.joints.Add(MJointType.LeftRingMeta);
        this.joints.Add(MJointType.LeftRingDistal);
        this.joints.Add(MJointType.LeftRingTip);

        this.joints.Add(MJointType.LeftLittleProximal);
        this.joints.Add(MJointType.LeftLittleMeta);
        this.joints.Add(MJointType.LeftLittleDistal);
        this.joints.Add(MJointType.LeftLittleTip);

        this.joints.Add(MJointType.RightElbow);
        this.joints.Add(MJointType.RightThumbMid);
        this.joints.Add(MJointType.RightThumbMeta);
        this.joints.Add(MJointType.RightThumbCarpal);
        this.joints.Add(MJointType.RightThumbTip);

        this.joints.Add(MJointType.RightIndexProximal);
        this.joints.Add(MJointType.RightIndexMeta);
        this.joints.Add(MJointType.RightIndexDistal);
        this.joints.Add(MJointType.RightIndexTip);

        this.joints.Add(MJointType.RightMiddleProximal);
        this.joints.Add(MJointType.RightMiddleMeta);
        this.joints.Add(MJointType.RightMiddleDistal);
        this.joints.Add(MJointType.RightMiddleTip);

        this.joints.Add(MJointType.RightRingProximal);
        this.joints.Add(MJointType.RightRingMeta);
        this.joints.Add(MJointType.RightRingDistal);
        this.joints.Add(MJointType.RightRingTip);

        this.joints.Add(MJointType.RightLittleProximal);
        this.joints.Add(MJointType.RightLittleMeta);
        this.joints.Add(MJointType.RightLittleDistal);
        this.joints.Add(MJointType.RightLittleTip);

    }


}

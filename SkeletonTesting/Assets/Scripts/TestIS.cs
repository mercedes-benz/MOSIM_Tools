using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMICSharp.Adapter;
using MMICSharp.Clients;
using MMICSharp.Common.Communication;
using MMICSharp.Common.Tools;
using MMIStandard;
using MMIUnity;

using Newtonsoft.Json;

using System.Linq;

namespace MMIUnity
{

    public static class ExportExtensions
    {

        public static string SerializeMJoint(this MJoint j)
        {
            string s = "";
            /*
            Dictionary<string, string> stringvals = new Dictionary<string, string>();
            Dictionary<string, double[]> floatvals = new Dictionary<string, double[]>();
            Dictionary<string, List<string>> channels = new Dictionary<string, List<string>>();
            stringvals.Add("Name", j.ID);
            stringvals.Add("Type", j.Type.ToString());
            stringvals.Add("Parent", j.Parent);
            floatvals.Add("Position", new double[] { j.Position.X, j.Position.Y, j.Position.Z });
            floatvals.Add("Position", new double[] { j.Rotation.X, j.Rotation.Y, j.Rotation.Z, j.Rotation.W });
            channels.Add("Channels", new List<string>());
            foreach(MChannel c in j.Channels)
            {
                channels["Channels"].Add(c.ToString());
            }
            */

            s += JsonConvert.SerializeObject(j);
            return s;
        }
    }
    public class TestIS : UnityAvatarBase
    {
        Dictionary<string, Vector3> lastPos = new Dictionary<string, Vector3>();
        Dictionary<string, Quaternion> lastRot = new Dictionary<string, Quaternion>();
        MAvatarPostureValues posture;
        MAvatarPostureValues zeroPosture;

        public JointPairsReaderFromMapping pairsREader;
        public MirroredJointsLoader mjl;


        public JointMapUI jointMapUI;

        //public string ConfigurationFilePath = "configurations/avatar.mos";


        public bool Avatar2IS2Avatar = false;


        public bool Avatar2IS = false;
        public bool IS2Avatar = false;

        public bool start = false;
        private bool started = false;

        private bool playMOSIMAnimation = false;

        public bool IsStarted()
        {
            return started;
        }

        public JointMap jointMap { get; set; } = new JointMap();

        private int tryID = 0;

        private MAvatarPosture TestPostures_Zero;

        List<double[]> frames = new List<double[]>();
        int frame_counter = 0;


        public void ToggleAvatar2ISRetargeting(bool newVal)
        {
            this.Avatar2IS = newVal;
        }

        public void ResetBasePosture()
        {
            //Debug.Log("ResetBasePosture ");

            zeroPosture = this.skelVis.GetZeroPosture();
            if (posture == null)
            {
                posture = new MAvatarPostureValues(zeroPosture.AvatarID, zeroPosture.PostureData);
            }
            posture.PostureData = zeroPosture.PostureData;
            this.AssignPostureValues(zeroPosture);

        }

        public void SaveConfig(string filename, Dictionary<string, Quaternion> base_rotations)
        {
            MAvatarPosture p = this.GenerateGlobalPosture();
            if (!this.started)
            {
                return;
            }
            if (base_rotations != null)
            {
                for (int i = 0; i < p.Joints.Count; i++)
                {
                    string name = p.Joints[i].ID;
                    if (base_rotations.ContainsKey(name))
                    {
                        Quaternion rotation = p.Joints[i].Rotation.ToQuaternion();
                        Quaternion brot = base_rotations[name];
                        rotation = rotation * Quaternion.Inverse(brot);
                        p.Joints[i].Rotation = rotation.ToMQuaternion();
                    }


                }
            }

            string s = JsonConvert.SerializeObject(p, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include,

                //DefaultValueHandling = DefaultValueHandling.Ignore
            });
            //Debug.Log(s);
            //MAvatarPosture p = JsonConvert.DeserializeObject<MAvatarPosture>(s);



            //Debug.Log(JsonConvert.SerializeObject(p, Formatting.Indented));
            System.IO.File.WriteAllText(filename, s);
        }

        public void SaveConfig()
        {
            this.SaveConfig(this.ConfigurationFilePath, null);
        }
        public void SetConfigurationFilePath(UnityEngine.UI.InputField path)
        {
            this.ConfigurationFilePath = path.text;
        }


        public void LoadConfig()
        {
            this.LoadConfig(this.ConfigurationFilePath);
            jointMapUI.jointMap = this.jointMap;
        }

        public void PlayExampleClip()
        {
            Debug.Log("Start playing MOSIM example");
            string[] lines = System.IO.File.ReadAllLines("example.mos");
            frames.Clear();
            bool motion = false;
            foreach (string s in lines)
            {
                if (motion && s.Length > 10)
                {
                    string[] splits = s.Trim().Split(' ');
                    double[] vals = new double[(int)(splits.Length)];
                    for (int i = 0; i < vals.Length; i++)
                    {
                        vals[i] = double.Parse(splits[i]);
                    }
                    frames.Add(vals);
                }
                if (s == "MOTION")
                {
                    motion = true;
                }
            }

            this.Avatar2IS = false;
            this.IS2Avatar = true;
            this.playMOSIMAnimation = !this.playMOSIMAnimation;
            this.frame_counter = 0;
        }

        public void LoadConfig(string filename)
        {
            string s = System.IO.File.ReadAllText(filename);
            MAvatarPosture p = JsonConvert.DeserializeObject<MAvatarPosture>(s);
            string name = "lalala" + tryID;
            this.AvatarID = name;
            p.AvatarID = this.AvatarID;
            if (this.skelVis != null)
            {
                this.skelVis.root.Destroy();
            }
            this.SetupRetargeting(name, p);
            tryID++;
            ResetBasePosture();

            foreach (var entry in this.bonenameMap)
            {
                GameObject joint = GameObject.Find(entry.Key);
                if (joint != null)
                {
                    this.jointMap.SetTransform(entry.Value, joint.transform);
                }
            }
            this.jointMapUI.jointMap = this.jointMap;
        }

        public void RealignSkeletons()
        {
            this.skelVis.root.AlignAvatar();
        }

        public bool SetBoneMap(JointMap jointMapping)
        {
            bool change = false;
            if (jointMapping != null && jointMapping.GetJointMap() != null)
            {
                //Debug.Log("Reset Bonemap");
                System.Collections.ObjectModel.ReadOnlyDictionary<MJointType, Transform> jointmap = jointMapping.GetJointMap();
                Dictionary<string, MJointType> namemap = new Dictionary<string, MJointType>();
                foreach (KeyValuePair<MJointType, Transform> entry in jointmap)
                {
                    if (entry.Value != null)
                    {
                        namemap.Add(entry.Value.name, entry.Key);
                        if (this.bonenameMap != null && (!this.bonenameMap.ContainsKey(entry.Value.name) || this.bonenameMap[entry.Value.name] != entry.Key) || this.bonenameMap == null)
                        {
                            change = true;
                        }
                    }

                }


                if (change)
                {
                    Debug.Log("Checking for change: " + change);
                    this.bonenameMap = namemap;

                    if (!started)
                    {
                        ManagedStart();
                    } else
                    {
                        string name = "lalala" + tryID;
                        this.AvatarID = name;
                        MAvatarPosture p = this.GenerateGlobalPosture();
                        p.AvatarID = name;
                        if (this.skelVis != null)
                        {
                            this.skelVis.root.Destroy();
                        }
                        this.SetupRetargeting(name, p);
                        if (posture == null)
                            posture = new MAvatarPostureValues();
                        posture.AvatarID = p.AvatarID;
                        tryID++;
                        ResetBasePosture();
                    }


                    

                }
            }

            if(change)
            {
                if (pairsREader == null)
                {
                    pairsREader = this.gameObject.AddComponent<JointPairsReaderFromMapping>();
                }
                pairsREader.bonenameMap = bonenameMap;
                pairsREader.root = this.RootTransform;

                if (mjl == null)
                {
                    mjl = gameObject.AddComponent<MirroredJointsLoader>();
                }

                mjl.UpdateMirrors(pairsREader);

            }
            return change;

        }



        override
        protected void Start()
        {
        }

        protected void ManagedStart()
        {
            Debug.Log("Managed Start");
            base.Start();

            if (this.AvatarID == null)
            {
                this.AvatarID = "asdf" + Random.Range(0, 1000);
            }
            string id = this.AvatarID;//"asdf"+Random.Range(0, 1000);

            if (System.IO.File.Exists(this.ConfigurationFilePath))
            {
                //this.LoadConfig();

                string s = System.IO.File.ReadAllText(this.ConfigurationFilePath);
                MAvatarPosture p = JsonConvert.DeserializeObject<MAvatarPosture>(s);
                p.AvatarID = id;

                this.SetupRetargeting(id, p);
                this.AssignPostureValues(retargetingService.RetargetToIntermediate(p));
                foreach (var entry in p.Joints)
                {
                    if (entry.Type != MJointType.Undefined)
                    {
                        Transform t = this.transform.GetChildRecursiveByName(entry.ID);
                        if (t != null)
                        {
                            this.jointMap.SetTransform(entry.Type, t);
                        }
                    }
                }
                if (this.jointMapUI != null)
                {
                    this.jointMapUI.jointMap = this.jointMap;
                }


            }
            else
            {
                this.SetupRetargeting(id);
            }
            start = false;
            started = true;

        }

        public void LateUpdate()
        {
            if (start)
            {
                ManagedStart();
            }
            else if (started)
            {

                if (posture == null)
                {
                    posture = this.GetRetargetedPosture();
                    this.AssignPostureValues(posture);
                    if (this.skelVis != null)
                    {
                        posture.PostureData = this.skelVis.GetRetargetedPostureValues();
                        this.AssignPostureValues(posture);
                    }

                }
                else
                {
                    if (Avatar2IS2Avatar)
                    {
                        Avatar2IS = IS2Avatar = false;
                        InitializePositions(this.RootBone);
                        posture = this.GetRetargetedPosture();
                        this.AssignPostureValues(posture);
                        float error = PositionalError(this.RootBone);
                        MAvatarPostureValues p2 = this.GetRetargetedPosture();
                        this.AssignPostureValues(p2);
                        float error2 = PositionalError(this.RootBone);
                        float diff = 0.0f;
                        for (int i = 0; i < p2.PostureData.Count; i++)
                        {
                            diff += Mathf.Abs((float)(p2.PostureData[i] - posture.PostureData[i]));
                        }
                        Debug.Log("bi-directional error: " + error + " " + diff + " " + error2);
                    }
                    else
                    {
                        if (Avatar2IS)
                        {
                            // Forward = false;
                            posture = this.GetRetargetedPosture();
                        }
                        else if (IS2Avatar)
                        {
                            // Backwards = false;
                            if (playMOSIMAnimation && this.frames.Count > 0 && this.frame_counter < this.frames.Count)
                            {
                                posture.PostureData = new List<double>(this.frames[this.frame_counter]);
                                this.frame_counter += 1;
                            }
                            else
                            {
                                posture.PostureData = this.skelVis.GetRetargetedPostureValues();
                            }

                            this.AssignPostureValues(posture);
                        }
                    }
                }
            }
        }

        private float PositionalError(Transform t)
        {
            float res = 0.0f;
            if (t.name.Contains("Mesh"))
            {
                return 0.0f;
            }
            res = (t.position - lastPos[t.name]).magnitude;

            res += (t.rotation.eulerAngles - lastRot[t.name].eulerAngles).magnitude;
            if (res > 0.001)
            {
                Debug.Log("Positional Error: " + t.name + " " + res);
            }

            for (int i = 0; i < t.childCount; i++)
            {
                float p = PositionalError(t.GetChild(i));
                res += p;
            }
            return res;
        }

        private void InitializePositions(Transform t)
        {
            this.lastPos[t.name] = new Vector3(t.position.x, t.position.y, t.position.z);
            this.lastRot[t.name] = new Quaternion(t.rotation.x, t.rotation.y, t.rotation.z, t.rotation.w);
            for (int i = 0; i < t.childCount; i++)
            {
                InitializePositions(t.GetChild(i));
            }
        }

    }
}

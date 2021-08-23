using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMICSharp.Adapter;
using MMICSharp.Clients;
using MMICSharp.Common.Communication;
using MMICSharp.Common.Tools;
using MMIStandard;
using MMIUnity;

using MMICSharp.Common.Communication;
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

            s = Serialization.ToJsonString<MJoint>(j); //JsonConvert.SerializeObject(j);
            return s;
        }
    }
    public class TestIS : UnityAvatarBase
    {
        Dictionary<string, Vector3> lastPos = new Dictionary<string, Vector3>();
        Dictionary<string, Quaternion> lastRot = new Dictionary<string, Quaternion>();
        MAvatarPostureValues posture;
        MAvatarPostureValues zeroPosture;
        private MAvatarPosture initialPosture;

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

            string s = Serialization.ToJsonString<MAvatarPosture>(p);
            /*
            string s = JsonConvert.SerializeObject(p, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include,

                //DefaultValueHandling = DefaultValueHandling.Ignore
            });
            */
            //Debug.Log(s);
            //MAvatarPosture p = JsonConvert.DeserializeObject<MAvatarPosture>(s);

            //Debug.Log(JsonConvert.SerializeObject(p, Formatting.Indented));
            System.IO.File.WriteAllText(filename, s);

            string initialP = Serialization.ToJsonString<MAvatarPosture>(this.initialPosture);
            System.IO.File.WriteAllText(filename + "_initial", initialP);
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
        }

        public void PlayExampleClip()
        {
            Debug.Log("Start playing MOSIM example");
            string[] lines = System.IO.File.ReadAllLines("Assets/Samples/ExampleClips/example.mos");
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
                    // TODO: Remove this fix with new retargeted example data
                    //List<double> newValues = new List<double>() { vals[0], 0, vals[2], 1, 0, 0, 0 };
                    //vals[0] = 0;
                    //vals[2] = 0;
                    //newValues.AddRange(vals);
                    frames.Add(vals.ToArray());    
                    //frames.Add(vals);
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
            MAvatarPosture p = Serialization.FromJsonString<MAvatarPosture>(s);//JsonConvert.DeserializeObject<MAvatarPosture>(s);
            string name = "lalala" + tryID;
            this.AvatarID = name;
            p.AvatarID = this.AvatarID;
            if (this.skelVis != null)
            {
                this.skelVis.root.Destroy();
            }
            if(p.Joints[0].ID != "_VirtualRoot")
            {
                this.UseVirtualRoot = false;
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
            this.started = true;
        }

        public void RealignSkeletons()
        {
            
            this.skelVis.AlignAvatar();
        }
        
        public void ResetBoneMap2()
        {
            this.bonenameMap = this.GetComponent<JointMapper2>().GetJointMap();
            if(!started)
            {
                ManagedStart();
            } else
            {
                this.AvatarID = name;
                MAvatarPosture p = this.GenerateGlobalPosture(); 
                p.AvatarID = name;
                if (this.skelVis != null)
                {
                    this.skelVis.root.Destroy();
                }
                this.SetupRetargeting(name, p);
                if(this.skelVis.root.reference == null)
                {
                    this.skelVis.root.reference = this.transform;
                }
                if (posture == null)
                    posture = new MAvatarPostureValues();
                posture.AvatarID = p.AvatarID;
                tryID++;
                ResetBasePosture();
            }
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
                        if (this.skelVis.root.reference == null)
                        {
                            this.skelVis.root.reference = this.transform;
                        }

                        if (posture == null)
                            posture = new MAvatarPostureValues();
                        posture.AvatarID = p.AvatarID;
                        tryID++;
                        ResetBasePosture();
                    }


                    

                }
            }

            return change;

        }
         
        public override MAvatarPosture SetupRetargeting(string id)
        {
            MAvatarPosture ret = base.SetupRetargeting(id);
            this.skelVis.alignment = new JointAlignment();
            return ret;
        }

        public override MAvatarPosture SetupRetargeting(string id, MAvatarPosture reference)
        {
            MAvatarPosture ret = base.SetupRetargeting(id, reference);
            this.skelVis.alignment = new JointAlignment();
            return ret;
        }

        override
        protected void Start()
        {
            this.initialPosture = GenerateGlobalPosture();
            //this.UseVirtualRoot = false;
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

            if (false && System.IO.File.Exists(this.ConfigurationFilePath))
            {
                //this.LoadConfig();
                
                MAvatarPosture p = this.SetupRetargeting(id);
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

            }
            else
            {
                this.SetupRetargeting(id);
            }
            if (this.skelVis.root.reference == null)
            {
                this.skelVis.root.reference = this.transform;
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
                        InitializePositions(this.Pelvis);
                        posture = this.GetRetargetedPosture();
                        this.AssignPostureValues(posture);
                        float error = PositionalError(this.Pelvis);
                        MAvatarPostureValues p2 = this.GetRetargetedPosture();
                        this.AssignPostureValues(p2);
                        float error2 = PositionalError(this.Pelvis);
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
                            if (playMOSIMAnimation && this.frames.Count > 0 && this.frame_counter < this.frames.Count -1)
                            {
                                posture.PostureData = new List<double>(this.frames[this.frame_counter]);
                                if(Time.deltaTime > 1 / 30)
                                {
                                    this.frame_counter += 2;
                                }
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

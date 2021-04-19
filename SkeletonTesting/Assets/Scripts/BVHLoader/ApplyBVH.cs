using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

public class Joint
{
    private Vector3 offset { get; set; }
    private List<Joint> children = new List<Joint>();
    private Joint parent { get; set; }
    private Vector3 endNote { get; set; } = Vector3.zero;

    private bool translation = false;
    private string rotationOrder = "zxy";

    public string name = "";

    public Matrix4x4 transformationMatrix;
    public Quaternion globalRotation = Quaternion.identity;

    public GameObject jointObj;
   

    public void BuildHierarchy(GameObject baseObject)
    {
        //this.jointObj = GameObject.Instantiate(baseObject);
        this.jointObj = new GameObject();
        if (this.parent != null)
        {
            this.jointObj.transform.SetParent(this.parent.jointObj.transform);
            this.jointObj.transform.position = this.parent.jointObj.transform.position;
        }
        this.jointObj.transform.localScale = new Vector3(1, 1, 1);
        this.jointObj.name = this.name;
        this.jointObj.transform.name = this.name;
        this.jointObj.transform.position += new Vector3(-this.offset.x, this.offset.y, this.offset.z) / 100;
        this.jointObj.transform.rotation = new Quaternion(0, 0, 0, 1);

        JointInfo jointInfo = this.jointObj.AddComponent<JointInfo>();
        if(this.jointObj.transform.parent != null)
        {
            JointInfo parentJointInfo = this.jointObj.transform.parent.GetComponent<JointInfo>();
            if (parentJointInfo != null)
            {
                parentJointInfo.childJoints.Add(jointInfo);
            }
        }

        Quaternion rotation = new Quaternion();
        // float scale = 10f;
        if (this.children.Count > 0)
        {
            Joint target_child = this.children[0];
            float max_child_y = 0;
            if(this.children.Count > 1)
            {
                foreach(var c in this.children)
                {
                    if(Math.Abs(c.offset.y) > max_child_y)
                    {
                        target_child = c;
                        max_child_y = Math.Abs(c.offset.y);
                    }
                    //if(c.offset.y > c.offset.x && c.offset.y > c.offset.z) {
                     //   target_child = c;
                     //   break;
                    //}
                }
            }
            Vector3 upDir= new Vector3(-target_child.offset.x, target_child.offset.y, target_child.offset.z);
            rotation = Quaternion.LookRotation(Vector3.forward, upDir);
        }
        else
        {
            Vector3 endPoint = new Vector3(-endNote.x, endNote.y, endNote.z) / 100;
            rotation = Quaternion.FromToRotation(Vector3.up, endPoint);
            jointInfo.endPoints.Add(Vector3.up * endPoint.magnitude);
        }
        this.jointObj.transform.rotation = rotation;


        foreach (Joint c in this.children)
        {
            c.BuildHierarchy(baseObject);
        }
        
    }

    /// <summary>
    /// 
    /// This function creates a readable string conversion of the Joint Hierarchy. 
    /// </summary>
    /// <param name="prefix"></param>
    /// <returns></returns>
    public string ToPrettyString(string prefix = "")
    {
        string s = prefix + this.name + "; " + this.offset.ToString() + "; " + this.rotationOrder + "; " + this.endNote + "\n";

        foreach (Joint child in this.children)
        {
            s += child.ToPrettyString(prefix + "  ");
        }
        return s;
    }

    public Joint(Joint parent, string name)
    {
        this.parent = parent;
        this.name = name;
    }

    private Vector3 StringVec(string[] itemList)
    {
        string s1 = itemList[1];
        string s2 = itemList[2];
        string s3 = itemList[3];
        return new Vector3(float.Parse(s1), float.Parse(s2), float.Parse(s3));
    }

    /// <summary>
    /// This function parses the hierarchy of the BVH File and create the respective Joint Hierarchy. 
    /// </summary>
    /// <param name="text"></param>
    /// <param name="counter"></param>
    /// <returns></returns>
    public int parseHierarchy(string[] text, int counter)
    {
        // TODO: Implement Hierarchy!
        while (text[counter].Trim() != "MOTION")
        {
            string currentLine = text[counter];
            if (text[counter].Contains("JOINT"))
            {
                string childName = Regex.Split(text[counter].Trim(), @"\s+")[1];
                Joint child = new Joint(this, childName);
                counter = child.parseHierarchy(text, counter+1);
                this.children.Add(child);
            }
            else if (text[counter].Contains("OFFSET"))
            {
                string[] splits = Regex.Split(text[counter].Trim(), @"\s+");
                this.offset = StringVec(splits);
                counter += 1;
            }
            else if (text[counter].Contains("CHANNELS"))
            {
                string[] splits = Regex.Split(text[counter].Trim(), @"\s+");
                int start = 2;
                if (splits[1] == "6")
                {
                    this.translation = true;
                    start = 5;
                }
                this.rotationOrder = (splits[start].Replace("rotation", "") + splits[start + 1].Replace("rotation", "") + splits[start + 2].Replace("rotation", "")).ToLower();
                counter += 1;
            }
            else if (text[counter].Contains("{") || text[counter].Trim() == "")
            {
                counter += 1;
            }
            else if (text[counter].Contains("End Site"))
            {
                this.endNote = StringVec(Regex.Split(text[counter + 2].Trim(), @"\s+"));
                counter += 4;
            }
            else if (text[counter].Contains("}"))
            {
                return counter + 1;
            }
        }
        return counter+1;
    }

    private Quaternion GetRotation(Quaternion rot, float[] rotations, string order, int id)
    {
        
        
        

        if (id == order.Length)
        {
            rot.x = -rot.x;
            rot.w = -rot.w;
            return rot;
        }
        if(order[id] == 'x')
        {
            Quaternion qx = Quaternion.AngleAxis(rotations[id], Vector3.right);
            rot = rot * qx;
            //rot = qx * rot;
        } else if(order[id] == 'y')
        {
            Quaternion qy = Quaternion.AngleAxis(rotations[id], Vector3.up);
            rot = rot * qy;
            //rot = qy * rot;
        } else
        {
            Quaternion qz = Quaternion.AngleAxis(rotations[id], Vector3.forward);
            rot = rot * qz;
            //rot = qz * rot;
        }
        // order = order.Remove(0, 1);
        return GetRotation(rot, rotations, order, id+1);
    }

    /// <summary>
    /// This function parses a single motion frame (line of rotations in BVH file).
    /// </summary>
    /// <param name="frame"></param>
    /// <param name="counter"></param>
    /// <returns></returns>
    public int ParseMotionFrame(string[] frame, int counter)
    {
        /* Tips:
            * Axis Mapping: Z -> Vector3.forward; Y -> Vector3.up; X -> Vector3.right
            * Translation: Translation along X axis has to be negated (Unity.x = - BVH.x)
            * Rotation: Rotation around Forward and Up axis has to be negated. 

            * BVH File Format: https://research.cs.wisc.edu/graphics/Courses/cs-838-1999/Jeff/BVH.html
        */

        // TODO: Implement motion parser!

        Vector3 trans = new Vector3(-this.offset.x, this.offset.y, this.offset.z);
        if (this.translation)
        {
            string[] stringvec = new string[] { "", frame[counter], frame[counter + 1], frame[counter + 2] };
            Vector3 trans2 = StringVec(stringvec);
            counter += 3;
            trans2.x = -trans2.x;
            trans = trans2;
        }

        float rot1 = float.Parse(frame[counter]);
        float rot2 = float.Parse(frame[counter + 1]);
        float rot3 = float.Parse(frame[counter + 2]);


        counter += 3;

        Quaternion rot = Quaternion.identity;
        rot = GetRotation(rot, new float[] { rot1, rot2, rot3 }, this.rotationOrder, 0);

        this.transformationMatrix = Matrix4x4.TRS(trans, rot, new Vector3(1, 1, 1));

        if (this.parent != null)
        {
            this.transformationMatrix = this.parent.transformationMatrix * this.transformationMatrix;
        }

        foreach (Joint child in this.children)
        {
            counter = child.ParseMotionFrame(frame, counter);
        }
        return counter;
    }

    /// <summary>
    /// Return the global Position of this joint. 
    /// </summary>
    /// <returns></returns>
    public Vector3 GetJointPosition()
    {
        // TODO: Return Global Joint Position
        return this.transformationMatrix.GetColumn(3) / 100;
    }


    /// <summary>
    /// Return the global Rotation of this joint. 
    /// </summary>
    /// <returns></returns>
    public Quaternion GetJointRotation()
    {
        // TODO: Return Global Joint ROTATION
        return this.transformationMatrix.rotation;
    }
    

    /// <summary>
    /// Return Joint by name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public Joint GetByName(string name)
    {
        if(this.name == name)
        {
            return this;
        }
        foreach(Joint child in this.children)
        {
            Joint j = child.GetByName(name);
            if(j != null)
            {
                return j;
            }
        }
        return null;
    }
}


public class ApplyBVH : MonoBehaviour
{
    // Start is called before the first frame update

    public bool start = false;
    private Joint root;
    public int motionFrame = 0;

    private List<string> lines;
    public string file;

    // Required only in Root to store the characters base rotations. 
    public Dictionary<string, Quaternion> base_rotations = new Dictionary<string, Quaternion>();
    private bool isInit = false;

    public GameObject baseObject;

    public Joint Root
    {
        get {return root;}
    }
    public bool Isinit
    {
        get {return isInit;}
    }
    public GameObject Init(string path)
    {
        this.file = path;
        Debug.Log("Files: " + file);
        
        if (!File.Exists(path))
        {
            Debug.Log("File not existing!");
        } else
        {
            Debug.Log("File Existing");
            // Read all the lines
            List<string> lines = new List<string>();
            using (StreamReader sr = File.OpenText(path))
            {
                
                string s = "";
                while((s = sr.ReadLine()) != null)
                {
                    lines.Add(s);
                }
                Debug.Log("Read lines " +  (lines.Count).ToString());
            }

            // Parse the Hierarchy 
            string name = Regex.Split(lines[1].Trim(), @"\s+")[1];
            this.root = new Joint(null, name);
            int counter = root.parseHierarchy(lines.ToArray(), 2);

            while (Regex.Split(lines[counter].Trim(), @"\s+").Length < 15)
            {
               counter += 1;
            }

            root.BuildHierarchy(baseObject);

            // Parse the motion
            //this.root.ParseMotionFrame(Regex.Split(lines[counter], @"\s+"), 0);
            //this.ApplyToTransform(this.root.jointObj.transform);

            motionFrame = counter + 1;
            this.lines = lines;
            ComputeBaseRotations(this.root.jointObj.transform);
            isInit = true;
        }
        this.root.ParseMotionFrame(Regex.Split(this.lines[motionFrame], @"\s+"), 0);//.Split(' ')
        // Apply motion to transform hierarchy.
        this.ApplyToTransform(this.root.jointObj.transform);
        return this.root.jointObj;
    }

    /// <summary>
    /// We have to use LateUpdate to properly transfer the motion to unities Transform hierarchy. 
    /// </summary>
    private void LateUpdate()
    {
        if (start && isInit)
        {
            // select next frame. 
            if (motionFrame >= this.lines.Count)
            {
                this.Init(file);
            }
            // Parse the motion frame
            this.root.ParseMotionFrame(Regex.Split(this.lines[motionFrame], @"\s+"), 0);
            motionFrame += 1;
            // Apply motion to transform hierarchy.
            this.ApplyToTransform(this.root.jointObj.transform);
            // ApplyToTransform1();
        }

    }

    public void ComputeBaseRotations(Transform t)
    {
        Joint j = this.root.GetByName(t.name);
        if (j != null)
        {
            if (!this.base_rotations.ContainsKey(t.name))
            {
                this.base_rotations.Add(t.name, new Quaternion(t.rotation.x, t.rotation.y, t.rotation.z, t.rotation.w));
            }
        }
        for (int id = 0; id < t.childCount; id++)
        {
            if (!t.GetChild(id).name.Contains("_end"))
            {
                ComputeBaseRotations(t.GetChild(id));
            }
        }
    }


    public void ApplyToTransform(Transform t)
    {
        Joint j = this.root.GetByName(t.name);
        if(j != null)
        {
            t.position = j.GetJointPosition();
            Quaternion r = j.GetJointRotation();

            // multiply joint rotation with base rotation in zero-posture. This is a Unity specfic requirement. 
            if(this.base_rotations.ContainsKey(t.name))
            {
                t.rotation = r * this.base_rotations[t.name];
            } else
            {
                t.rotation = r;
            }
            
        }
        for(int id = 0; id < t.childCount; id++)
        {
            if (!t.GetChild(id).name.Contains("_end"))
            {
                ApplyToTransform(t.GetChild(id));
            }
        }
    }

}

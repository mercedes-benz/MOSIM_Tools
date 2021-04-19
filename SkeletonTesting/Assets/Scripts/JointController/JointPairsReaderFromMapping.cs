using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MMIStandard;
using MMIUnity;

public class JointPairsReaderFromMapping : JointPairsReader
{

    public Dictionary<string, MJointType> bonenameMap;
    public Transform root;

    private MJointType getPartner(MJointType type)
    {
        string name = type.ToString();
        MJointType result = MJointType.Undefined;
        if (name.Contains("Left"))
        {
            string newname = name.Replace("Left", "Right");
            Enum.TryParse<MJointType>(newname, out result);
        } else if(name.Contains("Right"))
        {
            string newname = name.Replace("Right", "Left");
            Enum.TryParse<MJointType>(newname, out result);
        }
        return result;
    }
    public override Tuple<Transform, Transform>[] ReadJointPairs()
    {
        List<Tuple<Transform, Transform>> result = new List<Tuple<Transform, Transform>>();
        if(bonenameMap == null)
        {
            this.bonenameMap = GetComponent<TestIS>().bonenameMap;
        }
        if (bonenameMap != null)
        {
            Dictionary<MJointType, string> reverseMap = bonenameMap.Invert();
            foreach (KeyValuePair<MJointType, string> entry in reverseMap)
            {
                Transform t1, t2;
                MJointType partner = getPartner(entry.Key);
                if(partner != MJointType.Undefined && reverseMap.ContainsKey(partner))
                {
                    t1 = root.GetChildRecursiveByName(entry.Value);
                    t2 = root.GetChildRecursiveByName(reverseMap[partner]);
                    result.Add(new Tuple<Transform, Transform>(t1, t2));
                }
                
                //Debug.Log(entry.Key.ToString());
            }
        } else
        {
            Debug.Log("Bonenamap not set");
        }
        return result.ToArray();
    }
}


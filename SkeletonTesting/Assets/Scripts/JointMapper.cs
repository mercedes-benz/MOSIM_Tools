using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.SceneManagement;
using MMIStandard;

public class JointMapper
{
    public JointMap jointMap {get; private set;}
    public Vector3 right {get; private set;} = Vector3.right;
    private Vector3 forward = Vector3.forward;

    // public JointMapper(Transform root=null)
    // {
    //     jointMap = new JointMap();
    //     ReadOnlyDictionary<MJointType, Transform> jm = jointMap.GetJointMap();
    //     JointInfo pelvis = null;
    //     bool isPelvisFound = false;
    //     (MJointType, MJointType)[] legJointTypePairsArray =
    //         {
    //             (MJointType.LeftHip, MJointType.RightHip),
    //             (MJointType.LeftKnee, MJointType.RightKnee),
    //             (MJointType.LeftAnkle, MJointType.RightAnkle),
    //             (MJointType.LeftBall, MJointType.RightBall),
    //             (MJointType.LeftBallTip, MJointType.RightBallTip)
    //         };
    //     (MJointType, MJointType)[] armJointPairsArray =
    //         {
    //             (MJointType.LeftShoulder, MJointType.RightShoulder),
    //             (MJointType.LeftElbow, MJointType.RightElbow),
    //             (MJointType.LeftWrist, MJointType.RightWrist)
    //         };
    //     // Find PelvisCentre
    //     if(root == null)
    //         root = FindRootJoint();
    //     if(root != null)
    //     {
    //         jointMap.SetTransform(MJointType.PelvisCentre, root);
    //         pelvis = root.GetComponent<JointInfo>();
    //         isPelvisFound = true;
    //     }
    //     if(isPelvisFound)
    //     {
    //         if (FindLegs(pelvis))
    //         {
    //             // set the right direction
    //             this.right = GetRightDir(jm[MJointType.LeftAnkle].GetComponent<JointInfo>());
    //             foreach(var jointTypePair in legJointTypePairsArray)
    //             {
    //                 LeftRight(jm[jointTypePair.Item1], jm[jointTypePair.Item2], this.right);
    //             }
    //             JointInfo leftHip = jm[MJointType.LeftHip].GetComponent<JointInfo>();
    //             JointInfo rightHip = jm[MJointType.RightHip].GetComponent<JointInfo>();
    //             if(FindFirstSpine(pelvis, leftHip, rightHip))  // set MJointType.S1L5Joint
    //             {
    //                 JointInfo firstSpine = jm[MJointType.S1L5Joint].GetComponent<JointInfo>();
    //                 if(FindSpineAndChest(firstSpine))
    //                 {
    //                     JointInfo chest = jm[MJointType.T1T2Joint].GetComponent<JointInfo>();
    //                     if(FindArmsAndNeck(chest))
    //                     {
    //                         foreach (var jointTypePair in armJointPairsArray)
    //                         {
    //                             LeftRight(jm[jointTypePair.Item1], jm[jointTypePair.Item2], this.right);
    //                         }
    //                         JointInfo leftWrist = jm[MJointType.LeftWrist].GetComponent<JointInfo>();
    //                         JointInfo rightWrist = jm[MJointType.RightWrist].GetComponent<JointInfo>();
    //                         JointInfo neck = jm[MJointType.C4C5Joint].GetComponent<JointInfo>();
    //                         FindFingers(leftWrist, rightWrist);
    //                         FindHead(neck);
    //                     }
    //                 }
    //             }
    //         }
    //     }
    // }

    public bool BuildJointMap(Transform root=null)
    {
        jointMap = new JointMap();
        ReadOnlyDictionary<MJointType, Transform> jm = jointMap.GetJointMap();
        JointInfo pelvis = null;
        bool isPelvisFound = false;
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
        // Find PelvisCentre
        if(root == null)
            root = FindRootJoint();
        if(root != null)
        {
            jointMap.SetTransform(MJointType.PelvisCentre, root);
            pelvis = root.GetComponent<JointInfo>();
            isPelvisFound = true;
        }
        if(isPelvisFound)
        {
            if (FindLegs(pelvis))
            {
                // set the right direction
                this.right = GetRightDir(jm[MJointType.LeftAnkle].GetComponent<JointInfo>());
                foreach(var jointTypePair in legJointTypePairsArray)
                {
                    LeftRight(jm[jointTypePair.Item1], jm[jointTypePair.Item2], this.right);
                }
                JointInfo leftHip = jm[MJointType.LeftHip].GetComponent<JointInfo>();
                JointInfo rightHip = jm[MJointType.RightHip].GetComponent<JointInfo>();
                if(FindFirstSpine(pelvis, leftHip, rightHip))  // set MJointType.S1L5Joint
                {
                    JointInfo firstSpine = jm[MJointType.S1L5Joint].GetComponent<JointInfo>();
                    if(FindSpineAndChest(firstSpine))
                    {
                        JointInfo chest = jm[MJointType.T1T2Joint].GetComponent<JointInfo>();
                        if(FindArmsAndNeck(chest))
                        {
                            foreach (var jointTypePair in armJointPairsArray)
                            {
                                LeftRight(jm[jointTypePair.Item1], jm[jointTypePair.Item2], this.right);
                            }
                            JointInfo leftWrist = jm[MJointType.LeftWrist].GetComponent<JointInfo>();
                            JointInfo rightWrist = jm[MJointType.RightWrist].GetComponent<JointInfo>();
                            JointInfo neck = jm[MJointType.C4C5Joint].GetComponent<JointInfo>();
                            FindFingers(leftWrist, rightWrist);
                            FindHead(neck);
                        }
                        else
                        {
                            Debug.Log("No Arms");
                            return false;
                        }
                    }
                    else
                    {
                        Debug.Log("No chest");
                        return false;
                    }
                }
                else
                {
                    Debug.Log("NO spine");
                    return false;
                }
            }
            else
            {
                Debug.Log("No legs");
                return false;
            }
        }
        else
        {
            Debug.Log("No Root");
            return false;
        }
        return true;
    }

    
    public static Transform FindRootJoint()
    {
        Scene scene = SceneManager.GetActiveScene();
        GameObject[] rootGameObjects = scene.GetRootGameObjects();
        Transform rootJoint = null;
        foreach(var go in rootGameObjects)
        {
            rootJoint = FindRootJoint(go.transform);
            if(rootJoint != null)
                return rootJoint;
        }
        return rootJoint;
    }
    public static Transform FindRootJoint(Transform transform)
    {
        if(transform.GetComponent<JointInfo>() != null && transform.GetComponent<JointInfo>().childJoints.Count == 3)
            return transform;
        foreach(Transform child in transform)
        {
            Transform rootJoint = FindRootJoint(child);
            if(rootJoint != null)
                return rootJoint;
        }
        return null;
    }



    private bool CheckChildCountEquality(JointInfo ji1, JointInfo ji2, int nJoints=-1, int nEndPoints=-1)
    {
        bool isEqule = (ji1.childJoints.Count == ji2.childJoints.Count && ji1.endPoints.Count == ji2.endPoints.Count);
        if(nJoints >= 0)
            isEqule = isEqule && ji1.childJoints.Count == nJoints;
        if(nEndPoints >= 0)
            isEqule = isEqule && ji2.endPoints.Count == nEndPoints;
        return isEqule;
    }

    public static bool NearEqual(float a, float b, float epsilon=0.0001F)
    {
        const float min = 1e-40F;
        float absA = Math.Abs(a);
        float absB = Math.Abs(b);
        float diff = Math.Abs(a - b);
        if(a.Equals(b))
            return true;
        if(a == 0 || b == 0 || absA + absB < min)
            return diff < min * epsilon;
        return diff / (absA + absB) < epsilon;
    }

    private bool LeftLeg(JointInfo joint)
    {
        if(joint.childJoints.Count > 0)
        {
            return LeftLeg(joint.childJoints[0]);
        } else
        {
            forward = joint.transform.up;
            forward.y = 0;
            Vector3 left = Vector3.Cross(forward, Vector3.up);
            Vector3 thighPos = joint.transform.position;
            thighPos.y = 0;
            float angle = Vector3.Angle(left, thighPos);
            return Math.Abs(angle) < 90;
        }
    }

    private bool LeftArm(JointInfo info)
    {
        Vector3 left = Vector3.Cross(forward, Vector3.up);
        Vector3 armPos = info.transform.position;
        armPos.y = 0;
        float angle = Vector3.Angle(left, armPos);
        return Math.Abs(angle) < 90;
    }

    private bool FindLegs(JointInfo pelvis)
    {
        JointInfo thigh1 = null, thigh2 = null;
        JointInfo calf1 = null, calf2 = null;
        JointInfo foot1 =null, foot2 = null;
        JointInfo ball1 = null, ball2 = null;
        float bone1_length, bone2_length;
        bool found = false;

        

        if(pelvis.childJoints.Count != 3)
            return found;

        
        for (int i = 0; i < pelvis.childJoints.Count; ++i)
        {
            if(pelvis.childJoints[i].childJoints[0].transform.position.y <= pelvis.transform.position.y)
            {
                if(thigh1 == null)
                {
                    thigh1 = pelvis.childJoints[i];
                } else
                {
                    thigh2 = pelvis.childJoints[i];
                }
            }
        }
        if(! LeftLeg(thigh1))
        {
            JointInfo thigh3 = thigh1;
            thigh1 = thigh2;
            thigh2 = thigh3;
        }

        calf1 = thigh1.childJoints[0];
        calf2 = thigh2.childJoints[0];
        foot1 = calf1.childJoints[0];
        foot2 = calf2.childJoints[0];
        if(foot1.childJoints.Count > 0)
        {
            ball1 = foot1.childJoints[0];
            ball2 = foot2.childJoints[0];
        }
        found = true;

        /*
        for (int i = 0; i < pelvis.childJoints.Count; ++i)
       {
            for(int j = i+1; j < pelvis.childJoints.Count; ++j)
            {
                thigh1 = pelvis.childJoints[i];
                thigh2 = pelvis.childJoints[j];
                bone1_length = Vector3.Distance(pelvis.transform.position, thigh1.transform.position);
                bone2_length = Vector3.Distance(pelvis.transform.position, thigh2.transform.position);
                if (CheckChildCountEquality(thigh1, thigh2, 1, 0) && NearEqual(bone1_length, bone2_length))
                {
                    calf1 = thigh1.childJoints[0];
                    calf2 = thigh2.childJoints[0];
                    bone1_length = Vector3.Distance(thigh1.transform.position, calf1.transform.position);
                    bone2_length = Vector3.Distance(thigh2.transform.position, calf2.transform.position);
                    if (CheckChildCountEquality(calf1, calf2, 1, 0) && NearEqual(bone1_length ,bone2_length))
                    {
                        foot1 = calf1.childJoints[0];
                        foot2 = calf2.childJoints[0];
                        bone1_length = Vector3.Distance(calf1.transform.position, foot1.transform.position);
                        bone2_length = Vector3.Distance(calf2.transform.position, foot2.transform.position);
                        if (CheckChildCountEquality(foot1, foot2) && NearEqual(bone1_length ,bone2_length))
                        {
                            if (foot1.childJoints.Count > 0)
                            {
                                ball1 = foot1.childJoints[0];
                                ball2 = foot2.childJoints[0];
                                bone1_length = Vector3.Distance(foot1.transform.position, ball1.transform.position);
                                bone2_length = Vector3.Distance(foot2.transform.position, ball2.transform.position);
                            }
                            found = true;
                            break;
                        }
                    }
                }
            }
        }*/
        if(found)
        {
            jointMap.SetTransform(MJointType.LeftHip, thigh1.transform);
            jointMap.SetTransform(MJointType.RightHip, thigh2.transform);
            jointMap.SetTransform(MJointType.LeftKnee, calf1.transform);
            jointMap.SetTransform(MJointType.RightKnee, calf2.transform);
            jointMap.SetTransform(MJointType.LeftAnkle, foot1.transform);
            jointMap.SetTransform(MJointType.RightAnkle, foot2.transform);

            if(ball1 != null && ball2 != null)
            {
                jointMap.SetTransform(MJointType.LeftBall, ball1.transform);
                jointMap.SetTransform(MJointType.RightBall, ball2.transform);
            }
        }
        return found;
    }

    private Vector3 GetRightDir(JointInfo ankle)
    {
        if(ankle == null)
            return Vector3.right;
        if(ankle.totalChildrenCount == 0 || ankle.transform.parent == null)
            return Vector3.right;
        Vector3 up = ankle.transform.parent.position - ankle.transform.position;
        Vector3 forward;
        if(ankle.childJoints.Count > 0)
            forward = ankle.childJoints[0].transform.position - ankle.transform.position;
        else
            forward = ankle.endPoints[0];
        return Vector3.Cross(up, forward).normalized;
    }

    private void LeftRight(Transform t1, Transform t2, Vector3 right)
    {
        Transform tmp;
        if(t1 && t2 && Vector3.Dot((t2.position - t1.position), right) < 0)
        {
            tmp = t1;
            t1 = t2;
            t2 = tmp;
        }
    }

    private bool FindFirstSpine(JointInfo pelvis, JointInfo leftHip, JointInfo rightHip)
    {
        if(pelvis.childJoints.Count != 3)
            return false;
        foreach(JointInfo jointInfo in pelvis.childJoints)
        {
            if(jointInfo != leftHip && jointInfo != rightHip)
            {
                jointMap.SetTransform(MJointType.S1L5Joint, jointInfo.transform);
                return true;
            }
        }
        return false;
    }

    private bool FindSpineAndChest(JointInfo firstSpine)
    {
        MJointType[] spineList = {MJointType.S1L5Joint, MJointType.T12L1Joint};
        JointInfo spine = firstSpine;
        int i = 0;
        while(spine.childJoints.Count == 1)
        {
            if(i < spineList.Length)
                jointMap.SetTransform(spineList[i], spine.transform);
            spine = spine.childJoints[0];
            ++i;
        }
        if(spine.childJoints.Count == 3)
        {
            int withChildren = 0;
            JointInfo next = null;
            foreach(JointInfo info in spine.childJoints)
            {
                if(info.childJoints.Count > 0)
                {
                    withChildren++;
                    next = info;
                }
            }
            if(withChildren < 3)
            {
                return FindSpineAndChest(next);
            }
            jointMap.SetTransform(MJointType.T1T2Joint, spine.transform);
            return true;
        }
        return false;
    }

    private void GetArm(JointInfo shoulder, List<JointInfo> arm)
    {
        arm.Add(shoulder);
        if(shoulder.childJoints.Count > 1)
        {
            return;
        }
        foreach(JointInfo child in shoulder.childJoints)
        {
            GetArm(child, arm);
        }
    }

    private bool FindArmsAndNeck(JointInfo chest)
    {
        bool isFound = false;
        int neckIndex = 0;
        (MJointType, MJointType)[] jointTypePairArray =
            {
                (MJointType.LeftWrist, MJointType.RightWrist),
                (MJointType.LeftElbow, MJointType.RightElbow),
                (MJointType.LeftShoulder, MJointType.RightShoulder)
            };
        List<(JointInfo, JointInfo)> jointPairList = new List<(JointInfo, JointInfo)>();
        if(chest.childJoints.Count != 3)
            return isFound;

        JointInfo leftShoulder = null, rightShoulder = null;
        int maxI = 0;
        float maxH = 0;
        for (int i = 0; i < chest.childJoints.Count; ++i)
        {
            float h = chest.childJoints[i].childJoints[0].transform.position.y;
            if (maxH == 0)
            {
                maxH = h;
            } else if(h > maxH)
            {
                maxH = h;
                maxI = i;
            }
        }
        float maxLeft = float.MinValue;
        for (int i = 0; i < chest.childJoints.Count; ++i)
        {
            if(i != maxI)
            {
                if(chest.childJoints[i].transform.position.x > maxLeft)
                {
                    maxLeft = chest.childJoints[i].transform.position.x;
                    leftShoulder = rightShoulder;
                    rightShoulder = chest.childJoints[i];
                } else
                {
                    leftShoulder = chest.childJoints[i];
                }
                //if(leftShoulder == null)
                //{
                //    leftShoulder = chest.childJoints[i];
                //} else
                //{
                //    rightShoulder = chest.childJoints[i];
                //}
            }
        }
        jointMap.SetTransform(MJointType.C4C5Joint, chest.childJoints[maxI].transform);

        /*
        if (! LeftArm(leftShoulder) )
        {
            JointInfo shoulder3 = leftShoulder;
            leftShoulder = rightShoulder;
            rightShoulder = shoulder3;
        }*/

        List<JointInfo> leftArm = new List<JointInfo>();
        List<JointInfo> rightArm = new List<JointInfo>();
        GetArm(leftShoulder, leftArm);
        GetArm(rightShoulder, rightArm);
        jointMap.SetTransform(MJointType.LeftWrist, leftArm[leftArm.Count - 1].transform);
        jointMap.SetTransform(MJointType.LeftElbow, leftArm[leftArm.Count - 2].transform);
        jointMap.SetTransform(MJointType.LeftShoulder, leftArm[leftArm.Count - 3].transform);

        jointMap.SetTransform(MJointType.RightWrist, rightArm[leftArm.Count - 1].transform);
        jointMap.SetTransform(MJointType.RightElbow, rightArm[leftArm.Count - 2].transform);
        jointMap.SetTransform(MJointType.RightShoulder, rightArm[leftArm.Count - 3].transform);

        return true;
        /*
        for (int i = 0; i < chest.childJoints.Count; ++i)
        {
            for(int j = i+1; j < chest.childJoints.Count; ++j)
            {
                jointPairList = new List<(JointInfo, JointInfo)>();
                JointInfo joint1 = chest.childJoints[i];
                JointInfo joint2 = chest.childJoints[j];
                jointPairList.Add((joint1, joint2));
                float bone1_length = Vector3.Distance(chest.transform.position, joint1.transform.position);
                float bone2_length = Vector3.Distance(chest.transform.position, joint2.transform.position);
                while(CheckChildCountEquality(joint1, joint2, 1, 0) && NearEqual(bone1_length, bone2_length))
                {
                    bone1_length = Vector3.Distance(joint1.transform.position, joint1.childJoints[0].transform.position);
                    bone2_length = Vector3.Distance(joint2.transform.position, joint2.childJoints[0].transform.position);
                    joint1 = joint1.childJoints[0];
                    joint2 = joint2.childJoints[0];
                    jointPairList.Add((joint1, joint2));
                }
                // The last element in jointList is wrist
                // at least contain shoulder, elbow and wrist
                if(jointPairList.Count >= 3 || CheckChildCountEquality(joint1, joint2))
                {
                    for(int k = 0; k < chest.childJoints.Count; ++k)
                        if(k != i && k != j)
                            neckIndex = k;
                    isFound = true;
                    break;
                }
            }
            if(isFound)
                break;
        }
        if(isFound)
        {
            int n = jointPairList.Count-1;
            for (int k = 0; k < jointTypePairArray.Length; ++k)
            {
                jointMap.SetTransform(jointTypePairArray[k].Item1, jointPairList[n - k].Item1.transform);
                jointMap.SetTransform(jointTypePairArray[k].Item2, jointPairList[n - k].Item2.transform);
            }
            jointMap.SetTransform(MJointType.C4C5Joint, chest.childJoints[neckIndex].transform);
        }
        return isFound;
        */
    }

    private bool FindFingers(JointInfo leftWrist, JointInfo rightWrist)
    {
        (MJointType, MJointType)[][] fingersJointTypePairs= new (MJointType, MJointType)[5][];
        (MJointType, MJointType)[] thumbJointTypePairs = 
            {
                (MJointType.LeftThumbMid, MJointType.RightThumbMid),
                (MJointType.LeftThumbMeta, MJointType.RightThumbMeta),
                (MJointType.LeftThumbCarpal, MJointType.RightThumbCarpal),
                (MJointType.LeftThumbTip, MJointType.LeftThumbTip)
            };
        fingersJointTypePairs[0] = thumbJointTypePairs;
        string[] fingerStr = {"Index", "Middle", "Ring", "Little"};
        string[] jointStr = { "Proximal", "Meta", "Distal", "Tip"}; // TODO This is factually wrong, but requires a renaming of meta to mid or an additional joint. 
        (MJointType, MJointType)[] indexFingerPairs = new (MJointType, MJointType)[4];
        for(int i = 0; i < fingerStr.Length; ++i)
        {
            (MJointType, MJointType)[] fingerJointTypePairs = new (MJointType, MJointType)[jointStr.Length];
            for(int j = 0; j < jointStr.Length; ++j)
            {
                MJointType leftJoint = (MJointType)Enum.Parse(typeof(MJointType), "Left"+fingerStr[i]+jointStr[j]);
                MJointType rightJoint = (MJointType)Enum.Parse(typeof(MJointType), "Right"+fingerStr[i]+jointStr[j]);
                fingerJointTypePairs[j] = (leftJoint, rightJoint);
            }
            fingersJointTypePairs[i+1] = fingerJointTypePairs;
        }
        if(leftWrist.childJoints.Count == 0 || rightWrist.childJoints.Count == 0)
            return false;
        if(leftWrist.childJoints.Count != rightWrist.childJoints.Count)
            return false;
        List<JointInfo> leftFirstJoints = new List<JointInfo>(leftWrist.childJoints);
        List<JointInfo> rightFirstJoints = new List<JointInfo>(rightWrist.childJoints);
        // Joint which is nearest to wrist is thumbMid
        leftFirstJoints.Sort((x, y) => - x.transform.position.z.CompareTo(y.transform.position.z));
        rightFirstJoints.Sort((x, y) => - x.transform.position.z.CompareTo(y.transform.position.z));
        //leftFirstJoints.Sort((x, y) => Vector3.Distance(x.transform.position, leftWrist.transform.position).CompareTo(Vector3.Distance(y.transform.position, leftWrist.transform.position)));
        //rightFirstJoints.Sort((x, y) => Vector3.Distance(x.transform.position, rightWrist.transform.position).CompareTo(Vector3.Distance(y.transform.position, rightWrist.transform.position)));
        //var leftThumbMid = leftFirstJoints[0];
        //var rightThumbMid = rightFirstJoints[0];

        // sort finger from thumb to little
        //leftFirstJoints.Sort((x, y) => Vector3.Distance(x.transform.position, leftThumbMid.transform.position).CompareTo(Vector3.Distance(y.transform.position, leftThumbMid.transform.position)));
        //rightFirstJoints.Sort((x, y) => Vector3.Distance(x.transform.position, rightThumbMid.transform.position).CompareTo(Vector3.Distance(y.transform.position, rightThumbMid.transform.position)));

        int fingerCount = Math.Min(fingersJointTypePairs.Length, leftFirstJoints.Count);
        for(int i = 0; i < fingerCount; ++i)
        {
            var leftFingerJoint = leftFirstJoints[i];
            var rightFingerJoint = rightFirstJoints[i];
            jointMap.SetTransform(fingersJointTypePairs[i][0].Item1, leftFingerJoint.transform);
            jointMap.SetTransform(fingersJointTypePairs[i][0].Item2, rightFingerJoint.transform);
            int j = 1;
            while(CheckChildCountEquality(leftFingerJoint, rightFingerJoint, 1, 0) && j < fingersJointTypePairs[i].Length)
            {
                leftFingerJoint = leftFingerJoint.childJoints[0];
                rightFingerJoint = rightFingerJoint.childJoints[0];
                jointMap.SetTransform(fingersJointTypePairs[i][j].Item1, leftFingerJoint.transform);
                jointMap.SetTransform(fingersJointTypePairs[i][j].Item2, rightFingerJoint.transform);
                ++j;
            }
        }

        return true;
    }

    private bool FindHead(JointInfo neck)
    {
        if(neck.childJoints.Count == 1)
        {
            JointInfo headJoint = neck.childJoints[0];
            jointMap.SetTransform(MJointType.HeadJoint, headJoint.transform);
            if(headJoint.childJoints.Count == 1)
            {
                jointMap.SetTransform(MJointType.HeadTip, headJoint.childJoints[0].transform);
            }
            return true;
        }
        return false;
    }
}
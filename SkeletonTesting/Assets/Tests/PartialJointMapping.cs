using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using MMIUnity;
using MMIStandard;

namespace Tests
{
    public class PartialJointMapping
    {
        public string SceneName = "SampleScene";
        public string CharTag = "Character";

        public IEnumerator SetupScene()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(SceneName);
            yield return new WaitForSeconds(0.2f);
        }

        private static MAvatarPosture CopyPosture(MAvatarPosture p)
        {
            MAvatarPosture p2 = new MAvatarPosture();
            p2.AvatarID = p.AvatarID;
            p2.Joints = new List<MJoint>();
            foreach (MJoint j in p.Joints)
            {
                MJoint nj = new MJoint();
                nj.ID = j.ID;
                nj.Type = j.Type;
                nj.Position = new MVector3(j.Position.X, j.Position.Y, j.Position.Z);
                nj.Rotation = new MQuaternion(j.Rotation.X, j.Rotation.Y, j.Rotation.Z, j.Rotation.W);
                p2.Joints.Add(nj);
            }
            return p2;
        }

        private static bool EqualPosture(MAvatarPosture p, MAvatarPosture p2)
        {
            bool check = true;
            Assert.IsTrue(p.Joints.Count - p2.Joints.Count == 0);
            for (int i = 0; i < p.Joints.Count; i++)
            {
                MJoint j1 = p.Joints[i];
                MJoint j2 = p2.Joints[i];

                Assert.IsTrue(j1.ID.Equals(j2.ID));
                if (j1.Position.Subtract(j2.Position).Magnitude() > 0.0001)
                {
                    //Debug.Log("Setup Error: joint positions not equal. " + j1.ID);
                    check = false;
                }
                float qd = Mathf.Abs((float)(j1.Rotation.X - j2.Rotation.X)) + Mathf.Abs((float)(j1.Rotation.Y - j2.Rotation.Y)) + Mathf.Abs((float)(j1.Rotation.Z - j2.Rotation.Z)) + Mathf.Abs((float)(j1.Rotation.W - j2.Rotation.W));
                if (qd > 0.0001)
                {
                    //Debug.Log("Setup Error: joint rotations not equal. " + j1.ID);
                    check = false;
                }

            }
            return check;
        }


        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator T01_RemoveWrist()
        {
            yield return SetupScene();

            GameObject character = GameObject.FindGameObjectWithTag(CharTag);
            TestIS core = character.GetComponent<TestIS>();

            MAvatarPosture p = core.GenerateGlobalPosture();
            p.AvatarID += "1";
            p.Joints.RemoveRange(9, 15);
            p.Joints.RemoveRange(13, 15);

            core.GetRetargetingService().SetupRetargeting(p);
        }
    }
}

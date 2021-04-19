using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using MMIUnity;
using MMIStandard;


namespace Tests
{
    public class is_to_target_tests
    {

        public string SceneName = "SampleScene";
        public string CharTag = "Character";

        public static float criterion = 0.0002f;
        public static int runs = 50;

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
                if (!(j1.ID.Contains("vis123bone_") || j1.ID.Contains("Mesh")))
                {

                    if (j1.Position.Subtract(j2.Position).Magnitude() > criterion)
                    {
                        Debug.Log("Setup Error: joint positions not equal. " + j1.ID + " " + j1.Position.Subtract(j2.Position).Magnitude());
                        check = false;
                    }
                    float qd = Mathf.Abs((float)(j1.Rotation.X - j2.Rotation.X)) + Mathf.Abs((float)(j1.Rotation.Y - j2.Rotation.Y)) + Mathf.Abs((float)(j1.Rotation.Z - j2.Rotation.Z)) + Mathf.Abs((float)(j1.Rotation.W - j2.Rotation.W));
                    if (qd > criterion)
                    {
                        Debug.Log("Setup Error: joint rotations not equal. " + j1.ID + " " + qd);
                        check = false;
                    }
                }

            }
            return check;
        }


        [UnityTest]
        public IEnumerator T00_VisJoints_GetRetargetedPostureValues()
        {
            yield return SetupScene();

            GameObject character = GameObject.FindGameObjectWithTag(CharTag);
            TestIS core = character.GetComponent<TestIS>();

            yield return new WaitForSeconds(0.1f);

            MAvatarPostureValues serviceVals = core.GetRetargetedPosture();

            yield return null;

            List<double> visualizationVals = core.skelVis.GetRetargetedPostureValues();

            yield return null;

            MAvatarPostureValues doubleCheck = core.GetRetargetedPosture();

            Assert.IsTrue(serviceVals.PostureData.Count == visualizationVals.Count);

            for (int j = 0; j < visualizationVals.Count; j++)
            {
                Assert.IsTrue(Mathf.Abs((float)(serviceVals.PostureData[j] - visualizationVals[j])) < criterion);
            }

            for (int j = 0; j < visualizationVals.Count; j++)
            {
                Assert.IsTrue(Mathf.Abs((float)(doubleCheck.PostureData[j] - visualizationVals[j])) < criterion);
            }



        }

        [UnityTest]
        public IEnumerator T01_VisJoints_GetRetargetedPostureValuesDrift()
        {
            yield return SetupScene();

            GameObject character = GameObject.FindGameObjectWithTag(CharTag);
            TestIS core = character.GetComponent<TestIS>();

            yield return new WaitForSeconds(0.1f);

            List<double> values = core.skelVis.GetRetargetedPostureValues();

            for(int i = 0; i< runs; i++)
            {
                List<double> newVals = core.skelVis.GetRetargetedPostureValues();
                Assert.IsTrue(newVals.Count == values.Count);

                for(int j = 0; j<newVals.Count; j++)
                {
                    Assert.IsTrue(Mathf.Abs((float)(newVals[j] - values[j])) < criterion);
                }

                yield return null;
            }
        }
        [UnityTest]
        public IEnumerator T02_RetargetingService()
        {
            yield return SetupScene();

            GameObject character = GameObject.FindGameObjectWithTag(CharTag);
            TestIS core = character.GetComponent<TestIS>();

            yield return new WaitForSeconds(0.1f);
            MAvatarPostureValues posture = core.GetRetargetedPosture();
            MAvatarPostureValues newPosture = new MAvatarPostureValues();
            newPosture.AvatarID = posture.AvatarID;
            MAvatarPosture before = core.GenerateGlobalPosture();

            for (int i = 0; i < runs; i++)
            {
                newPosture.PostureData = core.skelVis.GetRetargetedPostureValues();
                Assert.IsTrue(posture.PostureData.Count == newPosture.PostureData.Count);

                for (int j = 0; j < newPosture.PostureData.Count; j++)
                {
                    Assert.IsTrue(Mathf.Abs((float)(newPosture.PostureData[j] - posture.PostureData[j])) < criterion);
                }

                //core.AssignPostureValues(newPosture);
                MAvatarPosture p = core.GetRetargetingService().RetargetToTarget(newPosture);

                Assert.IsTrue(EqualPosture(before, p));

                core.GetSkeletonAccess().SetChannelData(newPosture);

                core.RootBone.ApplyGlobalJoints(p.Joints);
                
                yield return null;

                MAvatarPosture after = core.GenerateGlobalPosture();
                Assert.IsTrue(EqualPosture(before, after));
            }


        }


        [UnityTest]
        public IEnumerator T02_VisJoints_ToPosture()
        {
            yield return SetupScene();

            GameObject character = GameObject.FindGameObjectWithTag(CharTag);
            TestIS core = character.GetComponent<TestIS>();

            yield return new WaitForSeconds(0.1f);

            MAvatarPostureValues posture = core.GetRetargetedPosture();
            MAvatarPostureValues newPosture = new MAvatarPostureValues();
            newPosture.AvatarID = posture.AvatarID;
            MAvatarPosture before = core.GenerateGlobalPosture();

            for (int i = 0; i < runs; i++)
            {
                newPosture.PostureData = core.skelVis.GetRetargetedPostureValues();
                Assert.IsTrue(posture.PostureData.Count == newPosture.PostureData.Count);

                for (int j = 0; j < newPosture.PostureData.Count; j++)
                {
                    Assert.IsTrue(Mathf.Abs((float)(newPosture.PostureData[j] - posture.PostureData[j])) < criterion, "Not equal: " + newPosture.PostureData[j] + " " + posture.PostureData[j]);
                }

                core.AssignPostureValues(newPosture);

                yield return null;

                MAvatarPosture after = core.GenerateGlobalPosture();
                Assert.IsTrue(EqualPosture(before, after));



            }
        }
    }
}

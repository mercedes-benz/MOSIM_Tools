using UnityEngine;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using UnityEditor;
using System.Linq;
//using Cinemachine;

public class CameraTimelineCombo : MonoBehaviour
{
    // XML File

    public static CameraTimelineCombo ins;
    public string fileName = null;
    string filePath = null;

    private void Awake()
    {
        ins = this;
    }

    // list of times
    public TimeDatabase timeDB = new TimeDatabase();


    public void SaveTimes()
    {

        for (int i = 0; i < timeDB.list.Count; i++)
        {
            timeDB.list[i].followAvatar = CameraTimelineEditorCombo.followAvatarTimestamp[i];
        }
        //open a new xml file
        XmlSerializer serializer = new XmlSerializer(typeof(TimeDatabase));
        if (filePath == null)
        {
            filePath = EditorUtility.SaveFilePanel("Save Timeline", Application.dataPath, "Timeline.xml", "xml");
        }
        else
        {
            filePath = EditorUtility.SaveFilePanel("Save Timeline", filePath, "Timeline.xml", "xml");
        }
        try
        {
            FileStream stream = new FileStream(filePath, FileMode.Create);
            serializer.Serialize(stream, timeDB);
            stream.Close();
            Debug.Log("Timeline Saved!");
        }
        catch
        {
            Debug.Log("Saving Process Cancelled!");
        }

    }

    public bool LoadTimes()
    {
        //load an xml file
        XmlSerializer serializer = new XmlSerializer(typeof(TimeDatabase));
        if (filePath == null)
        {
            filePath = EditorUtility.OpenFilePanel("Load Timeline", Application.dataPath, "xml");
        }
        else
        {
            filePath = EditorUtility.OpenFilePanel("Load Timeline", filePath, "xml");
        }
        try
        {

            FileStream stream = new FileStream(filePath, FileMode.Open);
            timeDB = serializer.Deserialize(stream) as TimeDatabase;
            stream.Close();
            ind = -1;
            LoadTimeStamps();
            fileName = Path.GetFileName(filePath);
            Debug.Log("Timeline Loaded!");
            CameraTimelineEditorCombo.deletingTimestamp = false;
            return true;
        }
        catch
        {
            Debug.Log("Loading Process Cancelled!");
            return false;
        }
    }







    // Camera Timeline
    private GameObject objVcam;
    //public CinemachineVirtualCamera vcam;
    private GameObject objCam;
    public float CurrentTimeSeconds;
    public Vector3 CurrentCameraPosition;
    public Vector3 CurrentCameraRotation;

    public Vector3 defaultPosition = new Vector3(5.3f, 1.74f, -0.49f);
    public Vector3 defaultRotation = new Vector3(10.017f, -90f, 0f);

    public Vector3[] timelinePositions = new Vector3[0];
    public Vector3[] timelineRotations = new Vector3[0];

    public string[] staticCamName = new string[0];
    public Vector3[] staticCamPos = new Vector3[0];
    public Vector3[] staticCamRot = new Vector3[0];

    //public Vector3[] timelineFollowPos = new Vector3[0];
    //public Vector3[] timelineFollowRot = new Vector3[0];
    public Vector3[] timelinePositionsInterpolated = new Vector3[0];
    public Vector3[] timelineRotationsInterpolated = new Vector3[0];
    public float[] timelineTime = new float[0];

    //create curves
    public AnimationCurve curveXPosition = new AnimationCurve();
    public AnimationCurve curveYPosition = new AnimationCurve();
    public AnimationCurve curveZPosition = new AnimationCurve();
    public AnimationCurve curveXRotation = new AnimationCurve();
    public AnimationCurve curveYRotation = new AnimationCurve();
    public AnimationCurve curveZRotation = new AnimationCurve();

    public bool enableFlying;
    public bool enableFollowAvatar;
    private bool timeEnableFollowAvatar;

    public static Vector3 defaultFollowAvatarPos = new Vector3(0, 1.5f, -2);
    public static Vector3 defaultFollowAvatarRot = new Vector3(0, 0, 0);
    Quaternion q_cam;
    Quaternion q_avatar;

    public bool enableStaticCam;
    public int ind;

    private bool enableFollowAvatarPrev;
    public GameObject AvatarObj;
    Vector3 AvatarPosOrig;
    Quaternion AvatarRotOrig;
    Quaternion AvatarRotNew;
    Vector3 changeRot;
    Vector3 changePos;
    Camera cam;
    //CinemachineBrain objCamBrain;

    public float totalTime;
    public bool simRunning;
    public bool simPaused;
    private Vector3 distanceYaw;

    void Start()
    {
        objCam = this.gameObject;
        objCam.transform.position = defaultPosition;
        objCam.transform.eulerAngles = defaultRotation;
        AvatarObj = GameObject.Find("Avatar");
        AvatarRotOrig = AvatarObj.transform.rotation;
        AvatarPosOrig = AvatarObj.transform.position;
        Debug.Log(AvatarObj);
        cam = objCam.GetComponent<Camera>();

        q_cam = Quaternion.Euler(defaultFollowAvatarRot);
        q_avatar = Quaternion.Euler(AvatarObj.transform.eulerAngles);
        //objVcam = new GameObject();
        //objVcam.name = "Follow Avatar Cam";
        //vcam = objVcam.AddComponent<CinemachineVirtualCamera>();
        //vcam.enabled = false;

    }

    void Update()
    {

        // Drawing lines between the points
        drawNewPositionLines();


        // Follow Avatar Function
        int currentFollowInd = -1;
        if (!PhotoboothScriptSceneGen.PhotoboothRunning)
        {
            AvatarObj = GameObject.Find("Avatar");
            //AvatarRotOrig = AvatarObj.transform.rotation;

            //  This is used just to test the "Follow Avatar" function
            AvatarObj.transform.eulerAngles = new Vector3(-360 / 30 * CurrentTimeSeconds, -360 / 30 * CurrentTimeSeconds, -360 / 30 * CurrentTimeSeconds);


            //AvatarRotNew = AvatarObj.transform.rotation;

            bool currentTimeFound = false;
            for (int i = 1; i < timelineTime.Length; i++)
            {
                if (!currentTimeFound)
                {
                    if ((CurrentTimeSeconds < timelineTime[i]) && (timelineTime.Length == CameraTimelineEditorCombo.followAvatarTimestamp.Length))
                    {
                        currentTimeFound = true;
                        currentFollowInd = i - 1;
                        timeEnableFollowAvatar = (CameraTimelineEditorCombo.followAvatarTimestamp[i-1] || CameraTimelineEditorCombo.enableFollowAvatar);
                    }
                }

            }
            if (enableFollowAvatar)
            {
                objCam.transform.position = AvatarObj.transform.TransformPoint(defaultFollowAvatarPos);
                objCam.transform.rotation = AvatarObj.transform.rotation * Quaternion.Inverse(q_avatar) * q_cam;
            }
            else if (timeEnableFollowAvatar && !enableStaticCam)
            {
                objCam.transform.position = AvatarObj.transform.TransformPoint(timeDB.list[currentFollowInd].followAvatarPos);
                objCam.transform.rotation = AvatarObj.transform.rotation * timeDB.list[currentFollowInd].followAvatarRot;

            }
            else if (enableStaticCam)
            {
                if (ind >= 0)
                {
                    objCam.transform.position = timeDB.listCam[ind].camPos;
                    objCam.transform.eulerAngles = timeDB.listCam[ind].camRot;
                }
                else
                {
                    objCam.transform.position = defaultPosition;
                    objCam.transform.eulerAngles = defaultRotation;
                }
            }
        }


        if (!enableFlying && !enableFollowAvatar && !enableStaticCam && !timeEnableFollowAvatar)
        {
            if (timeDB.list.Count !=0)
            {
                objCam.transform.position = new Vector3(curveXPosition.Evaluate(CurrentTimeSeconds), curveYPosition.Evaluate(CurrentTimeSeconds), curveZPosition.Evaluate(CurrentTimeSeconds));
                objCam.transform.eulerAngles = new Vector3(curveXRotation.Evaluate(CurrentTimeSeconds), curveYRotation.Evaluate(CurrentTimeSeconds), curveZRotation.Evaluate(CurrentTimeSeconds));
            }
            else
            {
                objCam.transform.position = defaultPosition;
                objCam.transform.eulerAngles = defaultRotation;
            }

        }




        if (simRunning && totalTime <= timelineTime[timelineTime.Length - 1])
        {
            if (!simPaused)
            {
                totalTime += Time.unscaledDeltaTime;
            }
            CurrentTimeSeconds = totalTime;
        }
        else
        {
            if (simRunning)
            {
                simRunning = false;
                totalTime = 0;
            }
        }


    }




    //When the XML file is loaded into the class, as well as when a timestamp is added/edited/deleted, this copies the values of the TimeDatabase into the vectors that are used.
    public void LoadTimeStamps()
    {
        timelinePositions = new Vector3[timeDB.list.Count];
        timelineRotations = new Vector3[timeDB.list.Count];
        //timelineFollowPos = new Vector3[timeDB.list.Count];
        //timelineFollowRot = new Vector3[timeDB.list.Count];
        timelineTime = new float[timeDB.list.Count];

        staticCamName = new string[timeDB.listCam.Count];
        staticCamPos = new Vector3[timeDB.listCam.Count];
        staticCamRot = new Vector3[timeDB.listCam.Count];

        CameraTimelineEditorCombo.followAvatarTimestamp = new bool[timeDB.list.Count];
        CameraTimelineEditorCombo.showFoldout = new bool[timeDB.list.Count];
        CameraTimelineEditorCombo.showFoldout = Enumerable.Repeat(true, timeDB.list.Count).ToArray();

        for (int i = 0; i < timeDB.list.Count; i++)
        {
            timelinePositions[i] = timeDB.list[i].camPos;
            timelineRotations[i] = timeDB.list[i].camRot;
            //timelineFollowPos[i] = timeDB.list[i].followAvatarPos;
            //timelineFollowRot[i] = timeDB.list[i].followAvatarRot;
            timelineTime[i] = timeDB.list[i].time;
            CameraTimelineEditorCombo.followAvatarTimestamp[i] = timeDB.list[i].followAvatar;


        }
        for (int i = 0; i < timeDB.listCam.Count; i++)
        {
            staticCamName[i] = timeDB.listCam[i].camName;
            staticCamPos[i] = timeDB.listCam[i].camPos;
            staticCamRot[i] = timeDB.listCam[i].camRot;
        }
        timelineRotations = rotationCorrection(timelineRotations);
        timelinePositionsInterpolated = SmoothLinePosition(timelinePositions, timelineTime, 0.0166f);
        timelineRotationsInterpolated = SmoothLineRotation(timelineRotations, timelineTime, 0.0166f);

    }

    //Saves the current time, position, and rotation to the list of timestamps, Adds the timestamp in the correct order based on time in the list.
    public void SaveTimeStamp()
    {
        if (timeDB.list.Count == 0)
        {
            timeDB.list.Add(new TimeEntry() { time = CurrentTimeSeconds, camPos = CameraTimelineEditorCombo.cam.transform.position, camRot = CameraTimelineEditorCombo.cam.transform.eulerAngles, followAvatar = false, followAvatarPos = defaultFollowAvatarPos, followAvatarRot = Quaternion.Inverse(CameraTimelineEditorCombo.AvatarObj.transform.rotation) * Quaternion.Euler(defaultFollowAvatarRot) });
            CameraTimelineEditorCombo.editingTimestamp = new bool[1];
        }
        else
        {
            bool found = false;
            for (int i = 0; i < timeDB.list.Count; i++)
            {
                if (timeDB.list[i].time == CurrentTimeSeconds && !CameraTimelineEditorCombo.editingTimestamp.Any(val => val == true))
                {
                    found = true;
                    Debug.LogError("Unable to add duplicate Timestamp time. Either edit or delete the original.");
                }
                if (timeDB.list[i].time > CurrentTimeSeconds && !found)
                {
                    timeDB.list.Insert(i, new TimeEntry() { time = CurrentTimeSeconds, camPos = CameraTimelineEditorCombo.cam.transform.position, camRot = CameraTimelineEditorCombo.cam.transform.eulerAngles, followAvatar = false, followAvatarPos = defaultFollowAvatarPos, followAvatarRot = Quaternion.Inverse(CameraTimelineEditorCombo.AvatarObj.transform.rotation) * Quaternion.Euler(defaultFollowAvatarRot) });

                    List<bool> tempList = CameraTimelineEditorCombo.editingTimestamp.ToList();
                    tempList.Insert(i, false);
                    CameraTimelineEditorCombo.editingTimestamp = tempList.ToArray();

                    tempList = CameraTimelineEditorCombo.showFoldout.ToList();
                    tempList.Insert(i, true);
                    CameraTimelineEditorCombo.showFoldout = tempList.ToArray();

                    found = true;
                    Debug.Log("Point was Saved!");

                }
            }
            if (!found)
            {
                timeDB.list.Add(new TimeEntry() { time = CurrentTimeSeconds, camPos = CameraTimelineEditorCombo.cam.transform.position, camRot = CameraTimelineEditorCombo.cam.transform.eulerAngles, followAvatar = false, followAvatarPos = defaultFollowAvatarPos, followAvatarRot = Quaternion.Inverse(CameraTimelineEditorCombo.AvatarObj.transform.rotation) * Quaternion.Euler(defaultFollowAvatarRot) });

                List<bool> tempList = new List<bool>(CameraTimelineEditorCombo.editingTimestamp);
                tempList.Add(false);
                CameraTimelineEditorCombo.editingTimestamp = tempList.ToArray();

                tempList = new List<bool>(CameraTimelineEditorCombo.showFoldout);
                tempList.Add(false);
                CameraTimelineEditorCombo.showFoldout = tempList.ToArray();

                Debug.Log("Point was Saved!");
            }
        }
        LoadTimeStamps();
        CameraTimelineEditorCombo.numOfTimestamps = timelineTime.Length;
        for (int q = 0; q < timeDB.list.Count; q++)
        {
            Debug.Log("Time " + (q+1).ToString() + " Follow: " + CameraTimelineEditorCombo.followAvatarTimestamp[q]);
        }
    }

    //Deletes the current time stamp, must be on the time stamp to delete it (this function is also used in the Edit Timestamp function as the Edit function deletes the timestamp and adds the new one when saved)
    public void DeleteTimeStamp(int listIndex)
    {
        timeDB.list.Remove(timeDB.list[listIndex]);

        List<bool> tempList = CameraTimelineEditorCombo.editingTimestamp.ToList();
        tempList.Remove(tempList[listIndex]);
        CameraTimelineEditorCombo.editingTimestamp = tempList.ToArray();

        LoadTimeStamps();
    }

    //Resets the timestamps currently saved in the inspector (does NOT change saved XML files)
    public void ResetTimeStamps()
    {
        timeDB = new TimeDatabase();
        LoadTimeStamps();
        fileName = null;
        Debug.Log("Timeline was Reset!");
    }



    // For static camera position and rotation
    public void AddStaticCam()
    {
        string newName = "Position ";
        int q = 0;
        if (staticCamName.Length != 0)
        {
            while (staticCamName.Contains(newName + q.ToString()))
            {
                q++;
            }
        }

        newName += q.ToString();

        timeDB.listCam.Add(new staticCamPosRot() { camPos = CameraTimelineEditorCombo.cam.transform.position, camRot = CameraTimelineEditorCombo.cam.transform.eulerAngles, camName = newName});
        ind = staticCamName.Length;
        LoadTimeStamps();
    }

    public void RemoveStaticCam()
    {
        if (staticCamName.Length != 0 && ind >= 0)
        {
            timeDB.listCam.Remove(timeDB.listCam[ind]);
            if (ind >= timeDB.listCam.Count)
                ind--;
            LoadTimeStamps();
        }
        else
        {
            Debug.LogError("No Static Camera Location Selected. Therefore, cannot remove.");
            LoadTimeStamps();
        }
    }

    public void ChangeStaticCamName(string newName, int indx)
    {
        timeDB.listCam[indx].camName = newName;
        LoadTimeStamps();
    }





    public void JumpToPositionRotation(GameObject cam, float time, Vector3 pos, Vector3 rot)
    {
        cam.transform.position = pos;
        cam.transform.eulerAngles = rot;
        CurrentTimeSeconds = time;
    }


    // Function for following the movement of the avatar (with the ability to swivel the camera around him during movement)
    // ...
    // Cinema camera (or whatever it's called) might be available to use if I can create it in the script, as it has a follow object function


    public void drawNewPositionLines()
    {
        System.Random rnd = new System.Random();
        for (int i = 0; i < timelinePositionsInterpolated.Length - 1; i++)
        {
            Debug.DrawLine(timelinePositionsInterpolated[i], timelinePositionsInterpolated[i + 1], Color.black); //new Color32((byte)rnd.Next(0,255), (byte)rnd.Next(0, 255), (byte)rnd.Next(0, 255), 255));
        }
        for (int i = 0; i < timelineTime.Length - 1; i++)
        {
            Debug.DrawLine(timelinePositions[i], timelinePositions[i + 1], Color.red);
        }
    }

    //used to account for the fact that when going from, for example, 350 degrees to 10 degrees, it should pass through 0, not rotate all the way back around.
    public Vector3[] rotationCorrection(Vector3[] inputRotations)
    {
        float constForCorrection = 360;

        for (int i = 0; i < inputRotations.Length - 1; i++)
        {
            // X: 0 degree crossing direction: from 350 --> 10 degrees
            if (inputRotations[i].x - inputRotations[i + 1].x > 180)
            {
                for (int q = i + 1; q < inputRotations.Length; q++)
                {
                    inputRotations[q].x += constForCorrection;
                }
            }
            // X: 0 degree crossing direction: from 10 --> 350 degrees
            if (inputRotations[i].x - inputRotations[i + 1].x < -180)
            {
                for (int q = i + 1; q < inputRotations.Length; q++)
                {
                    inputRotations[q].x -= constForCorrection;
                }
            }


            // Y: 0 degree crossing direction: from 350 --> 10 degrees
            if (inputRotations[i].y - inputRotations[i + 1].y > 180)
            {
                for (int q = i+1; q < inputRotations.Length; q++)
                {
                    inputRotations[q].y += constForCorrection;
                }
            }
            // Y: 0 degree crossing direction: from 10 --> 350 degrees
            if (inputRotations[i].y - inputRotations[i + 1].y < -180)
            {
                for (int q = i+1; q < inputRotations.Length; q++)
                {
                    inputRotations[q].y -= constForCorrection;
                }
            }

            // Z: 0 degree crossing direction: from 350 --> 10 degrees
            if (inputRotations[i].z - inputRotations[i + 1].z > 180)
            {
                for (int q = i + 1; q < inputRotations.Length; q++)
                {
                    inputRotations[q].z += constForCorrection;
                }
            }
            // Z: 0 degree crossing direction: from 10 --> 350 degrees
            if (inputRotations[i].z - inputRotations[i + 1].z < -180)
            {
                for (int q = i + 1; q < inputRotations.Length; q++)
                {
                    inputRotations[q].z -= constForCorrection;
                }
            }
        }


        return inputRotations;
    }

    public Vector3[] SmoothLinePosition(Vector3[] inputPoints, float[] inputTimes, float segmentSize)
    {
        //create keyframe sets
        Keyframe[] keysX = new Keyframe[inputPoints.Length];
        Keyframe[] keysY = new Keyframe[inputPoints.Length];
        Keyframe[] keysZ = new Keyframe[inputPoints.Length];

        //set keyframes
        for (int i = 0; i < inputPoints.Length; i++)
        {
            // change i to inputTimes[i]
            keysX[i] = new Keyframe(inputTimes[i], inputPoints[i].x);           // CHANGED
            keysY[i] = new Keyframe(inputTimes[i], inputPoints[i].y);
            keysZ[i] = new Keyframe(inputTimes[i], inputPoints[i].z);
        }

        //apply keyframes to curves
        curveXPosition.keys = keysX;
        curveYPosition.keys = keysY;
        curveZPosition.keys = keysZ;

        //smooth curve tangents
        for (int i = 0; i < inputPoints.Length; i++)
        {
            curveXPosition.SmoothTangents(i, 0);
            curveYPosition.SmoothTangents(i, 0);
            curveZPosition.SmoothTangents(i, 0);
        }

        //list to write smoothed values to
        List<Vector3> lineSegments = new List<Vector3>();

        //find segments in each section
        for (int i = 0; i < inputPoints.Length; i++)
        {
            //add first point
            lineSegments.Add(inputPoints[i]);

            //make sure within range of array
            if (i + 1 < inputPoints.Length)
            {
                //find distance to next point
                float distanceToNext = Vector3.Distance(inputPoints[i], inputPoints[i + 1]);

                //number of segments
                int segments = (int)(distanceToNext / segmentSize);

                //add segments
                for (int s = 1; s < segments; s++)
                {
                    //interpolated time on curve
                    float time = ((float)s / (float)segments) * (inputTimes[i+1]-inputTimes[i]) + (float)inputTimes[i];               // CHANGED from: float time = ((float)s / (float)segments) + (float)i;

                    //sample curves to find smoothed position
                    Vector3 newSegment = new Vector3(curveXPosition.Evaluate(time), curveYPosition.Evaluate(time), curveZPosition.Evaluate(time));

                    //add to list
                    lineSegments.Add(newSegment);
                }
            }
        }

        return lineSegments.ToArray();
    }

    public Vector3[] SmoothLineRotation(Vector3[] inputPoints, float[] inputTimes, float segmentSize)
    {
        //create keyframe sets
        Keyframe[] keysX = new Keyframe[inputPoints.Length];
        Keyframe[] keysY = new Keyframe[inputPoints.Length];
        Keyframe[] keysZ = new Keyframe[inputPoints.Length];

        //set keyframes
        for (int i = 0; i < inputPoints.Length; i++)
        {
            // change i to inputTimes[i]
            //if (inputPoints[i].x < 0)
            //{
            //    inputPoints[i].x += 360;
            //}
            //if (inputPoints[i].y < 0)
            //{
            //    inputPoints[i].y += 360;
            //}
            //if (inputPoints[i].z < 0)
            //{
            //    inputPoints[i].z += 360;
            //}
            //Debug.Log("X: " + inputPoints[i].x.ToString("F4"));
            //Debug.Log("Y: " + inputPoints[i].y.ToString("F4"));
            //Debug.Log("Z: " + inputPoints[i].z.ToString("F4"));
            keysX[i] = new Keyframe(inputTimes[i], inputPoints[i].x);           // CHANGED
            keysY[i] = new Keyframe(inputTimes[i], inputPoints[i].y);
            keysZ[i] = new Keyframe(inputTimes[i], inputPoints[i].z);
        }

        //apply keyframes to curves
        curveXRotation.keys = keysX;
        curveYRotation.keys = keysY;
        curveZRotation.keys = keysZ;

        //smooth curve tangents
        for (int i = 0; i < inputPoints.Length; i++)
        {
            curveXRotation.SmoothTangents(i, 0);
            curveYRotation.SmoothTangents(i, 0);
            curveZRotation.SmoothTangents(i, 0);
        }

        //list to write smoothed values to
        List<Vector3> lineSegments = new List<Vector3>();

        //find segments in each section
        for (int i = 0; i < inputPoints.Length; i++)
        {
            //add first point
            lineSegments.Add(inputPoints[i]);

            //make sure within range of array
            if (i + 1 < inputPoints.Length)
            {
                //find distance to next point
                float distanceToNext = Vector3.Distance(inputPoints[i], inputPoints[i + 1]);

                //number of segments
                int segments = (int)(distanceToNext / segmentSize);

                //add segments
                for (int s = 1; s < segments; s++)
                {
                    //interpolated time on curve
                    float time = ((float)s / (float)segments) * (inputTimes[i + 1] - inputTimes[i]) + (float)inputTimes[i];               // CHANGED from: float time = ((float)s / (float)segments) + (float)i;

                    //sample curves to find smoothed position
                    Vector3 newSegment = new Vector3(curveXRotation.Evaluate(time), curveYRotation.Evaluate(time), curveZRotation.Evaluate(time));

                    //add to list
                    lineSegments.Add(newSegment);
                }
            }
        }

        return lineSegments.ToArray();
    }


}

[System.Serializable]
public class TimeEntry
{
    public float time;
    public Vector3 camPos;
    public Vector3 camRot;
    public bool followAvatar;
    public Vector3 followAvatarPos;
    public Quaternion followAvatarRot;
}

[System.Serializable]
public class staticCamPosRot
{
    public Vector3 camPos;
    public Vector3 camRot;
    public string camName;
}

[System.Serializable]
public class TimeDatabase
{
    [XmlArray("TimeStamps")]
    public List<TimeEntry> list = new List<TimeEntry>();
    [XmlArray("StaticCamera")]
    public List<staticCamPosRot> listCam = new List<staticCamPosRot>();
}

using System;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CameraTimelineCombo))]
public class CameraTimelineEditorCombo : Editor
{
    bool foldoutPosition = true;
    bool foldoutRotation = true;
    CameraTimelineCombo camTimeline;
    public static GameObject cam;
    public static CameraTimelineCombo camTimelineScript;

    private SerializedProperty camPos, camRot, camTime;

    GUIStyle styleOn = new GUIStyle();
    GUIStyle styleOff = new GUIStyle();
    Color newColor;
    string colorEdit = "white";
    public static bool[] showFoldout;

    public static bool[] editingTimestamp = new bool[0];
    bool editingATimestamp;
    public static bool[] followAvatarTimestamp = new bool[0];
    public static bool deletingTimestamp = false;
    bool wasDeleted;
    bool enableFlying;
    public static bool enableFollowAvatar;
    public static bool enableStaticCam;
    public static int ind;
    bool renaming;
    string temp = "";
    bool adding;

    bool editingFollowAvatar = false;
    int editingFollowAvatarInd = -1;
    Quaternion defaultAvatarRot;

    public static GameObject AvatarObj;

    Vector3 p;
    Vector3 r;

    public static int numOfTimestamps = 0;

    // ONLY THINGS LEFT TO DO ARE TO FIX THE ANGLE ROTATION PROBLEM WITH SIMULATION
    // AND TO CODE UP THE "FOLLOW AVATAR" FEATURE

   

    private void OnEnable()
    {
        camTimeline = (CameraTimelineCombo)target;
        cam = camTimeline.gameObject;

        camTimelineScript = cam.GetComponent<CameraTimelineCombo>();
        styleOn.richText = true;
        styleOff.richText = false;
        numOfTimestamps = camTimeline.timelineTime.Length;
        editingTimestamp = new bool[numOfTimestamps];
        //followAvatarTimestamp = new bool[numOfTimestamps];
        showFoldout = Enumerable.Repeat(true, numOfTimestamps).ToArray();
        camTimeline.LoadTimeStamps();

        AvatarObj = GameObject.Find("Avatar");
        defaultAvatarRot = AvatarObj.transform.rotation;


    }

    public override void OnInspectorGUI()
    {
        
        AvatarObj = GameObject.Find("Avatar");

        numOfTimestamps = camTimeline.timelineTime.Length;

        enableFlying = camTimelineScript.enableFlying;
        enableFollowAvatar = camTimelineScript.enableFollowAvatar;
        enableStaticCam = camTimelineScript.enableStaticCam;
        ind = camTimelineScript.ind;

        string flyingTxt = "This enables the user to use WASD (position) and arrow keys (rotation) to move the camera around while NOT in playmode. (Must click on inspector to work)\n\nFly Camera Script is made for flying in playmode. (Must click on the Game View to work)";
        string avatarTxt = "This will ignore the path determined by the timestamps and follow the avatar through the simulation.";
        string staticCamTxt = "This will ignore the path determined by the timestamps, place the camera in a location and orientation, and will not let the camera move.";

        camTimelineScript.enableFlying = EditorGUILayout.Toggle(new GUIContent("Enable Camera Flying", flyingTxt), enableFlying);
        camTimelineScript.enableFollowAvatar = EditorGUILayout.Toggle(new GUIContent("Enable Follow Avatar", avatarTxt), enableFollowAvatar);
        camTimelineScript.enableStaticCam = EditorGUILayout.Toggle(new GUIContent("Enable Static Camera", staticCamTxt), enableStaticCam);
        if (!camTimelineScript.enableStaticCam)
            adding = false;

        GUILayout.Space(10);


        
        if (enableStaticCam)
        {
            GUILayout.BeginHorizontal();
            if (!renaming && !adding && !editingFollowAvatar && !editingATimestamp)
            {
                if (GUILayout.Button(new GUIContent("Add", "Add a new static location for the camera from the current camera position and orientation. Enables WASD and arrow key camera movement to position the camera.")) && !renaming)
                {
                    adding = true;
                }
                if (GUILayout.Button(new GUIContent("Remove", "Removes the currently selected static location for the camera.")) && !renaming)
                {
                    camTimeline.RemoveStaticCam();
                }
                if (ind >= 0)
                {
                    if (GUILayout.Button(new GUIContent("Rename", "Allows renaming of the currently selected static location for the camera.")) && !renaming)
                    {
                        renaming = true;
                        temp = camTimeline.timeDB.listCam[ind].camName;
                    }
                }
                camTimelineScript.ind = EditorGUILayout.Popup(camTimelineScript.ind, camTimeline.staticCamName);

            }
            else if (renaming)
            {
                if (GUILayout.Button(new GUIContent("Save?", "Saves the new name.")))
                {
                    renaming = false;
                    camTimeline.ChangeStaticCamName(temp, camTimelineScript.ind);
                }
                if (GUILayout.Button(new GUIContent("Cancel", "Cancels the renaming.")))
                {
                    renaming = false;
                }

                temp = EditorGUILayout.TextField("Location Name: ", temp);

            }
            else if (adding)
            {
                if (GUILayout.Button(new GUIContent("Add?", "Saves the new static location for the camera from the current camera position and orientation.")))
                {
                    camTimeline.AddStaticCam();
                    adding = false;
                }
                if (GUILayout.Button(new GUIContent("Cancel", "Cancels the adding process.")))
                {
                    adding = false;
                }

            }
            GUILayout.EndHorizontal();
            if (editingATimestamp || editingFollowAvatar)
                GUILayout.Space(19);
        }


        if (camTimelineScript.enableFollowAvatar && !enableFollowAvatar)
        {
            camTimelineScript.enableFlying = false;
            adding = false;
            camTimelineScript.enableStaticCam = false;
        }
        if (camTimelineScript.enableFlying && !enableFlying)
        {
            camTimelineScript.enableFollowAvatar = false;
            adding = false;
            camTimelineScript.enableStaticCam = false;
        }
        if (camTimelineScript.enableStaticCam && !enableStaticCam)
        {
            camTimelineScript.enableFollowAvatar = false;
            camTimelineScript.enableFlying = false;
        }



        if (!enableFlying && !enableFollowAvatar && !enableStaticCam)
        {
            GUILayout.Label("<i><color=gray>Default camera path enabled.</color></i>", styleOn);
        }





        // This is for flying with the arrow keys while NOT in play mode
        if ((enableFlying || adding || editingFollowAvatar || editingATimestamp) && !Application.isPlaying && !renaming)
        {
            p = new Vector3(0, 0, 0);
            if (Event.current.keyCode == (KeyCode.W))
            {
                p += new Vector3(0, 0, 1);
            }
            if (Event.current.keyCode == (KeyCode.S))
            {
                p += new Vector3(0, 0, -1);
            }
            if (Event.current.keyCode == (KeyCode.A))
            {
                p += new Vector3(-1, 0, 0);
            }
            if (Event.current.keyCode == (KeyCode.D))
            {
                p += new Vector3(1, 0, 0);
            }
            p /= 30;
            cam.transform.Translate(p);


            r = cam.transform.eulerAngles;
            int multiplier = 1;
            if (Event.current.keyCode == (KeyCode.RightArrow))
            {
                r += new Vector3(0, 1, 0) * (multiplier);
            }
            if (Event.current.keyCode == (KeyCode.LeftArrow))
            {
                r += new Vector3(0, -1, 0) * (multiplier);
            }
            if (Event.current.keyCode == (KeyCode.UpArrow))
            {
                r += new Vector3(-1, 0, 0) * (multiplier);
            }
            if (Event.current.keyCode == (KeyCode.DownArrow))
            {
                r += new Vector3(1, 0, 0) * (multiplier);
            }
            cam.transform.eulerAngles = r;
        }



        if (numOfTimestamps >= 2 && Application.isPlaying)
        {
            GUILayout.Space(20);
            if (!camTimelineScript.simRunning)
            {
                if (GUILayout.Button("Start Simulation (aka camera movement)"))
                { //added those lines to make sure simulation always works (Adam)
                    camTimelineScript.enableFlying = false;
                    camTimelineScript.enableFollowAvatar = false;
                    camTimelineScript.enableStaticCam = false;
                    camTimelineScript.simRunning = true;
                    
                    //if (!Application.isPlaying)
                    //{
                    //    camTimelineScript.runSim(cam);
                    //}
                    
                    
                    // if no timestamp at time = 0, create one at the default position and time = 0 OR I could simply start the simulationi from where the camera is (which I guess would be the defaults anyways 🤔

                    // then I will run the function in CameraTimelineCombo to move the camera

                    // Once I have a spline function in place to determine the path, the timeline time slider will determine the position of the camera (sliding the time around moves the camera to where it would be at the time in the spline) --> This would only happen AFTER hitting the start simulation button AND while being paused, otherwise, while the simulation is running, the slider can't be changed manually



                    
                }
            }
            else
            {
                GUILayout.BeginHorizontal("box");
                if (!camTimelineScript.simPaused)
                {
                    if (GUILayout.Button("Pause Simulation"))
                    {
                        camTimelineScript.simPaused = true;
                    }
                }
                else
                {
                    if (GUILayout.Button("Continue Simulation"))
                    { //added those lines to make sure simulation always works (Adam)
                        camTimelineScript.enableFlying = false;
                        camTimelineScript.enableFollowAvatar = false;
                        camTimelineScript.enableStaticCam = false;
                        camTimelineScript.simPaused = false;
                        // if camera is moved during the pause, it should resume by jumping to the next camera location and continuing as normal
                    }
                }

                if (GUILayout.Button("Stop Simulation"))
                {
                    camTimelineScript.simRunning = false;
                    camTimelineScript.simPaused = false;
                    camTimelineScript.totalTime = 0;
                    // should the camera stay where it is or jump back to the start?
                }
                GUILayout.EndHorizontal();
            }
        }




        GUILayout.Space(30);
        camTimeline.CurrentTimeSeconds = EditorGUILayout.Slider("Current Time in Simulation (sec):", camTimeline.CurrentTimeSeconds, 0, 50);
        if (camTimeline.timelineTime.Any(val => val == camTimeline.CurrentTimeSeconds))
        {
            int idx = Array.IndexOf(camTimeline.timelineTime, camTimeline.CurrentTimeSeconds);
            EditorGUILayout.LabelField("Current Timestamp: " + (idx + 1).ToString() + "\t(Time: " + camTimeline.CurrentTimeSeconds.ToString("F4") + " sec)");
            GUILayout.Space(15);
        }
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Previous Timestamp"))
        {
            int tempTimeIndex = 0;
            bool found = false;
            for (int i = 0; i < camTimeline.timelineTime.Length; i++)
            {

                if (camTimeline.CurrentTimeSeconds <= camTimeline.timelineTime[i] && !found && camTimeline.CurrentTimeSeconds > camTimeline.timelineTime[0])
                {
                    tempTimeIndex = i - 1;
                    found = true;
                    Debug.Log("Stepped Backward!");
                }
            }
            if (camTimeline.timelineTime.Length != 0)
            {
                if (!found)
                {
                    if (camTimeline.CurrentTimeSeconds > camTimeline.timelineTime[camTimeline.timelineTime.Length - 1])
                    {
                        tempTimeIndex = camTimeline.timelineTime.Length - 1;
                        found = true;
                        Debug.Log("Stepped Backward!");
                    }
                    else if (camTimeline.CurrentTimeSeconds <= camTimeline.timelineTime[0])
                    {
                        Debug.LogError("No Previous Timestamp!");
                    }
                }
                if (found)
                {
                    camTimelineScript.JumpToPositionRotation(cam, camTimelineScript.timelineTime[tempTimeIndex], camTimelineScript.timelinePositions[tempTimeIndex], camTimelineScript.timelineRotations[tempTimeIndex]);
                }
            }
            else
            {
                Debug.LogError("There are no Timestamps!");
            }


        }
        if (GUILayout.Button("Next Timestamp"))
        {
            int tempTimeIndex = 0;
            bool found = false;
            float currentTime = camTimeline.CurrentTimeSeconds;
            int timelineLength = camTimeline.timelineTime.Length;

            for (int i = 0; i < camTimeline.timelineTime.Length; i++)
            {

                if (currentTime < camTimeline.timelineTime[i] && !found)
                {
                    tempTimeIndex = i;
                    found = true;
                    Debug.Log("Stepped Forward!");
                }
                else if (currentTime == camTimeline.timelineTime[i] && !found && currentTime != camTimeline.timelineTime[timelineLength - 1])
                {
                    tempTimeIndex = i + 1;
                    found = true;
                    Debug.Log("Stepped Forward!");
                }
            }
            if (timelineLength != 0)
            {
                if (!found && currentTime >= camTimeline.timelineTime[timelineLength - 1])
                {
                    Debug.Log("No Future Timestamp!"); //info level is suffcient, error is for things that crash functionality (Adam)
                }
            }
            else
            {
                Debug.Log("There are no Timestamps!"); //info level is suffcient, error is for things that crash functionality (Adam)
            }
            if (found)
            {
                camTimelineScript.JumpToPositionRotation(cam, camTimelineScript.timelineTime[tempTimeIndex], camTimelineScript.timelinePositions[tempTimeIndex], camTimelineScript.timelineRotations[tempTimeIndex]);
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(30);

        if (!adding && !editingFollowAvatar && !editingATimestamp)
        {
            enableButtons();
            GUILayout.Space(30);
        }
        else
        {
            GUILayout.Space(110);
        }



        if (camTimeline.gameObject.GetComponent<CameraTimelineCombo>().fileName != "" && camTimeline.gameObject.GetComponent<CameraTimelineCombo>().fileName != null)
        {
            EditorGUILayout.LabelField("Base File Loaded: \t" + camTimeline.gameObject.GetComponent<CameraTimelineCombo>().fileName);
        }
        else
        {
            EditorGUILayout.LabelField("No File Loaded.");
        }
        if (numOfTimestamps != 0)
        {
            GUILayout.Space(15);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Timeline for Camera Movement:");
            if (GUILayout.Button("Open All"))
            {
                showFoldout = Enumerable.Repeat(true, numOfTimestamps).ToArray();
            }
            if (GUILayout.Button("Close All"))
            {
                showFoldout = Enumerable.Repeat(false, numOfTimestamps).ToArray();
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.Space(15);

        Color newColor1 = Color.black;
        Color newColor2 = Color.blue;
        Color newColor3Editing = Color.green;


        // these are the actual timestamps for the Inspector
        for (int i = 0; i < numOfTimestamps; i++)
        {
            if (i < numOfTimestamps - 1)
            {
                if (camTimeline.CurrentTimeSeconds >= camTimeline.timelineTime[i] && camTimeline.CurrentTimeSeconds < camTimeline.timelineTime[i + 1])
                {
                    colorEdit = "red";
                }
                else
                {
                    colorEdit = "white";
                }
            }
            else
            {
                if (camTimeline.CurrentTimeSeconds >= camTimeline.timelineTime[i])
                {
                    colorEdit = "red";
                }
                else
                {
                    colorEdit = "white";
                }
            }
            GUILayout.Space(12.5f);
            showFoldout[i] = EditorGUILayout.Foldout(showFoldout[i], "<color=" + colorEdit + ">Timestamp " + (i+1).ToString() + "\t(Time: " + camTimeline.timelineTime[i].ToString("F4") + " sec)</color>", true, styleOn);

            if (showFoldout[i])
            {

                if (i % 2 == 0) { GUI.backgroundColor = newColor1; }
                else { GUI.backgroundColor = newColor2; }
                GUILayout.BeginVertical("box");
                GUILayout.BeginHorizontal();
                colorEdit = "white";


                if (editingTimestamp.Any(val => val == true) && !deletingTimestamp && !camTimeline.simRunning && !adding)
                {
                    if (editingTimestamp[i] == true)
                    {
                        colorEdit = "green";
                        EditorGUILayout.LabelField("<color=" + colorEdit + ">Timestamp " + (i + 1).ToString() + "</color>", styleOn);

                        GUI.backgroundColor = newColor3Editing;
                        if (GUILayout.Button("Save Current View / Time?"))
                        {
                            camTimelineScript.DeleteTimeStamp(i);
                            camTimelineScript.SaveTimeStamp();
                            editingATimestamp = false;
                            //camTimeline.enableFlying = false;
                            Debug.Log("Saved Edit of Timestamp!\n");
                        }
                        if (GUILayout.Button("Cancel"))
                        {
                            //camTimeline.enableFlying = false;
                            editingATimestamp = false;
                            editingTimestamp[i] = false;
                            Debug.Log("Cancelled Edit of Timestamp " + i.ToString() + ".\n");
                        }
                    }
                    else { EditorGUILayout.LabelField("<color=" + colorEdit + ">Timestamp " + (i + 1).ToString() + "</color>", styleOn); }
                }
                else
                {
                    EditorGUILayout.LabelField("<color=" + colorEdit + ">Timestamp " + (i + 1).ToString() + "</color>", styleOn);
                    if (!camTimeline.simRunning && !adding)
                    {
                        if (!deletingTimestamp)
                        {
                            GUILayout.BeginHorizontal();
                            if (i != numOfTimestamps - 1)
                            {
                                if (!followAvatarTimestamp[i])
                                {
                                    if (editingFollowAvatarInd == -1 && GUILayout.Button(new GUIContent("Follow Avatar", "Currently not Following.\n\nThis option tells the camera to follow the avatar between this timestamp and the next timestamp. Default distance is used behind the avatar unless edited.")))
                                    {
                                        followAvatarTimestamp[i] = true;
                                        camTimelineScript.timeDB.list[i].followAvatar = true;
                                    }
                                }
                                else
                                {
                                    if (!editingFollowAvatar)
                                    {
                                        if (GUILayout.Button(new GUIContent("Edit Follow", "Enables the user to move the camera using WASD and arrow keys. Using this option will allow the user to set the current distance and orientation relative to the Avatar as the follow distance and orientation for this timestamp.")))
                                        {
                                            editingFollowAvatar = true;
                                            editingFollowAvatarInd = i;
                                        }
                                        if (GUILayout.Button(new GUIContent("Unfollow Avatar", "Currently Following.\n\nThis option tells the camera to follow the avatar between this timestamp and the next timestamp.\n\nUnfollowing will reset the follow location and rotation to the default values.")))
                                        {
                                            followAvatarTimestamp[i] = false;
                                            camTimelineScript.timeDB.list[i].followAvatar = false;
                                            camTimelineScript.timeDB.list[i].followAvatarPos = CameraTimelineCombo.defaultFollowAvatarPos;
                                            camTimelineScript.timeDB.list[i].followAvatarRot = Quaternion.Inverse(defaultAvatarRot) * Quaternion.Euler(CameraTimelineCombo.defaultFollowAvatarRot);
                                        }
                                    }
                                    else if (editingFollowAvatarInd == i)
                                    {
                                        if (GUILayout.Button(new GUIContent("Save Follow", "Sets the follow distance and orientation relative to Avatar.")))
                                        {
                                            followAvatarPosRot(i);
                                            editingFollowAvatar = false;
                                            editingFollowAvatarInd = -1;
                                        }
                                        if (GUILayout.Button(new GUIContent("Cancel", "Cancels the Edit Follow operation.")))
                                        {
                                            editingFollowAvatar = false;
                                            editingFollowAvatarInd = -1;
                                        }
                                    }


                                }

                            }

                            if (!editingFollowAvatar && GUILayout.Button(new GUIContent("Edit", "Allows the user to edit the time, position, or rotation for this timestamp")))
                            {
                                editingTimestamp[i] = true;
                                editingATimestamp = true;
                                //camTimeline.enableFlying = true;
                                Debug.Log("Editing Timestamp " + i.ToString() + "...\n");
                            }
                            GUILayout.EndHorizontal();
                        }
                        else
                        {
                            if (GUILayout.Button("Delete?"))
                            {
                                camTimelineScript.DeleteTimeStamp(i);
                                numOfTimestamps--;
                                wasDeleted = true;
                            }
                        }
                    }
                    else
                    {
                        if (camTimeline.timeDB.list[i].followAvatar)
                        {
                            GUILayout.Label("<color=white><i>Following</i></color>", styleOn);
                        }
                        else
                        {
                            GUILayout.Label("<color=white><i>Not Following</i></color>", styleOn);
                        }
                    }
                }





                GUILayout.EndHorizontal();

                //textColor = EditorGUILayout.ColorField(textColor); user can select a color

                if (!wasDeleted)
                {
                    //EditorGUILayout.LabelField(new GUIContent("Time: \t\t" + camTimeline.timelineTime[i].ToString("F4"), "The time of the simulation for this timestamp."));
                    EditorGUILayout.Vector3Field(new GUIContent("Position:", "The position of the camera at this timestamp."), camTimeline.timelinePositions[i]);
                    EditorGUILayout.Vector3Field(new GUIContent("Rotation:", "The rotation of the camera at this timestamp."), camTimeline.timelineRotations[i]);
                }
                else { wasDeleted = false; }

                GUILayout.EndVertical();
                GUILayout.Space(12.5f);
            }
            
        }
        // ------------------------------------------------

        GUI.backgroundColor = Color.white;
        GUILayout.Space(30);
        camTimeline.CurrentTimeSeconds = EditorGUILayout.Slider("Current Time in Simulation (sec):", camTimeline.CurrentTimeSeconds, 0, 50);

        GUILayout.Space(30);

        // Drawing lines between the points while the Game is not running (aka, while in Editor Mode)
        if (!Application.isPlaying)
        {
            camTimelineScript.timelinePositionsInterpolated = camTimelineScript.SmoothLinePosition(camTimelineScript.timelinePositions, camTimelineScript.timelineTime, 0.0166f);
            camTimelineScript.timelineRotationsInterpolated = camTimelineScript.SmoothLineRotation(camTimelineScript.timelineRotations, camTimelineScript.timelineTime, 0.0166f);
            camTimelineScript.drawNewPositionLines();

            if (!editingTimestamp.Any(val => val == true) && !enableFlying && !enableStaticCam && !editingFollowAvatar && !editingATimestamp)
            {
                if (numOfTimestamps != 0)
                {
                    cam.transform.position = new Vector3(camTimelineScript.curveXPosition.Evaluate(camTimelineScript.CurrentTimeSeconds), camTimelineScript.curveYPosition.Evaluate(camTimelineScript.CurrentTimeSeconds), camTimelineScript.curveZPosition.Evaluate(camTimelineScript.CurrentTimeSeconds));
                    cam.transform.eulerAngles = new Vector3(camTimelineScript.curveXRotation.Evaluate(camTimelineScript.CurrentTimeSeconds), camTimelineScript.curveYRotation.Evaluate(camTimelineScript.CurrentTimeSeconds), camTimelineScript.curveZRotation.Evaluate(camTimelineScript.CurrentTimeSeconds));
                }
                else
                {
                    cam.transform.position = camTimelineScript.defaultPosition;
                    cam.transform.eulerAngles = camTimelineScript.defaultRotation;
                }

            }
            else if (enableStaticCam)
            {
                if (camTimelineScript.ind >= 0 && !adding && camTimeline.timeDB.listCam.Count>0)
                {
                    cam.transform.position = camTimeline.timeDB.listCam[camTimelineScript.ind].camPos;
                    cam.transform.eulerAngles = camTimeline.timeDB.listCam[camTimelineScript.ind].camRot;
                }
            }
        }


    }

    void followAvatarPosRot(int i)
    {
        camTimeline.timeDB.list[i].followAvatarPos = AvatarObj.transform.InverseTransformPoint(cam.transform.position);
        camTimeline.timeDB.list[i].followAvatarRot = Quaternion.Inverse(AvatarObj.transform.rotation) * cam.transform.rotation;
        
        
        //camTimeline.timeDB.list[i].followAvatarRot = AvatarObj.transform.rotation * Quaternion.Inverse(cam.transform.rotation);

        //if (Mathf.Sign(cam.transform.forward.z) == -Math.Sign(AvatarObj.transform.forward.z) || Mathf.Sign(cam.transform.right.x) == -Mathf.Sign(AvatarObj.transform.right.x))
        //{
        //    camTimeline.timeDB.list[i].followAvatarRot = new Vector3(camTimeline.timeDB.list[i].followAvatarRot.x, camTimeline.timeDB.list[i].followAvatarRot.y * -1, camTimeline.timeDB.list[i].followAvatarRot.z);
        //}
    }

    void enableButtons()
    {


        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("<b><color=white>Timestamps</color></b>",styleOn);
        GUILayout.FlexibleSpace();
        GUILayout.FlexibleSpace();
        GUILayout.Label("<b><color=white>Full Timeline</color></b>",styleOn);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        if (GUILayout.Button("Add"))
        {
            camTimelineScript.SaveTimeStamp();
            camTimelineScript.drawNewPositionLines();
        }

        if (!deletingTimestamp)
        {
            if (GUILayout.Button("Delete"))
            {
                // essentially: if (camTimeline.CurrentTimeSeconds IS IN camTimeline.timelineTime) --> call the DeleteTimeStamp(); ELSE {Debug.Error("Must be on a timestamp to delete a timestamp.")}
                deletingTimestamp = true;
            }
            
        }
        else
        {
            if (GUILayout.Button("Stop Deleting"))
            {
                deletingTimestamp = false;
            }
        }
        GUILayout.EndVertical();





        GUILayout.BeginVertical();
        if (GUILayout.Button("Save"))
        {
            camTimelineScript.SaveTimes();
        }
        if (GUILayout.Button("Load"))
        {
            if (camTimelineScript.LoadTimes())
            {
                numOfTimestamps = camTimeline.timelineTime.Length;
                editingTimestamp = new bool[numOfTimestamps];
                camTimelineScript.drawNewPositionLines();
            }


        }
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();
        GUILayout.Label("");
        if (GUILayout.Button("Clear All"))
        {
            editingTimestamp = new bool[0];
            numOfTimestamps = 0;
            camTimelineScript.ResetTimeStamps();
            camTimelineScript.drawNewPositionLines();
        }
        GUILayout.Label("");
        GUILayout.EndHorizontal();

        

    }

    //void cameraPosition()
    //{
    //    foldoutPosition = EditorGUILayout.Foldout(foldoutPosition, "Current Camera Position", true);
    //    if (foldoutPosition)
    //    {
    //        var level = EditorGUI.indentLevel;
    //        EditorGUI.indentLevel++;
    //        camTimeline.CurrentCameraPosition.x = EditorGUILayout.Slider("X: ", cam.transform.position.x, -15f, 15f);
    //        camTimeline.CurrentCameraPosition.y = EditorGUILayout.Slider("Y: ", cam.transform.position.y, -15f, 15f);
    //        camTimeline.CurrentCameraPosition.z = EditorGUILayout.Slider("Z: ", cam.transform.position.z, -15f, 15f);
    //        EditorGUI.indentLevel = level;
    //    }
    //    cam.transform.position = camTimeline.CurrentCameraPosition;
    //}
    //void cameraRotation()
    //{
    //    foldoutRotation = EditorGUILayout.Foldout(foldoutRotation, "Current Camera Rotation", true);
    //    if (foldoutRotation)
    //    {
    //        var level = EditorGUI.indentLevel;
    //        EditorGUI.indentLevel++;

    //        //Vector3 temp;
    //        //if (cam.transform.rotation.eulerAngles.y >= 0 && cam.transform.rotation.eulerAngles.y < 180)
    //        //{
    //        //    temp.y = cam.transform.rotation.eulerAngles.y;
    //        //}
    //        //else
    //        //{
    //        //    temp.y = cam.transform.rotation.eulerAngles.y + 360;
    //        //}


    //        camTimeline.CurrentCameraRotation.x = EditorGUILayout.Slider("X: ", cam.transform.rotation.eulerAngles.x, -1f, 360f);
    //        camTimeline.CurrentCameraRotation.y = EditorGUILayout.Slider("Y: ", cam.transform.rotation.eulerAngles.y, -1f, 360f);
    //        camTimeline.CurrentCameraRotation.z = EditorGUILayout.Slider("Z: ", cam.transform.rotation.eulerAngles.z, -1f, 360f);
    //        EditorGUI.indentLevel = level;
    //    }
    //    cam.transform.eulerAngles = camTimeline.CurrentCameraRotation;
    //}




}
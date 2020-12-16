using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MMIUnity.TargetEngine.Scene;
using System.Linq;
using UnityEditor;
using System.IO;
using System;

[ExecuteInEditMode] //needed to make sure cameras get initalized on adding the script to object (Adam)
public class PhotoboothScriptSceneGen: MonoBehaviour
{
    [Header("Main Camera of the Scene")]
    [SerializeField] private Camera mainCam;
    [SerializeField] private Camera EdgeDetectCam;
    public static Camera edgeDetectPublicCam;

    //[Tooltip("If number of picture is set to 1, it will only take 1 picture at 0 degrees and not rotate in that direction.")]
    [Header("Number of pictures taken. (Yaw: 360, Pitch: 180, Roll: 90 Degrees)")]
    [VectorLabels("Yaw", "Pitch", "Roll")]
    [SerializeField] Vector3 NumberOfPictures = new Vector3(8f, 5f, 3f);
    [VectorLabels("Minimum", "Maximum")]

    public static float RangeOfAnglesYaw = 360f;
    Vector2 RangeOfAnglesPitch = new Vector2(0, 180);
    public static float RangeOfAnglesRoll = 90f;

    [Header("Conditions for Symmetry Check")]
    [Tooltip("This provides a little percentage range to the grayscale values when checking symmetry. (Default: 0.05)")]
    [SerializeField] float AcceptableGrayRange = 0.05f;
    [Tooltip("When orienting the object, accepting pixel match ratio which means it is symmetric at that angle. (Default: 0.75)")]
    [SerializeField] float AcceptablePixelMatchRatio = 0.75f;
    [Tooltip("This is the percentage of matching pixels where the script will not check for the 'best symmetry' angle. (Default: 0.9)")]
    [SerializeField] float ExceptionPixelMatchRatio = 0.9f;
    [Tooltip("For symmetry, how many pixels to increment by when comparing. (Default: 2 [every other pixel])")]
    [SerializeField] float PixelSkipSize = 2;
    [Tooltip("Scale for the Screenshot saved image. (Default: 0.5)")]
    [SerializeField] float ScreenshotScale = 0.5f;
    [Tooltip("This defines how many of the top 'Best Views' will be saved. (Default: 3)")]
    [SerializeField] int NumBestViewPics = 3;
    public static float pixelskip;
    public static float shotscale;
    public static int numbestpics;

    [Header("Weights for Best View")]
    [SerializeField] private float ProjectedAreaWeight = 0.25f;
    [SerializeField] private float VisibleSurfaceAreaWeight = 0.40f;
    [SerializeField] private float CenterOfMassWeight = 0.1f;
    [SerializeField] private float SymmetryWeight = 0.05f;
    [SerializeField] private float VisibleEdgesWeight = 0.10f;
    [SerializeField] private float MeshTrianglesWeight = 0.10f;

    public static float PA_weight;
    public static float VSA_weight;
    public static float CoM_weight;
    public static float sym_weight;
    public static float edges_weight;
    public static float triangles_weight;

    public static Scene originalScene;
    public static string originalSceneStr;
    public static Scene newScene;
    public static string newSceneStr = "Photobooth";
    private IEnumerable<GameObject> partsAndToolsObjs;
    private static List<GameObject> gameObjs;
    public static Camera photoCamOriginal;
    public static GameObject lightGameObject;
    public static int currentObjIndex = 0;
    //private static bool takingPictures;
    public static int numOfObjs;

    public static float accGrayRng;
    public static float accPixRat;
    public static float excPixRat;
    static public int numYawPics;
    static public int numPitchPics;
    static public int numRollPics;
    static public int numOfPicsPerObject;
    static public Vector2 angUpDownPics;
    //static public Texture2D tex;
    private bool picCopied = false;

    public static bool PhotoboothRunning;
    public bool startPhotoShoot = false;

    static public Vector3 tempSpot;

    public bool[] scriptEnabled;
    public bool FinishedPhotoShoot = false;

    
    void OnEnable() //auto initialization of main and edge detect cam if they are not manually overriden (Adam)
    {
        
        if (mainCam==null)
        mainCam = Camera.main;
        if (EdgeDetectCam == null)
        {
            var a = Resources.Load<Camera>("Edge Detection Camera");
                if (a!=null)
                EdgeDetectCam = a;
        }
    }
    


    void Start()
    {
        PA_weight = ProjectedAreaWeight;
        VSA_weight = VisibleSurfaceAreaWeight;
        CoM_weight = CenterOfMassWeight;
        sym_weight = SymmetryWeight;
        edges_weight = VisibleEdgesWeight;
        triangles_weight = MeshTrianglesWeight;
    }

    public void StartTakingPictures()
    {
        partsAndToolsObjs = GetPartsAndToolObjs();

        if (mainCam.gameObject.GetComponent<CameraTimelineCombo>())
            mainCam.gameObject.GetComponent<CameraTimelineCombo>().enableFlying = true;

        if (currentObjIndex != 0)
            removePelvisCenters();

        PhotoboothRunning = true;
        VariableScript.sw_total.Start();
        StartCoroutine(startPhotoSession());
    }

    public void SetDoneFlag()
    {
        string newPath = Application.dataPath + "/../" + "/Screenshots/";
        File.WriteAllText(newPath + "done.tmp", "Miniatures are ready");
    }

    public void ClearDoneFlag()
    {
        string newPath = Application.dataPath + "/../" + "/Screenshots/";
        if (File.Exists(newPath + "done.tmp"))
            File.Delete(newPath + "done.tmp");
    }

    public bool isDoneFlagSet()
    {
        string newPath = Application.dataPath + "/../" + "/Screenshots/";
        return (File.Exists(newPath + "done.tmp"));
    }

    void Update()
    {
        if (!Application.isPlaying) //in editor mode simply ignore that function (Adam)
        {
             if (isDoneFlagSet())
             {
                RestoreMonoScriptsState();
                ClearDoneFlag();
             }
            return;
        }

        if (Input.GetKeyUp(KeyCode.Space) || TakePic.readyForNextPart == true || startPhotoShoot == true)
        {
            StartTakingPictures();
            startPhotoShoot = false;
        }
        if (Input.GetKeyUp(KeyCode.KeypadEnter))
        {
            //objToRotate = pedalCar;
            //float timeToRotate = 6f;
            //float timeStep = 0.1f;
            //rotateObjOverTime(rotateXYZ, timeToRotate, timeStep);
            //objToRotate.transform.Rotate(degreesToRotate / (timeToRotate / timeStep), Space.Self);

            //char[] items = new char[] { 'A', 'B', 'C', 'D', 'E' };

            //foreach (IEnumerable<char> permutation in PermuteUtils.Permute(items, 3))
            //{
            //    string s = "[";
            //    //Console.Write("[");
            //    foreach (char c in permutation)
            //    {
            //        //Console.Write(" " + c);
            //        s += " " + c;
            //    }
            //    //Console.WriteLine(" ]");
            //    s += " ]";
            //    Debug.Log(s);
            //}

            UnitySimulationStart();
            //StartCoroutine(visibleEdgesLength());
        }
    }

    IEnumerator startPhotoSession()
    {
        VariableScript.sw_photobooth.Start();
        if (currentObjIndex == 0)
        {

            accGrayRng = AcceptableGrayRange;
            accPixRat = AcceptablePixelMatchRatio;
            excPixRat = ExceptionPixelMatchRatio;
            numOfObjs = gameObjs.Count;
            pixelskip = PixelSkipSize;
            shotscale = ScreenshotScale;
            numbestpics = NumBestViewPics;

            originalScene = SceneManager.GetActiveScene();
            originalSceneStr = originalScene.name;
            newCameraAndLight();
            newScene = SceneManager.CreateScene(newSceneStr);
            yield return new WaitUntil(() => newScene.name != null);

            SceneManager.LoadSceneAsync(newSceneStr, LoadSceneMode.Additive);
            numYawPics = (int)NumberOfPictures.x;
            numPitchPics = (int)NumberOfPictures.y;
            numRollPics = (int)NumberOfPictures.z;
            angUpDownPics = RangeOfAnglesPitch;
            numOfPicsPerObject = numYawPics * numPitchPics * numRollPics;

            VariableScript.projectedArea = new float[numOfPicsPerObject * numOfObjs];
            VariableScript.ratioVisibleSurfaceArea = new float[numOfPicsPerObject * numOfObjs];
            VariableScript.QualityOfView = new float[numOfPicsPerObject * numOfObjs];
            VariableScript.centerOfMassX = new float[numOfPicsPerObject * numOfObjs];
            VariableScript.centerOfMassY = new float[numOfPicsPerObject * numOfObjs];
            VariableScript.symmetryInView = new float[numOfPicsPerObject * numOfObjs];
            VariableScript.meshTriangles = new float[numOfPicsPerObject * numOfObjs];
            VariableScript.visibleEdges = new float[numOfPicsPerObject * numOfObjs];

            VariableScript.pictureFilePath = new string[numOfPicsPerObject * numOfObjs];
            VariableScript.pictureFileName = new string[numOfPicsPerObject * numOfObjs];
            VariableScript.numOfPicturesPerObj = new int[numOfObjs];

            // deleting any old folder of "best views", a new folder will be created when the "saveBestViews" function is called
            string newPath = Application.dataPath + "/../" + "/Screenshots/";
            if (System.IO.Directory.Exists(newPath))
                System.IO.Directory.Delete(newPath, true);

            // setting this when I want to test a specific object, it will go from this object through the end of the list, e.g. 2 = hammer, 3 = ratchet, 7 = nut, 9 = screw
            //currentObjIndex = 12;
        }
        else
        {
            originalScene = SceneManager.GetSceneByName(originalSceneStr);
            SceneManager.SetActiveScene(originalScene);
        }
        if (currentObjIndex < numOfObjs)
        {
            copyPasteObj(gameObjs[currentObjIndex]);
            SceneManager.SetActiveScene(newScene);
            SceneManager.UnloadSceneAsync(originalScene);
            currentObjIndex++;
            TakePic.readyForPic = true;
            VariableScript.sw_photobooth.Stop();
        }
        else if (currentObjIndex == numOfObjs)
        {
            currentObjIndex = 0;
            SceneManager.MoveGameObjectToScene(photoCamOriginal.gameObject, SceneManager.GetActiveScene());
            SceneManager.MoveGameObjectToScene(lightGameObject, SceneManager.GetActiveScene());
            TakePic.readyForNextPart = false;
            Destroy(photoCamOriginal.gameObject);
            Destroy(lightGameObject.gameObject);
            Destroy(edgeDetectPublicCam.gameObject);
            SceneManager.UnloadSceneAsync(newScene);

            NumberOfPictures.x = numYawPics;
            NumberOfPictures.y = numPitchPics;
            NumberOfPictures.z = numRollPics;
            RangeOfAnglesPitch = angUpDownPics;

            VariableScript.sw_photobooth.Stop();
            VariableScript.sw_total.Stop();
            double totalTime = VariableScript.sw_total.Elapsed.TotalMilliseconds;
            Debug.Log("Total Time: " + totalTime.ToString("F4"));
            Debug.Log("Photobooth Time: " + VariableScript.sw_photobooth.Elapsed.TotalMilliseconds.ToString("F4") + " (" + (VariableScript.sw_photobooth.Elapsed.TotalMilliseconds / totalTime).ToString("F2") + ")");
            Debug.Log("Take Pic Time: " + VariableScript.sw_takepic.Elapsed.TotalMilliseconds.ToString("F4") + " (" + (VariableScript.sw_takepic.Elapsed.TotalMilliseconds / totalTime).ToString("F2") + ")");
            
            Debug.Log("--Start Time: " + VariableScript.sw_starttakepic.Elapsed.TotalMilliseconds.ToString("F4") + " (" + (VariableScript.sw_starttakepic.Elapsed.TotalMilliseconds / totalTime).ToString("F2") + ")");
            Debug.Log("--Light Symmetry Time: " + VariableScript.sw_lightsym.Elapsed.TotalMilliseconds.ToString("F4") + " (" + (VariableScript.sw_lightsym.Elapsed.TotalMilliseconds / totalTime).ToString("F2") + ")");
            Debug.Log("--Euler Angles Time: " + VariableScript.sw_eulang.Elapsed.TotalMilliseconds.ToString("F4") + " (" + (VariableScript.sw_eulang.Elapsed.TotalMilliseconds / totalTime).ToString("F2") + ")");
            Debug.Log("--Textures Time: " + VariableScript.sw_textures.Elapsed.TotalMilliseconds.ToString("F4") + " (" + (VariableScript.sw_textures.Elapsed.TotalMilliseconds / totalTime).ToString("F2") + ")");
            Debug.Log("--Raycasting Time: " + VariableScript.sw_raycast.Elapsed.TotalMilliseconds.ToString("F4") + " (" + (VariableScript.sw_raycast.Elapsed.TotalMilliseconds / totalTime).ToString("F2") + ")");
            Debug.Log("--Quality of View Time: " + VariableScript.sw_qualityofview.Elapsed.TotalMilliseconds.ToString("F4") + " (" + (VariableScript.sw_qualityofview.Elapsed.TotalMilliseconds / totalTime).ToString("F2") + ")");
            Debug.Log("--Unloading Time: " + VariableScript.sw_unloading.Elapsed.TotalMilliseconds.ToString("F4") + " (" + (VariableScript.sw_unloading.Elapsed.TotalMilliseconds / totalTime).ToString("F2") + ")");
            Debug.Log("--Normalize Time: " + VariableScript.sw_normalize.Elapsed.TotalMilliseconds.ToString("F4") + " (" + (VariableScript.sw_normalize.Elapsed.TotalMilliseconds / totalTime).ToString("F2") + ")");
            Debug.Log("--Save Pics Time: " + VariableScript.sw_savepic.Elapsed.TotalMilliseconds.ToString("F4") + " (" + (VariableScript.sw_savepic.Elapsed.TotalMilliseconds / totalTime).ToString("F2") + ")");
            Debug.Log("--End Scene Time: " + VariableScript.sw_endscene.Elapsed.TotalMilliseconds.ToString("F4") + " (" + (VariableScript.sw_endscene.Elapsed.TotalMilliseconds / totalTime).ToString("F2") + ")");
            Debug.Log("Investigation Time: " + VariableScript.sw_investigate.Elapsed.TotalMilliseconds.ToString("F4") + " (" + (VariableScript.sw_investigate.Elapsed.TotalMilliseconds / totalTime).ToString("F2") + ")");
            
            PhotoboothRunning = false;
            SetDoneFlag();
            EditorApplication.ExecuteMenuItem("Edit/Play");
        }

    }

    public void StoreMonoBehaviorScriptsState()
    {
        var scripts = GameObject.FindObjectsOfType<MonoBehaviour>();
        scriptEnabled = new bool[scripts.Length];
        for (int i = 0; i < scripts.Length; i++)
        {
            scriptEnabled[i] = scripts[i].enabled;

            if (scripts[i].GetType().Name == "MMISceneObject") //scripts to enable
                scripts[i].enabled = true;
            else
            {
                /*                        if (((scripts[i].GetType().BaseType.Name == "MonoBehaviour") || //scripts to disable
                                             (scripts[i].GetType().BaseType.Name == "MMIAvatar") ||
                                             (scripts[i].GetType().BaseType.Name == "AvatarBehavior")) &&
                                            (scripts[i].GetType().Name != "PhotoboothScriptSceneGen"))
                                            scripts[i].enabled = false;*/
                if ((scripts[i].GetType().BaseType.Name == "MonoBehaviour") && (scripts[i].GetType().Name != "PhotoboothScriptSceneGen"))
                    scripts[i].enabled = false;
                if (scripts[i].GetType().IsSubclassOf(typeof(MonoBehaviour)) && (scripts[i].GetType().Name != "PhotoboothScriptSceneGen"))
                    scripts[i].enabled = false;

                //if (((scripts[i].GetType().BaseType.Name == "MMIAvatar") || (scripts[i].GetType().BaseType.Name == "AvatarBehavior")) &&
                //(scripts[i].GetType().Name != "PhotoboothScriptSceneGen"))
                //   scripts[i].enabled = false;
            }
        }
    }

    private void RestoreMonoScriptsState()
    {
        var scripts = GameObject.FindObjectsOfType<MonoBehaviour>();
        for (int i = 0; i < scripts.Length; i++)
           scripts[i].enabled = scriptEnabled[i];
    }

    private void removePelvisCenters()
    {
        UnityEngine.Object[] tempObjs = newScene.GetRootGameObjects();
        foreach (UnityEngine.Object obj in tempObjs)
        {
            Destroy(obj);
        }

    }

    GameObject[] GetPartsAndToolObjs()
    {
        Component[] lst = this.GetComponentsInChildren<MMISceneObject>();
        //lst[0].GetComponent<MMISceneObject>().Constraints
        gameObjs = new List<GameObject>();
        bool hasAnyRenderers = false;
        foreach (MMISceneObject cmp in lst) //go through MMIScene objects and take only those of type "part" (Adam)
            if (cmp.Type == MMISceneObject.Types.Part)
            {
                foreach (Transform child in cmp.transform)
                {
                    /*                    if (child.gameObject.GetComponent<Renderer>())
                                        {

                                            hasAnyRenderers = true;
                                        }*/
                    //this way it is more compact and does the same thing you "or" the hasAnyRenderes with new value, so essentially you update it to true when object has renderer and do not change it otherwise (Adam)
                    hasAnyRenderers |= child.gameObject.GetComponent<Renderer>();
                }
                 //&& (child.gameObject.GetComponent<MMISceneObject>() == null)); /we want only children that do not have MMISceneObject attached to it (grasping points, finger placement etc.) - thus the second part of the statement (Adam)
                
                if (hasAnyRenderers || cmp.GetComponent<Renderer>())
                {
                    gameObjs.Add((GameObject)cmp.gameObject);
                }
                hasAnyRenderers = false;
            }
        return gameObjs.ToArray();
    }

    private void newCameraAndLight()
    {
        photoCamOriginal = Instantiate(mainCam);
        edgeDetectPublicCam = Instantiate(EdgeDetectCam);
        edgeDetectPublicCam.GetComponent<AudioListener>().enabled = false;
        edgeDetectPublicCam.enabled = false;
        GameObject.DontDestroyOnLoad(edgeDetectPublicCam);
        photoCamOriginal.name = "PhotoCam";
        photoCamOriginal.orthographic = true;
        photoCamOriginal.enabled = false;
        photoCamOriginal.nearClipPlane = 0;
        photoCamOriginal.farClipPlane = 10000;
        photoCamOriginal.GetComponent<AudioListener>().enabled = false;
        //photoCamOriginal.backgroundColor = Color.green;
        photoCamOriginal.GetComponent<Camera>().clearFlags = mainCam.GetComponent<Camera>().clearFlags;
        GameObject.DontDestroyOnLoad(photoCamOriginal);
        mainCam.enabled = false;
        photoCamOriginal.enabled = true;
        lightGameObject = Instantiate(GameObject.Find("Directional Light"));
        GameObject.DontDestroyOnLoad(lightGameObject);
    }

    private GameObject copyPasteObj(GameObject obj)
    {
        GameObject tempObj = Instantiate(obj);
        tempObj.AddComponent<TakePic>();
        tempObj.name = tempObj.name.Substring(0, tempObj.name.Length - 7);
        GameObject.DontDestroyOnLoad(tempObj);
        TakePic.currentObj = tempObj;
        return tempObj;
    }

    //IEnumerator hist()
    //{
    //    yield return new WaitForEndOfFrame();
    //    tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
    //    tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
    //    tex.Apply();
    //    yield return new WaitForEndOfFrame();
    //}




    //public static void normalizedQualities()
    //{
    //    int total = 0;
    //    for (int i = 0; i < VariableScript.numOfPicturesPerObj.Length; i++)
    //    {
    //        int numpics = VariableScript.numOfPicturesPerObj[i];
    //        // Projected Area Normalization
    //        float tempMax_ProjArea;
    //        float[] subsetOfQualityOfView_ProjArea = new float[numpics];
    //        Array.Copy(VariableScript.projectedArea, i, subsetOfQualityOfView_ProjArea, 0, numpics);
    //        tempMax_ProjArea = subsetOfQualityOfView_ProjArea.Max();
    //        if (tempMax_ProjArea != 0)
    //        {
    //            for (int q = 0; q < numpics; q++)
    //            {
    //                subsetOfQualityOfView_ProjArea[q] /= tempMax_ProjArea;
    //                VariableScript.projectedArea[q + i] = subsetOfQualityOfView_ProjArea[q];
    //            }
    //        }

    //        // Surface Area Normalization
    //        float tempMax_SurfArea;
    //        float[] subsetOfQualityOfView_SurfArea = new float[numpics];
    //        Array.Copy(VariableScript.ratioVisibleSurfaceArea, i, subsetOfQualityOfView_SurfArea, 0, numpics);
    //        tempMax_SurfArea = subsetOfQualityOfView_SurfArea.Max();
    //        if (tempMax_SurfArea != 0)
    //        {
    //            for (int q = 0; q < numpics; q++)
    //            {
    //                subsetOfQualityOfView_SurfArea[q] /= tempMax_SurfArea;
    //                VariableScript.ratioVisibleSurfaceArea[q + i] = subsetOfQualityOfView_SurfArea[q];
    //            }
    //        }

    //        // Center Of Mass Normalization (different than others because this can be negative and we want the lowest CoM, so we have to invert the values by doing -1 and taking the absolute value)
    //        // Y-Direction
    //        float tempMax_CoM_Y;
    //        float[] subsetOfQualityOfView_CoM_Y = new float[numpics];
    //        Array.Copy(VariableScript.centerOfMassY, i, subsetOfQualityOfView_CoM_Y, 0, numpics);
    //        tempMax_CoM_Y = subsetOfQualityOfView_CoM_Y.Max();
    //        if (tempMax_CoM_Y != 0)
    //        {
    //            for (int q = 0; q < numpics; q++)
    //            {
    //                subsetOfQualityOfView_CoM_Y[q] /= tempMax_CoM_Y;
    //                VariableScript.centerOfMassY[q + i] = Mathf.Abs(subsetOfQualityOfView_CoM_Y[q] - 1);
    //            }
    //        }
    //        // X-Direction
    //        float tempMax_CoM_X;
    //        float[] subsetOfQualityOfView_CoM_X = new float[numpics];
    //        Array.Copy(VariableScript.centerOfMassX, i, subsetOfQualityOfView_CoM_X, 0, numpics);
    //        for (int q = 0; q < numpics; q++)
    //        {
    //            subsetOfQualityOfView_CoM_X[q] = Mathf.Abs(subsetOfQualityOfView_CoM_X[q]);
    //        }
    //        tempMax_CoM_X = subsetOfQualityOfView_CoM_X.Max();
    //        if (tempMax_CoM_X != 0)
    //        {
    //            for (int q = 0; q < numpics; q++)
    //            {
    //                subsetOfQualityOfView_CoM_X[q] /= tempMax_CoM_X;
    //                VariableScript.centerOfMassX[q + i] = Mathf.Abs(subsetOfQualityOfView_CoM_X[q] - 1);
    //            }
    //        }



    //        total += numpics;
    //    }



    //    //float tempMax;

    //    //// Projected Area Normalization
    //    //for (int i = 0; i < VariableScript.projectedArea.Length; i += numOfPicsPerObject)
    //    //{
    //    //    float[] subsetOfQualityOfView = new float[numOfPicsPerObject];
    //    //    Array.Copy(VariableScript.projectedArea, i, subsetOfQualityOfView, 0, numOfPicsPerObject);
    //    //    tempMax = subsetOfQualityOfView.Max();
    //    //    for (int q = 0; q < numOfPicsPerObject; q++)
    //    //    {
    //    //        subsetOfQualityOfView[q] /= tempMax;
    //    //        VariableScript.projectedArea[q + i] = subsetOfQualityOfView[q];
    //    //    }
    //    //}



    //    //// Surface Area Normalization
    //    //for (int i = 0; i < VariableScript.ratioVisibleSurfaceArea.Length; i += numOfPicsPerObject)
    //    //{
    //    //    float[] subsetOfQualityOfView = new float[numOfPicsPerObject];
    //    //    Array.Copy(VariableScript.ratioVisibleSurfaceArea, i, subsetOfQualityOfView, 0, numOfPicsPerObject);
    //    //    tempMax = subsetOfQualityOfView.Max();
    //    //    for (int q = 0; q < numOfPicsPerObject; q++)
    //    //    {
    //    //        subsetOfQualityOfView[q] /= tempMax;
    //    //        VariableScript.ratioVisibleSurfaceArea[q + i] = subsetOfQualityOfView[q];
    //    //    }
    //    //}




    //    //// Center Of Mass Normalization (different than others because this can be negative and we want the lowest CoM, so we have to invert the values by doing -1 and taking the absolute value)
    //    //for (int i = 0; i < VariableScript.centerOfMassY.Length; i += numOfPicsPerObject)
    //    //{
    //    //    float[] subsetOfQualityOfView = new float[numOfPicsPerObject];
    //    //    Array.Copy(VariableScript.centerOfMassY, i, subsetOfQualityOfView, 0, numOfPicsPerObject);
    //    //    tempMax = subsetOfQualityOfView.Max();
    //    //    for (int q = 0; q < numOfPicsPerObject; q++)
    //    //    {
    //    //        subsetOfQualityOfView[q] /= tempMax;
    //    //        VariableScript.centerOfMassY[q + i] = Mathf.Abs(subsetOfQualityOfView[q] - 1);
    //    //    }
    //    //}
    //    //for (int i = 0; i < VariableScript.centerOfMassX.Length; i += numOfPicsPerObject)
    //    //{
    //    //    float[] subsetOfQualityOfView = new float[numOfPicsPerObject];
    //    //    Array.Copy(VariableScript.centerOfMassX, i, subsetOfQualityOfView, 0, numOfPicsPerObject);
    //    //    for (int q = 0; q < numOfPicsPerObject; q++)
    //    //    {
    //    //        subsetOfQualityOfView[q] = Mathf.Abs(subsetOfQualityOfView[q]);
    //    //    }
    //    //    tempMax = subsetOfQualityOfView.Max();
    //    //    for (int q = 0; q < numOfPicsPerObject; q++)
    //    //    {
    //    //        subsetOfQualityOfView[q] /= tempMax;
    //    //        VariableScript.centerOfMassX[q + i] = Mathf.Abs(subsetOfQualityOfView[q] - 1);
    //    //    }
    //    //}

    //}

    private void saveBestViews(string filePath, string fileName, string newPath)
    {
        // load each file image path from variableScript string variable
        //byte[] bytes = null;

        //if (File.Exists(filePath))
        //{
        //    bytes = File.ReadAllBytes(filePath);
        //}

        

        if (!System.IO.Directory.Exists(newPath))
            System.IO.Directory.CreateDirectory(newPath);

        
        newPath += fileName;
        

        //// then immediately save in a new folder called "Best Views"
        bool finisedSaving()
        {
            //it's not enough to just check that the file exists, since it doesn't mean it's finished saving
            //we have to check if it can actually be opened
            Texture2D image;
            image = new Texture2D(Screen.width, Screen.height);
            bool imageLoadSuccess = image.LoadImage(System.IO.File.ReadAllBytes(newPath));
            Destroy(image);
            return imageLoadSuccess;
        }

        //FileUtil.CopyFileOrDirectory(filePath, newPath);
        if (fileName != null && fileName != "")
        {
            FileUtil.ReplaceFile(filePath, newPath);
            //File.WriteAllBytes(newPath, bytes);
            while (!finisedSaving()) { }
        }

        picCopied = true;


    }





    







    // ================================================== List of Arrays of Weights ================================================================
    // Exclusively used for Unity Simulation to try to find the best weights to use


    // same method as here: https://stackoverflow.com/questions/34970848/find-all-combination-that-sum-to-n-with-multiple-lists/34971783#34971783
    int previousIndex = 0;
    int currentIdx = 1;
    int[] currentCuts; 
    List<float[]> listWeights = new List<float[]>();

    private void UnitySimulationStart()
    {
        Debug.Log("Started!");


        //  Change these in UnitySimulation depending on the increments of the weights we want to test and the number of weights that need to be compared
        int UnitySimulationWeightIncrements = 5;
        int numOfWeights = 3;

        int[] weightRange = Enumerable.Range(0, (100 / UnitySimulationWeightIncrements + 1)).Select(x => x * UnitySimulationWeightIncrements).ToArray();//new int[] { 0, 10, 20, 30 };

        CombinationsWithRepitition(weightRange, numOfWeights);
        foreach (float[] x in listWeights)
        {
            string s = "(";
            for (int i = 0; i < numOfWeights+1; i++)
            {
                s += x[i].ToString() + " ";
            }
            s += ")";
            Debug.Log(s);
        }

        Debug.Log("Finished!");
    }

    private void CombinationsWithRepitition(int[] input, int length)
    {
        currentCuts = new int[length];
        if (length <= 0)
            listWeights = new List<float[]>();
        else
        {
            for (int q = 0; q < input.Count(); q++)
            {
                currentIdx = 1;
                currentCuts = new int[length];
                currentCuts[0] = input[q];
                previousIndex = q;
                nestedLoops(input, length, currentIdx, previousIndex);
            }
        }
    }

    private void nestedLoops(int[] input, int length, int idx, int prevIdx)
    {
        for (int i = prevIdx; i < input.Count(); i++)
        {
            //Debug.Log("i: " + i.ToString() + "\nidx: " + idx);
            currentCuts[idx] = input[i];
            prevIdx = i;
            //currentIdx = idx;
            if (idx < length - 1)
            {
                //currentIdx++;
                nestedLoops(input, length, currentIdx + 1, prevIdx);
            }
            else
            {
                List<int> temp = currentCuts.ToList();
                temp.Insert(0, 0);
                temp.Add(100);
                int[] cuts = temp.ToArray();
                float[] newWeights = new float[cuts.Length-1];
                for (int q = 0; q < cuts.Length - 1; q++)
                {
                    newWeights[q] = (float)(cuts[q + 1] - cuts[q])/100;
                }
                listWeights.Add(newWeights);
            }
        }
    }

    // ===============================================================================================================================================
    /*
    Vector3 rotateXYZ = new Vector3(45f, 45f, 45f);
    void rotateObjOverTime(Vector3 vectRotate, float timeToRotate, float timeStep)
    {
        float startTime = Time.time;
        StartCoroutine(Waiter(vectRotate, timeToRotate, timeStep));
    }
    IEnumerator Waiter(Vector3 vectRotate, float timeToRotate, float timeStep)
    {
        Vector3 rotObj = Vector3.zero;
        Vector3 degreesToRotate = vectRotate;

        while (rotObj.x < degreesToRotate.x)
        {
            objToRotate.transform.Rotate(degreesToRotate/(timeToRotate/timeStep), Space.Self);
            yield return new WaitForSeconds(timeStep);
            rotObj += degreesToRotate / (timeToRotate / timeStep);
        }
    }
    */







    


}



//  Found from:
//  https://stackoverflow.com/questions/852536/calculating-all-possible-sub-sequences-of-a-given-length-c
//  http://www.interact-sw.co.uk/iangblog/2004/09/16/permuterate

public class PermuteUtils
{
    // Returns an enumeration of enumerators, one for each permutation
    // of the input.
    public static IEnumerable<IEnumerable<T>> Permute<T>(IEnumerable<T> list, int count)
    {
        if (count == 0)
        {
            yield return new T[0];
        }
        else
        {
            int startingElementIndex = 0;
            foreach (T startingElement in list)
            {
                IEnumerable<T> remainingItems = AllExcept(list, startingElementIndex);

                foreach (IEnumerable<T> permutationOfRemainder in Permute(remainingItems, count - 1))
                {
                    yield return Concat<T>(
                        new T[] { startingElement },
                        permutationOfRemainder);
                }
                startingElementIndex += 1;
            }
        }
    }

    // Enumerates over contents of both lists.
    public static IEnumerable<T> Concat<T>(IEnumerable<T> a, IEnumerable<T> b)
    {
        foreach (T item in a) { yield return item; }
        foreach (T item in b) { yield return item; }
    }

    // Enumerates over all items in the input, skipping over the item
    // with the specified offset.
    public static IEnumerable<T> AllExcept<T>(IEnumerable<T> input, int indexToSkip)
    {
        int index = 0;
        foreach (T item in input)
        {
            if (index != indexToSkip) yield return item;
            index += 1;
        }
    }
}
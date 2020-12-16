using MMIUnity.TargetEngine.Scene;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TakePic : MonoBehaviour
{

    // Variables for scene changes

    public static GameObject currentObj;
    public static bool readyForPic = false;     //  for now this is set to false, as the trigger to start it will be the Spacebar. Afterwards, it will run by itself. If you want it to run photobooth script immediately on Play, simply change this to "true"
    public static bool readyForNextPart = false;


    bool picWithOutline = true;

    // Variables for photos

    private GameObject obj;
    private bool shotTaken;
    private bool symmetryCheckDone = false;
    private int shotNumberOfSpecificObject;
    private Vector3 boundsMinObj;
    private Vector3 boundsMaxObj;
    private Vector3 boundsCenterObj;
    private float sizeObjX;
    private float sizeObjY;
    private float sizeObjZ;
    private Vector3 sizeObj;
    private float xCenterAdj;
    private float yCenterAdj;
    private float zCenterAdj;
    private float partToScreenRatio;
    private float camAspectRatio;
    private Camera photoCam = PhotoboothScriptSceneGen.photoCamOriginal;
    private Bounds parentBounds;
    private Bounds parentBoundsReset;
    private static int picIndexForAllPictures = 0;
    private Texture2D currentTexAnalysis = null;
    private Texture2D currentTexCam = null;
    private Texture2D tex = null;
    private byte[] bytes = null;
    private bool updown360;
    private int adjNumPitchPics;

    Mesh objMesh;
    float area_Mesh;
    float area_total;
    int numVisibleTriangles;

    Vector3 lightTempPos;
    Vector3 lightTempRot;

    PhotoboothScriptSceneGen photoScene;

    void Update()
    {
        if (readyForPic)
        {
            VariableScript.sw_takepic.Start();
            readyForPic = false;
            readyForNextPart = false;
            StartCoroutine(itsPictureTime(currentObj));
            
        }
        //CalcPositons(parentBounds);

    }

    IEnumerator itsPictureTime(GameObject obj)
    {
        VariableScript.sw_starttakepic.Start();
        photoCam.transform.position = new Vector3(0, 0, 0);
        photoCam.transform.rotation = Quaternion.FromToRotation(Vector3.right, photoCam.transform.right);
        photoCam.transform.rotation = Quaternion.FromToRotation(Vector3.up, photoCam.transform.up);

        yield return new WaitForEndOfFrame();

        shotTaken = false;
        shotNumberOfSpecificObject = 0;
        obj.SetActive(true);
        for (int a = 0; a < obj.transform.childCount; a++)
        {
            if (obj.transform.GetChild(a).GetComponent<Renderer>())
            {
                //activate only meshes that are not attached to MMISceneObject - this way we eliminate grasp locations and simmilar that affect the view and they should not be there (Adam)
                obj.transform.GetChild(a).gameObject.SetActive(obj.transform.GetChild(a).GetComponent<MMISceneObject>() == null);
            }
        }
        removeAvatarOverlays(obj);
        yield return new WaitForEndOfFrame();
        centerAndRotate(obj);
        yield return new WaitForEndOfFrame();

        //----------- Setup for Photo Rotations ---------------------------
        Vector2 angleRangePitch = PhotoboothScriptSceneGen.angUpDownPics;
        int numPitchPics = PhotoboothScriptSceneGen.numPitchPics;
        int numYawPics = PhotoboothScriptSceneGen.numYawPics;
        int numRollPics = PhotoboothScriptSceneGen.numRollPics;
        //----------------------------------------------------------------



        // this could be deleted now since the angle range for pitch is defined now as 0 to 180 but it's okay to leave this for now I guess...
        // -------------------------------------------------------------------------
        // this if-statement makes it so if they define the max and min as the same rotational point (i.e. 0 and 360 degrees) that it will take the correct number of DIFFERENT angles for the pictures (i.e. if 3 pictures are wanted, 0, 120, and 240, NOT 0, 180, and 360)
        if ((Mathf.Abs(angleRangePitch.x - angleRangePitch.y) % 360 == 0))
        {
            numPitchPics++;
            updown360 = true;
        }
        //----------------------------------------------------------------

        VariableScript.sw_starttakepic.Stop();


        // ---------------------- Light and Symmetry Stuff ------------------------
        VariableScript.sw_lightsym.Start();
        lightTempPos = PhotoboothScriptSceneGen.lightGameObject.transform.position;
        lightTempRot = PhotoboothScriptSceneGen.lightGameObject.transform.rotation.eulerAngles;
        PhotoboothScriptSceneGen.lightGameObject.transform.position = PhotoboothScriptSceneGen.photoCamOriginal.transform.position;
        PhotoboothScriptSceneGen.lightGameObject.transform.eulerAngles = PhotoboothScriptSceneGen.photoCamOriginal.transform.rotation.eulerAngles;
        yield return new WaitForEndOfFrame();

        float acceptablePerc = PhotoboothScriptSceneGen.accPixRat;
        float exceptionPerc = PhotoboothScriptSceneGen.excPixRat;
        float currentRotation = 0f;
        float maxRotation = 90f;
        float rotationIncrement = 2f;
        Vector2 maxSym = new Vector2(0, 0);
        Vector3 percSym;
        string symAngleCheck = null;

        for (int i = 1; i <= 3; i++)
        {
            if (i == 1)
            {
                // Symmetry on Side 1
                symAngleCheck = " (0 degrees): ";
            }
            else if (i == 2)
            {
                // Symmetry on Side 2
                rotatePitch(-90, obj);
                centerDuringPhotos(obj);
                yield return new WaitForEndOfFrame();
                maxSym = new Vector2(0, 0);
                symAngleCheck = " (90 degrees): ";
            }
            else if (i == 3)
            {
                // Symmetry on Side 3
                rotateYaw(90, obj);
                centerDuringPhotos(obj);
                yield return new WaitForEndOfFrame();
                maxSym = new Vector2(0, 0);
                symAngleCheck = " (90 degrees side): ";
            }

            percSym = checkSymmetryInPic();
            if (!(percSym.x > exceptionPerc || percSym.y > exceptionPerc))
            {
                while (currentRotation < maxRotation)
                {
                    rotateRoll(rotationIncrement, obj);
                    currentRotation += rotationIncrement;
                    getBoundsAndSize(obj);
                    centerPartByDims(obj);
                    obj.transform.position += new Vector3(xCenterAdj, yCenterAdj, zCenterAdj);
                    yield return new WaitForEndOfFrame();

                    percSym = checkSymmetryInPic();
                    if (percSym.x > maxSym.x)
                    {
                        maxSym.x = percSym.x;
                        maxSym.y = currentRotation;
                    }
                    if (percSym.y > maxSym.x)
                    {
                        maxSym.x = percSym.y;
                        maxSym.y = currentRotation;
                    }

                }

                if (maxSym.x > acceptablePerc)
                {
                    rotateRoll(-(currentRotation - maxSym.y), obj);
                }
                else
                {
                    rotateRoll(-currentRotation, obj);
                }
            }
        }

        rotateYaw(-90, obj);
        centerDuringPhotos(obj);
        rotatePitch(90, obj);
        centerDuringPhotos(obj);


        // UNCOMMENT THESE

        //PhotoboothScriptSceneGen.lightGameObject.transform.position = lightTempPos;
        //PhotoboothScriptSceneGen.lightGameObject.transform.eulerAngles = lightTempRot;
        yield return new WaitForEndOfFrame();

        VariableScript.sw_lightsym.Stop();

        // ------------------------ End of Light and Symmetry Stuff ------------------------------





        VariableScript.sw_starttakepic.Start();

        // ---------------------- Beginning of Picture Process ------------------------
        //                       (rotates to min up-down angle)
        rotatePitch(angleRangePitch.x, obj);
        centerDuringPhotos(obj);
        yield return new WaitForEndOfFrame();
        // ----------------------------------------------------------------------------





        // ----- this should probably be removed at some point as it does not do anything anymore but that means I need to change variable referenes ------

        if (updown360)
        {
            adjNumPitchPics = numPitchPics - 1;
        }
        else
        {
            adjNumPitchPics = numPitchPics;
        }

        if (numPitchPics <= 0 || numYawPics <= 0  || numRollPics <= 0)
        {
            Debug.LogError("Number of Pictures cannot be less than 1.");
        }
        // -------------------------------------------------------------------------------------

        VariableScript.sw_starttakepic.Stop();


        // -------------------------------------------------------------------------------------------------------------------------------------------------
        // Creating an array of all of the different rotations for the object. By doing so, it is possible to then remove duplicate orientations to reduce runtime.
        VariableScript.sw_eulang.Start();
        bool[] repeatedAngle = new bool[PhotoboothScriptSceneGen.numOfPicsPerObject];
        Vector3[] objOrientation = new Vector3[PhotoboothScriptSceneGen.numOfPicsPerObject];
        GameObject tempObj = new GameObject();
        tempObj.transform.position = obj.transform.position;
        tempObj.transform.rotation = obj.transform.rotation;

        for (int roll = 0; roll < numRollPics; roll++)
        {
            for (int pitch = 0; pitch < adjNumPitchPics; pitch++)
            {
                for (int yaw = 0; yaw < numYawPics; yaw++)
                {
                    objOrientation[shotNumberOfSpecificObject] = tempObj.transform.eulerAngles;

                    rotateYaw(PhotoboothScriptSceneGen.RangeOfAnglesYaw / numYawPics, tempObj);
                    shotNumberOfSpecificObject++;
                }
                if (pitch < adjNumPitchPics - 1)
                {
                    rotatePitch(((angleRangePitch.y - angleRangePitch.x) / (numPitchPics - 1)), tempObj);
                }
            }
            if (roll < numRollPics - 1)
            {
                rotatePitch((angleRangePitch.y - angleRangePitch.x), tempObj);
                rotateRoll(PhotoboothScriptSceneGen.RangeOfAnglesRoll / (numRollPics - 1), tempObj);
            }
        }
        shotNumberOfSpecificObject = 0;
        for (int i = 0; i < objOrientation.Length; i++)
        {
            objOrientation[i] = roundToDecimal(objOrientation[i], 3f);
        }
        objOrientation = objOrientation.ToList().Distinct().ToArray();
        VariableScript.sw_eulang.Stop();

        // -----------------------------------------------------------------------------------------------------------------------------------------------------




        // ----------------------------------------------------------- New Picture Iteration Process ----------------------------------------------------------


        for (int i = 0; i < objOrientation.Length; i++)
        {
            obj.transform.eulerAngles = objOrientation[i];
            centerDuringPhotos(obj);

            // -------------THIS TAKES UP AROUND 44% OF THE TIME TO PROCESS (if commented out, the time for the "SavePic" increases a lot
            VariableScript.sw_investigate.Start();
            yield return new WaitForEndOfFrame();
            VariableScript.sw_investigate.Stop();
            // -----------------------------------------------------------------------------------------------------

            VariableScript.sw_textures.Start();
            currentTexAnalysis = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            currentTexAnalysis.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            currentTexAnalysis.Apply();
            VariableScript.sw_textures.Stop();

            VariableScript.sw_raycast.Start();
            Physics.SyncTransforms();
            createMesh();
            if (PhotoboothScriptSceneGen.VSA_weight != 0)
            {
                StartCoroutine(percAreaMeshTriangle());
            }
            VariableScript.sw_raycast.Stop();

            VariableScript.sw_qualityofview.Start();
            calculateQualityOfView(currentTexAnalysis, picIndexForAllPictures);
            picIndexForAllPictures++;
            shotNumberOfSpecificObject++;
            currentTexCam = null;
            VariableScript.sw_qualityofview.Stop();

            VariableScript.sw_unloading.Start();
            System.GC.Collect();
            Resources.UnloadUnusedAssets();
            VariableScript.sw_unloading.Stop();
        }

        // This is necessary because of when the update for the visible edges happen, they happen at the end of the frame so I have to save the results of the edges to one index earlier and since I have them all grouped in the "calculateQualityOfView", it is necessary to not call it on the first iterations of "calculateQualityOfView" and call it one more time afterward for the last one.
        StartCoroutine(visibleEdgesLength());


        VariableScript.sw_unloading.Start();
        System.GC.Collect();
        Resources.UnloadUnusedAssets();
        VariableScript.sw_unloading.Stop();

        VariableScript.numOfPicturesPerObj[PhotoboothScriptSceneGen.currentObjIndex - 1] = shotNumberOfSpecificObject;
        // ------------------------------------------------------------------------------------------------------------------------------------------------

        string newPath = Application.dataPath + "/../" + "/Weights/";
        if (!Directory.Exists(newPath))
            Directory.CreateDirectory(newPath);
        StreamWriter dataFile = new StreamWriter(newPath+"raw-" + obj.name + ".txt");
        dataFile.WriteLine("comX;comY;triangles;projected area;visible area;symmetry;visble edges");
        for (int i = 0; i < shotNumberOfSpecificObject; i++)
        {
            dataFile.WriteLine(VariableScript.centerOfMassX[i].ToString()+";"+
                               VariableScript.centerOfMassY[i].ToString() + ";" +
                               VariableScript.meshTriangles[i].ToString() + ";" +
                               VariableScript.projectedArea[i].ToString() + ";" +
                               VariableScript.ratioVisibleSurfaceArea[i].ToString() + ";" +
                               VariableScript.symmetryInView[i].ToString() + ";" +
                               VariableScript.visibleEdges[i].ToString()
                );
        }
        dataFile.Flush();
        dataFile.Close();
        


        // ------------------- Taking Pictures of Best Views -----------------------------------



        // Final Part of Code --> Determining the Best Views
        VariableScript.sw_normalize.Start();
        normalizedQualities();
        VariableScript.sw_normalize.Stop();

        dataFile = new StreamWriter(newPath+"norm-" + obj.name + ".txt");
        dataFile.WriteLine("comX;comY;triangles;projected area;visible area;symmetry;visble edges");
        for (int i = 0; i < shotNumberOfSpecificObject; i++)
        {
            dataFile.WriteLine(VariableScript.centerOfMassX[i].ToString() + ";" +
                               VariableScript.centerOfMassY[i].ToString() + ";" +
                               VariableScript.meshTriangles[i].ToString() + ";" +
                               VariableScript.projectedArea[i].ToString() + ";" +
                               VariableScript.ratioVisibleSurfaceArea[i].ToString() + ";" +
                               VariableScript.symmetryInView[i].ToString() + ";" +
                               VariableScript.visibleEdges[i].ToString()
                );
        }
        dataFile.Flush();
        dataFile.Close();

        VariableScript.sw_savepic.Start();
        // Applies the weight of that metric and adds up the weighted 'score' for each metric 
        for (int i = 0; i < VariableScript.QualityOfView.Length; i++)
        {
            VariableScript.QualityOfView[i] = 0;

            VariableScript.QualityOfView[i] +=
                (VariableScript.projectedArea[i] * PhotoboothScriptSceneGen.PA_weight +
                VariableScript.ratioVisibleSurfaceArea[i] * PhotoboothScriptSceneGen.VSA_weight +
                (VariableScript.centerOfMassY[i] + VariableScript.centerOfMassX[i]) / 2 * PhotoboothScriptSceneGen.CoM_weight) +
                VariableScript.symmetryInView[i] * PhotoboothScriptSceneGen.sym_weight +
                VariableScript.meshTriangles[i] * PhotoboothScriptSceneGen.triangles_weight +
                VariableScript.visibleEdges[i] * PhotoboothScriptSceneGen.edges_weight;
        }


        // saves best view for each part
        int numbestpics = PhotoboothScriptSceneGen.numbestpics;
        int numpicsper = VariableScript.numOfPicturesPerObj[PhotoboothScriptSceneGen.currentObjIndex - 1];
        float[] subsetOfQualityOfView = new float[numpicsper];
        Array.Copy(VariableScript.QualityOfView, picIndexForAllPictures - numpicsper, subsetOfQualityOfView, 0, numpicsper);
        if (subsetOfQualityOfView.Length != 0)
        {
            float[] maxValue = new float[numbestpics];
            int[] maxIndex = new int[numbestpics];
           
            for (int i = 0; i < numbestpics; i++)
            {
                maxValue[i] = subsetOfQualityOfView.Max();
                maxIndex[i] = subsetOfQualityOfView.ToList().IndexOf(maxValue[i]);
                subsetOfQualityOfView[maxIndex[i]] = 0;

                obj.transform.eulerAngles = objOrientation[maxIndex[i]];
                centerDuringPhotos(obj);
                yield return new WaitForEndOfFrame();
                shotTaken = false;
                takeAndSavePic(obj, i+1, maxIndex[i]);
                yield return new WaitUntil(() => shotTaken = true);
            }
        }
        VariableScript.sw_savepic.Stop();

        if (picWithOutline)
        {
            photoCam.enabled = true;
            PhotoboothScriptSceneGen.edgeDetectPublicCam.enabled = false;
        }



        // ----------------------------------------------------------------------------------



        // ------------------ End of Scene / Change Scene --------------------------------
        VariableScript.sw_endscene.Start();
        UnityEngine.Object.Destroy(currentTexAnalysis);
        UnityEngine.Object.Destroy(currentTexCam);
        System.GC.Collect();

        VariableScript.sw_unloading.Start();
        Resources.UnloadUnusedAssets();
        VariableScript.sw_unloading.Stop();

        
        yield return new WaitForEndOfFrame();
        sceneChange();
        yield return new WaitForEndOfFrame();
        readyForNextPart = true;
        VariableScript.sw_endscene.Stop();
        VariableScript.sw_takepic.Stop();


        //--------------------------------------------------------------------------------
    }

    void normalizedQualities()
    {
        int numpics = VariableScript.numOfPicturesPerObj[PhotoboothScriptSceneGen.currentObjIndex - 1];
        // Projected Area Normalization
        float tempMax_ProjArea;
        float[] subsetOfQualityOfView_ProjArea = new float[numpics];
        Array.Copy(VariableScript.projectedArea, (picIndexForAllPictures - numpics), subsetOfQualityOfView_ProjArea, 0, numpics);
        tempMax_ProjArea = subsetOfQualityOfView_ProjArea.Max();
        if (tempMax_ProjArea != 0)
        {
            for (int q = 0; q < numpics; q++)
            {
                subsetOfQualityOfView_ProjArea[q] /= tempMax_ProjArea;
                VariableScript.projectedArea[q + picIndexForAllPictures - numpics] = subsetOfQualityOfView_ProjArea[q];
            }
        }

        // Surface Area Normalization
        float tempMax_SurfArea;
        float[] subsetOfQualityOfView_SurfArea = new float[numpics];
        Array.Copy(VariableScript.ratioVisibleSurfaceArea, (picIndexForAllPictures - numpics), subsetOfQualityOfView_SurfArea, 0, numpics);
        tempMax_SurfArea = subsetOfQualityOfView_SurfArea.Max();
        if (tempMax_SurfArea != 0)
        {
            for (int q = 0; q < numpics; q++)
            {
                subsetOfQualityOfView_SurfArea[q] /= tempMax_SurfArea;
                VariableScript.ratioVisibleSurfaceArea[q + picIndexForAllPictures - numpics] = subsetOfQualityOfView_SurfArea[q];
            }
        }

        // Center Of Mass Normalization (different than others because this can be negative and we want the lowest CoM, so we have to invert the values by doing -1 and taking the absolute value)
        // Y-Direction
        float tempMax_CoM_Y;
        float[] subsetOfQualityOfView_CoM_Y = new float[numpics];
        Array.Copy(VariableScript.centerOfMassY, (picIndexForAllPictures - numpics), subsetOfQualityOfView_CoM_Y, 0, numpics);
        tempMax_CoM_Y = subsetOfQualityOfView_CoM_Y.Max();
        if (tempMax_CoM_Y != 0)
        {
            for (int q = 0; q < numpics; q++)
            {
                subsetOfQualityOfView_CoM_Y[q] /= tempMax_CoM_Y;
                VariableScript.centerOfMassY[q + picIndexForAllPictures - numpics] = Mathf.Abs(subsetOfQualityOfView_CoM_Y[q] - 1);
            }
        }

        // X-Direction
        float tempMax_CoM_X;
        float[] subsetOfQualityOfView_CoM_X = new float[numpics];
        Array.Copy(VariableScript.centerOfMassX, (picIndexForAllPictures - numpics), subsetOfQualityOfView_CoM_X, 0, numpics);
        for (int q = 0; q < numpics; q++)
        {
            subsetOfQualityOfView_CoM_X[q] = Mathf.Abs(subsetOfQualityOfView_CoM_X[q]);
        }
        tempMax_CoM_X = subsetOfQualityOfView_CoM_X.Max();
        if (tempMax_CoM_X != 0)
        {
            for (int q = 0; q < numpics; q++)
            {
                subsetOfQualityOfView_CoM_X[q] /= tempMax_CoM_X;
                VariableScript.centerOfMassX[q + picIndexForAllPictures - numpics] = Mathf.Abs(subsetOfQualityOfView_CoM_X[q] - 1);
            }
        }

        // Symmetry
        float tempMax_Sym;
        float[] subsetOfQualityOfView_Sym = new float[numpics];
        Array.Copy(VariableScript.symmetryInView, (picIndexForAllPictures - numpics), subsetOfQualityOfView_Sym, 0, numpics);
        tempMax_Sym = subsetOfQualityOfView_Sym.Max();
        if (tempMax_Sym != 0)
        {
            for (int q = 0; q < numpics; q++)
            {
                subsetOfQualityOfView_Sym[q] /= tempMax_Sym;
                VariableScript.symmetryInView[q + picIndexForAllPictures - numpics] = Mathf.Abs(subsetOfQualityOfView_Sym[q] - 1);
            }
        }

        // Mesh Triangles
        float tempMax_triangles;
        float[] subsetOfQualityOfView_triangles = new float[numpics];
        Array.Copy(VariableScript.meshTriangles, (picIndexForAllPictures - numpics), subsetOfQualityOfView_triangles, 0, numpics);
        tempMax_triangles = subsetOfQualityOfView_triangles.Max();
        if (tempMax_triangles != 0)
        {
            for (int q = 0; q < numpics; q++)
            {
                subsetOfQualityOfView_triangles[q] /= tempMax_triangles;
                VariableScript.meshTriangles[q + picIndexForAllPictures - numpics] = Mathf.Abs(subsetOfQualityOfView_triangles[q] - 1);
            }
        }

        // Visible Edges Length
        float tempMax_edges;
        float[] subsetOfQualityOfView_edges = new float[numpics];
        Array.Copy(VariableScript.visibleEdges, (picIndexForAllPictures - numpics), subsetOfQualityOfView_edges, 0, numpics);
        tempMax_edges = subsetOfQualityOfView_edges.Max();
        if (tempMax_edges != 0)
        {
            for (int q = 0; q < numpics; q++)
            {
                //subsetOfQualityOfView_edges[q] /= tempMax_edges;
                VariableScript.visibleEdges[q + picIndexForAllPictures - numpics] = subsetOfQualityOfView_edges[q];
            }
        }
    }

    void centerAndRotate(GameObject currentObj)
    {
        currentObj.transform.position = new Vector3(0, 0, 0);
        currentObj.transform.rotation = Quaternion.FromToRotation(Vector3.right, currentObj.transform.right);
        currentObj.transform.rotation = Quaternion.FromToRotation(Vector3.up, currentObj.transform.up);

        getBoundsAndSize(currentObj);
        photoCam.transform.position = new Vector3(0, 0, -((boundsMinObj).magnitude)) * photoCam.aspect;
        photoCam.orthographicSize = sizeObj.magnitude * photoCam.aspect;

        centerPartByDims(currentObj);
        currentObj.transform.position = new Vector3(xCenterAdj, yCenterAdj, zCenterAdj);
        getBoundsAndSize(currentObj);
        partToScreenRatio = ((photoCam.WorldToScreenPoint(boundsMaxObj).x - photoCam.WorldToScreenPoint(boundsMinObj).x) / sizeObjX) * (sizeObj.magnitude) / photoCam.pixelHeight;

        float percScreen = 1f;
        float adjustRatio = (percScreen / partToScreenRatio);
        photoCam.orthographicSize = photoCam.orthographicSize / adjustRatio;
    }

    void centerDuringPhotos(GameObject obj)
    {
        getBoundsAndSize(obj);
        centerPartByDims(obj);
        obj.transform.position += new Vector3(xCenterAdj, yCenterAdj, zCenterAdj);
    }

    void centerPartByDims(GameObject currentObj)
    {
        xCenterAdj = -(boundsMaxObj.x + boundsMinObj.x) / 2;
        yCenterAdj = -(boundsMaxObj.y + boundsMinObj.y) / 2;
        zCenterAdj = -(boundsMaxObj.z + boundsMinObj.z) / 2;
    }

    void getBoundsAndSize(GameObject currentObj)
    {
        getRealBounds();
        sizeObj = parentBounds.size;
        sizeObjX = parentBounds.size.x;
        sizeObjY = parentBounds.size.y;
        sizeObjZ = parentBounds.size.z;
        boundsCenterObj = parentBounds.center;
        boundsMaxObj = parentBounds.max;
        boundsMinObj = parentBounds.min;
    }

    void takeAndSavePic(GameObject part, int shotNum, int viewNum)
    {
        string folderPath = Application.dataPath + "/../" + "/Screenshots/";

        string folderPathG = folderPath + part.name + "/";
        if (!System.IO.Directory.Exists(folderPathG))
            System.IO.Directory.CreateDirectory(folderPathG);

        string screenshotName = part.name + " - " + shotNum + " (No. " + viewNum + ").png";
        string screenShotNamePath = System.IO.Path.Combine(folderPathG, screenshotName);

        bool finisedSaving()
        {
            //it's not enough to just check that the file exists, since it doesn't mean it's finished saving
            //we have to check if it can actually be opened
            Texture2D image;
            image = new Texture2D(Screen.width, Screen.height);
            bool imageLoadSuccess = image.LoadImage(System.IO.File.ReadAllBytes(screenShotNamePath));
            Destroy(image);
            System.GC.Collect();
            return imageLoadSuccess;
        }

        //Texture2D tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        //tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);

        getScaledScreenshotTexture(PhotoboothScriptSceneGen.shotscale);
        bytes = currentTexCam.EncodeToPNG();

        //Destroy(tex);
        //VariableScript.pictureFilePath[picIndexForAllPictures] = screenShotNamePath;
        //VariableScript.pictureFileName[picIndexForAllPictures] = screenshotName;
        File.WriteAllBytes(screenShotNamePath, bytes);

        while (!finisedSaving()) { }

        if (shotNum == 1)
        {
            var fileID = part.GetComponent<MMISceneObject>().TaskEditorLocalID;
            File.Copy(screenShotNamePath, folderPath + fileID.ToString() + ".png");
        }

        bytes = null;
        currentTexCam = null;
        shotTaken = true;
    }

    void getScaledScreenshotTexture(float scale)
    {
        Camera currentCamera = PhotoboothScriptSceneGen.photoCamOriginal;
        if (picWithOutline)
        {
            currentCamera = PhotoboothScriptSceneGen.edgeDetectPublicCam;
        }
        int tw = (int)(photoCam.pixelWidth * scale); //thumb width
        int th = (int)(photoCam.pixelHeight * scale); //thumb height
        RenderTexture rt = new RenderTexture(tw, th, 24, RenderTextureFormat.ARGB32);
        rt.antiAliasing = 4;
        if (currentCamera.targetTexture != null)
            currentCamera.targetTexture.Release();
        currentCamera.targetTexture = rt;
        currentCamera.Render();

        //Create the blank texture container
        currentTexCam = new Texture2D(tw, th, TextureFormat.RGB24, false);

        //Assign rt as the main render texture, so everything is drawn at the higher resolution
        RenderTexture.active = rt;

        //Read the current render into the texture container, thumb
        currentTexCam.ReadPixels(new Rect(0, 0, tw, th), 0, 0, false);
        currentTexCam.Apply();
        RenderTexture.active = null;
        if (currentCamera.targetTexture != null)
            currentCamera.targetTexture.Release();
        currentCamera.targetTexture = null;
        rt.DiscardContents();
        rt = null;
    }


    void removeAvatarOverlays(GameObject obj)
    {   //this way if cosimulation debugger script is not present there is no error (Adam)
        if (obj.GetComponent("CoSimulationDebugger"))
            obj.GetComponent("CoSimulationDebugger").SendMessage("enabled", false);

        if (obj.GetComponent("AJANMMIAvatar"))
            obj.GetComponent("AJANMMIAvatar").SendMessage("enabled", false);

        if (obj.GetComponent("MMIAvatar"))
            obj.GetComponent("MMIAvatar").SendMessage("enabled", false);

        /*
        if (obj.GetComponent<CoSimulationDebugger>())
        {
            obj.GetComponent<CoSimulationDebugger>().enabled = false;
        }
        try
        {
            obj.GetComponent<AJANMMIAvatar>().enabled = false;
        }
        catch
        {
            try
            {
                obj.GetComponent<MMIAvatar>().enabled = false;
            }
            catch
            {
                return;
            }
        }*/
    }

    void sceneChange()
    {
        SceneManager.MoveGameObjectToScene(currentObj, SceneManager.GetActiveScene());
        SceneManager.LoadSceneAsync(PhotoboothScriptSceneGen.originalSceneStr, LoadSceneMode.Additive);
    }

    void rotatePitch(float deg, GameObject obj)
    {
        obj.transform.Rotate(new Vector3(deg, 0, 0), Space.World);
    }

    void rotateYaw(float deg, GameObject obj)
    {
        obj.transform.Rotate(new Vector3(0, deg, 0), Space.World);
    }

    void rotateRoll(float deg, GameObject obj)
    {
        obj.transform.Rotate(new Vector3(0, 0, deg), Space.World);
    }

    Vector3 checkSymmetryInPic()
    {
        System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
        st.Start();

        int numMatch = 0;
        int totalChecked = 0;
        Vector3 pixelPerc = new Vector3(0f,0f, 0f);
        float colorPercCriteria = PhotoboothScriptSceneGen.accGrayRng; //0.2f;    // this is used to say if the grayscale number is +/- this percent, it is acceptable and considered the same pixel color

        tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        tex.Apply();

        int left = 0;
        int bottom = 0;
        int right = tex.width-1;
        int top = tex.height-1;
        Vector2 picSize = new Vector2(tex.width, tex.height);
        float backgroundColor = tex.GetPixel(left, bottom).grayscale;

        //This is used to adjust how many pixels to skip over when comparing images. This way, it normalizes the speed of the symmetry check a little (if someone uses Unity Game view in full screen, it will be at [approximately] the same speed as someone who uses it in a smaller view) 
        int pixelSquareSizeForComparison = Mathf.RoundToInt(PhotoboothScriptSceneGen.pixelskip);
        if (pixelSquareSizeForComparison <= 0)
            pixelSquareSizeForComparison = 1;

        // Left and Right comparison
        while (bottom <= picSize.y)
        {
            while (left <= picSize.x / 2)
            {
                if (!(tex.GetPixel(left, bottom).grayscale == backgroundColor && tex.GetPixel(right, bottom).grayscale == backgroundColor))
                {
                    float leftbot = tex.GetPixel(left, bottom).grayscale;
                    float rightbot = tex.GetPixel(right, bottom).grayscale;

                    if ((leftbot >= (rightbot - colorPercCriteria)) && (leftbot <= (rightbot + colorPercCriteria)))
                    {
                        //Debug.Log("Incremented");
                        numMatch++;
                    }
                    totalChecked++;
                }
                left += pixelSquareSizeForComparison;
                right -= pixelSquareSizeForComparison;
            }
            bottom += pixelSquareSizeForComparison;
            left = 0;
            right = (int)picSize.x - 1;
        }
        pixelPerc.x = (float)numMatch / (float)totalChecked;


        // Top and Bottom comparison
        numMatch = 0;
        totalChecked = 0;
        left = 0;
        bottom = 0;
        right = (int)picSize.x - 1;
        top = (int)picSize.y - 1;

        while (left <= picSize.x)
        {
            while (bottom <= picSize.y / 2)
            {
                if (!(tex.GetPixel(left, bottom).grayscale == backgroundColor && tex.GetPixel(left, top).grayscale == backgroundColor))
                {
                    float leftbot = tex.GetPixel(left, bottom).grayscale;
                    float lefttop = tex.GetPixel(left, top).grayscale;

                    if ((leftbot >= (lefttop - colorPercCriteria)) && (leftbot <= (lefttop + colorPercCriteria)))
                    {
                        //Debug.Log("Incremented");
                        numMatch++;
                    }
                    totalChecked++;
                }
                bottom += pixelSquareSizeForComparison;
                top -= pixelSquareSizeForComparison;
            }
            left += pixelSquareSizeForComparison;
            bottom = 0;
            top = (int)picSize.y - 1;
        }
        pixelPerc.y = (float)numMatch / (float)totalChecked;

        //pixelPerc.z = calculateQualityOfView(tex);

        st.Stop();
        //Debug.Log(string.Format("Checking Symmetry took {0} ms to complete", st.ElapsedMilliseconds));

        return pixelPerc;

    }

    void createMesh()
    {
        VariableScript.sw_mesh.Start();
        if (currentObj.GetComponent<MeshFilter>())
        {
            objMesh = currentObj.GetComponent<MeshFilter>().mesh;
        }
        else
        {
            currentObj.AddComponent<MeshFilter>();
            objMesh = currentObj.GetComponent<MeshFilter>().mesh;
            MeshFilter[] meshFilters = currentObj.GetComponentsInChildren<MeshFilter>();
            SkinnedMeshRenderer[] skinnedMeshRenderers = currentObj.GetComponentsInChildren<SkinnedMeshRenderer>();
            CombineInstance[] combine = new CombineInstance[meshFilters.Length + skinnedMeshRenderers.Length];

            Matrix4x4 myTransform = currentObj.transform.worldToLocalMatrix;
            int w = 0;
            while (w < meshFilters.Length)
            {
                if (meshFilters[w].gameObject.GetComponent<BoxCollider>())
                {
                    meshFilters[w].gameObject.GetComponent<BoxCollider>().enabled = false;
                }
                combine[w].mesh = meshFilters[w].sharedMesh;
                combine[w].transform = myTransform * meshFilters[w].transform.localToWorldMatrix;
                w++;
            }
            w = 0;
            while (w < skinnedMeshRenderers.Length)
            {
                combine[w + meshFilters.Length].mesh = skinnedMeshRenderers[w].sharedMesh;
                combine[w + meshFilters.Length].transform = myTransform * skinnedMeshRenderers[w].transform.localToWorldMatrix;
                w++;
            }
            currentObj.GetComponent<MeshFilter>().mesh = new Mesh();
            currentObj.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
            currentObj.SetActive(true);
            objMesh = currentObj.GetComponent<MeshFilter>().mesh;
        }
        VariableScript.sw_mesh.Stop();
    }

    void getRealBounds()
    {
        VariableScript.sw_bounds.Start();
        Mesh mesh;
        bool foundFirstChild = false;
        if (currentObj.GetComponent<MeshFilter>())
        {
            mesh = currentObj.GetComponent<MeshFilter>().mesh;
            parentBounds = GeometryUtility.CalculateBounds(mesh.vertices, currentObj.transform.localToWorldMatrix);
        }
        else if (currentObj.GetComponent<SkinnedMeshRenderer>())
        {
            mesh = currentObj.GetComponent<SkinnedMeshRenderer>().sharedMesh;
            parentBounds = GeometryUtility.CalculateBounds(mesh.vertices, currentObj.transform.localToWorldMatrix);
        }
        else
        {
            parentBounds = parentBoundsReset;
            foreach (Transform child in currentObj.transform)
            {
                if (child.gameObject.GetComponent<MeshFilter>() && !(foundFirstChild))
                {
                    mesh = child.gameObject.GetComponent<MeshFilter>().mesh;
                    parentBounds = GeometryUtility.CalculateBounds(mesh.vertices, child.localToWorldMatrix);
                    foundFirstChild = true;
                }
                else if (child.gameObject.GetComponent<SkinnedMeshRenderer>() && !(foundFirstChild))
                {
                    mesh = child.gameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh;
                    parentBounds = GeometryUtility.CalculateBounds(mesh.vertices, child.localToWorldMatrix);
                    foundFirstChild = true;
                }
            }
        }
        foreach (Transform child in currentObj.transform)
        {
            if (child.gameObject.GetComponent<MeshFilter>())
            {
                mesh = child.gameObject.GetComponent<MeshFilter>().mesh;
                parentBounds.Encapsulate(GeometryUtility.CalculateBounds(mesh.vertices, child.localToWorldMatrix));
            }
            else if (child.gameObject.GetComponent<SkinnedMeshRenderer>())
            {
                mesh = child.gameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh;
                parentBounds.Encapsulate(GeometryUtility.CalculateBounds(mesh.vertices, child.localToWorldMatrix));
            }
        }
        VariableScript.sw_bounds.Stop();

    }

    IEnumerator percAreaMeshTriangle()
    {
        area_Mesh = 0;
        Vector3[] vertices = objMesh.vertices;
        int[] triangles = objMesh.triangles;
        //Debug.Log(triangles.Length);
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 p1 = vertices[triangles[i + 0]];
            Vector3 p2 = vertices[triangles[i + 1]];
            Vector3 p3 = vertices[triangles[i + 2]];
            Vector3 v1 = p2 - p1;
            Vector3 v2 = p3 - p1;
            Vector3 crossProd = new Vector3((v1.y * v2.z - v2.y * v1.z), (v1.x * v2.z - v2.x * v1.z), (v1.x * v2.y - v2.x * v1.y));
            area_Mesh += crossProd.magnitude / 2;
        }
        //Debug.Log("Mesh: " + area);
        area_total = area_Mesh;

        if (TakePic.currentObj.GetComponent<BoxCollider>())
        {
            TakePic.currentObj.GetComponent<BoxCollider>().enabled = false;
        }
        if (!TakePic.currentObj.GetComponent<MeshCollider>())
        {
            objMesh = TakePic.currentObj.AddComponent<MeshCollider>().sharedMesh;
        }
        else
        {
            objMesh = TakePic.currentObj.GetComponent<MeshCollider>().sharedMesh;
        }


        area_Mesh = 0;
        numVisibleTriangles = 0;
        vertices = objMesh.vertices;
        triangles = objMesh.triangles;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 p1 = vertices[triangles[i + 0]];
            Vector3 p2 = vertices[triangles[i + 1]];
            Vector3 p3 = vertices[triangles[i + 2]];
            //Debug.Log(p1.ToString("F4"));
            //Debug.Log(p2.ToString("F4"));
            //Debug.Log(p3.ToString("F4"));
            currentObj.layer = 4;
            //  This will cast rays only against colliders in layer 8
            int layerMask = 1 << 4;

            //  But it is possible to collide against everything 'except' layer 8. The ~ operator does this, it inverts the bitmask.
            //  layerMask = ~layerMask;
            RaycastHit H;
            bool[] hit = new bool[3];
            Vector3 camPosition = photoCam.transform.position;
            float constOffset = 0.99999f;
            hit[0] = Physics.Raycast(camPosition, (currentObj.transform.TransformPoint(p1) - camPosition).normalized, out H, constOffset * (currentObj.transform.TransformPoint(p1) - camPosition).magnitude, layerMask);
            hit[1] = Physics.Raycast(camPosition, (currentObj.transform.TransformPoint(p2) - camPosition).normalized, out H, constOffset * (currentObj.transform.TransformPoint(p2) - camPosition).magnitude, layerMask);
            hit[2] = Physics.Raycast(camPosition, (currentObj.transform.TransformPoint(p3) - camPosition).normalized, out H, constOffset * (currentObj.transform.TransformPoint(p3) - camPosition).magnitude, layerMask);
            //Debug.DrawRay(camPosition, constOffset * (currentObj.transform.TransformPoint(p1) - camPosition), Color.red);
            //Debug.DrawRay(camPosition, constOffset * (currentObj.transform.TransformPoint(p2) - camPosition), Color.blue);
            //Debug.DrawRay(camPosition, constOffset * (currentObj.transform.TransformPoint(p3) - camPosition), Color.green);

            if (hit[0] == false && hit[1] == false && hit[2] == false) //((intersectRay.All(x => x)))
            {
                numVisibleTriangles++;
                Vector3 v1 = p2 - p1;
                Vector3 v2 = p3 - p1;
                Vector3 crossProd = new Vector3((v1.y * v2.z - v2.y * v1.z), (v1.x * v2.z - v2.x * v1.z), (v1.x * v2.y - v2.x * v1.y));
                area_Mesh += crossProd.magnitude / 2;
                //Debug.Log("NOT HIT!");
            }
            else
            {
                //Debug.Log("HIT!");
            }
        }
        //Debug.Log("Mesh: " + area);

        yield return new WaitForEndOfFrame();

        //return (area/total_area);
    }


    float ratio;
    IEnumerator visibleEdgesLength()
    {
        Camera edgeCam = PhotoboothScriptSceneGen.edgeDetectPublicCam;
        photoCam.enabled = false;
        edgeCam.enabled = true;
        edgeCam.name = "EdgeCam";
        edgeCam.orthographic = true;
        edgeCam.nearClipPlane = photoCam.nearClipPlane;
        edgeCam.farClipPlane = photoCam.farClipPlane;
        edgeCam.orthographicSize = photoCam.orthographicSize;
        edgeCam.transform.position = photoCam.transform.position;
        edgeCam.transform.eulerAngles = photoCam.transform.eulerAngles;
        edgeCam.GetComponent<AudioListener>().enabled = false;
        edgeCam.backgroundColor = Color.black;
        edgeCam.GetComponent<Camera>().clearFlags = photoCam.GetComponent<Camera>().clearFlags;

        foreach (Transform child in edgeCam.transform)
        {
            Camera c = child.gameObject.GetComponent<Camera>();
            c.orthographic = true;
            c.nearClipPlane = edgeCam.nearClipPlane;
            c.farClipPlane = edgeCam.farClipPlane;
            c.orthographicSize = edgeCam.orthographicSize;
        }
        edgeCam.GetComponent<EdgeDetect>().outlineColor = Color.cyan;

        yield return new WaitForEndOfFrame();


        VariableScript.sw_textures.Start();
        Texture2D tex_edge = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        tex_edge.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        tex_edge.Apply();
        VariableScript.sw_textures.Stop();


        //int pixelSquareSizeForComparison = Mathf.RoundToInt(PhotoboothScriptSceneGen.pixelskip);
        //if (pixelSquareSizeForComparison <= 0)
        //    pixelSquareSizeForComparison = 1;

        float total = tex_edge.width * tex_edge.height; // Mathf.Pow(pixelSquareSizeForComparison, 2);
        float count = 0;

        VariableScript.sw_getpixels.Start();
        Color[] pix = tex_edge.GetPixels();
        for (int i = 0; i < pix.Length; i++)
        {
            if (pix[i] == Color.cyan)
            {
                count++;
            }
        }
        VariableScript.sw_getpixels.Stop();

        tex_edge = null;
        ratio = count / total;
        if (!picWithOutline)
        {
            photoCam.enabled = true;
            edgeCam.enabled = false;
        }


        VariableScript.visibleEdges[picIndexForAllPictures-1] = ratio;

    }



    void calculateQualityOfView(Texture2D currentTexAnalysis, int picIndexForAllPictures)
    {
        // Projected View Area

        if (PhotoboothScriptSceneGen.PA_weight != 0)
        {
            VariableScript.sw_projectedarea.Start();
            int pixelSquareSizeForComparison = Mathf.RoundToInt(PhotoboothScriptSceneGen.pixelskip);
            if (pixelSquareSizeForComparison <= 0)
                pixelSquareSizeForComparison = 1;

            float total = currentTexAnalysis.width * currentTexAnalysis.height / Mathf.Pow(pixelSquareSizeForComparison, 2);
            float count = 0;

            VariableScript.sw_getpixels.Start();
            Color[] pix = currentTexAnalysis.GetPixels();
            for (int i = 0; i < pix.Length; i++)
            {
                if (pix[i].grayscale != pix[0].grayscale)
                {
                    count++;
                }
            }
            VariableScript.sw_getpixels.Stop();

            currentTexAnalysis = null;
            VariableScript.projectedArea[picIndexForAllPictures] = count / total;
            VariableScript.sw_projectedarea.Stop();
        }



        // Surface Area of visible sides
        if (PhotoboothScriptSceneGen.VSA_weight != 0)
        {
            VariableScript.ratioVisibleSurfaceArea[picIndexForAllPictures] = area_Mesh / area_total;
        }



        // Visible Edges Length

        if (PhotoboothScriptSceneGen.edges_weight != 0 && shotNumberOfSpecificObject != 0)
        {
            StartCoroutine(visibleEdgesLength());
            //VariableScript.visibleEdges[picIndexForAllPictures] = visibleEdgesLength();
        }




        //Center of Mass
        if (PhotoboothScriptSceneGen.CoM_weight != 0)
        {
            VariableScript.sw_com.Start();
            Rigidbody temp = null;
            if (currentObj.GetComponent<BoxCollider>())
            {
                currentObj.GetComponent<BoxCollider>().enabled = false;
            }
            if (!currentObj.GetComponent<Rigidbody>())
            {
                temp = currentObj.AddComponent<Rigidbody>();
            }
            else
            {
                temp = currentObj.GetComponent<Rigidbody>();
            }

            VariableScript.centerOfMassX[picIndexForAllPictures] = currentObj.GetComponent<Rigidbody>().worldCenterOfMass.x;
            VariableScript.centerOfMassY[picIndexForAllPictures] = currentObj.GetComponent<Rigidbody>().worldCenterOfMass.y - boundsMinObj.y;
            Debug.DrawLine(currentObj.GetComponent<Rigidbody>().worldCenterOfMass, new Vector3(0, 0, 0));
            DestroyImmediate(temp);
            VariableScript.sw_com.Stop();
        }




        // Symmetry
        if (PhotoboothScriptSceneGen.sym_weight != 0)
        {
            Vector3 symPer = checkSymmetryInPic();
            VariableScript.symmetryInView[picIndexForAllPictures] = (symPer.x + symPer.y) / 2;
        }

        //Debug.Log(VariableScript.symmetryInView[picIndexForAllPictures].ToString("f4"));
        //if (currentObj.name == "Hammer")
        //    Debug.Break();




        // Mesh Triangles
        if (PhotoboothScriptSceneGen.triangles_weight != 0)
        {
            VariableScript.meshTriangles[picIndexForAllPictures] = (area_Mesh / numVisibleTriangles);
        }


    }

    Vector3 roundToDecimal(Vector3 _vec, float _decimals)
    {
        _vec = new Vector3(Mathf.Round(_vec.x * Mathf.Pow(10f, _decimals)), Mathf.Round(_vec.y * Mathf.Pow(10f, _decimals)), Mathf.Round(_vec.z * Mathf.Pow(10f, _decimals))) / Mathf.Pow(10f, _decimals);
        return _vec;
    }







    public Color color = Color.green;

    private Vector3 v3FrontTopLeft;
    private Vector3 v3FrontTopRight;
    private Vector3 v3FrontBottomLeft;
    private Vector3 v3FrontBottomRight;
    private Vector3 v3BackTopLeft;
    private Vector3 v3BackTopRight;
    private Vector3 v3BackBottomLeft;
    private Vector3 v3BackBottomRight;

    void CalcPositons(Bounds bounds)
    {

        Vector3 v3Center = bounds.center;
        Vector3 v3Extents = bounds.extents;

        v3FrontTopLeft = new Vector3(v3Center.x - v3Extents.x, v3Center.y + v3Extents.y, v3Center.z - v3Extents.z);  // Front top left corner
        v3FrontTopRight = new Vector3(v3Center.x + v3Extents.x, v3Center.y + v3Extents.y, v3Center.z - v3Extents.z);  // Front top right corner
        v3FrontBottomLeft = new Vector3(v3Center.x - v3Extents.x, v3Center.y - v3Extents.y, v3Center.z - v3Extents.z);  // Front bottom left corner
        v3FrontBottomRight = new Vector3(v3Center.x + v3Extents.x, v3Center.y - v3Extents.y, v3Center.z - v3Extents.z);  // Front bottom right corner
        v3BackTopLeft = new Vector3(v3Center.x - v3Extents.x, v3Center.y + v3Extents.y, v3Center.z + v3Extents.z);  // Back top left corner
        v3BackTopRight = new Vector3(v3Center.x + v3Extents.x, v3Center.y + v3Extents.y, v3Center.z + v3Extents.z);  // Back top right corner
        v3BackBottomLeft = new Vector3(v3Center.x - v3Extents.x, v3Center.y - v3Extents.y, v3Center.z + v3Extents.z);  // Back bottom left corner
        v3BackBottomRight = new Vector3(v3Center.x + v3Extents.x, v3Center.y - v3Extents.y, v3Center.z + v3Extents.z);  // Back bottom right corner

        //v3FrontTopLeft = transform.TransformPoint(v3FrontTopLeft);
        //v3FrontTopRight = transform.TransformPoint(v3FrontTopRight);
        //v3FrontBottomLeft = transform.TransformPoint(v3FrontBottomLeft);
        //v3FrontBottomRight = transform.TransformPoint(v3FrontBottomRight);
        //v3BackTopLeft = transform.TransformPoint(v3BackTopLeft);
        //v3BackTopRight = transform.TransformPoint(v3BackTopRight);
        //v3BackBottomLeft = transform.TransformPoint(v3BackBottomLeft);
        //v3BackBottomRight = transform.TransformPoint(v3BackBottomRight);

        DrawBox();
    }

    void DrawBox()
    {
        //if (Input.GetKey (KeyCode.S)) {
        Debug.DrawLine(v3FrontTopLeft, v3FrontTopRight, color);
        Debug.DrawLine(v3FrontTopRight, v3FrontBottomRight, color);
        Debug.DrawLine(v3FrontBottomRight, v3FrontBottomLeft, color);
        Debug.DrawLine(v3FrontBottomLeft, v3FrontTopLeft, color);

        Debug.DrawLine(v3BackTopLeft, v3BackTopRight, color);
        Debug.DrawLine(v3BackTopRight, v3BackBottomRight, color);
        Debug.DrawLine(v3BackBottomRight, v3BackBottomLeft, color);
        Debug.DrawLine(v3BackBottomLeft, v3BackTopLeft, color);

        Debug.DrawLine(v3FrontTopLeft, v3BackTopLeft, color);
        Debug.DrawLine(v3FrontTopRight, v3BackTopRight, color);
        Debug.DrawLine(v3FrontBottomRight, v3BackBottomRight, color);
        Debug.DrawLine(v3FrontBottomLeft, v3BackBottomLeft, color);
        //}
    }

}






public static class ManageExtensions
{
    public static Bounds TransformBounds(this Transform _transform, Bounds _localBounds)
    {
        var center = _transform.TransformPoint(_localBounds.center);

        // transform the local extents' axes
        var extents = _localBounds.extents;
        var axisX = _transform.TransformVector(extents.x, 0, 0);
        var axisY = _transform.TransformVector(0, extents.y, 0);
        var axisZ = _transform.TransformVector(0, 0, extents.z);

        // sum their absolute value to get the world extents
        extents.x = Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x);
        extents.y = Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y);
        extents.z = Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z);

        return new Bounds { center = center, extents = extents };
    }

    //public static Vector3 RoundToDecimal(this Vector3 _vector3, Vector3 _vec, float _decimals)
    //{
    //    _vec = new Vector3(Mathf.Round(_vec.x * Mathf.Pow(10f, _decimals)), Mathf.Round(_vec.y * Mathf.Pow(10f,_decimals)), Mathf.Round(_vec.z * Mathf.Pow(10f, _decimals)));
    //    return _vec;
    //}
}

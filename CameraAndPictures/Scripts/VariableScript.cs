using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VariableScript : MonoBehaviour
{
    //variables for other script enable/disable status

    //public static bool[] scriptEnabled;

    // Variables for Photobooth

    public static float[] projectedArea = null;
    public static float[] ratioVisibleSurfaceArea = null;
    public static float[] centerOfMassX = null;
    public static float[] centerOfMassY = null;
    public static float[] QualityOfView = null;
    public static float[] symmetryInView = null;
    public static float[] visibleEdges = null;
    public static float[] meshTriangles = null;
    
    public static string[] pictureFilePath = null;
    public static string[] pictureFileName = null;
    public static int[] numOfPicturesPerObj = null;

    public static System.Diagnostics.Stopwatch sw_total = new System.Diagnostics.Stopwatch();
    public static System.Diagnostics.Stopwatch sw_photobooth = new System.Diagnostics.Stopwatch();
    public static System.Diagnostics.Stopwatch sw_takepic = new System.Diagnostics.Stopwatch();
    public static System.Diagnostics.Stopwatch sw_endscene = new System.Diagnostics.Stopwatch();
    public static System.Diagnostics.Stopwatch sw_lightsym = new System.Diagnostics.Stopwatch();
    public static System.Diagnostics.Stopwatch sw_eulang = new System.Diagnostics.Stopwatch();
    public static System.Diagnostics.Stopwatch sw_savepic = new System.Diagnostics.Stopwatch();
    public static System.Diagnostics.Stopwatch sw_raycast = new System.Diagnostics.Stopwatch();
    public static System.Diagnostics.Stopwatch sw_qualityofview = new System.Diagnostics.Stopwatch();
    public static System.Diagnostics.Stopwatch sw_projectedarea = new System.Diagnostics.Stopwatch();
    public static System.Diagnostics.Stopwatch sw_com = new System.Diagnostics.Stopwatch();
    public static System.Diagnostics.Stopwatch sw_unloading = new System.Diagnostics.Stopwatch();
    public static System.Diagnostics.Stopwatch sw_textures = new System.Diagnostics.Stopwatch();
    public static System.Diagnostics.Stopwatch sw_getpixels = new System.Diagnostics.Stopwatch();
    public static System.Diagnostics.Stopwatch sw_bounds = new System.Diagnostics.Stopwatch();
    public static System.Diagnostics.Stopwatch sw_mesh = new System.Diagnostics.Stopwatch();
    public static System.Diagnostics.Stopwatch sw_normalize = new System.Diagnostics.Stopwatch();
    public static System.Diagnostics.Stopwatch sw_starttakepic = new System.Diagnostics.Stopwatch();
    public static System.Diagnostics.Stopwatch sw_investigate = new System.Diagnostics.Stopwatch();

    // Variables for Camera Timeline
    

}

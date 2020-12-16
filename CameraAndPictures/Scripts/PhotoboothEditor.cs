using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MMIUnity.TargetEngine.Scene;

[CustomEditor(typeof(PhotoboothScriptSceneGen))]
//[CustomEditor(typeof(AJANMMIAvatar))]
public class PhotoboothEditor : Editor
{
    SerializedProperty m_mainCam;
    SerializedProperty m_EdgeDetectCam;
    SerializedProperty m_NumberOfPictures;
    SerializedProperty m_RangeOfAnglesYaw;
    SerializedProperty m_RangeOfAnglesPitch;
    SerializedProperty m_RangeOfAnglesRoll;
    SerializedProperty m_AcceptableGrayRange;
    SerializedProperty m_AcceptablePixelMatchRatio;
    SerializedProperty m_ExceptionPixelMatchRatio;
    SerializedProperty m_PixelSkipSize;
    SerializedProperty m_ScreenshotScale;
    SerializedProperty m_NumBestViewPics;
    SerializedProperty m_ProjectedAreaWeight;
    SerializedProperty m_VisibleSurfaceAreaWeight;
    SerializedProperty m_CenterOfMassWeight;
    SerializedProperty m_SymmetryWeight;
    SerializedProperty m_VisibleEdgesWeight;
    SerializedProperty m_MeshTrianglesWeight;

    // Start is called before the first frame update
    void OnEnable()
    {
        m_mainCam =serializedObject.FindProperty("mainCam");
        m_EdgeDetectCam = serializedObject.FindProperty("EdgeDetectCam");
        m_NumberOfPictures = serializedObject.FindProperty("NumberOfPictures");
        m_AcceptableGrayRange = serializedObject.FindProperty("AcceptableGrayRange");
        m_AcceptablePixelMatchRatio = serializedObject.FindProperty("AcceptablePixelMatchRatio");
        m_ExceptionPixelMatchRatio = serializedObject.FindProperty("ExceptionPixelMatchRatio");
        m_PixelSkipSize = serializedObject.FindProperty("PixelSkipSize");
        m_ScreenshotScale = serializedObject.FindProperty("ScreenshotScale");
        m_NumBestViewPics = serializedObject.FindProperty("NumBestViewPics");
        m_ProjectedAreaWeight = serializedObject.FindProperty("ProjectedAreaWeight");
        m_VisibleSurfaceAreaWeight = serializedObject.FindProperty("VisibleSurfaceAreaWeight");
        m_CenterOfMassWeight = serializedObject.FindProperty("CenterOfMassWeight");
        m_SymmetryWeight = serializedObject.FindProperty("SymmetryWeight");
        m_VisibleEdgesWeight = serializedObject.FindProperty("VisibleEdgesWeight");
        m_MeshTrianglesWeight = serializedObject.FindProperty("MeshTrianglesWeight");
    }

    // Update is called once per frame
    public override void OnInspectorGUI()
    {
        if (!Application.isPlaying)
            if (GUILayout.Button("Build picture database"))
            {
                HighLevelTaskEditor HLTE = Selection.activeGameObject.GetComponent<HighLevelTaskEditor>();
                HLTE.updatePartToolList();
                Selection.activeGameObject.GetComponent<PhotoboothScriptSceneGen>().StoreMonoBehaviorScriptsState();
                Selection.activeGameObject.GetComponent<PhotoboothScriptSceneGen>().startPhotoShoot = true;
                Selection.activeGameObject.GetComponent<PhotoboothScriptSceneGen>().ClearDoneFlag();
                EditorApplication.ExecuteMenuItem("Edit/Play");
            }

        EditorGUILayout.PropertyField(m_mainCam, new GUIContent("Main camera"));
        EditorGUILayout.PropertyField(m_EdgeDetectCam, new GUIContent("Edge camera"));
        
        EditorGUILayout.PropertyField(m_NumberOfPictures);
        EditorGUILayout.PropertyField(m_AcceptableGrayRange);
        EditorGUILayout.PropertyField(m_AcceptablePixelMatchRatio);
        EditorGUILayout.PropertyField(m_ExceptionPixelMatchRatio);
        EditorGUILayout.PropertyField(m_PixelSkipSize);
        EditorGUILayout.PropertyField(m_ScreenshotScale);
        EditorGUILayout.PropertyField(m_NumBestViewPics);
        EditorGUILayout.PropertyField(m_ProjectedAreaWeight);

        EditorGUILayout.PropertyField(m_VisibleSurfaceAreaWeight);
        EditorGUILayout.PropertyField(m_CenterOfMassWeight);
        EditorGUILayout.PropertyField(m_SymmetryWeight);
        EditorGUILayout.PropertyField(m_MeshTrianglesWeight);
        
        serializedObject.ApplyModifiedProperties();
    }
}

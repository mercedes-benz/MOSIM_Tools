using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CharactorBonesVisController))]
public class CharactorBonesVisControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var cbvc = target as CharactorBonesVisController;
        cbvc.alpha = EditorGUILayout.Slider(new GUIContent("Alpha"),cbvc.alpha, 0, 1);
    }
}

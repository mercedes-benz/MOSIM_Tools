using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CharacterMeshRendererController))]
public class CharacterMeshRendererControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var cmrc = target as CharacterMeshRendererController;
        cmrc.alpha = EditorGUILayout.Slider(new GUIContent("Alpha"), cmrc.alpha, 0, 1);
    }
}

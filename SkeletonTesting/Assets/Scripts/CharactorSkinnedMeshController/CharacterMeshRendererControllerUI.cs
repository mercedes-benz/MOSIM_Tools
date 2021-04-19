using UnityEngine;
[RequireComponent(typeof(CharacterMeshRendererController))]
public class CharacterMeshRendererControllerUI : MonoBehaviour
{
    public CharacterMeshRendererController characterMeshRendererController;
    void Awake()
    {
        characterMeshRendererController = GetComponent<CharacterMeshRendererController>();
    }

    void OnGUI()
    {
        GUILayout.Label("Opacity:");
        float alpha = characterMeshRendererController.alpha;
        alpha = GUILayout.HorizontalSlider(alpha, 0.0f, 1.0f, GUILayout.MinWidth(100));
        characterMeshRendererController.alpha = alpha;
    }
}
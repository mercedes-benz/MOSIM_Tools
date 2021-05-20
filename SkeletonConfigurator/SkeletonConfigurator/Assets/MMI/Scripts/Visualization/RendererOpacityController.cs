using UnityEngine;
using System.Collections.Generic;

/// <summary> Change opacity of a renderer </summary>
public class RendererOpacityController
{
    private Renderer renderer;
    private float alpha = 1.0f;

    private bool init = false;
    private List<int[]> materialSettings = new List<int[]>();
    
    public RendererOpacityController(Renderer renderer)
    {
        this.renderer = renderer;
    }

    public float Alpha
    {
        get
        {
            return alpha;
        }

        set
        {
            if (value < 0)
                alpha = 0;
            else if (value > 1)
                alpha = 1;
            else
                alpha = value;

            Material[] materials = renderer.materials;

            if(!init)
            {
                foreach (Material material in materials)
                {
                    int[] settings = new int[7];
                    settings[0] = material.GetInt("_SrcBlend");
                    settings[1] = material.GetInt("_DstBlend");
                    settings[2] = material.GetInt("_ZWrite");
                    if (material.IsKeywordEnabled("_ALPHATEST_ON"))
                        settings[3] = 1;
                    else
                        settings[3] = 0;

                    if (material.IsKeywordEnabled("_ALPHABLEND_ON"))
                        settings[4] = 1;
                    else
                        settings[4] = 0;


                    if (material.IsKeywordEnabled("_ALPHAPREMULTIPLY_ON"))
                        settings[5] = 1;
                    else
                        settings[5] = 0;

                    settings[6] = material.renderQueue;

                    this.materialSettings.Add(settings);
                }
            }

            // if alpha is 0, disable renderer
            renderer.enabled = (alpha != 0);
            if (renderer.enabled)
            {
                if (alpha == 1.0f)
                    for(int i = 0; i<materials.Length; i++)
                    {
                        Material material = materials[i];
                        material.SetInt("_SrcBlend", materialSettings[i][0]);
                        material.SetInt("_DstBlend", materialSettings[i][1]);
                        material.SetInt("_ZWrite", materialSettings[i][2]);
                        if (materialSettings[i][3] == 1)
                            material.EnableKeyword("_ALPHATEST_ON");
                        else 
                            material.DisableKeyword("_ALPHATEST_ON");
                        if (materialSettings[i][4] == 1)
                            material.EnableKeyword("_ALPHABLEND_ON");
                        else
                            material.DisableKeyword("_ALPHABLEND_ON");
                        if (materialSettings[i][5] == 1)
                            material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                        else
                            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = materialSettings[i][6];
                    }
                // else is transparent
                else
                    foreach (Material material in materials)
                    {
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        material.SetInt("_ZWrite", 0);
                        material.DisableKeyword("_ALPHATEST_ON");
                        material.DisableKeyword("_ALPHABLEND_ON");
                        material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = 3000;

                        Color color = material.color;
                        color.a = alpha;
                        material.color = color;
                    }
            }
        }
    }
}
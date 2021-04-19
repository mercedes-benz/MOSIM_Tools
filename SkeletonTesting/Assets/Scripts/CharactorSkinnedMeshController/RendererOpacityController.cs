using UnityEngine;

/// <summary> Change opacity of a renderer </summary>
public class RendererOpacityController
{
    private Renderer renderer;
    private float alpha = 1.0f;
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

            // if alpha is 0, disable renderer
            renderer.enabled = (alpha != 0);
            if (renderer.enabled)
            {
                if (alpha == 1.0f)
                    foreach (Material material in materials)
                    {
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                        material.SetInt("_ZWrite", 1);
                        material.DisableKeyword("_ALPHATEST_ON");
                        material.DisableKeyword("_ALPHABLEND_ON");
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = -1;
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
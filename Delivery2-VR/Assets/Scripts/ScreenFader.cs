using System.Collections;
using UnityEngine;

// creates a black quad in front of the camera and changes its alpha
// (the teleport pointer adds this script to the center eye anchor by itself)
public class ScreenFader : MonoBehaviour
{
    public Color fadeColor = Color.black;
    public float fadeDistance = 0.25f;

    private Material mat;
    private Renderer quadRenderer;
    private float currentAlpha;

    void Awake()
    {
        // quad just in front of the camera, big enough to cover the whole view
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "FadeQuad";
        Destroy(quad.GetComponent<Collider>());
        quad.transform.SetParent(transform, false);
        quad.transform.localPosition = new Vector3(0f, 0f, fadeDistance);
        quad.transform.localScale = new Vector3(3f, 3f, 1f);

        mat = new Material(Shader.Find("Sprites/Default"));
        mat.renderQueue = 4000; // draw it over everything else

        quadRenderer = quad.GetComponent<Renderer>();
        quadRenderer.sharedMaterial = mat;

        SetAlpha(0f);
    }

    // fades from the current alpha to the target (1 = black, 0 = clear)
    public IEnumerator FadeTo(float target, float duration)
    {
        float start = currentAlpha;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            SetAlpha(Mathf.Lerp(start, target, t / duration));
            yield return null;
        }

        SetAlpha(target);
    }

    void SetAlpha(float alpha)
    {
        currentAlpha = alpha;
        Color c = fadeColor;
        c.a = alpha;
        mat.color = c;

        // no need to draw it when it is invisible
        quadRenderer.enabled = alpha > 0.001f;
    }
}
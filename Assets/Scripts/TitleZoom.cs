using UnityEngine;

public class TitleZoom : MonoBehaviour
{
    public float zoomDuration = 1f;
    public Vector3 startScale = new Vector3(0.01f, 0.01f, 1f);
    public Vector3 endScale = new Vector3(1f, 1f, 1f);

    void Start()
    {
        // Start from small scale
        transform.localScale = startScale;
        StartCoroutine(ZoomIn());
    }

    private System.Collections.IEnumerator ZoomIn()
    {
        float t = 0f;

        while (t < zoomDuration)
        {
            t += Time.deltaTime;
            float lerp = t / zoomDuration;

            transform.localScale = Vector3.Lerp(startScale, endScale, lerp);
            yield return null;
        }

        transform.localScale = endScale; // make sure final scale is exact
    }
}

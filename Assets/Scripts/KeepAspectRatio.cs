using UnityEngine;

[RequireComponent(typeof(Camera))]
public class KeepAspectRatio : MonoBehaviour
{
    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        float ratio = ((float)Screen.width / Screen.height) / (16f / 9f);

        if (ratio < 1f)
            cam.rect = new Rect(0f, (1f - ratio) / 2f, 1f, ratio);
        else
            cam.rect = new Rect((1f - 1f / ratio) / 2f, 0f, 1f / ratio, 1f);
    }
}


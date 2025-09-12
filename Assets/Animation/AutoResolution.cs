using UnityEngine;

public class AutoResolution : MonoBehaviour
{
    // Resolução de referência (a que você fez o jogo, no seu caso 1920x1080)
    public Vector2 referenceResolution = new Vector2(1920, 1080);

    void Start()
    {
        AdaptResolution();
    }

    void AdaptResolution()
    {
        float targetAspect = referenceResolution.x / referenceResolution.y;
        float windowAspect = (float)Screen.width / (float)Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        Camera cam = Camera.main;

        if (scaleHeight < 1.0f)
        {
            // Adapta adicionando barras horizontais (letterbox)
            Rect rect = cam.rect;

            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;

            cam.rect = rect;
        }
        else
        {
            // Adapta adicionando barras verticais (pillarbox)
            float scaleWidth = 1.0f / scaleHeight;

            Rect rect = cam.rect;

            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;

            cam.rect = rect;
        }
    }
}

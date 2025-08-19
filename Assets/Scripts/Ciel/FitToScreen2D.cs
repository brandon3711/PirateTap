using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
public class FitToScreen2D : MonoBehaviour
{
    public enum FitMode { Width, Height, Cover }
    public FitMode mode = FitMode.Cover;
    public float padding = 0f; // marge en unités monde

    SpriteRenderer sr;

    void OnEnable()
    {
        sr = GetComponent<SpriteRenderer>();
        ApplyFit();
    }

    void LateUpdate() => ApplyFit();

    void ApplyFit()
    {
        if (sr == null || sr.sprite == null) return;

        Camera cam = Camera.main;
        if (cam == null || !cam.orthographic) return;

        // Taille du sprite en unités monde
        Vector2 spriteSize = sr.sprite.bounds.size;
        if (spriteSize.x <= 0f || spriteSize.y <= 0f) return;

        // Taille de l'écran (monde)
        float worldH = cam.orthographicSize * 2f;
        float worldW = worldH * Mathf.Max(0.0001f, cam.aspect);

        float scaleX = (worldW  + padding) / spriteSize.x;
        float scaleY = (worldH + padding) / spriteSize.y;

        float scale = mode switch
        {
            FitMode.Width  => scaleX,
            FitMode.Height => scaleY,
            _              => Mathf.Max(scaleX, scaleY) // Cover = couvre tout l'écran
        };

        scale = Mathf.Max(scale, 0.0001f); // évite 0
        transform.localScale = new Vector3(scale, scale, 1f);
    }
}

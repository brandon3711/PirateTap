using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
public class FitSpriteToCamera : MonoBehaviour
{
    public float padding = 0f; // marge en unités monde
    SpriteRenderer sr;

    void OnEnable()
    {
        sr = GetComponent<SpriteRenderer>();
        Apply();
    }

    void LateUpdate() => Apply();

    void Apply()
    {
        if (!sr || sr.sprite == null) return;
        var cam = Camera.main;
        if (!cam || !cam.orthographic) return;

        // Assure-toi qu'on peut régler la taille
        sr.drawMode = SpriteDrawMode.Tiled;

        // Dimensions de l'écran en unités monde
        float worldH = cam.orthographicSize * 2f;
        float worldW = worldH * cam.aspect;

        // On remplit l'écran en ajustant la "size" du sprite (pas le scale)
        sr.size = new Vector2(worldW + padding, worldH + padding);

        // Optionnel : on recentre / remet à plat
        transform.position = new Vector3(0f, 0f, 0f);
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }
}

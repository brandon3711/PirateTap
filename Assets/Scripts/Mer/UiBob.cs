using UnityEngine;

public class UIBob : MonoBehaviour
{
    [Header("Mouvement vertical (houle)")]
    public float amplitudeY = 8f;     // déplacement vertical (pixels UI)
    public float speedY     = 0.6f;   // vitesse verticale
    public float scalePulse = 0.01f;  // petite respiration (échelle)

    [Header("Drift horizontal (léger)")]
    public float amplitudeX = 6f;     // déplacement horizontal (pixels UI)
    public float speedX     = 0.25f;  // vitesse horizontale

    RectTransform rt;
    Vector2 startPos;
    Vector3 startScale;

    void Awake()
    {
        rt = (RectTransform)transform;
        startPos = rt.anchoredPosition;
        startScale = rt.localScale;
    }

    void Update()
    {
        float sx = Mathf.Sin(Time.time * speedX);
        float sy = Mathf.Sin(Time.time * speedY);

        // Position: léger drift en X + houle en Y
        rt.anchoredPosition = startPos + new Vector2(sx * amplitudeX, sy * amplitudeY);

        // Mini respiration (optionnelle)
        rt.localScale = startScale * (1f + sy * scalePulse);
    }
}

using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class CloudParallaxLayerUI : MonoBehaviour
{
    public bool autoCollectChildren = true;
    public RectTransform[] clouds;

    [Header("Défilement & boucle")]
    public float speed = 100f;        // px/s gauche
    public float spacing = 700f;      // distance entre nuages
    public float wrapPadding = 250f;  // marge de wrap

    [Header("Houle (optionnel)")]
    public float bobAmp = 0f;
    public float bobSpeed = 0f;

    [Header("Teinte par phase (reçues du BG Manager)")]
    public Color tintMorning = Color.white;
    public Color tintDay     = Color.white;
    public Color tintEvening = Color.white;
    public Color tintNight   = Color.white;

    RectTransform canvasRT;
    Vector2[] startPos;
    float[] phaseOffset;

    void Awake()
    {
        var canvas = GetComponentInParent<Canvas>();
        canvasRT = canvas ? canvas.GetComponent<RectTransform>() : null;

        if (autoCollectChildren || clouds == null || clouds.Length == 0)
        {
            var imgs = GetComponentsInChildren<Image>(true);
            clouds = new RectTransform[imgs.Length];
            for (int i = 0; i < imgs.Length; i++)
                clouds[i] = imgs[i].GetComponent<RectTransform>();
        }

        if (clouds == null || clouds.Length == 0)
        {
            Debug.LogWarning($"[Parallax] Aucun nuage sur {name}.", this);
            return;
        }

        startPos = new Vector2[clouds.Length];
        phaseOffset = new float[clouds.Length];
        for (int i = 0; i < clouds.Length; i++)
        {
            if (!clouds[i]) continue;
            startPos[i] = clouds[i].anchoredPosition;
            phaseOffset[i] = Random.value * 10f;
        }

        if (canvasRT)
        {
            float w = canvasRT.rect.width;
            float x = -w * 0.5f;
            for (int i = 0; i < clouds.Length; i++)
                if (clouds[i]) clouds[i].anchoredPosition = new Vector2(x + i * spacing, clouds[i].anchoredPosition.y);
        }

        Debug.Log($"[Parallax] {name} prêt ({clouds.Length} nuages).", this);
    }

    void Update()
    {
        if (clouds == null || canvasRT == null) return;

        float width = canvasRT.rect.width;
        float leftLimit = -width * 0.5f - wrapPadding;

        // Trouver le plus à droite pour respawn
        float rightMost = float.MinValue;
        for (int i = 0; i < clouds.Length; i++)
            if (clouds[i]) rightMost = Mathf.Max(rightMost, clouds[i].anchoredPosition.x);
        float rightSpawn = rightMost + spacing;

        float dx = speed * Time.deltaTime;

        for (int i = 0; i < clouds.Length; i++)
        {
            var rt = clouds[i];
            if (!rt) continue;

            Vector2 pos = rt.anchoredPosition;
            pos.x -= dx;

            if (bobAmp > 0f && bobSpeed > 0f)
            {
                float bob = Mathf.Sin((Time.time + phaseOffset[i]) * bobSpeed) * bobAmp;
                pos.y = startPos[i].y + bob;
            }

            if (pos.x < leftLimit) pos.x = rightSpawn;
            rt.anchoredPosition = pos;
        }
    }

    public void SetPhase(BackgroundManagerUI.Phase phase)
    {
        Color tint = tintDay;
        switch (phase)
        {
            case BackgroundManagerUI.Phase.Morning: tint = tintMorning; break;
            case BackgroundManagerUI.Phase.Day:     tint = tintDay;     break;
            case BackgroundManagerUI.Phase.Evening: tint = tintEvening; break;
            case BackgroundManagerUI.Phase.Night:   tint = tintNight;   break;
        }

        if (clouds == null) return;
        for (int i = 0; i < clouds.Length; i++)
        {
            var img = clouds[i] ? clouds[i].GetComponent<Image>() : null;
            if (!img) continue;
            var c = img.color;
            img.color = new Color(tint.r, tint.g, tint.b, c.a);
        }
    }
}

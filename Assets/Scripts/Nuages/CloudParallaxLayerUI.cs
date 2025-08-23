using UnityEngine;
using UnityEngine.UI;

public class CloudParallaxLayerUI : MonoBehaviour
{
    [Header("Nuages de cette couche (objets de la SCÈNE)")]
    public RectTransform[] clouds;

    [Header("Défilement & boucle")]
    [Tooltip("Pixels UI/seconde. Positif = vers la gauche.")]
    public float speed = 30f;
    [Tooltip("Écart X moyen entre nuages quand on les redistribue.")]
    public float spacing = 350f;
    [Tooltip("Marge hors écran avant de réapparaître à droite.")]
    public float wrapPadding = 120f;

    [Header("Houle (optionnel)")]
    public float bobAmp = 6f;    // amplitude verticale (px)
    public float bobSpeed = 0.5f;// vitesse houle

    [Header("Teinte par phase")]
    public Color tintMorning = Color.white;
    public Color tintDay     = Color.white;
    public Color tintEvening = new Color(1f, 0.9f, 0.85f, 1f);
    public Color tintNight   = new Color(0.75f, 0.82f, 1f, 1f);

    RectTransform canvasRT;
    Vector2[] basePos;
    float[] phaseOff;

    void Awake()
    {
        var canvas = GetComponentInParent<Canvas>();
        canvasRT = canvas ? canvas.GetComponent<RectTransform>() : null;

        if (clouds == null) clouds = new RectTransform[0];
        basePos  = new Vector2[clouds.Length];
        phaseOff = new float[clouds.Length];

        for (int i = 0; i < clouds.Length; i++)
        {
            if (!clouds[i]) continue;
            basePos[i]  = clouds[i].anchoredPosition;
            phaseOff[i] = Random.value * 10f;
        }

        DistributeHorizontally();
    }

    void DistributeHorizontally()
    {
        if (!canvasRT || clouds.Length == 0) return;
        float w = canvasRT.rect.width;
        float x = -w * 0.5f;
        for (int i = 0; i < clouds.Length; i++)
        {
            if (!clouds[i]) continue;
            var p = clouds[i].anchoredPosition;
            clouds[i].anchoredPosition = new Vector2(x, p.y);
            x += spacing;
        }
    }

    void Update()
    {
        if (!canvasRT || clouds.Length == 0) return;

        float w = canvasRT.rect.width;
        float left  = -w * 0.5f - wrapPadding;
        float right =  w * 0.5f + wrapPadding;

        for (int i = 0; i < clouds.Length; i++)
        {
            var rt = clouds[i];
            if (!rt) continue;

            // défilement
            var pos = rt.anchoredPosition;
            pos.x -= speed * Time.deltaTime;

            // houle
            float bob = Mathf.Sin((Time.time + phaseOff[i]) * bobSpeed) * bobAmp;
            pos.y = basePos[i].y + bob;

            // wrap : si trop à gauche -> réapparaît après le nuage le plus à droite
            if (pos.x < left)
            {
                float maxX = pos.x;
                for (int j = 0; j < clouds.Length; j++)
                    if (j != i && clouds[j])
                        maxX = Mathf.Max(maxX, clouds[j].anchoredPosition.x);
                pos.x = Mathf.Max(maxX, right) + spacing * 0.5f;
            }

            rt.anchoredPosition = pos;
        }
    }

    // Appelée par le BackgroundManagerUI
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

        for (int i = 0; i < clouds.Length; i++)
        {
            var rt = clouds[i];
            if (!rt) continue;
            var img = rt.GetComponent<Image>();
            if (!img) continue;

            var c = img.color; // on garde l’alpha existant
            img.color = new Color(tint.r, tint.g, tint.b, c.a);
        }
    }
}

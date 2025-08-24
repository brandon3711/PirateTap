using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BackgroundManagerUI : MonoBehaviour
{
    [Header("Refs (UI)")]
    public Image skyImage;   // BackgroundCanvas/SkyImage
    public Image seaImage;   // BackgroundCanvas/SeaImage

    [Header("Ciels (ordre: Matin, Jour, Soirée, Nuit)")]
    public Sprite skyMorning;
    public Sprite skyDay;
    public Sprite skyEvening;
    public Sprite skyNight;

    [Header("Couleurs mer (même ordre)")]
    public Color seaMorning = new Color(0.65f, 0.85f, 1f, 1f);
    public Color seaDay     = new Color(0.40f, 0.75f, 1f, 1f);
    public Color seaEvening = new Color(0.50f, 0.55f, 0.90f, 1f);
    public Color seaNight   = new Color(0.20f, 0.25f, 0.45f, 1f);

    [Header("Nuages (optionnel)")]
    public RectTransform layerFar;   // CloudsParallax/LayerFar (UI)
    public RectTransform layerNear;  // CloudsParallax/LayerNear (UI)
    [Range(0f, 1f)] public float cloudAlpha = 0.70f;  // opacité globale

    [Header("Couleurs nuages (par phase)")]
    public Color cloudMorning = new Color(1f, 1f, 1f, 1f); // blanc pur matin
    public Color cloudDay     = new Color(0.95f, 0.95f, 1f, 1f); // léger bleu
    public Color cloudEvening = new Color(1f, 0.85f, 0.85f, 1f); // rosé
    public Color cloudNight   = new Color(0.7f, 0.75f, 1f, 1f);  // bleu nuit

    [Header("Cycle")]
    public int levelsPerPhase = 5;       // tous les 5 niveaux on change
    public float transitionTime = 0.35f; // fondu mer/nuages

    public enum Phase { Morning = 0, Day = 1, Evening = 2, Night = 3 }

    Coroutine transitionCR;

    void Start()
    {
        ApplyForLevel(0); // par défaut : matin
    }

    public void ApplyForLevel(int levelIndex)
    {
        int phaseIndex = Mathf.FloorToInt(levelIndex / Mathf.Max(1, levelsPerPhase)) % 4;
        SetPhase((Phase)phaseIndex);
    }

    public void SetPhase(Phase phase)
    {
        Sprite targetSky;
        Color  targetSea;
        Color  targetCloud;

        switch (phase)
        {
            default:
            case Phase.Morning:
                targetSky = skyMorning; targetSea = seaMorning; targetCloud = cloudMorning; break;
            case Phase.Day:
                targetSky = skyDay;     targetSea = seaDay;     targetCloud = cloudDay;     break;
            case Phase.Evening:
                targetSky = skyEvening; targetSea = seaEvening; targetCloud = cloudEvening; break;
            case Phase.Night:
                targetSky = skyNight;   targetSea = seaNight;   targetCloud = cloudNight;   break;
        }

        if (transitionCR != null) StopCoroutine(transitionCR);
        transitionCR = StartCoroutine(TransitionTo(targetSky, targetSea, targetCloud, transitionTime));
    }

    IEnumerator TransitionTo(Sprite sky, Color sea, Color cloud, float t)
    {
        if (skyImage) skyImage.sprite = sky;

        Color seaStart   = seaImage ? seaImage.color : Color.white;
        Color cloudStart = GetCurrentCloudColor();
        Color cloudEnd   = new Color(cloud.r, cloud.g, cloud.b, cloudAlpha);

        if (t <= 0f)
        {
            if (seaImage) seaImage.color = sea;
            ApplyCloudTint(cloudEnd);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < t)
        {
            elapsed += Time.deltaTime;
            float k = Mathf.Clamp01(elapsed / t);

            if (seaImage) seaImage.color = Color.Lerp(seaStart, sea, k);
            ApplyCloudTint(Color.Lerp(cloudStart, cloudEnd, k));

            yield return null;
        }

        if (seaImage) seaImage.color = sea;
        ApplyCloudTint(cloudEnd);
    }

    // Récupère la couleur actuelle d’un nuage (sert pour le fondu)
    Color GetCurrentCloudColor()
    {
        if (layerFar)
        {
            var img = layerFar.GetComponentInChildren<Image>();
            if (img) return img.color;
        }
        if (layerNear)
        {
            var img = layerNear.GetComponentInChildren<Image>();
            if (img) return img.color;
        }
        return Color.white;
    }

    void ApplyCloudTint(Color tint)
    {
        if (layerFar)  TintAllImagesUnder(layerFar,  tint);
        if (layerNear) TintAllImagesUnder(layerNear, tint);
    }

    static void TintAllImagesUnder(Transform root, Color c)
    {
        if (!root) return;
        var imgs = root.GetComponentsInChildren<Image>(true);
        for (int i = 0; i < imgs.Length; i++)
        {
            imgs[i].color = c;
        }
    }
}

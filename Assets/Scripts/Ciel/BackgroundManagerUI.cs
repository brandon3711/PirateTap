using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BackgroundManagerUI : MonoBehaviour
{
    // --------- Références UI principales ---------
    [Header("Refs (UI)")]
    public Image skyImage;   // drag: BackgroundCanvas/SkyImage (Image)
    public Image seaImage;   // drag: BackgroundCanvas/SeaImage (Image)

    // --------- Sprites de ciel par phase ---------
    [Header("Ciels (ordre: Matin, Jour, Soirée, Nuit)")]
    public Sprite skyMorning;
    public Sprite skyDay;
    public Sprite skyEvening;
    public Sprite skyNight;

    // --------- Couleur de la mer par phase ---------
    [Header("Couleurs mer (même ordre)")]
    public Color seaMorning = new Color(0.65f, 0.85f, 1f, 1f);
    public Color seaDay     = new Color(0.40f, 0.75f, 1f, 1f);
    public Color seaEvening = new Color(0.50f, 0.55f, 0.90f, 1f);
    public Color seaNight   = new Color(0.20f, 0.25f, 0.45f, 1f);

    // --------- Nuages (parallax) ---------
    [Header("Nuages (parallax UI)")]
    public CloudParallaxLayerUI layerFar;   // drag: BackgroundCanvas/CloudsParallax/LayerFar
    public CloudParallaxLayerUI layerNear;  // drag: BackgroundCanvas/CloudsParallax/LayerNear

    // --------- Cycle & fondu ---------
    [Header("Cycle")]
    [Tooltip("Tous les X niveaux on passe à la phase suivante")]
    public int levelsPerPhase = 5;

    [Tooltip("Durée du fondu de couleur mer (0 = instant)")]
    public float transitionTime = 0.35f;

    // --------- Phase ---------
    public enum Phase { Morning = 0, Day = 1, Evening = 2, Night = 3 }

    [HideInInspector] public Phase currentPhase = Phase.Morning;

    Coroutine transitionCR;

    void Start()
    {
        // Au lancement, applique une phase par défaut (matin)
        ApplyForLevel(0);
    }

    /// <summary>
    /// Applique la phase en fonction de l'index de niveau (0,1,2,3…).
    /// </summary>
    public void ApplyForLevel(int levelIndex)
    {
        int phaseIndex = Mathf.FloorToInt(levelIndex / Mathf.Max(1, levelsPerPhase)) % 4;
        SetPhase((Phase)phaseIndex);
    }

    /// <summary>
    /// Force l'application d'une phase (utile pour tests ou changement direct).
    /// </summary>
    public void SetPhase(Phase phase)
    {
        currentPhase = phase;

        // Choix des cibles (ciel + couleur mer)
        Sprite targetSky;
        Color  targetSea;
        switch (phase)
        {
            default:
            case Phase.Morning: targetSky = skyMorning; targetSea = seaMorning; break;
            case Phase.Day:     targetSky = skyDay;     targetSea = seaDay;     break;
            case Phase.Evening: targetSky = skyEvening; targetSea = seaEvening; break;
            case Phase.Night:   targetSky = skyNight;   targetSea = seaNight;   break;
        }

        // Lance/relance le fondu mer + swap sprite ciel
        if (transitionCR != null) StopCoroutine(transitionCR);
        transitionCR = StartCoroutine(TransitionTo(targetSky, targetSea, transitionTime));

        // Informe les couches de nuages pour ajuster leur teinte
        if (layerFar)  layerFar.SetPhase(phase);
        if (layerNear) layerNear.SetPhase(phase);
    }

    /// <summary>
    /// Petit fondu de la mer. Le sprite du ciel est changé au début.
    /// </summary>
    IEnumerator TransitionTo(Sprite sky, Color sea, float t)
    {
        if (skyImage) skyImage.sprite = sky;

        if (t <= 0f)
        {
            if (seaImage) seaImage.color = sea;
            yield break;
        }

        float elapsed = 0f;
        Color start = seaImage ? seaImage.color : Color.white;

        while (elapsed < t)
        {
            elapsed += Time.deltaTime;
            float k = Mathf.Clamp01(elapsed / t);
            if (seaImage) seaImage.color = Color.Lerp(start, sea, k);
            yield return null;
        }

        if (seaImage) seaImage.color = sea;
    }

    // ---------- Helpers de test (facultatifs) ----------
    // Appelle ces méthodes depuis un LevelTester ou un bouton pour vérifier visuellement.
    public void NextPhaseForTest()
    {
        int i = ((int)currentPhase + 1) % 4;
        SetPhase((Phase)i);
    }

    public void PrevPhaseForTest()
    {
        int i = ((int)currentPhase + 3) % 4;
        SetPhase((Phase)i);
    }
}

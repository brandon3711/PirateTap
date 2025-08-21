using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BackgroundManagerUI : MonoBehaviour
{
    [Header("Refs (UI)")]
    public Image skyImage;   // drag: BackgroundCanvas/SkyImage
    public Image seaImage;   // drag: BackgroundCanvas/SeaImage

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

    [Header("Cycle")]
    public int levelsPerPhase = 5;    // tous les 5 niveaux on change
    public float transitionTime = 0.35f; // petit fondu (0 = instantané)

    public enum Phase { Morning = 0, Day = 1, Evening = 2, Night = 3 }

    Coroutine transitionCR;

    void Start()
    {
        // Au lancement, applique une phase par défaut (matin)
        ApplyForLevel(0);
    }

    public void ApplyForLevel(int levelIndex)
    {
        int phaseIndex = Mathf.FloorToInt(levelIndex / Mathf.Max(1, levelsPerPhase)) % 4;
        Phase phase = (Phase)phaseIndex;

        Sprite targetSky = skyMorning;
        Color  targetSea = seaMorning;

        switch (phase)
        {
            case Phase.Morning: targetSky = skyMorning; targetSea = seaMorning; break;
            case Phase.Day:     targetSky = skyDay;     targetSea = seaDay;     break;
            case Phase.Evening: targetSky = skyEvening; targetSea = seaEvening; break;
            case Phase.Night:   targetSky = skyNight;   targetSea = seaNight;   break;
        }

        if (transitionCR != null) StopCoroutine(transitionCR);
        transitionCR = StartCoroutine(TransitionTo(targetSky, targetSea, transitionTime));
    }

    IEnumerator TransitionTo(Sprite sky, Color sea, float t)
    {
        if (t <= 0f)
        {
            if (skyImage) skyImage.sprite = sky;
            if (seaImage) seaImage.color = sea;
            yield break;
        }

        // On fait un petit fondu de la mer (et on swap le sprite du ciel en début, c’est plus propre en UI)
        if (skyImage) skyImage.sprite = sky;

        float elapsed = 0f;
        Color seaStart = seaImage ? seaImage.color : Color.white;

        while (elapsed < t)
        {
            elapsed += Time.deltaTime;
            float k = Mathf.Clamp01(elapsed / t);
            if (seaImage) seaImage.color = Color.Lerp(seaStart, sea, k);
            yield return null;
        }

        if (seaImage) seaImage.color = sea;
    }
}

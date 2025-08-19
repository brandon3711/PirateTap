using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ProgressUI : MonoBehaviour
{
    [Header("Barre (Fill = Image Type: Filled Horizontal)")]
    public Image fillImage;   // Ton objet "Fill" (Image Type = Filled / Horizontal)

    [System.Serializable]
    public class StarRef
    {
        public Image image;         // StarBronze / StarSilver / StarGold
        public Sprite emptySprite;  // étoile grise
        public Sprite fullSprite;   // étoile couleur
        [Range(0f,1f)] public float threshold = 0.33f; // % à atteindre
        [HideInInspector] public bool unlocked;
    }

    [Header("Étoiles")]
    public StarRef bronze;
    public StarRef silver;
    public StarRef gold;

    [Header("Anim pop")]
    public float popScale = 1.2f;
    public float popTime  = 0.18f;

    float current; // 0..1

    void Start()
    {
        SetStar(bronze, false);
        SetStar(silver, false);
        SetStar(gold,   false);
        SetProgress(0f);
    }

    /// <summary>Fixe la progression (0..1) et gère le déblocage des étoiles.</summary>
    public void SetProgress(float t)
    {
        current = Mathf.Clamp01(t);

        if (fillImage != null)
            fillImage.fillAmount = current;

        TryUnlock(bronze);
        TryUnlock(silver);
        TryUnlock(gold);
    }

    /// <summary>Ajoute une quantité à la progression actuelle.</summary>
    public void AddProgress(float delta)
    {
        float start = GetProgress();
        SetProgress(start + delta);
    }

    /// <summary>Retourne la progression actuelle (0..1).</summary>
    public float GetProgress()
    {
        if (fillImage == null) return current;
        return Mathf.Clamp01(fillImage.fillAmount);
    }

    void TryUnlock(StarRef s)
    {
        if (s == null || s.image == null) return;
        if (s.unlocked) return;

        if (current >= s.threshold)
        {
            s.unlocked = true;
            SetStar(s, true);
            StartCoroutine(Pop(s.image.rectTransform));
        }
    }

    void SetStar(StarRef s, bool full)
    {
        if (s == null || s.image == null) return;
        if (full && s.fullSprite != null)       s.image.sprite = s.fullSprite;
        else if (!full && s.emptySprite != null) s.image.sprite = s.emptySprite;

        s.image.color = Color.white; // tu peux réduire l’alpha si tu veux un gris plus fade au départ
    }

    IEnumerator Pop(RectTransform rt)
    {
        if (rt == null) yield break;
        Vector3 a = Vector3.one;
        Vector3 b = a * popScale;

        float t = 0f;
        while (t < popTime) { t += Time.deltaTime; rt.localScale = Vector3.Lerp(a, b, t / popTime); yield return null; }
        t = 0f;
        while (t < popTime) { t += Time.deltaTime; rt.localScale = Vector3.Lerp(b, a, t / popTime); yield return null; }
        rt.localScale = a;
    }

    /// <summary>Reset complet des étoiles + barre.</summary>
    public void ResetStars()
    {
        bronze.unlocked = false;
        silver.unlocked = false;
        gold.unlocked   = false;

        SetStar(bronze, false);
        SetStar(silver, false);
        SetStar(gold,   false);

        SetProgress(0f);
    }
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ProgressUI : MonoBehaviour
{
    [Header("Barre (Fill = Image Type: Filled Horizontal)")]
    public Image fillImage;   // Ton objet "Fill"

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

    float current;

    void Start()
    {
        SetStar(bronze, false);
        SetStar(silver, false);
        SetStar(gold,   false);
        SetProgress(0f);
    }

    public void SetProgress(float t)
    {
        current = Mathf.Clamp01(t);
        if (fillImage) fillImage.fillAmount = current;

        TryUnlock(bronze);
        TryUnlock(silver);
        TryUnlock(gold);
    }

    void TryUnlock(StarRef s)
    {
        if (s.unlocked || s.image == null) return;
        if (current >= s.threshold)
        {
            s.unlocked = true;
            SetStar(s, true);
            StartCoroutine(Pop(s.image.rectTransform));
        }
    }

    void SetStar(StarRef s, bool full)
    {
        if (!s.image) return;
        s.image.sprite = full ? s.fullSprite : s.emptySprite;
        s.image.color  = Color.white; // tu peux baisser l’alpha si tu veux griser plus
    }

    IEnumerator Pop(RectTransform rt)
    {
        if (!rt) yield break;
        Vector3 a = Vector3.one;
        Vector3 b = a * popScale;

        float t = 0f;
        while (t < popTime) { t += Time.deltaTime; rt.localScale = Vector3.Lerp(a, b, t/popTime); yield return null; }
        t = 0f;
        while (t < popTime) { t += Time.deltaTime; rt.localScale = Vector3.Lerp(b, a, t/popTime); yield return null; }
        rt.localScale = a;
    }

    // (facultatif) reset visuel pour relancer un test
    public void ResetStars()
    {
        bronze.unlocked = silver.unlocked = gold.unlocked = false;
        SetStar(bronze, false); SetStar(silver, false); SetStar(gold, false);
        SetProgress(0f);
    }
}

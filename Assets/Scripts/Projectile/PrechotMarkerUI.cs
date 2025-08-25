using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// Anneau "pré-shot" placé à l'endroit d'impact prévu.
/// - Se rétrécit jusqu'au moment d'impact (tFlight)
/// - Clique = renvoie le timing (tClic - tImpact) au callback fourni
/// - Gère quelques petits FX (flash, popup) sans dépendances externes
/// NOTE : Ce composant travaille en UI local → utilise anchoredPosition.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class PrechotMarkerUI : MonoBehaviour, IPointerDownHandler
{
    [Header("Refs")]
    public RectTransform rt;      // RectTransform de la racine du marker
    public Image ringImage;       // L'image d'anneau (de préférence Type: Filled, Radial360)
    [Tooltip("Optionnel : petit texte qui pop (UnityEngine.UI.Text).")]
    public Text scorePopupText;   // Laisse vide si tu n'en veux pas

    [Header("Animation")]
    [Tooltip("Échelle de départ → fin pendant tFlight (1 → endScale).")]
    public float endScale = 0.35f;
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 0→1 mappé sur le temps

    // Etat
    private Vector2 _impactA;   // position d'impact en anchored (local au parent)
    private float _tFlight;     // temps jusqu'à l'impact
    private float _t0;          // temps de départ
    private bool _isTNT;
    private bool _consumed;

    // Callbacks depuis CannonShooterUI
    private System.Action<PrechotMarkerUI, float, bool> _onClick;     // (marker, dtFromImpact, isTNT)
    private System.Action<PrechotMarkerUI, bool> _onExpired;           // (marker, wasTNT)

    public void Setup(
        Vector2 impactAnchoredPos,
        float timeToImpact,
        bool isTNT,
        System.Action<PrechotMarkerUI, float, bool> onClick,
        System.Action<PrechotMarkerUI, bool> onExpired)
    {
        if (rt == null) rt = GetComponent<RectTransform>();

        _impactA = impactAnchoredPos;
        _tFlight = Mathf.Max(0.01f, timeToImpact);
        _isTNT   = isTNT;
        _onClick = onClick;
        _onExpired = onExpired;
        _t0 = Time.time;

        // Position en UI local (anchored)
        rt.anchoredPosition = _impactA;
        rt.localScale = Vector3.one; // on part de 1

        // Couleur de base (TNT = rouge léger)
        if (ringImage != null)
        {
            ringImage.color = _isTNT ? new Color(1f, 0.35f, 0.3f, 0.9f)
                                     : new Color(1f, 1f, 1f, 0.9f);
        }

        // Popup texte invisible au départ
        if (scorePopupText != null)
        {
            var c = scorePopupText.color;
            c.a = 0f;
            scorePopupText.color = c;
        }
    }

    void Update()
    {
        if (_consumed) return;

        float tNorm = Mathf.Clamp01((Time.time - _t0) / _tFlight);

        // Échelle : 1 → endScale selon la courbe
        float k = Mathf.Lerp(1f, endScale, scaleCurve.Evaluate(tNorm));
        rt.localScale = new Vector3(k, k, 1f);

        // Si l'image est en "Filled", on fait un compte à rebours visuel
        if (ringImage != null && ringImage.type == Image.Type.Filled)
            ringImage.fillAmount = 1f - tNorm;

        // Impact atteint sans clic → expiration
        if (tNorm >= 1f && !_consumed)
        {
            _consumed = true;
            _onExpired?.Invoke(this, _isTNT);
            Destroy(gameObject, 0.02f);
        }
    }

    /// <summary>
    /// Clic utilisateur : on transmet le décalage par rapport à l'impact (0 = parfait)
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (_consumed) return;

        float elapsed = Time.time - _t0;
        float dtFromImpact = elapsed - _tFlight; // pos/neg = trop tard/trop tôt
        _onClick?.Invoke(this, dtFromImpact, _isTNT);
    }

    // ---------- FX utilitaires (sans dépendance externe) ----------

    public void PingImpactFx()
    {
        // petit flash rapide de l'anneau
        if (ringImage != null && gameObject.activeInHierarchy)
            StartCoroutine(FlashRing());
    }

    public void PlayPerfectFx(int points)
    {
        Pop($"+{points}", new Color(0.2f, 1f, 0.4f, 1f));
    }

    public void PlayGoodFx(int points)
    {
        Pop($"+{points}", new Color(0.85f, 0.95f, 1f, 1f));
    }

    public void PlayTooEarlyLateFx()
    {
        Pop("OK", new Color(1f, 1f, 1f, 0.9f));
    }

    public void PlayBadFx()
    {
        Pop("–1 vie", new Color(1f, 0.3f, 0.25f, 1f));
    }

    public void Consume()
    {
        _consumed = true;
        Destroy(gameObject);
    }

    // ---------- Petites coroutines d'effet ----------

    private IEnumerator FlashRing()
    {
        // fade out rapide, puis retour
        float a0 = ringImage.color.a;
        Color c = ringImage.color;

        c.a = Mathf.Clamp01(a0 * 0.2f);
        ringImage.color = c;
        yield return new WaitForSeconds(0.06f);

        c.a = a0;
        ringImage.color = c;
        yield return new WaitForSeconds(0.06f);
    }

    private void Pop(string txt, Color col)
    {
        if (scorePopupText == null) return;

        scorePopupText.text = txt;
        scorePopupText.color = new Color(col.r, col.g, col.b, 0f);

        if (gameObject.activeInHierarchy)
            StartCoroutine(PopupRoutine(col));
    }

    private IEnumerator PopupRoutine(Color baseColor)
    {
        // petit pop : fade in + montée, puis fade out
        float durUp = 0.2f;
        float durDown = 0.25f;
        float deltaY = 40f;

        // départ
        Vector2 start = scorePopupText.rectTransform.anchoredPosition;
        Vector2 end = start + new Vector2(0f, deltaY);

        // Fade-in + move up
        float t = 0f;
        while (t < durUp)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / durUp);
            scorePopupText.color = new Color(baseColor.r, baseColor.g, baseColor.b, a);
            scorePopupText.rectTransform.anchoredPosition = Vector2.Lerp(start, end, a);
            yield return null;
        }

        // Fade-out
        t = 0f;
        while (t < durDown)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(1f - t / durDown);
            scorePopupText.color = new Color(baseColor.r, baseColor.g, baseColor.b, a);
            yield return null;
        }
    }
}

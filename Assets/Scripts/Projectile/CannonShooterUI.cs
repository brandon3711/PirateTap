using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Orchestre les tirs : cadence non linéaire, TNT aléatoire,
/// parabole en UI (anchoredPosition), spawn du projectile + pré-shot.
/// </summary>
public class CannonShooterUI : MonoBehaviour
{
    [Header("Refs")]
    public RectTransform muzzleAnchor;      // Boat/MuzzleAnchor
    public RectTransform projectileLayer;   // doit "stretch" sur tout l'écran
    public RectTransform frontFXLayer;      // idem
    public LivesUI livesUI;                 // optionnel
    public ScoreManager scoreManager;       // optionnel

    [Header("Prefabs (UI)")]
    public CannonballUI cannonballPrefab;
    public PrechotMarkerUI prechotPrefab;

    [Header("Sprites")]
    public Sprite cannonballSprite;
    public Sprite tntSprite;

    [Header("Cadence")]
    public float meanInterval = 1.6f;
    public float intervalJitter = 0.9f;
    [Range(0f, 1f)] public float burstChance = 0.25f;
    public Vector2Int burstCountRange = new Vector2Int(2, 4);
    public Vector2 burstGapRange = new Vector2(0.25f, 0.55f);

    [Header("Physique (UI)")]
    [Tooltip("Gravité (px/s²). Y vers le bas = négatif.")]
    public float gravityY = -1600f;
    public Vector2 flightTimeRange = new Vector2(0.9f, 1.6f);

    [Header("Contraintes d'impact (écran)")]
    public Vector2 impactYRange = new Vector2(450f, 1350f);
    public float impactXMargin = 90f;

    [Header("Gameplay points")]
    public int pointsPerfect = 100;
    public int pointsLateEarly = 50;
    public float perfectWindow = 0.08f;
    public float halfWindow = 0.25f;

    [Header("TNT")]
    [Range(0f, 1f)] public float tntProbability = 0.15f;

    // interne
    private float _nextShotTime;
    private readonly Queue<float> _burstSchedule = new Queue<float>();
    private Vector2 _canvasSize;

    void Start()
    {
        var canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.pixelRect.width > 0)
            _canvasSize = new Vector2(canvas.pixelRect.width, canvas.pixelRect.height);
        else
            _canvasSize = new Vector2(1080f, 1920f);

        ScheduleNext();
    }

    void Update()
    {
        float now = Time.time;

        if (_burstSchedule.Count > 0)
        {
            if (now >= _burstSchedule.Peek())
            {
                _burstSchedule.Dequeue();
                FireOne();
            }
            return;
        }

        if (now >= _nextShotTime)
        {
            if (Random.value < burstChance)
            {
                int n = Random.Range(burstCountRange.x, burstCountRange.y + 1);
                float t = now;
                for (int i = 0; i < n; i++)
                {
                    float gap = Random.Range(burstGapRange.x, burstGapRange.y);
                    t += gap;
                    _burstSchedule.Enqueue(t);
                }
            }
            else
            {
                FireOne();
            }
            ScheduleNext();
        }
    }

    void ScheduleNext()
    {
        float jitter = Random.Range(-intervalJitter, intervalJitter);
        _nextShotTime = Time.time + Mathf.Max(0.15f, meanInterval + jitter);
    }

    // --- Tir unique ---
    void FireOne()
    {
        if (muzzleAnchor == null || projectileLayer == null || frontFXLayer == null ||
            cannonballPrefab == null || prechotPrefab == null)
            return;

        // 1) Point d'impact en pixels écran (on évite bords/haut/bas)
        Vector2 impactScreen = new Vector2(
            Random.Range(impactXMargin, _canvasSize.x - impactXMargin),
            Random.Range(impactYRange.x, impactYRange.y)
        );

        // 2) Prépare la caméra correcte pour les conversions (Overlay vs ScreenSpace-Camera)
        var parentCanvas = projectileLayer.GetComponentInParent<Canvas>();
        Camera cam = null;
        if (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            cam = parentCanvas.worldCamera; // obligatoire en Screen Space - Camera

        // 3) Convertir départ + impact en coordonnées **anchored** du projectileLayer
        Vector2 startScreen = RectTransformUtility.WorldToScreenPoint(cam, muzzleAnchor.position);

        RectTransform layer = projectileLayer;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(layer, startScreen, cam, out Vector2 startA);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(layer, impactScreen, cam, out Vector2 impactA);

        // 4) Physique en UI (anchored)
        float tFlight = Random.Range(flightTimeRange.x, flightTimeRange.y);
        Vector2 gA = new Vector2(0f, gravityY);

        // v0 = (p - s - 0.5 g t^2) / t
        Vector2 v0A = (impactA - startA - 0.5f * gA * (tFlight * tFlight)) / tFlight;

        // 5) Type
        bool isTNT = (Random.value < tntProbability);

        // 6) Spawn projectile
        var proj = Instantiate(cannonballPrefab, projectileLayer);
        proj.Setup(startA, v0A, gA, tFlight, isTNT ? tntSprite : cannonballSprite);

        // 7) Pré-shot
        var marker = Instantiate(prechotPrefab, frontFXLayer);
        if (marker.rt == null) marker.rt = marker.GetComponent<RectTransform>();
        marker.rt.anchoredPosition = impactA;
        marker.Setup(impactA, tFlight, isTNT, OnMarkerClicked, OnMarkerExpired);

        // 8) Lien FX
        proj.BindMarker(marker);
    }

    // --- Callbacks ---
    private void OnMarkerClicked(PrechotMarkerUI marker, float dtFromImpact, bool isTNT)
    {
        if (isTNT)
        {
            if (livesUI != null) livesUI.LoseLife(1);
            marker.PlayBadFx();
            marker.Consume();
            return;
        }

        float dt = Mathf.Abs(dtFromImpact);
        if (dt <= perfectWindow)
        {
            if (scoreManager != null) scoreManager.AddScore(pointsPerfect);
            marker.PlayPerfectFx(pointsPerfect);
        }
        else if (dt <= halfWindow)
        {
            if (scoreManager != null) scoreManager.AddScore(pointsLateEarly);
            marker.PlayGoodFx(pointsLateEarly);
        }
        else
        {
            marker.PlayTooEarlyLateFx();
        }

        marker.Consume();
    }

    private void OnMarkerExpired(PrechotMarkerUI marker, bool wasTNT)
    {
        if (!wasTNT && livesUI != null)
            livesUI.LoseLife(1);
    }
}

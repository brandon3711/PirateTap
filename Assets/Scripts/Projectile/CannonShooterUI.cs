using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Orchestration des tirs: cadence non linéaire, choix TNT, calcul de l'arc,
/// spawn du projectile + du pré-shot synchronisé.
/// </summary>
public class CannonShooterUI : MonoBehaviour
{
    [Header("Refs")]
    public RectTransform muzzleAnchor;      // BoatMuzzleRef.muzzleAnchor (RectTransform)
    public RectTransform projectileLayer;   // UI layer pour projectiles
    public RectTransform frontFXLayer;      // UI layer pour pré-shot
    public LivesUI livesUI;                 // optionnel: ton script existant (pour LoseLife)
    public ScoreManager scoreManager;       // optionnel: ton script de score (AddScore)

    [Header("Prefabs (UI)")]
    public CannonballUI cannonballPrefab;   // prefab avec Image + script CannonballUI
    public PrechotMarkerUI prechotPrefab;   // prefab anneau cliquable

    [Header("Sprites")]
    public Sprite cannonballSprite;         // sprite boulet
    public Sprite tntSprite;                // sprite TNT (piège)

    [Header("Cadence")]
    [Tooltip("Intervalle moyen entre tirs (s).")]
    public float meanInterval = 1.6f;
    [Tooltip("Variation aléatoire autour de l'intervalle moyen (s).")]
    public float intervalJitter = 0.9f;
    [Tooltip("Chance d'entrer en 'burst' (salve).")]
    [Range(0f, 1f)] public float burstChance = 0.25f;
    [Tooltip("Nombre de tirs dans une salve.")]
    public Vector2Int burstCountRange = new Vector2Int(2, 4);
    [Tooltip("Intervalle entre tirs d'une salve (s).")]
    public Vector2 burstGapRange = new Vector2(0.25f, 0.55f);

    [Header("Physique (UI)")]
    [Tooltip("Gravité (pixels/s²), négative en Y (Canvas Overlay).")]
    public float gravityY = -1600f;

    [Tooltip("Temps de vol cible (s).")]
    public Vector2 flightTimeRange = new Vector2(0.9f, 1.6f);

    [Header("Contraintes d’impact (écran)")]
    [Tooltip("Bande verticale (en Y écran) autorisée pour l’impact (évite tout en haut/bas).")]
    public Vector2 impactYRange = new Vector2(450f, 1350f);
    [Tooltip("Marge latérale (évite les bords de l’écran).")]
    public float impactXMargin = 90f;

    [Header("Gameplay points")]
    public int pointsPerfect = 100;
    public int pointsLateEarly = 50;
    [Tooltip("Fenêtre (sec) autour de l'impact pour PERFECT.")]
    public float perfectWindow = 0.08f;
    [Tooltip("Fenêtre autour de l'impact où on accorde 'moitié des points'.")]
    public float halfWindow = 0.25f;

    [Header("TNT")]
    [Range(0f, 1f)] public float tntProbability = 0.15f;

    // interne
    private float _nextShotTime;
    private Queue<float> _burstSchedule = new Queue<float>();
    private Vector2 _canvasSize;

    void Start()
    {
        // calc taille canvas (en pixels)
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
            // parfois: burst
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

    void FireOne()
    {
        if (muzzleAnchor == null || cannonballPrefab == null || prechotPrefab == null) return;

        // 1) Choisir un point d’impact “raisonnable”
        Vector2 impact = new Vector2(
            Random.Range(impactXMargin, _canvasSize.x - impactXMargin),
            Random.Range(impactYRange.x, impactYRange.y)
        );

        // 2) Temps de vol
        float tFlight = Random.Range(flightTimeRange.x, flightTimeRange.y);

        // 3) Calcul de la vitesse initiale pour toucher impact en t = tFlight
        Vector2 start = muzzleAnchor.position;
        Vector2 g = new Vector2(0f, gravityY);
        // v0 = (p - s - 0.5 g t^2)/t
        Vector2 v0 = (impact - start - 0.5f * g * (tFlight * tFlight)) / tFlight;

        // 4) Type (boulet ou TNT)
        bool isTNT = (Random.value < tntProbability);

        // 5) Spawn projectile
        var proj = Instantiate(cannonballPrefab, projectileLayer);
        proj.Setup(start, v0, g, tFlight, isTNT ? tntSprite : cannonballSprite);

        // 6) Pré‑shot (anneau cliquable à la position d’impact)
        var marker = Instantiate(prechotPrefab, frontFXLayer);
        marker.Setup(impact, tFlight, isTNT, OnMarkerClicked, OnMarkerExpired);

        // 7) Liaison projectile ↔ marker (pour petits effets sync si besoin)
        proj.BindMarker(marker);
    }

    // Callback: joueur a cliqué
    private void OnMarkerClicked(PrechotMarkerUI marker, float dtFromImpact, bool isTNT)
    {
        if (isTNT)
        {
            // TNT cliquée → -1 vie
            if (livesUI != null) livesUI.LoseLife(1);
            marker.PlayBadFx();
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
            // en dehors fenêtre → rien (mais le marker va expirer et compter comme raté ? Non: on compte raté seulement si pas cliqué avant expiry)
            marker.PlayTooEarlyLateFx();
        }

        // Dans tous les cas, après clic (bon ou pas), on retire le marker pour ce tir
        marker.Consume();
    }

    // Callback: le marker a expiré sans clic (impact passé)
    private void OnMarkerExpired(PrechotMarkerUI marker, bool wasTNT)
    {
        if (!wasTNT)
        {
            // Boulet non cliqué → -1 vie
            if (livesUI != null) livesUI.LoseLife(1);
        }
        // TNT ignorée → aucun effet
    }
}

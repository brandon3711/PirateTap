using UnityEngine;
using UnityEngine.UI;

/// Déplace le projectile en **UI local** (anchoredPosition), avec une parabole.
public class CannonballUI : MonoBehaviour
{
    public Image img;               // assigner sur le prefab
    public RectTransform rt;        // assigner sur le prefab

    private Vector2 _startA;        // départ en anchored (local au parent)
    private Vector2 _v0A;           // vitesse initiale (px/s) en anchored
    private Vector2 _gA;            // gravité (px/s²) en anchored
    private float _tFlight;         // durée totale de vol
    private float _t0;              // temps de départ
    private bool _setup;
    private PrechotMarkerUI _marker;

    public void Setup(Vector2 startAnchored, Vector2 v0Anchored, Vector2 gAnchored, float tFlight, Sprite sprite)
    {
        _startA   = startAnchored;
        _v0A      = v0Anchored;
        _gA       = gAnchored;
        _tFlight  = tFlight;
        _t0       = Time.time;

        if (img != null && sprite != null) img.sprite = sprite;
        if (rt == null) rt = GetComponent<RectTransform>();
        _setup = true;

        // placer au départ (UI local au parent)
        if (rt != null) rt.anchoredPosition = _startA;
    }

    public void BindMarker(PrechotMarkerUI marker) => _marker = marker;

    void Update()
    {
        if (!_setup || rt == null) return;

        float t = Mathf.Clamp(Time.time - _t0, 0f, _tFlight);

        // Mouvement parabolique en **local UI** :
        // p(t) = s + v0*t + 0.5*g*t^2
        Vector2 p = _startA + _v0A * t + 0.5f * _gA * t * t;
        rt.anchoredPosition = p;

        // Orientation visuelle (facultatif) en fonction de la vitesse instantanée
        Vector2 v = _v0A + _gA * t;
        float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
        rt.localRotation = Quaternion.Euler(0, 0, angle - 90f); // ajuste selon ton sprite

        if (t >= _tFlight)
        {
            _marker?.PingImpactFx(); // petit flash sur le marker
            Destroy(gameObject);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Déplace le projectile en arc parabolique (UI).
/// Optionnel: trail simple plus tard. S'auto-détruit à la fin du vol.
/// </summary>
public class CannonballUI : MonoBehaviour
{
    public Image img; // assigner sur le prefab
    public RectTransform rt;

    private Vector2 _start;
    private Vector2 _v0;
    private Vector2 _g;
    private float _tFlight;
    private float _t0;
    private bool _setup;
    private PrechotMarkerUI _marker;

    public void Setup(Vector2 start, Vector2 v0, Vector2 g, float tFlight, Sprite sprite)
    {
        _start   = start;
        _v0      = v0;
        _g       = g;
        _tFlight = tFlight;
        _t0      = Time.time;
        if (img != null && sprite != null) img.sprite = sprite;
        if (rt == null) rt = GetComponent<RectTransform>();
        _setup = true;

        // placer au départ
        if (rt != null) rt.position = _start;
    }

    public void BindMarker(PrechotMarkerUI marker)
    {
        _marker = marker;
    }

    void Update()
    {
        if (!_setup || rt == null) return;

        float t = Mathf.Clamp(Time.time - _t0, 0f, _tFlight);
        // p(t) = s + v0 t + 0.5 g t^2
        Vector2 p = _start + _v0 * t + 0.5f * _g * t * t;
        rt.position = p;

        // petit angle d'orientation
        float angle = Mathf.Atan2((_v0 + _g * t).y, (_v0 + _g * t).x) * Mathf.Rad2Deg;
        rt.localRotation = Quaternion.Euler(0, 0, angle - 90f); // -90 si sprite "tête en haut"

        if (t >= _tFlight)
        {
            // impact
            // petit FX: on peut ping marker pour un flash
            _marker?.PingImpactFx();

            Destroy(gameObject);
        }
    }
}

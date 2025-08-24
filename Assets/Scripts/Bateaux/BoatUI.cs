using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Place un bateau au niveau de la "ligne d'eau" (haut de SeaImage),
/// applique une houle (bobbing + léger roulis),
/// et adapte sa teinte à la phase (couleur de la mer / teinte globale).
/// </summary>
[ExecuteAlways]
public class BoatUI : MonoBehaviour
{
    [Header("Refs (assigner dans l’Inspector)")]
    public RectTransform seaImageRect;       // Drag & drop: RectTransform de SeaImage
    public RectTransform boatRect;           // Drag & drop: RectTransform de cet objet "Boat"
    public Image boatImage;                  // Drag & drop: Image du "Boat"
    public MonoBehaviour backgroundManager;  // Optionnel: ton BackgroundManagerUI (pour lire les couleurs en direct)

    [Header("Placement")]
    [Tooltip("Décalage vertical en pixels au-dessus de la ligne d'eau (bord haut de SeaImage).")]
    public float waterlineOffset = 4f;

    [Tooltip("Décalage horizontal global (pixels). Permet de recaler le bateau à gauche/droite.")]
    public float baseXOffset = 0f;

    [Header("Houle (mouvement)")]
    [Tooltip("Amplitude verticale (pixels) du mouvement de houle.")]
    public float bobAmplitude = 6f;

    [Tooltip("Vitesse de la houle.")]
    public float bobSpeed = 0.6f;

    [Tooltip("Amplitude du roulis (degrés).")]
    public float rollAmplitude = 2.2f;

    [Tooltip("Vitesse du roulis (peut être égal ou différent de bobSpeed).")]
    public float rollSpeed = 0.8f;

    [Header("Parallax (très léger pour ‘loin’)")]
    [Tooltip("Vitesse de dérive horizontale (pixels/seconde). Valeur faible pour un effet lointain.")]
    public float driftSpeed = 4f;

    [Tooltip("Largeur de ping-pong horizontal (pixels).")]
    public float driftRange = 20f;

    [Header("Adaptation de couleur")]
    [Range(0f, 1f)]
    [Tooltip("0 = pas de recoloration, 1 = colle fortement à la teinte/mer.")]
    public float recolorStrength = 0.35f;

    [Tooltip("Couleur de base du bateau (multiplicateur). Laisse blanc si ton sprite est déjà bien.")]
    public Color baseBoatColor = Color.white;

    // Cache
    private Vector2 _initialBoatSize;
    private float _time0;

    // Optionnel: si ton BackgroundManagerUI expose ces infos, on les lira par réflexion (sans dépendance forte)
    private Color _currentSeaColor = Color.white;
    private Color _currentPhaseTint = Color.white;

    void OnEnable()
    {
        _time0 = Application.isPlaying ? Time.time : 0f;
        CacheInitials();
        ForceUpdateNow(); // place propre dès l’activation dans l’éditeur
    }

    void Update()
    {
        if (boatRect == null || seaImageRect == null) return;

        // 1) Mise à jour de la couleur depuis le BackgroundManagerUI si dispo
        ReadBackgroundColors();

        // 2) Calcul de la ligne d'eau (bord haut de SeaImage)
        //    On récupère la coordonnée Y du bord haut de SeaImage (en espace Canvas → monde).
        seaImageRect.GetWorldCorners(_corners);
        float seaTopY = _corners[1].y; // index 1 = top-left corner en WorldSpace

        // 3) Position de base du bateau (x,y)
        //    On garde l’X de BoatLayer + offset + drift ping-pong
        float t = Application.isPlaying ? (Time.time - _time0) : (Application.isEditor ? (float)UnityEditor.EditorApplication.timeSinceStartup : 0f);
        float drift = (driftRange > 0f) ? Mathf.PingPong(t * driftSpeed, driftRange) - (driftRange * 0.5f) : 0f;

        Vector3 worldPos = boatRect.position;
        worldPos.x = GetParentCenterX(boatRect) + baseXOffset + drift;

        // Y = top de la mer + petit offset + bobbing (sinus)
        float bob = Mathf.Sin(t * (Mathf.PI * 2f) * bobSpeed) * bobAmplitude;
        worldPos.y = seaTopY + waterlineOffset + bob;

        boatRect.position = worldPos;

        // 4) Roulis (rotation Z)
        float roll = Mathf.Sin(t * (Mathf.PI * 2f) * rollSpeed) * rollAmplitude;
        boatRect.localRotation = Quaternion.Euler(0f, 0f, roll);

        // 5) Recoloration douce du bateau selon phase/mer
        if (boatImage != null)
        {
            // Combine légèrement la teinte de mer et la teinte globale (si dispo)
            Color targetTint = Color.Lerp(_currentSeaColor, _currentPhaseTint, 0.5f);
            Color final = Color.Lerp(baseBoatColor, baseBoatColor * targetTint, recolorStrength);
            final.a = boatImage.color.a; // on préserve l’alpha courant
            boatImage.color = final;
        }
    }

#if UNITY_EDITOR
    // Pour forcer la mise à jour dans l’éditeur dès que tu changes un paramètre
    void OnValidate()
    {
        CacheInitials();
        ForceUpdateNow();
    }
#endif

    // --------- Internals ---------

    private Vector3[] _corners = new Vector3[4];

    private void CacheInitials()
    {
        if (boatRect != null)
            _initialBoatSize = boatRect.sizeDelta;
    }

    private void ForceUpdateNow()
    {
        if (!Application.isPlaying)
        {
            // Simule un Update à t=0 pour positionner proprement dans l’éditeur
            ReadBackgroundColors();
            if (seaImageRect != null && boatRect != null)
            {
                seaImageRect.GetWorldCorners(_corners);
                float seaTopY = _corners[1].y;

                Vector3 worldPos = boatRect.position;
                worldPos.x = GetParentCenterX(boatRect) + baseXOffset;
                worldPos.y = seaTopY + waterlineOffset;
                boatRect.position = worldPos;

                boatRect.localRotation = Quaternion.identity;

                if (boatImage != null)
                {
                    Color targetTint = Color.Lerp(_currentSeaColor, _currentPhaseTint, 0.5f);
                    Color final = Color.Lerp(baseBoatColor, baseBoatColor * targetTint, recolorStrength);
                    final.a = boatImage.color.a;
                    boatImage.color = final;
                }
            }
        }
    }

    private float GetParentCenterX(RectTransform rt)
    {
        var parent = rt.parent as RectTransform;
        if (parent == null) return rt.position.x;

        parent.GetWorldCorners(_corners);
        float left = _corners[0].x;
        float right = _corners[3].x;
        return (left + right) * 0.5f;
    }

    /// <summary>
    /// Essaie de lire les couleurs courantes depuis ton BackgroundManagerUI.
    /// Pour éviter de modifier trop tes scripts, on lit par réflexion (si des champs utiles existent).
    /// Champs/requêtes cherchés :
    /// - public Color currentSeaColor
    /// - public Color CurrentSeaColor { get; }
    /// - public Color currentPhaseTint
    /// - public Color CurrentPhaseTint { get; }
    /// </summary>
    private void ReadBackgroundColors()
    {
        _currentSeaColor = Color.white;
        _currentPhaseTint = Color.white;

        if (backgroundManager == null) return;

        var t = backgroundManager.GetType();

        // Sea color
        var seaField = t.GetField("currentSeaColor");
        var seaProp  = t.GetProperty("CurrentSeaColor");
        if (seaField != null && seaField.FieldType == typeof(Color))
            _currentSeaColor = (Color)seaField.GetValue(backgroundManager);
        else if (seaProp != null && seaProp.PropertyType == typeof(Color) && seaProp.CanRead)
            _currentSeaColor = (Color)seaProp.GetValue(backgroundManager, null);

        // Phase tint
        var tintField = t.GetField("currentPhaseTint");
        var tintProp  = t.GetProperty("CurrentPhaseTint");
        if (tintField != null && tintField.FieldType == typeof(Color))
            _currentPhaseTint = (Color)tintField.GetValue(backgroundManager);
        else if (tintProp != null && tintProp.PropertyType == typeof(Color) && tintProp.CanRead)
            _currentPhaseTint = (Color)tintProp.GetValue(backgroundManager, null);
    }
}

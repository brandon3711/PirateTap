using UnityEngine;
using UnityEngine.UI;

public class LivesUI : MonoBehaviour
{
    [Header("Références (peuvent être auto-remplies)")]
    public Image[] hearts;          // Les 3 images de cœur (ordre gauche→droite)
    public Sprite fullHeart;        // Sprite cœur plein
    public Sprite emptyHeart;       // Sprite cœur vide

    [Range(0, 3)] public int currentLives = 3;

    void Awake()
    {
        AutoBindIfNeeded();
        Refresh(currentLives);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindIfNeeded();
        Refresh(currentLives);
    }
#endif

    // Tente de remplir automatiquement le tableau et les sprites si rien n'est mis
    void AutoBindIfNeeded()
    {
        // Si le tableau est vide, on prend TOUTES les Image enfants, triées par position X
        if (hearts == null || hearts.Length == 0)
        {
            var imgs = GetComponentsInChildren<Image>(includeInactive: true);
            // On filtre pour ne garder que celles qui ressemblent à des cœurs (optionnel)
            // Ici on prend toutes sauf l'Image d’arrière-plan éventuelle
            // Si tu as nommé tes objets "Heart1/2/3", ce sera parfait.
            System.Array.Sort(imgs, (a, b) => a.rectTransform.position.x.CompareTo(b.rectTransform.position.x));
            hearts = imgs;
        }
    }

    public void Refresh(int lives)
    {
        if (hearts == null || hearts.Length == 0) return;

        currentLives = Mathf.Clamp(lives, 0, hearts.Length);

        for (int i = 0; i < hearts.Length; i++)
        {
            if (!hearts[i]) continue;

            bool filled = i < currentLives;

            // Si on n’a pas fourni les sprites, on grise l’Image au lieu de changer le sprite
            if (fullHeart && emptyHeart)
            {
                hearts[i].sprite = filled ? fullHeart : emptyHeart;
                hearts[i].color  = Color.white; // s’assure d’une opacité normale
            }
            else
            {
                // fallback visuel si aucun sprite fourni : colorer / décolorer
                hearts[i].color = filled ? Color.white : new Color(1f, 1f, 1f, 0.35f);
            }
        }
    }

    // Méthodes publiques appelées par LevelTester
    public void LoseLife(int amount = 1)  => Refresh(currentLives - amount);
    public void GainLife(int amount = 1)  => Refresh(currentLives + amount);
    public void SetLives(int value)       => Refresh(value);

    // Boutons de test dans l’inspector
    [ContextMenu("Test: -1 life")]
    void _TestLose() => LoseLife(1);

    [ContextMenu("Test: +1 life")]
    void _TestGain() => GainLife(1);
}

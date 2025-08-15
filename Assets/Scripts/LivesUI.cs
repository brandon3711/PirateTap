using UnityEngine;
using UnityEngine.UI;

public class LivesUI : MonoBehaviour
{
    [Header("Refs")]
    public Image[] hearts;      // Heart1, Heart2, Heart3 (dans cet ordre)
    public Sprite fullHeart;    // Heart_Full (rouge)
    public Sprite emptyHeart;   // Heart_Empty (clair)

    [Range(0, 3)]
    public int currentLives = 3;  // pour tester en Play

    private void Start()
    {
        Refresh(currentLives);
    }

    // Met à jour l'affichage selon le nombre de vies
    public void Refresh(int lives)
    {
        currentLives = Mathf.Clamp(lives, 0, hearts.Length);
        for (int i = 0; i < hearts.Length; i++)
        {
            bool filled = i < currentLives;
            if (hearts[i]) hearts[i].sprite = filled ? fullHeart : emptyHeart;
        }
    }

    // (Optionnel) méthodes pratiques pour plus tard
    public void LoseLife(int amount = 1) => Refresh(currentLives - amount);
    public void GainLife(int amount = 1) => Refresh(currentLives + amount);
}

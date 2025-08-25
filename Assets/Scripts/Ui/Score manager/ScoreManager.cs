using UnityEngine;
using UnityEngine.UI; // si tu utilises un Text UI classique
// Si tu utilises TextMeshPro, dé-commente les 2 lignes suivantes
// using TMPro;

public class ScoreManager : MonoBehaviour
{
    public int Score { get; private set; }

    [Header("UI (optionnel)")]
    public Text scoreText;             // Laisse vide si tu n'as pas de Text
    // public TextMeshProUGUI scoreTMP; // Utilise ceci si tu es en TMP

    public void AddScore(int points)
    {
        Score += points;
        UpdateUI();
    }

    public void ResetScore()
    {
        Score = 0;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (scoreText != null) scoreText.text = Score.ToString();
        // if (scoreTMP != null) scoreTMP.text = Score.ToString();
    }
}

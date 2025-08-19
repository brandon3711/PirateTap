using UnityEngine;

public class GameManagerDebug : MonoBehaviour
{
    [Header("Référence au vrai GameManager")]
    public GameManager gameManager;

    [Header("Quantités test progression")]
    [Tooltip("Ajout sur un hit 'OK' (touche A)")]
    public float addOk = 0.10f;
    [Tooltip("Ajout sur un hit 'GOOD' (touche Z)")]
    public float addGood = 0.20f;
    [Tooltip("Ajout sur un hit 'PERFECT' (touche E)")]
    public float addPerfect = 0.30f;

    private void Update()
    {
        if (!gameManager) return;

        // --- PROGRESSION ---
        if (Input.GetKeyDown(KeyCode.A)) // OK
            gameManager.progressUI?.AddProgress(addOk);

        if (Input.GetKeyDown(KeyCode.Z)) // GOOD
            gameManager.progressUI?.AddProgress(addGood);

        if (Input.GetKeyDown(KeyCode.E)) // PERFECT
            gameManager.progressUI?.AddProgress(addPerfect);

        // --- VIES ---
        if (Input.GetKeyDown(KeyCode.R)) // perdre 1 vie
            gameManager.livesUI?.LoseLife(1);

        if (Input.GetKeyDown(KeyCode.T)) // regagner 1 vie
            gameManager.livesUI?.GainLife(1);

        // --- RESET ---
        if (Input.GetKeyDown(KeyCode.U)) // reset barre + étoiles
            gameManager.progressUI?.SetProgress(0f);
    }
}

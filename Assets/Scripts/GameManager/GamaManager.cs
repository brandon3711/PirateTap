using UnityEngine;

public enum HitQuality { Okay, Good, Perfect, Bomb } // Bomb = vie perdue

public class GameManager : MonoBehaviour
{
    public static GameManager I;                // accès simple: GameManager.I

    [Header("UI refs")]
    public ProgressUI progressUI;               // ta barre + étoiles (script déjà en place)
    public LivesUI    livesUI;                  // tes cœurs (script déjà en place)

    [Header("Score & progression")]
    public int score = 0;
    [Tooltip("Score nécessaire pour remplir la barre à 100% (3 étoiles)")]
    public int goldScore = 200;                 // ajuste selon la difficulté

    [Header("Vies")]
    public int maxLives = 3;
    public int lives = 3;

    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        // Optionnel si tu changes de scènes :
        // DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StartLevel();
    }

    public void StartLevel()
    {
        score = 0;
        lives = maxLives;

        // reset UI
        if (progressUI) progressUI.SetProgress(0f);
        if (livesUI)    livesUI.Refresh(lives);
    }

    // --------- SCORE / PROGRESSION ----------
    public void AddScore(int amount)
    {
        score += Mathf.Max(0, amount);

        float t = 0f;
        if (goldScore > 0) t = Mathf.Clamp01(score / (float)goldScore);
        if (progressUI) progressUI.SetProgress(t);
    }

    // à appeler par les boulets selon la précision
    public void RegisterHit(HitQuality q)
    {
        switch (q)
        {
            case HitQuality.Perfect: AddScore(12); break; // bonus précision
            case HitQuality.Good:    AddScore(6);  break;
            case HitQuality.Okay:    AddScore(3);  break;
            case HitQuality.Bomb:    LoseLife();   break;
        }
    }

    // --------- VIES ----------
    public void LoseLife(int amount = 1)
    {
        lives = Mathf.Max(0, lives - amount);
        if (livesUI) livesUI.Refresh(lives);

        if (lives <= 0) EndLevel();
    }

    // --------- FIN DE PARTIE ----------
    public void EndLevel()
    {
        Debug.Log($"Fin ! Score={score}  Progress={(goldScore>0?(float)score/goldScore:0f):P0}");
        // plus tard: ouvrir l’écran de fin (score + étoiles + rejouer)
    }

    // ------ OUTILS DE TEST RAPIDES (clic droit sur le composant en Play) ------
    [ContextMenu("TEST/Hit OK")]     public void _Test_Hit_OK()     => RegisterHit(HitQuality.Okay);
    [ContextMenu("TEST/Hit Good")]   public void _Test_Hit_Good()   => RegisterHit(HitQuality.Good);
    [ContextMenu("TEST/Hit Perfect")]public void _Test_Hit_Perfect()=> RegisterHit(HitQuality.Perfect);
    [ContextMenu("TEST/Bomb")]       public void _Test_Bomb()       => RegisterHit(HitQuality.Bomb);
}

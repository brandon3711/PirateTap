using UnityEngine;

public class LevelTester : MonoBehaviour
{
    [Header("Références")]
    public BackgroundManagerUI background;   // glisser BackgroundCanvas (composant sur ce GO)
    public ProgressUI          progressUI;   // glisser ProgressBar (composant ProgressUI)
    public LivesUI             livesUI;      // glisser LivesContainer (le composant LivesUI sera pris)

    [Header("Touches")]
    public KeyCode nextLevelKey = KeyCode.Space; // +1 niveau
    public KeyCode fillKey      = KeyCode.A;     // maintenir = remplit la barre
    public KeyCode loseLifeKey  = KeyCode.Q;     // -1 vie
    public KeyCode resetKey     = KeyCode.R;     // reset

    [Header("Réglages debug")]
    public int   fakeLevel        = 0;
    public float fillRatePerSecond = 0.35f;
    public int   startLives        = 3;

    float currentProgress = 0f;

    void Awake()
    {
        // Auto‑bind si champs vides
        if (!background)  background = FindObjectOfType<BackgroundManagerUI>(true);
        if (!progressUI)  progressUI = FindObjectOfType<ProgressUI>(true);
        if (!livesUI)
        {
            livesUI = FindObjectOfType<LivesUI>(true);
            if (!livesUI && background) livesUI = background.GetComponentInChildren<LivesUI>(true);
        }

        // Petits logs pour t’assurer que tout est bien trouvé
        Debug.Log($"[LevelTester] BG={(background? "OK":"NULL")} | Progress={(progressUI? "OK":"NULL")} | Lives={(livesUI? "OK":"NULL")}");
    }

    void Start()
    {
        // Mise en place initiale
        if (livesUI) livesUI.SetLives(startLives);
        if (progressUI) progressUI.SetProgress(0f);
        if (background) background.ApplyForLevel(0);
        currentProgress = 0f;
    }

    void Update()
    {
        // Espace = niveau +1
        if (Input.GetKeyDown(nextLevelKey))
        {
            fakeLevel++;
            if (background) background.ApplyForLevel(fakeLevel);
            Debug.Log($"[LevelTester] Next level → {fakeLevel}");
        }

        // A maintenu = remplir la barre
        if (Input.GetKey(fillKey))
        {
            currentProgress += fillRatePerSecond * Time.deltaTime;
            currentProgress = Mathf.Clamp01(currentProgress);
            if (progressUI) progressUI.SetProgress(currentProgress);
        }

        // Q = perdre 1 vie
        if (Input.GetKeyDown(loseLifeKey))
        {
            if (livesUI) livesUI.LoseLife(1);
            Debug.Log("[LevelTester] Lose life");
        }

        // R = reset
        if (Input.GetKeyDown(resetKey))
        {
            fakeLevel = 0;
            currentProgress = 0f;
            if (background) background.ApplyForLevel(0);
            if (progressUI) progressUI.SetProgress(0f);
            if (livesUI) livesUI.SetLives(startLives);
            Debug.Log("[LevelTester] Reset");
        }
    }
}

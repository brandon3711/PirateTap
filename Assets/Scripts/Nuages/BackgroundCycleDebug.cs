using UnityEngine;

public class BackgroundCycleDebug : MonoBehaviour
{
    [Header("Refs")]
    public BackgroundManagerUI background;              // glisse ton objet qui a BackgroundManagerUI
    public CloudParallaxLayerUI[] cloudLayers;          // glisse LayerFar + LayerNear (leurs scripts)

    [Header("Touches")]
    public KeyCode nextKey = KeyCode.N;                 // ou Space si tu préfères
    public KeyCode prevKey = KeyCode.B;

    [Header("État (debug)")]
    public int fakeLevel = 0;                           // niveau simulé
    public bool logPhaseInConsole = true;

    void Reset()
    {
        // auto-find pratique
        if (!background) background = FindObjectOfType<BackgroundManagerUI>();
        cloudLayers = FindObjectsOfType<CloudParallaxLayerUI>();
    }

    void Start()
    {
        Apply();
    }

    void Update()
    {
        if (Input.GetKeyDown(nextKey))
        {
            fakeLevel++;
            Apply();
        }
        if (Input.GetKeyDown(prevKey))
        {
            fakeLevel = Mathf.Max(0, fakeLevel - 1);
            Apply();
        }
    }

    void Apply()
    {
        if (!background) return;

        // 1) Applique ciel + mer via BackgroundManagerUI
        background.ApplyForLevel(fakeLevel);

        // 2) Calcule la phase courante (même logique que dans BackgroundManagerUI)
        int lpp = Mathf.Max(1, background.levelsPerPhase);
        int phaseIndex = Mathf.FloorToInt(fakeLevel / lpp) % 4;
        var phase = (BackgroundManagerUI.Phase)phaseIndex;

        // 3) Informe les couches de nuages pour la teinte
        if (cloudLayers != null)
        {
            foreach (var layer in cloudLayers)
                if (layer) layer.SetPhase(phase);
        }

        if (logPhaseInConsole)
            Debug.Log($"[BG DEBUG] Level={fakeLevel}  Phase={phase} (index {phaseIndex})");
    }
}

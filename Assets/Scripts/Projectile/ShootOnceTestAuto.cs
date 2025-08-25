using UnityEngine;

public class ShootOnceTestAuto : MonoBehaviour
{
    private CannonShooterUI shooter;
    private bool firedOnce;

    void Awake()
    {
        // 1) Essaie sur le même GameObject
        shooter = GetComponent<CannonShooterUI>();
        if (shooter == null)
        {
            // 2) Sinon cherche n'importe où dans la scène (même désactivé)
            shooter = FindObjectOfType<CannonShooterUI>(true);
        }

        if (shooter == null)
        {
            Debug.LogWarning("[ShootOnceTestAuto] Aucun CannonShooterUI trouvé dans la scène. Je me désactive.");
            enabled = false;
        }
    }

    void Start()
    {
        // Tir unique automatique après 0.2 s
        Invoke(nameof(FireOnce), 0.2f);
    }

    void Update()
    {
        // Appuie sur F pour forcer un tir manuellement (utile pour tester)
        if (Input.GetKeyDown(KeyCode.F))
        {
            SafeFire();
        }
    }

    void FireOnce()
    {
        if (!firedOnce)
        {
            SafeFire();
            firedOnce = true;
        }
    }

    private void SafeFire()
    {
        if (shooter == null) return;
        // On appelle FireOne() sans exiger sa présence (elle est privée dans CannonShooterUI)
        shooter.SendMessage("FireOne", SendMessageOptions.DontRequireReceiver);
    }
}

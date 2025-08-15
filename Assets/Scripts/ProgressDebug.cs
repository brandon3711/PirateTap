using UnityEngine;

public class ProgressDebug : MonoBehaviour
{
    public ProgressUI ui;
    public float speed = 0.35f; // vitesse de remplissage (pour test)
    float t;

    void Update()
    {
        // Appuie sur P pour faire monter la barre de 0 -> 1 en boucle
        if (Input.GetKey(KeyCode.P))
        {
            t += Time.deltaTime * speed;
            if (t > 1f) t = 1f;
            ui.SetProgress(t);
        }

        // R pour reset
        if (Input.GetKeyDown(KeyCode.R))
        {
            t = 0f;
            ui.ResetStars();
        }
    }
}

using UnityEngine;

public class CloudParallaxLayer : MonoBehaviour
{
    [Header("Nuages de cette couche (Sprites)")]
    public Transform[] clouds; // glisse ici tes nuages enfants (Transform)
    
    [Header("Vitesse & boucle")]
    public float speed = 1f;       // unités / seconde (positif = gauche)
    public float spacing = 5f;     // distance horizontale entre nuages
    public float wrapPadding = 2f; // marge avant réapparition

    [Header("Houle (optionnel)")]
    public float bobAmp = 0.2f;   // amplitude verticale
    public float bobSpeed = 0.5f; // vitesse houle

    Vector3[] startPos;
    float[] phaseOffset;

    void Awake()
    {
        startPos = new Vector3[clouds.Length];
        phaseOffset = new float[clouds.Length];

        for (int i = 0; i < clouds.Length; i++)
        {
            if (clouds[i])
            {
                startPos[i] = clouds[i].localPosition;
                phaseOffset[i] = Random.value * 10f;
            }
        }

        DistributeHorizontally();
    }

    void DistributeHorizontally()
    {
        float x = -spacing * clouds.Length * 0.5f;
        for (int i = 0; i < clouds.Length; i++)
        {
            if (!clouds[i]) continue;
            Vector3 p = clouds[i].localPosition;
            clouds[i].localPosition = new Vector3(x, p.y, p.z);
            x += spacing;
        }
    }

    void Update()
    {
        if (clouds == null) return;

        for (int i = 0; i < clouds.Length; i++)
        {
            var tf = clouds[i];
            if (!tf) continue;

            // défilement vers la gauche
            Vector3 pos = tf.localPosition;
            pos.x -= speed * Time.deltaTime;

            // houle verticale
            float bob = Mathf.Sin((Time.time + phaseOffset[i]) * bobSpeed) * bobAmp;
            pos.y = startPos[i].y + bob;

            // wrap : réapparaît derrière le plus à droite
            float leftLimit = -spacing * clouds.Length * 0.5f - wrapPadding;
            float rightSpawn = spacing * clouds.Length * 0.5f + wrapPadding;
            if (pos.x < leftLimit)
            {
                float maxX = pos.x;
                for (int j = 0; j < clouds.Length; j++)
                    if (j != i && clouds[j])
                        maxX = Mathf.Max(maxX, clouds[j].localPosition.x);
                pos.x = maxX + spacing;
            }

            tf.localPosition = pos;
        }
    }
}

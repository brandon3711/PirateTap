using UnityEngine;
using UnityEngine.UI;

public class SpawnPlainImageTest : MonoBehaviour
{
    public RectTransform projectileLayer; // ton ProjectileLayer
    public Sprite testSprite;             // mets le sprite "boulet" ici

    void Start()
    {
        if (projectileLayer == null || testSprite == null)
        {
            Debug.LogError("[SpawnPlainImageTest] Assigne projectileLayer et testSprite.");
            return;
        }

        // 1) Crée une Image UI sous ProjectileLayer
        var go = new GameObject("PlainBall", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(projectileLayer, false);       // false = garde l’échelle/anchors du parent
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f); // centre
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;         // pile au centre
        rt.sizeDelta = new Vector2(96, 96);         // taille visible

        var img = go.GetComponent<Image>();
        img.sprite = testSprite;
        img.color = Color.white;
        img.raycastTarget = false;

        Debug.Log("[SpawnPlainImageTest] Image placée au CENTRE. Tu dois la voir.");
    }
}

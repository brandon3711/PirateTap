using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PrechotMarkerUI : MonoBehaviour, IPointerDownHandler
{
    public RectTransform rt;
    public Image ringImage;
    public UnityEngine.UI.Text scorePopupText;

    public void Setup(Vector2 impactPos, float timeToImpact, bool isTNT,
        System.Action<PrechotMarkerUI, float, bool> onClick,
        System.Action<PrechotMarkerUI, bool> onExpired) { }

    public void PingImpactFx() { }       // appelé par CannonballUI à l’impact
    public void PlayPerfectFx(int points) { }
    public void PlayGoodFx(int points) { }
    public void PlayTooEarlyLateFx() { }
    public void PlayBadFx() { }
    public void Consume() { }

    public void OnPointerDown(PointerEventData eventData) { }
}

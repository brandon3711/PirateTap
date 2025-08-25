using UnityEngine;

public class BoatMuzzleRef : MonoBehaviour
{
    public RectTransform muzzleAnchor; // glisse ici ton MuzzleAnchor

    public Vector3 GetMuzzleWorldPosition()
    {
        if (muzzleAnchor == null) return Vector3.zero;
        return muzzleAnchor.position; // World space (Canvas en Screen Space - Camera/Overlay)
    }
}

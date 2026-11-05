using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;

// Pushes the button face down while pressed and springs it back on release
public class TactileButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform buttonFace;
    [SerializeField] private float pressDistance = -14f;
    [SerializeField] private float releaseDuration = 0.15f;

    private float restY;

    private void Awake()
    {
        restY = buttonFace.anchoredPosition.y;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Tween.StopAll(buttonFace);
        SetFaceY(restY + pressDistance);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Tween.StopAll(buttonFace);
        Tween.UIAnchoredPositionY(buttonFace, restY, releaseDuration, Ease.OutBack);
    }

    private void OnDisable()
    {
        Tween.StopAll(buttonFace);
        SetFaceY(restY);
    }

    private void SetFaceY(float y)
    {
        Vector2 position = buttonFace.anchoredPosition;
        position.y = y;
        buttonFace.anchoredPosition = position;
    }
}

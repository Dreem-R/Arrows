using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// Turns a mouse click or touch into a world position. Taps on UI are ignored
// It uses an action callback rather than polling each frame, so a tap that starts and ends within one frame still counts
public class GameInput : MonoBehaviour
{
    private const string POINTER_PRESS_BINDING = "<Pointer>/press";

    public static GameInput Instance { get; private set; }

    public event EventHandler<OnTapEventArgs> OnTap;
    public class OnTapEventArgs : EventArgs
    {
        public Vector2 worldPosition;
    }

    [SerializeField] private Camera boardCamera;

    private readonly List<RaycastResult> uiRaycastResults = new List<RaycastResult>();
    private InputAction tapAction;

    private void Awake()
    {
        Instance = this;

        tapAction = new InputAction("Tap", InputActionType.Button, POINTER_PRESS_BINDING);
        tapAction.performed += TapAction_performed;
        tapAction.Enable();
    }

    private void TapAction_performed(InputAction.CallbackContext context)
    {
        Pointer pointer = Pointer.current;
        if (pointer == null)
        {
            return;
        }

        Vector2 screenPosition = pointer.position.ReadValue();
        if (IsOverUI(screenPosition))
        {
            return;
        }

        OnTap?.Invoke(this, new OnTapEventArgs
        {
            worldPosition = boardCamera.ScreenToWorldPoint(screenPosition),
        });
    }

    private bool IsOverUI(Vector2 screenPosition)
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            return false;
        }

        PointerEventData pointerData = new PointerEventData(eventSystem) { position = screenPosition };
        eventSystem.RaycastAll(pointerData, uiRaycastResults);
        return uiRaycastResults.Count > 0;
    }

    private void OnDestroy()
    {
        tapAction.performed -= TapAction_performed;
        tapAction.Dispose();
    }
}

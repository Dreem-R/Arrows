using PrimeTween;
using UnityEngine;

// Frames any level in the space left between the top and bottom UI at any window size, and shakes on a collision
[RequireComponent(typeof(Camera))]
public class BoardCamera : MonoBehaviour
{
    private const float MIN_BOARD_SCREEN_SHARE = 0.3f;

    [Tooltip("The lowest UI element at the top of the screen. The board stays below it.")]
    [SerializeField] private RectTransform topUI;
    [Tooltip("The highest UI element at the bottom of the screen. The board stays above it.")]
    [SerializeField] private RectTransform bottomUI;
    [Tooltip("World units kept clear around the level's lines. This has to cover half an arrowhead too.")]
    [SerializeField] private float padding = 0.9f;

    [Header("Collision shake")]
    [SerializeField] private float shakeStrength = 0.5f;
    [SerializeField] private float shakeDuration = 0.3f;

    private readonly Vector3[] uiCorners = new Vector3[4];

    private Camera boardCamera;
    private Vector2Int lastScreenSize;
    private Vector3 framedPosition;
    private Sequence shakeSequence;

    private void Awake()
    {
        boardCamera = GetComponent<Camera>();
        framedPosition = transform.position;
    }

    private void Start()
    {
        LevelSpawner.Instance.OnLevelSpawned += LevelSpawner_OnLevelSpawned;
        CollisionSystem.Instance.OnCollision += CollisionSystem_OnCollision;
    }

    private void LevelSpawner_OnLevelSpawned(object sender, System.EventArgs e)
    {
        FrameLevel();
    }

    private void CollisionSystem_OnCollision(object sender, CollisionSystem.OnCollisionEventArgs e)
    {
        StopShake();
        shakeSequence = Tween.ShakeCamera(boardCamera, shakeStrength, shakeDuration);
    }

    private void LateUpdate()
    {
        if (lastScreenSize.x != Screen.width || lastScreenSize.y != Screen.height)
        {
            FrameLevel();
        }
    }

    private void FrameLevel()
    {
        lastScreenSize = new Vector2Int(Screen.width, Screen.height);

        StopShake();

        Bounds bounds = LevelSpawner.Instance.GetLevelBounds();

        // The canvas is screen space overlay, so its world corners are in screen pixels
        Canvas.ForceUpdateCanvases();
        topUI.GetWorldCorners(uiCorners);
        float boardTopShare = uiCorners[0].y / Screen.height;
        bottomUI.GetWorldCorners(uiCorners);
        float boardBottomShare = uiCorners[1].y / Screen.height;
        float boardShare = Mathf.Max(boardTopShare - boardBottomShare, MIN_BOARD_SCREEN_SHARE);

        float sizeForHeight = (bounds.size.y + padding * 2f) / (2f * boardShare);
        float sizeForWidth = (bounds.size.x + padding * 2f) / (2f * boardCamera.aspect);
        float size = Mathf.Max(sizeForHeight, sizeForWidth);

        float boardCentreShare = boardBottomShare + boardShare * 0.5f;
        float verticalOffset = (boardCentreShare - 0.5f) * size * 2f;

        boardCamera.orthographicSize = size;
        framedPosition = new Vector3(bounds.center.x, bounds.center.y - verticalOffset, framedPosition.z);
        transform.position = framedPosition;
    }

    // Stopping a shake partway leaves the camera off centre, so it goes back to its framed position
    private void StopShake()
    {
        if (shakeSequence.isAlive)
        {
            shakeSequence.Stop();
            transform.position = framedPosition;
        }
    }
}

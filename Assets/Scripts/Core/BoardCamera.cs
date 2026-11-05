using PrimeTween;
using UnityEngine;

// Frames any level between the top and bottom UI at any window size, and shakes on a collision
[RequireComponent(typeof(Camera))]
public class BoardCamera : MonoBehaviour
{
    [Tooltip("Share of the screen height reserved for the lives at the top.")]
    [SerializeField, Range(0f, 0.4f)] private float topInset = 0.14f;
    [Tooltip("Share of the screen height reserved for the hint button at the bottom.")]
    [SerializeField, Range(0f, 0.4f)] private float bottomInset = 0.14f;
    [SerializeField] private float padding = 0.5f;

    [Header("Collision shake")]
    [SerializeField] private float shakeStrength = 0.5f;
    [SerializeField] private float shakeDuration = 0.3f;

    private Camera boardCamera;
    private Vector2Int lastScreenSize;

    private void Awake()
    {
        boardCamera = GetComponent<Camera>();
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
        Tween.ShakeCamera(boardCamera, shakeStrength, shakeDuration);
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

        Tween.StopAll(transform);

        Bounds bounds = LevelSpawner.Instance.GetLevelBounds();
        float usableHeightShare = 1f - topInset - bottomInset;

        float sizeForHeight = (bounds.size.y + padding * 2f) / (2f * usableHeightShare);
        float sizeForWidth = (bounds.size.x + padding * 2f) / (2f * boardCamera.aspect);
        float size = Mathf.Max(sizeForHeight, sizeForWidth);

        float usableCentre = bottomInset + usableHeightShare * 0.5f;
        float verticalOffset = (usableCentre - 0.5f) * size * 2f;

        boardCamera.orthographicSize = size;
        transform.position = new Vector3(bounds.center.x, bounds.center.y - verticalOffset, transform.position.z);
    }
}

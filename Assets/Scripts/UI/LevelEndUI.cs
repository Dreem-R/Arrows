using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The "You Won" and "You Lost" popup, with a Retry button that restarts the current level
public class LevelEndUI : MonoBehaviour
{
    private const string WON_TITLE = "You Won";
    private const string LOST_TITLE = "You Lost";

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Transform popup;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button retryButton;

    [Tooltip("Wait this long before the win popup, so the last line's removal can be seen.")]
    [SerializeField] private float winPopupDelay = 0.5f;
    [SerializeField] private float animationDuration = 0.35f;

    private void Awake()
    {
        retryButton.onClick.AddListener(() =>
        {
            GameManager.Instance.RetryLevel();
        });
    }

    private void Start()
    {
        GameManager.Instance.OnLevelStarted += GameManager_OnLevelStarted;
        GameManager.Instance.OnLevelWon += GameManager_OnLevelWon;
        GameManager.Instance.OnLevelLost += GameManager_OnLevelLost;

        Hide();
    }

    private void GameManager_OnLevelStarted(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void GameManager_OnLevelWon(object sender, System.EventArgs e)
    {
        Show(WON_TITLE, winPopupDelay);
    }

    private void GameManager_OnLevelLost(object sender, System.EventArgs e)
    {
        Show(LOST_TITLE, 0f);
    }

    private void Show(string title, float delay)
    {
        titleText.text = title;
        canvasGroup.gameObject.SetActive(true);
        canvasGroup.alpha = 0f;
        popup.localScale = Vector3.zero;

        Tween.Alpha(canvasGroup, 1f, animationDuration, Ease.OutQuad, startDelay: delay);
        Tween.Scale(popup, Vector3.one, animationDuration, Ease.OutBack, startDelay: delay);
    }

    private void Hide()
    {
        Tween.StopAll(canvasGroup);
        Tween.StopAll(popup);
        canvasGroup.gameObject.SetActive(false);
    }
}

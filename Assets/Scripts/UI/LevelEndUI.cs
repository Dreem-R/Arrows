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
    [SerializeField] private float fadeDuration = 0.15f;
    [SerializeField] private float unfoldWidthDuration = 0.15f;
    [SerializeField] private float unfoldHeightDuration = 0.35f;

    private Sequence popupSequence;

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

    // The popup opens out from a thin flat line
    private void Show(string title, float delay)
    {
        titleText.text = title;
        canvasGroup.gameObject.SetActive(true);
        canvasGroup.alpha = 0f;
        popup.localScale = new Vector3(0f, 0.05f, 1f);

        Tween.Alpha(canvasGroup, 1f, fadeDuration, Ease.OutQuad, startDelay: delay);
        popupSequence = Sequence.Create()
            .Chain(Tween.ScaleX(popup, 1f, unfoldWidthDuration, Ease.OutExpo, startDelay: delay))
            .Chain(Tween.ScaleY(popup, 1f, unfoldHeightDuration, Ease.OutBack));
    }

    private void Hide()
    {
        if (popupSequence.isAlive)
        {
            popupSequence.Stop();
        }

        Tween.StopAll(canvasGroup);
        canvasGroup.gameObject.SetActive(false);
    }
}

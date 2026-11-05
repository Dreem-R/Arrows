using System;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The "You Won" and "You Lost" popup. Its one button goes to the next level after a win, restarts from level 1 after the final win and retries after a loss
public class LevelEndUI : MonoBehaviour
{
    private const string WON_TITLE = "You Won";
    private const string LOST_TITLE = "You Lost";
    private const string RETRY_LABEL = "Retry";
    private const string NEXT_LABEL = "Next";
    private const string RESTART_LABEL = "Restart";

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Transform popup;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button actionButton;
    [SerializeField] private TextMeshProUGUI actionButtonText;

    [Tooltip("Wait this long before the win popup, so the last line's removal can be seen.")]
    [SerializeField] private float winPopupDelay = 0.5f;
    [SerializeField] private float fadeDuration = 0.15f;
    [SerializeField] private float unfoldWidthDuration = 0.15f;
    [SerializeField] private float unfoldHeightDuration = 0.35f;

    private Sequence popupSequence;
    private Action buttonAction;

    private void Awake()
    {
        actionButton.onClick.AddListener(() =>
        {
            buttonAction();
        });
    }

    private void Start()
    {
        GameManager.Instance.OnLevelStarted += GameManager_OnLevelStarted;
        GameManager.Instance.OnLevelWon += GameManager_OnLevelWon;
        GameManager.Instance.OnLevelLost += GameManager_OnLevelLost;

        Hide();
    }

    private void GameManager_OnLevelStarted(object sender, EventArgs e)
    {
        Hide();
    }

    private void GameManager_OnLevelWon(object sender, EventArgs e)
    {
        if (GameManager.Instance.HasNextLevel)
        {
            Show(WON_TITLE, NEXT_LABEL, GameManager.Instance.NextLevel, winPopupDelay);
        }
        else
        {
            Show(WON_TITLE, RESTART_LABEL, GameManager.Instance.RestartFromFirstLevel, winPopupDelay);
        }
    }

    private void GameManager_OnLevelLost(object sender, EventArgs e)
    {
        Show(LOST_TITLE, RETRY_LABEL, GameManager.Instance.RetryLevel, 0f);
    }

    // The popup opens out from a thin flat line
    private void Show(string title, string buttonLabel, Action action, float delay)
    {
        titleText.text = title;
        actionButtonText.text = buttonLabel;
        buttonAction = action;

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

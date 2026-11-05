using UnityEngine;
using UnityEngine.UI;

// The corner retry icon. It reloads the current level at any time during play
public class RetryButtonUI : MonoBehaviour
{
    [SerializeField] private Button retryButton;

    private void Awake()
    {
        retryButton.onClick.AddListener(() =>
        {
            GameManager.Instance.RetryLevel();
        });
    }
}

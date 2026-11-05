using TMPro;
using UnityEngine;

// Shows which level is being played, numbered by its place in the level list
public class LevelLabelUI : MonoBehaviour
{
    private const string LABEL_PREFIX = "LEVEL ";

    [SerializeField] private TextMeshProUGUI levelText;

    private void Start()
    {
        GameManager.Instance.OnLevelStarted += GameManager_OnLevelStarted;
    }

    private void GameManager_OnLevelStarted(object sender, System.EventArgs e)
    {
        levelText.text = LABEL_PREFIX + GameManager.Instance.CurrentLevelNumber;
    }
}

using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

// The Hint button. Hints are unlimited, so it only needs to forward the press
public class HintUI : MonoBehaviour
{
    [SerializeField] private Button hintButton;

    private void Awake()
    {
        hintButton.onClick.AddListener(() =>
        {
            HintSystem.Instance.ShowHint();
            Tween.StopAll(hintButton.transform);
            hintButton.transform.localScale = Vector3.one;
            Tween.PunchScale(hintButton.transform, Vector3.one * 0.15f, 0.25f);
        });
    }

    private void Start()
    {
        HintSystem.Instance.OnNoHintAvailable += HintSystem_OnNoHintAvailable;
    }

    private void HintSystem_OnNoHintAvailable(object sender, System.EventArgs e)
    {
        Tween.ShakeLocalPosition(hintButton.transform, new Vector3(12f, 0f, 0f), 0.3f, frequency: 25f);
    }
}

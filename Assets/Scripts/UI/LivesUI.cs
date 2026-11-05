using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

// Shows one heart per life. The number of hearts comes from the game settings
public class LivesUI : MonoBehaviour
{
    [SerializeField] private Image heartPrefab;
    [SerializeField] private Transform heartContainer;
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite emptyHeart;

    private readonly List<Image> hearts = new List<Image>();

    private void Start()
    {
        GameManager.Instance.OnLevelStarted += GameManager_OnLevelStarted;
        GameManager.Instance.OnLivesChanged += GameManager_OnLivesChanged;
    }

    private void GameManager_OnLevelStarted(object sender, System.EventArgs e)
    {
        foreach (Image heart in hearts)
        {
            Tween.StopAll(heart.transform);
            Destroy(heart.gameObject);
        }
        hearts.Clear();

        for (int i = 0; i < GameManager.Instance.Lives; i++)
        {
            Image heart = Instantiate(heartPrefab, heartContainer);
            heart.sprite = fullHeart;
            hearts.Add(heart);
        }
    }

    private void GameManager_OnLivesChanged(object sender, GameManager.OnLivesChangedEventArgs e)
    {
        for (int i = 0; i < hearts.Count; i++)
        {
            bool isFull = i < e.lives;
            bool wasJustLost = !isFull && hearts[i].sprite == fullHeart;

            hearts[i].sprite = isFull ? fullHeart : emptyHeart;

            if (wasJustLost)
            {
                Tween.PunchScale(hearts[i].transform, Vector3.one * 0.6f, 0.4f);
            }
        }
    }
}

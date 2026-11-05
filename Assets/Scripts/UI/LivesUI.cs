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

    private Image fallingHeart;
    private Sequence fallingHeartSequence;
    private Transform beatingHeart;
    private Sequence heartbeatSequence;

    private void Start()
    {
        GameManager.Instance.OnLevelStarted += GameManager_OnLevelStarted;
        GameManager.Instance.OnLivesChanged += GameManager_OnLivesChanged;
    }

    private void GameManager_OnLevelStarted(object sender, System.EventArgs e)
    {
        StopHeartbeat();
        StopFallingHeart();

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

        StartHeartbeatIfLastLife(GameManager.Instance.Lives);
    }

    private void GameManager_OnLivesChanged(object sender, GameManager.OnLivesChangedEventArgs e)
    {
        StopHeartbeat();

        for (int i = 0; i < hearts.Count; i++)
        {
            bool isFull = i < e.lives;
            bool wasJustLost = !isFull && hearts[i].sprite == fullHeart;

            hearts[i].sprite = isFull ? fullHeart : emptyHeart;

            if (wasJustLost)
            {
                PlayHeartLost(hearts[i]);
            }
        }

        StartHeartbeatIfLastLife(e.lives);
    }

    // A full copy pops out and falls away while the now empty heart shakes in its slot
    private void PlayHeartLost(Image slot)
    {
        StopFallingHeart();

        float size = slot.rectTransform.rect.height;
        Vector3 slotPosition = slot.transform.localPosition;
        Vector3 popPosition = slotPosition + new Vector3(0f, size * 0.2f, 0f);
        Vector3 fallPosition = slotPosition + new Vector3(size * 0.35f, -size * 1.1f, 0f);
        Vector3 popTilt = new Vector3(0f, 0f, 12f);
        Vector3 fallSpin = new Vector3(0f, 0f, -100f);
        const float popDuration = 0.16f;
        const float fallDuration = 0.6f;

        fallingHeart = Instantiate(slot, heartContainer);
        fallingHeart.sprite = fullHeart;
        fallingHeart.GetComponent<LayoutElement>().ignoreLayout = true;

        // The layout group set the slot's size and position, so the copy needs them set by hand
        RectTransform copy = fallingHeart.rectTransform;
        copy.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, slot.rectTransform.rect.width);
        copy.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
        copy.localPosition = slotPosition;

        Tween.ShakeLocalPosition(slot.transform, new Vector3(size * 0.06f, 0f, 0f), 0.2f, frequency: 25f);

        fallingHeartSequence = Sequence.Create()
            .Chain(Tween.LocalPosition(copy, popPosition, popDuration, Ease.OutQuad))
            .Group(Tween.Scale(copy, 1.3f, popDuration, Ease.OutQuad))
            .Group(Tween.LocalEulerAngles(copy, Vector3.zero, popTilt, popDuration, Ease.OutQuad))
            .Chain(Tween.LocalPosition(copy, fallPosition, fallDuration, Ease.InQuad))
            .Group(Tween.LocalEulerAngles(copy, popTilt, fallSpin, fallDuration, Ease.InQuad))
            .Group(Tween.Scale(copy, 0.6f, fallDuration, Ease.InQuad))
            .Group(Tween.Alpha(fallingHeart, 0f, fallDuration, Ease.InQuad))
            .OnComplete(this, ui => ui.DestroyFallingHeart());
    }

    private void StopFallingHeart()
    {
        if (fallingHeartSequence.isAlive)
        {
            fallingHeartSequence.Stop();
        }

        DestroyFallingHeart();
    }

    private void DestroyFallingHeart()
    {
        if (fallingHeart != null)
        {
            Destroy(fallingHeart.gameObject);
            fallingHeart = null;
        }
    }

    private void StartHeartbeatIfLastLife(int lives)
    {
        if (lives != 1)
        {
            return;
        }

        beatingHeart = hearts[0].transform;
        heartbeatSequence = Sequence.Create(cycles: -1)
            .Chain(Tween.Scale(beatingHeart, 1.2f, 0.09f, Ease.OutQuad))
            .Chain(Tween.Scale(beatingHeart, 1f, 0.09f, Ease.InQuad))
            .Chain(Tween.Scale(beatingHeart, 1.12f, 0.09f, Ease.OutQuad))
            .Chain(Tween.Scale(beatingHeart, 1f, 0.09f, Ease.InQuad))
            .ChainDelay(0.6f);
    }

    private void StopHeartbeat()
    {
        if (heartbeatSequence.isAlive)
        {
            heartbeatSequence.Stop();
        }

        if (beatingHeart != null)
        {
            beatingHeart.localScale = Vector3.one;
            beatingHeart = null;
        }
    }
}

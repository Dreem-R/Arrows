using PrimeTween;
using UnityEngine;

// How an arrowhead looks and animates. It only reacts to events from ArrowHead and Line
[RequireComponent(typeof(ArrowHead), typeof(SpriteRenderer))]
public class ArrowHeadVisual : MonoBehaviour
{
    private const int IDLE_SORTING_ORDER = 100;
    private const int ACTIVE_SORTING_ORDER = 110;

    [SerializeField] private float disabledAlpha = 0.3f;

    [Header("Hint")]
    [SerializeField] private float hintBeatGap = 0.12f;
    [SerializeField] private float hintRestDuration = 0.55f;

    [Header("Removal")]
    [SerializeField] private ParticleSystem removalBurstPrefab;

    private ArrowHead head;
    private SpriteRenderer spriteRenderer;
    private Sequence introSequence;
    private Sequence hintSequence;
    private Tween scaleTween;
    private Tween colorTween;
    private Tween dimTween;
    private float stateAlpha = 1f;
    private float visibility = 1f;

    private void Awake()
    {
        head = GetComponent<ArrowHead>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        head.OnInitialized += ArrowHead_OnInitialized;
        head.OnTapDenied += ArrowHead_OnTapDenied;
        head.OnHintChanged += ArrowHead_OnHintChanged;
    }

    private void ArrowHead_OnInitialized(object sender, System.EventArgs e)
    {
        head.Line.OnStateChanged += Line_OnStateChanged;
        head.Line.OnDimChanged += Line_OnDimChanged;
        head.Line.OnHintChanged += Line_OnHintChanged;

        spriteRenderer.color = head.Line.Color;
        spriteRenderer.sortingOrder = IDLE_SORTING_ORDER;

        PlayIntro();
    }

    private void Line_OnStateChanged(object sender, Line.OnStateChangedEventArgs e)
    {
        StopIntro();

        bool isActive = head.Line.ActiveHead == head;

        switch (e.newState)
        {
            case Line.State.Moving:
                if (isActive)
                {
                    spriteRenderer.sortingOrder = ACTIVE_SORTING_ORDER;
                }
                else
                {
                    SetAlpha(disabledAlpha);
                }
                break;

            case Line.State.Resetting:
                if (isActive)
                {
                    PlayFailFlash();
                }
                break;

            case Line.State.Idle:
                spriteRenderer.sortingOrder = IDLE_SORTING_ORDER;
                SetAlpha(1f);
                break;

            case Line.State.Completed:
                PlayRemoval();
                break;
        }
    }

    private void Line_OnDimChanged(object sender, System.EventArgs e)
    {
        UpdateDimming();
    }

    private void Line_OnHintChanged(object sender, System.EventArgs e)
    {
        UpdateDimming();
    }

    // On a hinted line only the safe head stays bright, so the unsafe one can't be mistaken for it
    private void UpdateDimming()
    {
        dimTween.Stop();

        GameSettingsSO settings = GameManager.Instance.Settings;
        bool isUnsafeHintedHead = head.Line.IsHinted && !head.IsHinted;
        float targetVisibility = head.Line.IsDimmed || isUnsafeHintedHead ? settings.dimmedAlpha : 1f;
        if (!Mathf.Approximately(visibility, targetVisibility))
        {
            dimTween = Tween.Custom(this, visibility, targetVisibility, settings.dimDuration,
                (visual, value) => visual.SetVisibility(value));
        }
    }

    private void ArrowHead_OnTapDenied(object sender, System.EventArgs e)
    {
        Tween.ShakeLocalPosition(transform, new Vector3(0.08f, 0f, 0f), 0.25f, frequency: 25f);
    }

    private void ArrowHead_OnHintChanged(object sender, System.EventArgs e)
    {
        StopIntro();

        if (hintSequence.isAlive)
        {
            hintSequence.Stop();
        }

        transform.localScale = Vector3.one;

        if (head.IsHinted)
        {
            PlayHintBeats();
        }
    }

    // The start head lands as the line begins drawing and the end head lands when the line reaches it
    private void PlayIntro()
    {
        float landDelay = head.IsAtLineStart ? 0f : GameManager.Instance.Settings.levelIntroDuration;

        transform.localScale = Vector3.zero;
        introSequence = Sequence.Create()
            .Chain(Tween.Scale(transform, new Vector3(1.35f, 0.65f, 1f), 0.1f, Ease.OutQuad, startDelay: landDelay))
            .Chain(Tween.Scale(transform, new Vector3(0.85f, 1.2f, 1f), 0.08f, Ease.OutQuad))
            .Chain(Tween.Scale(transform, Vector3.one, 0.1f, Ease.OutQuad));
    }

    private void StopIntro()
    {
        if (introSequence.isAlive)
        {
            introSequence.Stop();
            transform.localScale = Vector3.one;
        }
    }

    private void PlayHintBeats()
    {
        hintSequence = Sequence.Create(cycles: -1)
            .Chain(Tween.Scale(transform, new Vector3(1.45f, 0.7f, 1f), 0.09f, Ease.OutQuad))
            .Chain(Tween.Scale(transform, new Vector3(0.85f, 1.35f, 1f), 0.09f, Ease.OutQuad))
            .Chain(Tween.Scale(transform, Vector3.one, 0.1f, Ease.OutQuad))
            .ChainDelay(hintBeatGap)
            .Chain(Tween.Scale(transform, new Vector3(1.3f, 0.8f, 1f), 0.08f, Ease.OutQuad))
            .Chain(Tween.Scale(transform, Vector3.one, 0.1f, Ease.OutQuad))
            .ChainDelay(hintRestDuration);
    }

    private void PlayFailFlash()
    {
        colorTween.Stop();
        colorTween = Tween.Custom(this, 1f, 0f, GameManager.Instance.Settings.failFlashDuration,
            (visual, value) => visual.SetTint(Color.Lerp(visual.head.Line.Color, GameManager.Instance.Settings.failColor, value)),
            Ease.InQuad);
    }

    private void PlayRemoval()
    {
        scaleTween.Stop();
        colorTween.Stop();

        scaleTween = Tween.Scale(transform, Vector3.zero, GameManager.Instance.Settings.removeDuration, Ease.InBack);

        if (removalBurstPrefab != null)
        {
            ParticleSystem burst = Instantiate(removalBurstPrefab, transform.position, Quaternion.identity);
            ParticleSystem.MainModule main = burst.main;
            main.startColor = head.Line.Color;
        }
    }

    private void SetAlpha(float alpha)
    {
        stateAlpha = alpha;
        SetTint(spriteRenderer.color);
    }

    private void SetVisibility(float value)
    {
        visibility = value;
        SetTint(spriteRenderer.color);
    }

    private void SetTint(Color color)
    {
        color.a = stateAlpha * visibility;
        spriteRenderer.color = color;
    }

    private void OnDestroy()
    {
        if (introSequence.isAlive)
        {
            introSequence.Stop();
        }

        if (hintSequence.isAlive)
        {
            hintSequence.Stop();
        }

        Tween.StopAll(this);
        Tween.StopAll(transform);
    }
}

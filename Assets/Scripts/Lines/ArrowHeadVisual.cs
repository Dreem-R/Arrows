using PrimeTween;
using UnityEngine;

// How an arrowhead looks and animates. It only reacts to events from ArrowHead and Line
[RequireComponent(typeof(ArrowHead), typeof(SpriteRenderer))]
public class ArrowHeadVisual : MonoBehaviour
{
    private const int IDLE_SORTING_ORDER = 100;
    private const int ACTIVE_SORTING_ORDER = 110;

    [SerializeField] private float disabledAlpha = 0.3f;
    [SerializeField] private Color failColor = new Color(0.1f, 0.1f, 0.1f);

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
        colorTween = Tween.Custom(this, 1f, 0f, GameManager.Instance.Settings.resetDuration + 0.2f,
            (visual, value) => visual.spriteRenderer.color = Color.Lerp(visual.head.Line.Color, visual.failColor, value),
            Ease.InQuad);
    }

    private void PlayRemoval()
    {
        scaleTween.Stop();
        colorTween.Stop();

        scaleTween = Tween.Scale(transform, Vector3.zero, GameManager.Instance.Settings.removeDelay * 0.8f, Ease.InBack);

        if (removalBurstPrefab != null)
        {
            ParticleSystem burst = Instantiate(removalBurstPrefab, transform.position, Quaternion.identity);
            ParticleSystem.MainModule main = burst.main;
            main.startColor = head.Line.Color;
        }
    }

    private void SetAlpha(float alpha)
    {
        Color color = spriteRenderer.color;
        color.a = alpha;
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

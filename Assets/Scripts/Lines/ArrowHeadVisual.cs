using PrimeTween;
using UnityEngine;

// How an arrowhead looks and animates. It only reacts to events from ArrowHead and Line
[RequireComponent(typeof(ArrowHead), typeof(SpriteRenderer))]
public class ArrowHeadVisual : MonoBehaviour
{
    private const int IDLE_SORTING_ORDER = 100;
    private const int ACTIVE_SORTING_ORDER = 110;

    [SerializeField] private float disabledAlpha = 0.3f;
    [SerializeField] private float introDuration = 0.35f;
    [SerializeField] private float introStagger = 0.02f;
    [SerializeField] private Color failColor = new Color(0.1f, 0.1f, 0.1f);

    [Header("Hint")]
    [SerializeField] private float hintScale = 1.5f;
    [SerializeField] private float hintPulseDuration = 0.4f;

    [Header("Removal")]
    [SerializeField] private ParticleSystem removalBurstPrefab;

    private ArrowHead head;
    private SpriteRenderer spriteRenderer;
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

        transform.localScale = Vector3.zero;
        scaleTween = Tween.Scale(transform, Vector3.one, introDuration, Ease.OutBack,
            startDelay: head.Line.Index * introStagger);
    }

    private void Line_OnStateChanged(object sender, Line.OnStateChangedEventArgs e)
    {
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
        scaleTween.Stop();

        if (head.IsHinted)
        {
            scaleTween = Tween.Scale(transform, Vector3.one * hintScale, hintPulseDuration,
                Ease.InOutSine, cycles: -1, cycleMode: CycleMode.Yoyo);
        }
        else
        {
            transform.localScale = Vector3.one;
        }
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
        Tween.StopAll(this);
        Tween.StopAll(transform);
    }
}

using PrimeTween;
using UnityEngine;

// Draws a line with a LineRenderer: trims it as the head travels, flashes on failure and pulses for hints
[RequireComponent(typeof(Line), typeof(LineRenderer))]
public class LineVisual : MonoBehaviour
{
    private const int HINTED_SORTING_ORDER = 90;

    [SerializeField] private float width = 0.065f;
    [SerializeField] private float introDuration = 0.35f;
    [SerializeField] private float introStagger = 0.02f;

    [Header("Failure")]
    [SerializeField] private Color failColor = new Color(0.1f, 0.1f, 0.1f);

    [Header("Hint")]
    [SerializeField] private float hintWidthMultiplier = 2.2f;
    [SerializeField] private float hintPulseDuration = 0.4f;

    private Line line;
    private LineRenderer lineRenderer;
    private Vector3[] pointBuffer;
    private int baseSortingOrder;
    private Tween widthTween;
    private Tween colorTween;

    private void Awake()
    {
        line = GetComponent<Line>();
        lineRenderer = GetComponent<LineRenderer>();

        line.OnInitialized += Line_OnInitialized;
        line.OnStateChanged += Line_OnStateChanged;
        line.OnHintChanged += Line_OnHintChanged;
    }

    private void Line_OnInitialized(object sender, System.EventArgs e)
    {
        pointBuffer = new Vector3[line.Path.MaxPointCount];
        baseSortingOrder = line.Index;
        lineRenderer.sortingOrder = baseSortingOrder;
        SetColor(line.Color);
        Redraw();

        lineRenderer.widthMultiplier = 0f;
        widthTween = Tween.Custom(this, 0f, width, introDuration,
            (visual, value) => visual.lineRenderer.widthMultiplier = value,
            Ease.OutCubic, startDelay: line.Index * introStagger);
    }

    private void Line_OnStateChanged(object sender, Line.OnStateChangedEventArgs e)
    {
        if (e.newState == Line.State.Resetting)
        {
            colorTween.Stop();
            colorTween = Tween.Custom(this, 1f, 0f, GameManager.Instance.Settings.resetDuration + 0.2f,
                (visual, value) => visual.SetColor(Color.Lerp(visual.line.Color, visual.failColor, value)),
                Ease.InQuad);
        }

        if (e.newState == Line.State.Idle || e.newState == Line.State.Completed)
        {
            Redraw();
        }
    }

    private void Line_OnHintChanged(object sender, System.EventArgs e)
    {
        widthTween.Stop();

        if (line.IsHinted)
        {
            lineRenderer.sortingOrder = HINTED_SORTING_ORDER;
            widthTween = Tween.Custom(this, width, width * hintWidthMultiplier, hintPulseDuration,
                (visual, value) => visual.lineRenderer.widthMultiplier = value,
                Ease.InOutSine, cycles: -1, cycleMode: CycleMode.Yoyo);
        }
        else
        {
            lineRenderer.sortingOrder = baseSortingOrder;
            lineRenderer.widthMultiplier = width;
        }
    }

    private void LateUpdate()
    {
        if (line.CurrentState == Line.State.Moving || line.CurrentState == Line.State.Resetting)
        {
            Redraw();
        }
    }

    private void Redraw()
    {
        line.GetVisibleRange(out float fromDistance, out float toDistance);
        int pointCount = line.Path.CopyPoints(fromDistance, toDistance, pointBuffer);
        lineRenderer.positionCount = pointCount;
        lineRenderer.SetPositions(pointBuffer);
    }

    private void SetColor(Color color)
    {
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }

    private void OnDestroy()
    {
        Tween.StopAll(this);
    }
}

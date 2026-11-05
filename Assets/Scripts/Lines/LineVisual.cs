using PrimeTween;
using UnityEngine;

// Draws a line with a LineRenderer: draws itself in on level start, trims as the head travels, flashes on failure and thickens for hints
[RequireComponent(typeof(Line), typeof(LineRenderer))]
public class LineVisual : MonoBehaviour
{
    private const int HINTED_SORTING_ORDER = 90;

    [SerializeField] private float width = 0.065f;

    [Header("Failure")]
    [SerializeField] private Color failColor = new Color(0.1f, 0.1f, 0.1f);

    [Header("Hint")]
    [SerializeField] private float hintWidthMultiplier = 2.2f;
    [SerializeField] private float hintWidthDuration = 0.15f;
    [SerializeField] private float dimmedAlpha = 0.5f;
    [SerializeField] private float dimDuration = 0.15f;

    private Line line;
    private LineRenderer lineRenderer;
    private Vector3[] pointBuffer;
    private int baseSortingOrder;
    private float introProgress = 1f;
    private Color currentColor;
    private float visibility = 1f;
    private Tween introTween;
    private Tween widthTween;
    private Tween colorTween;
    private Tween dimTween;

    private void Awake()
    {
        line = GetComponent<Line>();
        lineRenderer = GetComponent<LineRenderer>();

        line.OnInitialized += Line_OnInitialized;
        line.OnStateChanged += Line_OnStateChanged;
        line.OnHintChanged += Line_OnHintChanged;
        line.OnDimChanged += Line_OnDimChanged;
    }

    private void Line_OnInitialized(object sender, System.EventArgs e)
    {
        pointBuffer = new Vector3[line.Path.MaxPointCount];
        baseSortingOrder = line.Index;
        lineRenderer.sortingOrder = baseSortingOrder;
        lineRenderer.widthMultiplier = width;
        SetColor(line.Color);

        SetIntroProgress(0f);
        introTween = Tween.Custom(this, 0f, 1f, GameManager.Instance.Settings.levelIntroDuration,
            (visual, value) => visual.SetIntroProgress(value),
            Ease.InOutSine);
    }

    private void Line_OnStateChanged(object sender, Line.OnStateChangedEventArgs e)
    {
        // A line tapped during the intro skips straight to fully drawn
        if (introTween.isAlive)
        {
            introTween.Stop();
        }
        introProgress = 1f;

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
            widthTween = Tween.Custom(this, lineRenderer.widthMultiplier, width * hintWidthMultiplier, hintWidthDuration,
                (visual, value) => visual.lineRenderer.widthMultiplier = value,
                Ease.OutQuad);
        }
        else
        {
            lineRenderer.sortingOrder = baseSortingOrder;
            lineRenderer.widthMultiplier = width;
        }
    }

    private void Line_OnDimChanged(object sender, System.EventArgs e)
    {
        dimTween.Stop();

        float targetVisibility = line.IsDimmed ? dimmedAlpha : 1f;
        if (!Mathf.Approximately(visibility, targetVisibility))
        {
            dimTween = Tween.Custom(this, visibility, targetVisibility, dimDuration,
                (visual, value) => visual.SetVisibility(value));
        }
    }

    private void LateUpdate()
    {
        if (line.CurrentState == Line.State.Moving || line.CurrentState == Line.State.Resetting)
        {
            Redraw();
        }
    }

    private void SetIntroProgress(float progress)
    {
        introProgress = progress;
        Redraw();
    }

    private void Redraw()
    {
        line.GetVisibleRange(out float fromDistance, out float toDistance);
        toDistance = Mathf.Min(toDistance, line.Path.Length * introProgress);

        int pointCount = line.Path.CopyPoints(fromDistance, toDistance, pointBuffer);
        lineRenderer.positionCount = pointCount;
        lineRenderer.SetPositions(pointBuffer);
    }

    private void SetVisibility(float value)
    {
        visibility = value;
        SetColor(currentColor);
    }

    private void SetColor(Color color)
    {
        currentColor = color;
        color.a *= visibility;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }

    private void OnDestroy()
    {
        Tween.StopAll(this);
    }
}

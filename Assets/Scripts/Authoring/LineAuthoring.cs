using UnityEngine;
using UnityEngine.Splines;

// Marks a spline in the LevelAuthoring scene as one line of the level. Used in the Editor only
[RequireComponent(typeof(SplineContainer))]
public class LineAuthoring : MonoBehaviour
{
    private const int GIZMO_SEGMENTS = 48;
    private const float GIZMO_HEAD_RADIUS = 0.1f;

    public Color color = Color.white;

    private void OnDrawGizmos()
    {
        SplineContainer container = GetComponent<SplineContainer>();
        if (container.Spline == null || container.Spline.Count < 2)
        {
            return;
        }

        Gizmos.color = color;

        Vector3 previousPoint = container.EvaluatePosition(0f);
        Gizmos.DrawSphere(previousPoint, GIZMO_HEAD_RADIUS);

        for (int i = 1; i <= GIZMO_SEGMENTS; i++)
        {
            Vector3 point = container.EvaluatePosition(i / (float)GIZMO_SEGMENTS);
            Gizmos.DrawLine(previousPoint, point);
            previousPoint = point;
        }

        Gizmos.DrawSphere(previousPoint, GIZMO_HEAD_RADIUS);
    }
}

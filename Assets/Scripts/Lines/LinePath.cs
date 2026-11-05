using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

// A spline sampled once into evenly spaced points, addressed by distance along the line
// Movement, drawing and hint prediction all read this, so they always agree on where a head is
public class LinePath
{
    private const int MIN_SEGMENT_COUNT = 8;

    private readonly Vector2[] points;
    private readonly float[] distances;

    public float Length { get; }
    public int MaxPointCount => points.Length + 2;

    public LinePath(Spline spline, float samplesPerUnit)
    {
        int segmentCount = Mathf.Max(MIN_SEGMENT_COUNT, Mathf.CeilToInt(spline.GetLength() * samplesPerUnit));
        points = new Vector2[segmentCount + 1];
        distances = new float[segmentCount + 1];

        for (int i = 0; i <= segmentCount; i++)
        {
            float3 position = spline.EvaluatePosition(i / (float)segmentCount);
            points[i] = new Vector2(position.x, position.y);

            if (i > 0)
            {
                distances[i] = distances[i - 1] + Vector2.Distance(points[i - 1], points[i]);
            }
        }

        Length = distances[segmentCount];
    }

    public Vector2 GetPosition(float distance)
    {
        int segment = FindSegment(distance);
        float t = Mathf.InverseLerp(distances[segment], distances[segment + 1], distance);
        return Vector2.Lerp(points[segment], points[segment + 1], t);
    }

    public Vector2 GetDirection(float distance)
    {
        int segment = FindSegment(distance);
        return (points[segment + 1] - points[segment]).normalized;
    }

    // Writes the part of the line between two distances into buffer and returns the point count
    public int CopyPoints(float fromDistance, float toDistance, Vector3[] buffer)
    {
        if (toDistance <= fromDistance)
        {
            return 0;
        }

        int count = 0;
        buffer[count++] = GetPosition(fromDistance);

        for (int i = FindSegment(fromDistance) + 1; i < points.Length && distances[i] < toDistance; i++)
        {
            buffer[count++] = points[i];
        }

        buffer[count++] = GetPosition(toDistance);
        return count;
    }

    private int FindSegment(float distance)
    {
        int low = 0;
        int high = distances.Length - 2;

        while (low < high)
        {
            int middle = (low + high + 1) / 2;
            if (distances[middle] <= distance)
            {
                low = middle;
            }
            else
            {
                high = middle - 1;
            }
        }

        return low;
    }
}

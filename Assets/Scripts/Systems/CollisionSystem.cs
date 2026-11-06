using System;
using System.Collections.Generic;
using UnityEngine;

// Checks moving arrowheads against moving and stationary arrowheads once all lines have moved this frame. Line paths are never checked
// It uses plain math rather than physics so that HintSystem can predict collisions with the same rule
public class CollisionSystem : MonoBehaviour
{
    private const float LEAVE_CHECK_STEP = 0.05f;

    public static CollisionSystem Instance { get; private set; }

    public event EventHandler<OnCollisionEventArgs> OnCollision;
    public class OnCollisionEventArgs : EventArgs
    {
        public Vector2 position;
    }

    private void Awake()
    {
        Instance = this;
    }

    private void LateUpdate()
    {
        IReadOnlyList<Line> lines = LevelSpawner.Instance.ActiveLines;

        // A collision can end the level partway through, so this checks IsPlaying on every pass
        for (int i = 0; i < lines.Count && GameManager.Instance.IsPlaying; i++)
        {
            Line line = lines[i];
            if (!line.IsMoving)
            {
                continue;
            }

            CheckAgainstMovingHeads(line, lines, i + 1);

            if (line.IsMoving)
            {
                CheckAgainstStationaryHeads(line, lines);
            }
        }
    }

    private void CheckAgainstMovingHeads(Line line, IReadOnlyList<Line> lines, int firstIndex)
    {
        for (int i = firstIndex; i < lines.Count; i++)
        {
            Line other = lines[i];
            if (!other.IsMoving)
            {
                continue;
            }

            if (HeadsTouch(line.PreviousHeadPosition, line.HeadPosition, other.PreviousHeadPosition, other.HeadPosition))
            {
                line.FailMovement();
                other.FailMovement();
                RaiseCollision((line.HeadPosition + other.HeadPosition) * 0.5f);
                return;
            }
        }
    }

    private void CheckAgainstStationaryHeads(Line line, IReadOnlyList<Line> lines)
    {
        foreach (Line other in lines)
        {
            if (other == line)
            {
                continue;
            }

            if (HitsIfStationary(line, other.StartHead) || HitsIfStationary(line, other.EndHead))
            {
                line.FailMovement();
                RaiseCollision(line.HeadPosition);
                return;
            }
        }
    }

    // A line's active head is checked as a moving head instead, or is rewinding and can't be hit
    private bool HitsIfStationary(Line line, ArrowHead head)
    {
        return head != head.Line.ActiveHead && HeadTouchesStationary(line, line.IsMovingFromStart, line.PreviousTravelled,
            line.PreviousHeadPosition, line.HeadPosition, head.HomePosition);
    }

    public static bool HeadTouchesStationary(Line line, bool fromStart, float previousTravelled, Vector2 from, Vector2 to, Vector2 stationaryHead)
    {
        return HeadsTouch(from, to, stationaryHead, stationaryHead) && !IsStillLeavingHead(line, fromStart, previousTravelled, stationaryHead);
    }

    // Heads can rest touching each other, so one touching a line's start only counts once the line has moved clear of it
    // The check stops at the start of this step, because a step that begins inside the radius always counts as touching
    private static bool IsStillLeavingHead(Line line, bool fromStart, float previousTravelled, Vector2 stationaryHead)
    {
        float collisionDistance = GameManager.Instance.Settings.collisionDistance;

        for (float distance = 0f; distance < previousTravelled; distance += LEAVE_CHECK_STEP)
        {
            if (Vector2.Distance(line.GetPositionFrom(fromStart, distance), stationaryHead) >= collisionDistance)
            {
                return false;
            }
        }

        return Vector2.Distance(line.GetPositionFrom(fromStart, previousTravelled), stationaryHead) < collisionDistance;
    }

    // Whether two heads came within collision distance while each moved in a straight line over one step
    // Testing the whole step means fast heads or a slow frame can't pass through each other unnoticed
    public static bool HeadsTouch(Vector2 headAFrom, Vector2 headATo, Vector2 headBFrom, Vector2 headBTo)
    {
        Vector2 offsetAtStart = headAFrom - headBFrom;
        Vector2 offsetChange = (headATo - headBTo) - offsetAtStart;
        float changeSquared = offsetChange.sqrMagnitude;

        float closestTime = changeSquared > Mathf.Epsilon
            ? Mathf.Clamp01(-Vector2.Dot(offsetAtStart, offsetChange) / changeSquared)
            : 0f;

        float collisionDistance = GameManager.Instance.Settings.collisionDistance;
        return (offsetAtStart + offsetChange * closestTime).sqrMagnitude < collisionDistance * collisionDistance;
    }

    private void RaiseCollision(Vector2 position)
    {
        OnCollision?.Invoke(this, new OnCollisionEventArgs { position = position });
    }
}

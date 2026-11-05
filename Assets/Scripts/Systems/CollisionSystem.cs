using System;
using System.Collections.Generic;
using UnityEngine;

// Checks arrowheads against each other once all lines have moved this frame. Line paths are never checked
// It uses plain math rather than physics so that HintSystem can predict collisions with the same rule
public class CollisionSystem : MonoBehaviour
{
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
        if (!GameManager.Instance.IsPlaying)
        {
            return;
        }

        IReadOnlyList<Line> lines = LevelSpawner.Instance.ActiveLines;
        bool idleHeadsBlock = LevelSpawner.Instance.CurrentLevel.stationaryHeadsBlock;

        for (int i = 0; i < lines.Count; i++)
        {
            Line line = lines[i];
            if (!line.IsMoving)
            {
                continue;
            }

            CheckAgainstMovingHeads(line, lines, i + 1);

            if (idleHeadsBlock && line.IsMoving)
            {
                CheckAgainstIdleHeads(line, lines);
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

    private void CheckAgainstIdleHeads(Line line, IReadOnlyList<Line> lines)
    {
        foreach (Line other in lines)
        {
            if (other == line || other.CurrentState != Line.State.Idle)
            {
                continue;
            }

            if (HeadTouchesPoint(line, other.StartHead.transform.position) ||
                HeadTouchesPoint(line, other.EndHead.transform.position))
            {
                line.FailMovement();
                RaiseCollision(line.HeadPosition);
                return;
            }
        }
    }

    private bool HeadTouchesPoint(Line line, Vector2 point)
    {
        return HeadsTouch(line.PreviousHeadPosition, line.HeadPosition, point, point);
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

using System;
using System.Collections.Generic;
using UnityEngine;

// Finds every line that could be completed right now without a collision and highlights one at random
// It plays the current board forward in a simulation, without touching the real lines
// A shown hint keeps getting checked, so it never points at a head that has stopped being safe
public class HintSystem : MonoBehaviour
{
    private const float SIMULATION_STEP = 1f / 60f;
    private const float RECHECK_INTERVAL = 0.1f;

    public static HintSystem Instance { get; private set; }

    public event EventHandler OnNoHintAvailable;

    // A legal line, and which of its two heads can safely be started
    public struct LegalLine
    {
        public Line line;
        public bool startHeadIsSafe;
        public bool endHeadIsSafe;
    }

    private class SimulatedHead
    {
        public Line line;
        public bool fromStart;
        public float travelled;
        public Vector2 previousPosition;
        public Vector2 position;

        public SimulatedHead(Line line, bool fromStart, float travelled)
        {
            this.line = line;
            this.fromStart = fromStart;
            this.travelled = travelled;
            position = line.GetPositionFrom(fromStart, travelled);
            previousPosition = position;
        }

        public bool HasArrived => travelled >= line.Path.Length;

        public void Step(float distance)
        {
            travelled = Mathf.Min(travelled + distance, line.Path.Length);
            previousPosition = position;
            position = line.GetPositionFrom(fromStart, travelled);
        }
    }

    private Line hintedLine;
    private float recheckTimer;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        LineSelector.Instance.OnLineStarted += LineSelector_OnLineStarted;
    }

    private void Update()
    {
        if (hintedLine == null)
        {
            return;
        }

        recheckTimer -= Time.deltaTime;
        if (recheckTimer <= 0f)
        {
            recheckTimer = RECHECK_INTERVAL;
            RecheckHint();
        }
    }

    private void LineSelector_OnLineStarted(object sender, EventArgs e)
    {
        ClearHint();
    }

    public void ShowHint()
    {
        if (!GameManager.Instance.IsPlaying)
        {
            return;
        }

        ClearHint();

        List<LegalLine> legalLines = FindLegalLines();
        if (legalLines.Count == 0)
        {
            OnNoHintAvailable?.Invoke(this, EventArgs.Empty);
            return;
        }

        LegalLine choice = legalLines[UnityEngine.Random.Range(0, legalLines.Count)];
        hintedLine = choice.line;
        hintedLine.SetHint(choice.startHeadIsSafe, choice.endHeadIsSafe);
        recheckTimer = RECHECK_INTERVAL;
        UpdateDimming();
    }

    private void RecheckHint()
    {
        if (!GameManager.Instance.IsPlaying || hintedLine.CurrentState != Line.State.Idle)
        {
            ClearHint();
            return;
        }

        bool startHeadIsSafe = CanComplete(hintedLine, true);
        bool endHeadIsSafe = CanComplete(hintedLine, false);

        if (!startHeadIsSafe && !endHeadIsSafe)
        {
            ClearHint();
            return;
        }

        if (startHeadIsSafe != hintedLine.StartHead.IsHinted || endHeadIsSafe != hintedLine.EndHead.IsHinted)
        {
            hintedLine.SetHint(startHeadIsSafe, endHeadIsSafe);
        }

        UpdateDimming();
    }

    // Moving heads stay fully visible, since the player needs to see them to avoid a crash
    private void UpdateDimming()
    {
        foreach (Line line in LevelSpawner.Instance.ActiveLines)
        {
            line.SetDimmed(line != hintedLine && line.CurrentState == Line.State.Idle);
        }
    }

    public List<LegalLine> FindLegalLines()
    {
        List<LegalLine> legalLines = new List<LegalLine>();

        foreach (Line line in LevelSpawner.Instance.ActiveLines)
        {
            if (line.CurrentState != Line.State.Idle)
            {
                continue;
            }

            bool startHeadIsSafe = CanComplete(line, true);
            bool endHeadIsSafe = CanComplete(line, false);

            if (startHeadIsSafe || endHeadIsSafe)
            {
                legalLines.Add(new LegalLine
                {
                    line = line,
                    startHeadIsSafe = startHeadIsSafe,
                    endHeadIsSafe = endHeadIsSafe,
                });
            }
        }

        return legalLines;
    }

    private bool CanComplete(Line candidateLine, bool fromStart)
    {
        IReadOnlyList<Line> lines = LevelSpawner.Instance.ActiveLines;
        SimulatedHead candidate = new SimulatedHead(candidateLine, fromStart, 0f);

        List<SimulatedHead> movingHeads = new List<SimulatedHead>();
        foreach (Line line in lines)
        {
            if (line.IsMoving)
            {
                movingHeads.Add(new SimulatedHead(line, line.IsMovingFromStart, line.Travelled));
            }
        }

        List<Vector2> idleHeadPositions = LevelSpawner.Instance.CurrentLevel.stationaryHeadsBlock
            ? GetBlockingHeadPositions(lines, candidateLine)
            : new List<Vector2>();

        float stepDistance = GameManager.Instance.Settings.arrowSpeed * SIMULATION_STEP;

        while (!candidate.HasArrived)
        {
            candidate.Step(stepDistance);
            foreach (SimulatedHead head in movingHeads)
            {
                head.Step(stepDistance);
            }

            if (TouchesAny(candidate, movingHeads) || TouchesAnyPoint(candidate, idleHeadPositions))
            {
                return false;
            }

            RemoveFinishedHeads(movingHeads, idleHeadPositions);
        }

        return true;
    }

    // Resetting lines count as idle here, because their heads will be back home almost immediately
    private List<Vector2> GetBlockingHeadPositions(IReadOnlyList<Line> lines, Line candidateLine)
    {
        List<Vector2> positions = new List<Vector2>();

        foreach (Line line in lines)
        {
            if (line == candidateLine || line.IsMoving)
            {
                continue;
            }

            positions.Add(line.GetPositionFrom(true, 0f));
            positions.Add(line.GetPositionFrom(false, 0f));
        }

        return positions;
    }

    private bool TouchesAny(SimulatedHead candidate, List<SimulatedHead> movingHeads)
    {
        foreach (SimulatedHead head in movingHeads)
        {
            if (CollisionSystem.HeadsTouch(candidate.previousPosition, candidate.position, head.previousPosition, head.position))
            {
                return true;
            }
        }

        return false;
    }

    private bool TouchesAnyPoint(SimulatedHead head, List<Vector2> points)
    {
        foreach (Vector2 point in points)
        {
            if (CollisionSystem.HeadsTouch(head.previousPosition, head.position, point, point))
            {
                return true;
            }
        }

        return false;
    }

    // Takes out heads that arrive or crash into each other, since they can no longer reach the candidate
    private void RemoveFinishedHeads(List<SimulatedHead> movingHeads, List<Vector2> idleHeadPositions)
    {
        for (int i = movingHeads.Count - 1; i >= 0; i--)
        {
            SimulatedHead head = movingHeads[i];

            if (head.HasArrived || TouchesAnyPoint(head, idleHeadPositions))
            {
                movingHeads.RemoveAt(i);
                continue;
            }

            for (int j = i - 1; j >= 0; j--)
            {
                SimulatedHead other = movingHeads[j];
                if (CollisionSystem.HeadsTouch(head.previousPosition, head.position, other.previousPosition, other.position))
                {
                    movingHeads.RemoveAt(i);
                    movingHeads.RemoveAt(j);
                    i--;
                    break;
                }
            }
        }
    }

    private void ClearHint()
    {
        if (hintedLine != null)
        {
            hintedLine.ClearHint();
        }

        hintedLine = null;

        foreach (Line line in LevelSpawner.Instance.ActiveLines)
        {
            line.SetDimmed(false);
        }
    }
}

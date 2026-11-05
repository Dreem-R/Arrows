using System;
using UnityEngine;

// The gameplay state of one coloured line. It moves one arrowhead along the path and reports
// what happens through events
public class Line : MonoBehaviour
{
    public enum State
    {
        Idle,
        Moving,
        Resetting,
        Completed,
    }

    public event EventHandler OnInitialized;
    public event EventHandler OnHintChanged;
    public event EventHandler OnDimChanged;
    public event EventHandler<OnStateChangedEventArgs> OnStateChanged;
    public class OnStateChangedEventArgs : EventArgs
    {
        public State previousState;
        public State newState;
    }

    [SerializeField] private ArrowHead arrowHeadPrefab;

    private GameSettingsSO settings;
    private float travelled;
    private float resetSpeed;
    private bool isHalted;

    public LinePath Path { get; private set; }
    public Color Color { get; private set; }
    public int Index { get; private set; }
    public State CurrentState { get; private set; } = State.Idle;
    public ArrowHead StartHead { get; private set; }
    public ArrowHead EndHead { get; private set; }

    // The head that is travelling or rewinding. Null while the line is idle
    public ArrowHead ActiveHead { get; private set; }
    public float Travelled => travelled;
    public bool IsHinted { get; private set; }
    public bool IsDimmed { get; private set; }
    public Vector2 HeadPosition { get; private set; }
    public Vector2 PreviousHeadPosition { get; private set; }

    public bool IsMoving => CurrentState == State.Moving;
    public bool IsMovingFromStart => ActiveHead == StartHead;

    public void Initialize(LineData data, GameSettingsSO gameSettings, int index)
    {
        settings = gameSettings;
        Color = data.color;
        Index = index;
        Path = new LinePath(data.spline, settings.samplesPerUnit);

        StartHead = SpawnHead(true);
        EndHead = SpawnHead(false);

        OnInitialized?.Invoke(this, EventArgs.Empty);
    }

    private ArrowHead SpawnHead(bool isAtLineStart)
    {
        ArrowHead head = Instantiate(arrowHeadPrefab, transform);
        head.Initialize(this, isAtLineStart);
        return head;
    }

    private void Update()
    {
        if (isHalted)
        {
            return;
        }

        switch (CurrentState)
        {
            case State.Moving:
                Advance();
                break;
            case State.Resetting:
                Rewind();
                break;
        }
    }

    // Starts the given head if this line is free. The opposite head is disabled until the line finishes or resets
    public bool TryStartMoving(ArrowHead head)
    {
        if (CurrentState != State.Idle)
        {
            return false;
        }

        ActiveHead = head;
        travelled = 0f;
        HeadPosition = GetPositionFrom(IsMovingFromStart, 0f);
        PreviousHeadPosition = HeadPosition;
        SetState(State.Moving);
        return true;
    }

    // Called on a collision. The head rewinds to where it started and the line regrows
    public void FailMovement()
    {
        if (CurrentState != State.Moving)
        {
            return;
        }

        resetSpeed = travelled / settings.resetDuration;
        SetState(State.Resetting);
    }

    // Freezes the line once the level has ended
    public void Halt()
    {
        isHalted = true;
    }

    // Marks this line as a hint. Each flag says whether that head is safe to start right now
    public void SetHint(bool startHeadIsSafe, bool endHeadIsSafe)
    {
        IsHinted = startHeadIsSafe || endHeadIsSafe;
        StartHead.SetHinted(startHeadIsSafe);
        EndHead.SetHinted(endHeadIsSafe);
        OnHintChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ClearHint()
    {
        SetHint(false, false);
    }

    public void SetDimmed(bool isDimmed)
    {
        if (IsDimmed == isDimmed)
        {
            return;
        }

        IsDimmed = isDimmed;
        OnDimChanged?.Invoke(this, EventArgs.Empty);
    }

    // The part of the line still visible, as a range of distances from the line's start
    public void GetVisibleRange(out float fromDistance, out float toDistance)
    {
        bool isTrimmed = ActiveHead != null;
        fromDistance = isTrimmed && IsMovingFromStart ? travelled : 0f;
        toDistance = isTrimmed && !IsMovingFromStart ? Path.Length - travelled : Path.Length;
    }

    // Position after travelling a distance, measured from the chosen end of the line
    public Vector2 GetPositionFrom(bool fromStart, float distance)
    {
        return Path.GetPosition(fromStart ? distance : Path.Length - distance);
    }

    // Travel direction after a distance, measured from the chosen end of the line
    public Vector2 GetDirectionFrom(bool fromStart, float distance)
    {
        return fromStart ? Path.GetDirection(distance) : -Path.GetDirection(Path.Length - distance);
    }

    private void Advance()
    {
        travelled = Mathf.Min(travelled + settings.arrowSpeed * Time.deltaTime, Path.Length);
        UpdateActiveHead();

        if (travelled >= Path.Length)
        {
            Complete();
        }
    }

    private void Rewind()
    {
        travelled = Mathf.Max(travelled - resetSpeed * Time.deltaTime, 0f);

        if (travelled > 0f)
        {
            UpdateActiveHead();
            return;
        }

        ActiveHead.ReturnHome();
        ActiveHead = null;
        SetState(State.Idle);
    }

    private void UpdateActiveHead()
    {
        bool fromStart = IsMovingFromStart;
        PreviousHeadPosition = HeadPosition;
        HeadPosition = GetPositionFrom(fromStart, travelled);

        // During a rewind the head still faces the way it was travelling
        ActiveHead.SetPose(HeadPosition, GetDirectionFrom(fromStart, travelled));
    }

    private void Complete()
    {
        SetState(State.Completed);
        Destroy(gameObject, settings.removeDuration);
    }

    private void SetState(State newState)
    {
        State previousState = CurrentState;
        CurrentState = newState;
        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
        {
            previousState = previousState,
            newState = newState,
        });
    }
}

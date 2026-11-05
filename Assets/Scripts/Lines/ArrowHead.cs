using System;
using UnityEngine;

// One tappable end of a line. Its look lives in ArrowHeadVisual
public class ArrowHead : MonoBehaviour
{
    public event EventHandler OnInitialized;
    public event EventHandler OnTapDenied;
    public event EventHandler OnHintChanged;

    public Line Line { get; private set; }
    public bool IsAtLineStart { get; private set; }
    public bool IsHinted { get; private set; }

    public void Initialize(Line line, bool isAtLineStart)
    {
        Line = line;
        IsAtLineStart = isAtLineStart;
        ReturnHome();
        OnInitialized?.Invoke(this, EventArgs.Empty);
    }

    public void SetPose(Vector2 position, Vector2 direction)
    {
        transform.SetPositionAndRotation(position, Quaternion.LookRotation(Vector3.forward, direction));
    }

    public void ReturnHome()
    {
        SetPose(Line.GetPositionFrom(IsAtLineStart, 0f), Line.GetDirectionFrom(IsAtLineStart, 0f));
    }

    public void SetHinted(bool isHinted)
    {
        if (IsHinted == isHinted)
        {
            return;
        }

        IsHinted = isHinted;
        OnHintChanged?.Invoke(this, EventArgs.Empty);
    }

    public void DenyTap()
    {
        OnTapDenied?.Invoke(this, EventArgs.Empty);
    }
}

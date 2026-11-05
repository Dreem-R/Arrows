using System;
using UnityEngine;

// Decides which arrowhead a tap selects and starts its line
public class LineSelector : MonoBehaviour
{
    public static LineSelector Instance { get; private set; }

    public event EventHandler OnLineStarted;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GameInput.Instance.OnTap += GameInput_OnTap;
    }

    private void GameInput_OnTap(object sender, GameInput.OnTapEventArgs e)
    {
        if (!GameManager.Instance.IsPlaying)
        {
            return;
        }

        FindClosestHeads(e.worldPosition, out ArrowHead closestReadyHead, out ArrowHead closestBusyHead);

        // Heads can overlap, so a usable head always wins over a closer disabled one
        if (closestReadyHead != null)
        {
            closestReadyHead.Line.TryStartMoving(closestReadyHead);
            OnLineStarted?.Invoke(this, EventArgs.Empty);
        }
        else if (closestBusyHead != null)
        {
            closestBusyHead.DenyTap();
        }
    }

    private void FindClosestHeads(Vector2 position, out ArrowHead closestReadyHead, out ArrowHead closestBusyHead)
    {
        float maxDistance = GameManager.Instance.Settings.tapRadius;
        float closestReadyDistance = maxDistance;
        float closestBusyDistance = maxDistance;
        closestReadyHead = null;
        closestBusyHead = null;

        foreach (Line line in LevelSpawner.Instance.ActiveLines)
        {
            bool isReady = line.CurrentState == Line.State.Idle;

            foreach (ArrowHead head in new[] { line.StartHead, line.EndHead })
            {
                float distance = Vector2.Distance(position, head.transform.position);

                if (isReady && distance < closestReadyDistance)
                {
                    closestReadyDistance = distance;
                    closestReadyHead = head;
                }
                else if (!isReady && distance < closestBusyDistance)
                {
                    closestBusyDistance = distance;
                    closestBusyHead = head;
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

// Builds a level from its LevelSO and tracks the lines still on the board
public class LevelSpawner : MonoBehaviour
{
    private const int MIN_KNOTS_PER_LINE = 2;

    public static LevelSpawner Instance { get; private set; }

    public event EventHandler OnLevelSpawned;
    public event EventHandler OnAllLinesCompleted;

    [SerializeField] private Line linePrefab;

    private readonly List<Line> activeLines = new List<Line>();

    public IReadOnlyList<Line> ActiveLines => activeLines;
    public LevelSO CurrentLevel { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void SpawnLevel(LevelSO level, GameSettingsSO settings)
    {
        ClearLevel();
        CurrentLevel = level;

        for (int i = 0; i < level.lines.Count; i++)
        {
            LineData lineData = level.lines[i];
            if (lineData.spline.Count < MIN_KNOTS_PER_LINE)
            {
                Debug.LogWarning($"{level.name}: line {i} was skipped because it has fewer than {MIN_KNOTS_PER_LINE} knots.", level);
                continue;
            }

            Line line = Instantiate(linePrefab, transform);
            line.name = $"Line {i}";
            line.OnStateChanged += Line_OnStateChanged;
            line.Initialize(lineData, settings, i);
            activeLines.Add(line);
        }

        OnLevelSpawned?.Invoke(this, EventArgs.Empty);
    }

    public void HaltAllLines()
    {
        foreach (Line line in activeLines)
        {
            line.Halt();
        }
    }

    // The area every line occupies, used to frame the camera
    public Bounds GetLevelBounds()
    {
        Bounds bounds = new Bounds();
        bool hasPoint = false;

        foreach (Line line in activeLines)
        {
            for (float distance = 0f; distance < line.Path.Length; distance += 0.1f)
            {
                Vector3 point = line.Path.GetPosition(distance);
                if (hasPoint)
                {
                    bounds.Encapsulate(point);
                }
                else
                {
                    bounds = new Bounds(point, Vector3.zero);
                    hasPoint = true;
                }
            }
        }

        return bounds;
    }

    private void Line_OnStateChanged(object sender, Line.OnStateChangedEventArgs e)
    {
        if (e.newState != Line.State.Completed)
        {
            return;
        }

        activeLines.Remove((Line)sender);

        if (activeLines.Count == 0)
        {
            OnAllLinesCompleted?.Invoke(this, EventArgs.Empty);
        }
    }

    private void ClearLevel()
    {
        activeLines.Clear();

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}

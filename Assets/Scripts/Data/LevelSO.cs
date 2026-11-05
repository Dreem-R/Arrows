using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

// One coloured line: its colour and the spline an arrowhead travels along
[Serializable]
public class LineData
{
    public Color color = Color.white;
    public Spline spline = new Spline();
}

// A level is only data. Add a new asset to get a new level, with no code changes
[CreateAssetMenu(menuName = "Arrows/Level")]
public class LevelSO : ScriptableObject
{
    [Tooltip("Off follows the brief: only moving arrowheads collide. On also makes idle arrowheads block moving ones.")]
    public bool stationaryHeadsBlock;

    public List<LineData> lines = new List<LineData>();
}

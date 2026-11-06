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

// One level: its lines
[CreateAssetMenu(menuName = "Arrows/Level")]
public class LevelSO : ScriptableObject
{
    public List<LineData> lines = new List<LineData>();
}

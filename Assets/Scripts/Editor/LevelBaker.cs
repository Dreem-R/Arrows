using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

// Adds Bake and Load buttons to LevelAuthoring, so levels are drawn in the Scene view and stored as data
[CustomEditor(typeof(LevelAuthoring))]
public class LevelBaker : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LevelAuthoring authoring = (LevelAuthoring)target;

        using (new EditorGUI.DisabledScope(authoring.level == null))
        {
            if (GUILayout.Button("Bake Splines To Level"))
            {
                Bake(authoring);
            }

            if (GUILayout.Button("Load Splines From Level"))
            {
                Load(authoring);
            }
        }
    }

    public static void Bake(LevelAuthoring authoring)
    {
        Undo.RecordObject(authoring.level, "Bake Level");
        authoring.level.lines.Clear();

        foreach (LineAuthoring lineAuthoring in authoring.GetComponentsInChildren<LineAuthoring>())
        {
            authoring.level.lines.Add(new LineData
            {
                color = lineAuthoring.color,
                spline = BakeSpline(lineAuthoring),
            });
        }

        EditorUtility.SetDirty(authoring.level);
        AssetDatabase.SaveAssetIfDirty(authoring.level);
        Debug.Log($"Baked {authoring.level.lines.Count} lines to {authoring.level.name}.", authoring.level);
    }

    public static void Load(LevelAuthoring authoring)
    {
        foreach (LineAuthoring lineAuthoring in authoring.GetComponentsInChildren<LineAuthoring>())
        {
            Undo.DestroyObjectImmediate(lineAuthoring.gameObject);
        }

        for (int i = 0; i < authoring.level.lines.Count; i++)
        {
            LineData lineData = authoring.level.lines[i];

            GameObject lineObject = new GameObject($"Line {i}");
            Undo.RegisterCreatedObjectUndo(lineObject, "Load Level");
            lineObject.transform.SetParent(authoring.transform, false);

            lineObject.AddComponent<SplineContainer>().Spline = new Spline(lineData.spline);
            lineObject.AddComponent<LineAuthoring>().color = lineData.color;
        }

        EditorUtility.SetDirty(authoring.gameObject);
    }

    // Copies the spline into world space, so moving a line in the scene moves it in the level
    private static Spline BakeSpline(LineAuthoring lineAuthoring)
    {
        Spline source = lineAuthoring.GetComponent<SplineContainer>().Spline;
        Matrix4x4 localToWorld = lineAuthoring.transform.localToWorldMatrix;

        Spline baked = new Spline();
        for (int i = 0; i < source.Count; i++)
        {
            baked.Add(source[i].Transform(localToWorld), source.GetTangentMode(i));
        }

        return baked;
    }
}

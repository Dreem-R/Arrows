using UnityEngine;

// The root of a level being edited. Its children are LineAuthoring splines drawn in the Scene view
// The Bake button in the Inspector writes them to the LevelSO. Used in the Editor only
public class LevelAuthoring : MonoBehaviour
{
    public LevelSO level;
}

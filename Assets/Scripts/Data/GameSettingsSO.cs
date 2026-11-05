using UnityEngine;

// Rules and tuning shared by every level. Gameplay code never hard-codes these values
[CreateAssetMenu(menuName = "Arrows/Game Settings")]
public class GameSettingsSO : ScriptableObject
{
    [Header("Rules")]
    [Min(1)] public int startingLives = 3;

    [Header("Movement")]
    [Tooltip("Arrowhead speed in world units per second.")]
    [Min(0.1f)] public float arrowSpeed = 3f;
    [Tooltip("Seconds a failed arrowhead takes to rewind to its start.")]
    [Min(0.01f)] public float resetDuration = 0.35f;
    [Tooltip("Seconds a completed line stays alive so its removal animation can play.")]
    [Min(0f)] public float removeDelay = 0.4f;
    [Tooltip("Seconds each line takes to draw itself when a level starts.")]
    [Min(0.01f)] public float levelIntroDuration = 0.45f;

    [Header("Collision and input")]
    [Tooltip("Two arrowheads collide when their centres come closer than this.")]
    [Min(0.01f)] public float collisionDistance = 0.28f;
    [Tooltip("How far from an arrowhead a tap can land and still select it.")]
    [Min(0.01f)] public float tapRadius = 0.5f;

    [Header("Path quality")]
    [Tooltip("How many points per world unit are sampled from each spline.")]
    [Min(1f)] public float samplesPerUnit = 16f;
}

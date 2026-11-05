using System;
using System.Collections.Generic;
using UnityEngine;

// Owns the rules of a play session: lives, winning, losing and retrying
// It runs after other scripts so that everything has subscribed before the first level loads
[DefaultExecutionOrder(100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum State
    {
        Playing,
        Won,
        Lost,
    }

    public event EventHandler OnLevelStarted;
    public event EventHandler OnLevelWon;
    public event EventHandler OnLevelLost;
    public event EventHandler<OnLivesChangedEventArgs> OnLivesChanged;
    public class OnLivesChangedEventArgs : EventArgs
    {
        public int lives;
    }

    [SerializeField] private GameSettingsSO settings;
    [SerializeField] private List<LevelSO> levelList;
    [SerializeField] private int currentLevelIndex;

    private State state;

    public GameSettingsSO Settings => settings;
    public int Lives { get; private set; }
    public bool IsPlaying => state == State.Playing;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        CollisionSystem.Instance.OnCollision += CollisionSystem_OnCollision;
        LevelSpawner.Instance.OnAllLinesCompleted += LevelSpawner_OnAllLinesCompleted;

        LoadCurrentLevel();
    }

    public void RetryLevel()
    {
        LoadCurrentLevel();
    }

    private void LoadCurrentLevel()
    {
        state = State.Playing;
        Lives = settings.startingLives;

        LevelSpawner.Instance.SpawnLevel(levelList[currentLevelIndex], settings);

        OnLevelStarted?.Invoke(this, EventArgs.Empty);
    }

    private void CollisionSystem_OnCollision(object sender, CollisionSystem.OnCollisionEventArgs e)
    {
        if (!IsPlaying)
        {
            return;
        }

        Lives--;
        OnLivesChanged?.Invoke(this, new OnLivesChangedEventArgs { lives = Lives });

        if (Lives <= 0)
        {
            state = State.Lost;
            LevelSpawner.Instance.HaltAllLines();
            OnLevelLost?.Invoke(this, EventArgs.Empty);
        }
    }

    private void LevelSpawner_OnAllLinesCompleted(object sender, EventArgs e)
    {
        if (!IsPlaying)
        {
            return;
        }

        state = State.Won;
        OnLevelWon?.Invoke(this, EventArgs.Empty);
    }
}

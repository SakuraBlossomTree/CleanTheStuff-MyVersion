using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Core Components")]
    public GameTimer gameTimer;
    public TrashSpawnerRandom trashSpawner;

    [Header("Spawn Points")]
    public Transform[] playerSpawnPoints;
    public Transform[] trashSpawnPoints;

    private int currentLevel = 0;
    private GameObject player;
    private bool levelActive = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("A GameObject with the tag 'Player' is required in the scene.");
            return;
        }

        if (gameTimer != null)
        {
            gameTimer.OnTimerEnd += HandleTimeUp;
        }

        StartLevel(currentLevel);
    }

    void OnDestroy()
    {
        if (gameTimer != null)
        {
            gameTimer.OnTimerEnd -= HandleTimeUp;
        }
    }

    void Update()
    {
        if (!levelActive || !gameTimer.isRunning) return;

        if (TrashCleaner.trashCollected >= trashSpawner.maxSpawns)
        {
            Debug.Log("Level Complete! Collected " + TrashCleaner.trashCollected + "/" + trashSpawner.maxSpawns);
            levelActive = false;
            AdvanceLevel();
        }
    }

    void HandleTimeUp()
    {
        Debug.Log("Timer ended but trash still remains. Level failed.");
        StartLevel(currentLevel);
    }

    void StartLevel(int levelIndex)
    {
        if (levelIndex >= playerSpawnPoints.Length || levelIndex >= trashSpawnPoints.Length)
        {
            Debug.Log("All levels completed!");
            return;
        }

        Debug.Log("Starting level " + levelIndex);

        TrashCleaner.ResetProgress();

        // Move player to this level's spawn point
        player.transform.position = playerSpawnPoints[levelIndex].position;

        // Move trash spawner to this level's location
        if (trashSpawner != null)
        {
            trashSpawner.transform.position = trashSpawnPoints[levelIndex].position;
            trashSpawner.ResetSpawner();
        }

        // Destroy leftover trash
        foreach (GameObject oldTrash in GameObject.FindGameObjectsWithTag("Trash"))
        {
            Destroy(oldTrash);
        }

        gameTimer.ResetTimer();
        gameTimer.StartTimer();

        levelActive = true;
    }

    void AdvanceLevel()
    {
        currentLevel++;
        StartLevel(currentLevel);
    }
}
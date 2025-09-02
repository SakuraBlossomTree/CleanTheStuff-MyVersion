using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Core Components")]
    public GameTimer gameTimer;
    public TrashSpawner trashSpawner;

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

        if (!levelActive) return;

        if (!gameTimer.isRunning) return;

        if (trashSpawner.ActiveTrash == 0)
        {
            levelActive = false;
            AdvanceLevel();
        }
    }

    void HandleTimeUp()
    {
        Debug.Log("⛔ Timer ended but trash still remains! Level failed.");

        StartLevel(currentLevel);
    }

    void StartLevel(int levelIndex)
    {
        if (levelIndex >= playerSpawnPoints.Length || levelIndex >= trashSpawnPoints.Length)
        {
            Debug.Log("🎉 All levels completed!");
            return;
        }

        Debug.Log("✅ Starting level " + levelIndex);

        player.transform.position = playerSpawnPoints[levelIndex].position;

        foreach (GameObject oldTrash in GameObject.FindGameObjectsWithTag("Trash"))
        {
            Destroy(oldTrash);
        }

        if (trashSpawner != null)
        {
            trashSpawner.transform.position = trashSpawnPoints[levelIndex].position;
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
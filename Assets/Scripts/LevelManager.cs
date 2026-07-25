using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private int currentLevel = 0;

    void Start()
    {
        StartLevel(currentLevel);
    }

    void Update()
    {
        // Reset the current level with R
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Level reset by player.");
            StartLevel(currentLevel);
        }
    }

    void StartLevel(int levelIndex)
    {
        Debug.Log("Starting level " + levelIndex);
        // Level setup goes here
    }
}
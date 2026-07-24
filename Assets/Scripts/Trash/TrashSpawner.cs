using UnityEngine;
using System.Collections;

public class TrashSpawner : MonoBehaviour
{
    [System.Serializable]
    public class TrashType
    {
        public GameObject prefab;
        public float spawnChance;
    }

    [System.Serializable]
    public class SpawnPoint
    {
        public Transform point;
        [HideInInspector] public bool isOccupied = false;
    }

    [Header("Trash Settings")]
    public TrashType[] trashTypes;

    [Header("Spawn Points")]
    public SpawnPoint[] spawnPoints;

    [Header("Timing")]
    public float minSpawnTime = 2f;
    public float maxSpawnTime = 5f;

    [Header("Level Quota")]
    public int maxTrashSpawns = 15; // Set this in the Inspector for each level!

    // Tracking variables for the Win Condition
    public int ActiveTrash { get; private set; } = 0;
    public int TotalTrashSpawned { get; private set; } = 0;
    public int TotalTrashCollected { get; private set; } = 0;

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            // Stop spawning if we hit the max quota for the level
            if (TotalTrashSpawned >= maxTrashSpawns)
            {
                yield return null;
                continue;
            }

            yield return new WaitForSeconds(Random.Range(minSpawnTime, maxSpawnTime));
            TrySpawnTrash();
        }
    }

    void TrySpawnTrash()
    {
        if (spawnPoints.Length == 0 || trashTypes.Length == 0) return;

        int index = Random.Range(0, spawnPoints.Length);
        SpawnPoint sp = spawnPoints[index];

        if (!sp.isOccupied)
        {
            GameObject trashPrefab = GetRandomTrashPrefab();
            if (trashPrefab != null)
            {
                GameObject spawnedTrash = Instantiate(trashPrefab, sp.point.position, sp.point.rotation);
                sp.isOccupied = true;
                ActiveTrash++;
                TotalTrashSpawned++; // Track total spawned

                Trash trashComp = spawnedTrash.GetComponent<Trash>();
                if (trashComp != null)
                {
                    trashComp.onTrashDestroyed += () =>
                    {
                        sp.isOccupied = false;
                        ActiveTrash--;
                        TotalTrashCollected++; // Track total collected for win condition
                    };
                }
                else
                {
                    Destroy(spawnedTrash, 10f);
                    sp.isOccupied = false;
                    ActiveTrash--;
                    TotalTrashCollected++;
                }
            }
        }
    }

    GameObject GetRandomTrashPrefab()
    {
        float total = 0f;
        foreach (TrashType type in trashTypes) total += type.spawnChance;

        float randomValue = Random.Range(0, total);
        float cumulative = 0f;

        foreach (TrashType type in trashTypes)
        {
            cumulative += type.spawnChance;
            if (randomValue <= cumulative) return type.prefab;
        }
        return null;
    }

    // Called by LevelManager when restarting a level to ensure a clean slate
    public void ResetSpawner()
    {
        ActiveTrash = 0;
        TotalTrashSpawned = 0;
        TotalTrashCollected = 0;
        foreach (var sp in spawnPoints)
        {
            sp.isOccupied = false;
        }
    }
}
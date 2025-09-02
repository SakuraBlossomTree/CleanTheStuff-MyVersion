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

    public int ActiveTrash { get; private set; } = 0;

    void Start()
    {
        // Start the continuous spawning process
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            // Wait random time between spawns
            yield return new WaitForSeconds(Random.Range(minSpawnTime, maxSpawnTime));
            TrySpawnTrash();
        }
    }

    void TrySpawnTrash()
    {
        if (spawnPoints.Length == 0 || trashTypes.Length == 0) return;

        // Pick random spawn point
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

                // Setup cleanup callback to free spawn point when trash is destroyed
                Trash trashComp = spawnedTrash.GetComponent<Trash>();
                if (trashComp != null)
                {
                    trashComp.onTrashDestroyed += () =>
                    {
                        sp.isOccupied = false;
                        ActiveTrash--;
                    };
                }
                else
                {
                    // Fallback: auto-destroy after 10 seconds if no Trash component
                    Destroy(spawnedTrash, 10f); 
                    sp.isOccupied = false;
                    ActiveTrash--;
                }
            }
        }
    }

    GameObject GetRandomTrashPrefab()
    {
        // Calculate total spawn chance for weighted random selection
        float total = 0f;
        foreach (TrashType type in trashTypes)
        {
            total += type.spawnChance;
        }

        float randomValue = Random.Range(0, total);
        float cumulative = 0f;

        // Find which trash type the random value selects
        foreach (TrashType type in trashTypes)
        {
            cumulative += type.spawnChance;
            if (randomValue <= cumulative)
            {
                return type.prefab;
            }
        }
        return null;
    }
}
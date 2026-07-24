using UnityEngine;
using System.Collections;

public class TrashSpawnerRandom : MonoBehaviour
{
    [System.Serializable]
    public class TrashType
    {
        public GameObject prefab;
        public float spawnChance;
    }

    [Header("Trash Settings")]
    public TrashType[] trashTypes;

    [Header("Spawn Area")]
    public Vector3 areaSize = new Vector3(10, 0, 10);
    public Vector3 areaCenter = Vector3.zero;

    [Header("Timing")]
    public float minSpawnTime;
    public float maxSpawnTime;

    [Header("Limit")]
    public int maxSpawns;

    // CHANGED: Made this publicly readable so LevelManager can check it
    public int CurrentSpawnCount { get; private set; } = 2;

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (CurrentSpawnCount < maxSpawns)
        {
            yield return new WaitForSeconds(Random.Range(minSpawnTime, maxSpawnTime));
            TrySpawnTrash();
        }
    }

    void TrySpawnTrash()
    {
        if (CurrentSpawnCount >= maxSpawns) return;

        GameObject trashPrefab = GetRandomTrashPrefab();
        if (trashPrefab != null)
        {
            Vector3 randomPos = GetRandomPosition();
            Instantiate(trashPrefab, randomPos, Quaternion.identity);
            CurrentSpawnCount++;
        }
    }

    Vector3 GetRandomPosition()
    {
        Vector3 randomOffset = new Vector3(
            Random.Range(-areaSize.x / 2, areaSize.x / 2),
            Random.Range(-areaSize.y / 2, areaSize.y / 2),
            Random.Range(-areaSize.z / 2, areaSize.z / 2)
        );
        return transform.position + areaCenter + randomOffset;
    }

    GameObject GetRandomTrashPrefab()
    {
        float total = 0f;
        foreach (TrashType type in trashTypes)
            total += type.spawnChance;

        float randomValue = Random.Range(0, total);
        float cumulative = 0f;

        foreach (TrashType type in trashTypes)
        {
            cumulative += type.spawnChance;
            if (randomValue <= cumulative)
                return type.prefab;
        }
        return null;
    }

    // Reset for level restarts
    public void ResetSpawner()
    {
        CurrentSpawnCount = 0;
        StopAllCoroutines();
        StartCoroutine(SpawnLoop());
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawCube(transform.position + areaCenter, areaSize);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + areaCenter, areaSize);
    }
}
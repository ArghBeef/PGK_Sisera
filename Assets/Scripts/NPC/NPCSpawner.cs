using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    [Header("NPC Prefabs")]
    [SerializeField] private GameObject[] npcPrefabs;

    [Header("Spawn Settings")]
    [SerializeField] private int spawnCount = 3;
    [SerializeField] private float spawnRadius = 5f;
    [SerializeField] private bool spawnOnStart = true;

    [Header("Optional")]
    [SerializeField] private bool parentSpawnedNPCsToSpawner = false;

    private void Start()
    {
        if (spawnOnStart)
            SpawnNPCs();
    }

    public void SpawnNPCs()
    {
        if (npcPrefabs == null || npcPrefabs.Length == 0)
        {
            return;
        }

        for (int i = 0; i < spawnCount; i++)
        {
            SpawnOneNPC();
        }
    }

    public GameObject SpawnOneNPC()
    {
        GameObject prefab = GetRandomPrefab();

        if (prefab == null)
            return null;

        Vector3 spawnPosition = GetRandomSpawnPosition();

        GameObject spawnedNPC = Instantiate(
            prefab,
            spawnPosition,
            Quaternion.Euler(0f, Random.Range(0f, 360f), 0f)
        );

        if (parentSpawnedNPCsToSpawner)
            spawnedNPC.transform.SetParent(transform);

        return spawnedNPC;
    }

    private GameObject GetRandomPrefab()
    {
        int index = Random.Range(0, npcPrefabs.Length);
        return npcPrefabs[index];
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;

        Vector3 position = transform.position;
        position.x += randomCircle.x;
        position.z += randomCircle.y;

        return position;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}
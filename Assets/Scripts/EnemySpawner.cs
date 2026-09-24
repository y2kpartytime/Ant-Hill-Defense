using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemySpawner : MonoBehaviour
{
    public int enemyCount = 4;
    public float spawnRate = 4f;
    private float spawnTimer;
    public Transform enemySpawnPoint;
    public GameObject enemyPrefab;
    public Tilemap existingTilemap;

    void Start()
    {
        spawnTimer = spawnRate;
    }

    void Update()
    {
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            SpawnEnemy();
            spawnTimer = spawnRate;
        }
    }

    public void SpawnEnemy()
    {
        if (enemyCount <= 0)
        {
            return;
        }
        GameObject enemy = Instantiate(enemyPrefab, enemySpawnPoint.position, enemySpawnPoint.rotation);
        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();

        if (enemyAI != null)
        {
            enemyAI.tilemap = existingTilemap;
        }

        enemyCount--;
    }
}
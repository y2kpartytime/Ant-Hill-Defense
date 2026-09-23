using UnityEngine;
using UnityEngine.Tilemaps;

public class UnitSpawner : MonoBehaviour
{
    public static UnitSpawner Instance;
    public GameObject workerPrefab;
    public GameObject soldierPrefab;
    public Transform spawnPoint;
    public Transform colonyReturnPoint;
    public Tilemap groundTilemap;

    void Awake()
    {
        Instance = this;
    }

    public void SpawnWorker()
    {
        if (FoodManager.Instance.food < 5)
            return;

        FoodManager.Instance.food -= 5;

        GameObject newUnit = Instantiate(
            workerPrefab,
            spawnPoint.position,
            Quaternion.identity
        );

        WorkerAnt ant = newUnit.GetComponent<WorkerAnt>();
        if (ant != null)
        {
            ant.colonyReturnPoint = colonyReturnPoint;
            ant.tilemap = groundTilemap;
        }

        FoodManager.Instance.AddUnit();
        FoodManager.Instance.UpdateUI();
    }

    public void SpawnSoldier()
    {
        if (FoodManager.Instance.food < 5)
            return;

        FoodManager.Instance.food -= 5;

        GameObject newUnit = Instantiate(
            soldierPrefab,
            spawnPoint.position,
            Quaternion.identity
        );
        FoodManager.Instance.AddUnit();
        FoodManager.Instance.UpdateUI();
    }
}
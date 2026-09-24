using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorkerAnt : MonoBehaviour
{
    public Tilemap tilemap;
    public float moveSpeed = 3f;
    private List<Vector3> path = new List<Vector3>();
    private int pathIndex = 0;
    public Transform colonyReturnPoint;
    private int carriedFood = 0;
    private FoodScript targetFood;
    private enum AntState
    {
        Idle,
        GoingToFood,
        ReturningHome,
        Digging,
        Attacking
    }

    private AntState state = AntState.Idle;

    private Vector3 foodPosition;

    void Start()
    {
        if (UnitSelections.Instance != null)
        {
            UnitSelections.Instance.unitList.Add(gameObject);
        }
    }

    void OnDestroy()
    {
        if (UnitSelections.Instance != null)
        {
            UnitSelections.Instance.unitList.Remove(gameObject);
            UnitSelections.Instance.unitsSelected.Remove(gameObject);
        }
    }

    void Update()
    {
        FollowPath();

        // Only ONE selected worker handles the mouse click.
        if (Input.GetMouseButtonDown(1) && IsPrimarySelectedWorker())
        {
            HandleRightClick();
        }
    }

    void MoveSelectedUnits(Vector3Int targetCell)
{
    if (UnitSelections.Instance == null)
        return;

    if (!IsWalkable(targetCell))
        return;

    int index = 0;

    foreach (GameObject unit in UnitSelections.Instance.unitsSelected)
    {
        if (unit == null)
            continue;

        WorkerAnt ant = unit.GetComponent<WorkerAnt>();

        if (ant != null && ant.tilemap != null)
        {
            Vector3Int offset = GetFormationOffset(index);

            Vector3Int antTarget = targetCell + offset;

            // If the offset position isn't walkable,
            // just use the original target.
            if (!ant.IsWalkable(antTarget))
            {
                antTarget = targetCell;
            }

            ant.MoveTo(antTarget);

            index++;
        }
    }
}

    public void MoveTo(Vector3Int targetCell, bool normalMove = true)
    {
        Vector3Int startCell =
            tilemap.WorldToCell(transform.position);

        List<Vector3Int> cellPath =
            FindPath(startCell, targetCell);

        if (cellPath == null)
        {
            Debug.Log("No path found!");
            return;
        }

        path.Clear();

        foreach (Vector3Int cell in cellPath)
        {
            path.Add(tilemap.GetCellCenterWorld(cell));
        }

        pathIndex = 0;

        if (normalMove)
        {
            state = AntState.Idle;
        }
    }

    void FollowPath()
    {
        if (pathIndex >= path.Count)
        {
            // REACHED FOOD
            if (state == AntState.GoingToFood)
            {
                if (targetFood == null)
                {
                    state = AntState.Idle;
                    path.Clear();
                    return;
                }

                CollectFood();

                if (state == AntState.ReturningHome)
                {
                    return;
                }

                // Food still exists, so return home.
                if (targetFood != null)
                {
                    ReturnHome();
                }

                return;
            }

            // REACHED COLONY
            
            else if (state == AntState.ReturningHome)
            {
                DeliverFood();

                // Go back for another piece if food still exists.
                if (targetFood != null)
                {
                    GatherFood(targetFood);
                }
                else
                {
                    state = AntState.Idle;
                    path.Clear();
                }

                return;
            }

            return;
        }

        Vector3 target = path[pathIndex];

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            transform.position = target;
            pathIndex++;
        }
    }

    bool IsWalkable(Vector3Int cell)
    {
        if (tilemap == null)
            return false;

        if (!tilemap.cellBounds.Contains(cell))
            return false;

        // Can't walk through a solid tile
        if (tilemap.HasTile(cell))
            return false;

        // Check for nearby solid ground
        int groundDistance = 2;

        for (int x = -groundDistance; x <= groundDistance; x++)
        {
            for (int y = -groundDistance; y <= groundDistance; y++)
            {
                // Only check tiles below or beside the ant
                if (y > 0)
                    continue;

                Vector3Int nearbyCell = cell + new Vector3Int(x, y, 0);

                if (tilemap.HasTile(nearbyCell))
                    return true;
            }
        }

        return false;
    }

    List<Vector3Int> FindPath(
        Vector3Int start,
        Vector3Int target)
    {
        if (!IsWalkable(target))
            return null;

        List<Vector3Int> open = new List<Vector3Int>();
        HashSet<Vector3Int> closed = new HashSet<Vector3Int>();

        Dictionary<Vector3Int, Vector3Int> cameFrom =
            new Dictionary<Vector3Int, Vector3Int>();

        Dictionary<Vector3Int, int> gScore =
            new Dictionary<Vector3Int, int>();

        Dictionary<Vector3Int, int> fScore =
            new Dictionary<Vector3Int, int>();

        open.Add(start);
        gScore[start] = 0;
        fScore[start] = Heuristic(start, target);

        while (open.Count > 0)
        {
            Vector3Int current = GetLowestFScore(open, fScore);

            if (current == target)
            {
                return ReconstructPath(
                    cameFrom,
                    current
                );
            }

            open.Remove(current);
            closed.Add(current);

            foreach (Vector3Int neighbour in GetNeighbours(current))
            {
                if (closed.Contains(neighbour))
                    continue;

                if (!IsWalkable(neighbour))
                    continue;

                int newGScore =
                    gScore[current] + 1;

                if (!gScore.ContainsKey(neighbour) ||
                    newGScore < gScore[neighbour])
                {
                    cameFrom[neighbour] = current;
                    gScore[neighbour] = newGScore;

                    fScore[neighbour] =
                        newGScore +
                        Heuristic(neighbour, target);

                    if (!open.Contains(neighbour))
                    {
                        open.Add(neighbour);
                    }
                }
            }
        }

        return null;
    }

    int Heuristic(Vector3Int a, Vector3Int b)
    {
        return Mathf.Abs(a.x - b.x) +
               Mathf.Abs(a.y - b.y);
    }

    Vector3Int GetLowestFScore(
        List<Vector3Int> open,
        Dictionary<Vector3Int, int> fScore)
    {
        Vector3Int best = open[0];

        for (int i = 1; i < open.Count; i++)
        {
            if (fScore[open[i]] < fScore[best])
            {
                best = open[i];
            }
        }

        return best;
    }

    List<Vector3Int> ReconstructPath(
        Dictionary<Vector3Int, Vector3Int> cameFrom,
        Vector3Int current)
    {
        List<Vector3Int> result =
            new List<Vector3Int>();

        result.Add(current);

        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            result.Add(current);
        }

        result.Reverse();

        return result;
    }

    List<Vector3Int> GetNeighbours(Vector3Int cell)
    {
        return new List<Vector3Int>
        {
            cell + Vector3Int.up,
            cell + Vector3Int.down,
            cell + Vector3Int.left,
            cell + Vector3Int.right
        };
    }



    public static void SelectFood(FoodScript food)
{
    if (UnitSelections.Instance == null)
        return;

    foreach (GameObject unit in UnitSelections.Instance.unitsSelected)
    {
        if (unit == null)
            continue;

        WorkerAnt ant = unit.GetComponent<WorkerAnt>();

        if (ant != null)
        {
            ant.GatherFood(food);
        }
    }
}

    void GatherFood(FoodScript food)
    {
        if (food == null)
        {
            state = AntState.Idle;
            return;
        }

        targetFood = food;
        foodPosition = food.transform.position;

        Vector3Int targetCell =
            tilemap.WorldToCell(food.transform.position);

        MoveTo(targetCell, false);
        state = AntState.GoingToFood;
    }

    void ReturnHome()
    {
        if (colonyReturnPoint == null)
        {
            state = AntState.Idle;
            return;
        }

        Vector3Int homeCell =
            tilemap.WorldToCell(colonyReturnPoint.position);

        MoveTo(homeCell, false);

        state = AntState.ReturningHome;
    }

    void DeliverFood()
    {
        if (carriedFood <= 0)
            return;

        FoodManager.Instance.AddFood(carriedFood);

        Debug.Log("Ant delivered " + carriedFood + " food.");

        carriedFood = 0;
    }

    void CollectFood()
    {
        if (targetFood == null)
        {
            state = AntState.Idle;
            path.Clear();
            return;
        }

        bool collected = targetFood.TakeFood(1);

        if (!collected)
        {
            targetFood = null;
            state = AntState.Idle;
            path.Clear();
            return;
        }

        carriedFood = 1;

        Debug.Log("Ant collected 1 food.");

        // Food has run out.
        if (targetFood.foodAmount <= 0)
        {
            targetFood = null;

            // IMPORTANT:
            // The ant is carrying food, so it must still go home.
            ReturnHome();
        }
    }

    void HandleRightClick()
    {
        if (tilemap == null || Camera.main == null)
            return;

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(
            Input.mousePosition
        );

        mousePosition.z = 0f;
        Collider2D hit = Physics2D.OverlapPoint(mousePosition);

        if (hit != null)
        {
            EnemyAI enemy = hit.GetComponentInParent<EnemyAI>();

            if (enemy != null)
            {
                AttackSelectedEnemy(enemy.gameObject);
                return;
            }

            FoodScript food = hit.GetComponentInParent<FoodScript>();

            if (food != null)
            {
                SelectFood(food);
                return;
            }
        }

        // --------------------------------
        // 1. FOOD
        // --------------------------------

        if (hit != null)
        {
            FoodScript food = hit.GetComponentInParent<FoodScript>();

            if (food != null)
            {
                SelectFood(food);
                return;
            }
        }

        // --------------------------------
        // 2. TILE
        // --------------------------------

        Vector3Int clickedCell =
            tilemap.WorldToCell(mousePosition);

        if (tilemap.HasTile(clickedCell))
        {
            // Only dig if the tile is directly
            // next to a selected worker.
            TryDigSelectedWorker(clickedCell);

            return;
        }

        // --------------------------------
        // 3. EMPTY GROUND
        // --------------------------------

        MoveSelectedUnits(clickedCell);
    }
    bool IsPrimarySelectedWorker()
    {
        if (UnitSelections.Instance == null)
            return false;

        if (UnitSelections.Instance.unitsSelected == null)
            return false;

        if (UnitSelections.Instance.unitsSelected.Count == 0)
            return false;

        GameObject firstSelected =
            UnitSelections.Instance.unitsSelected[0];

        if (firstSelected == null)
            return false;

        WorkerAnt worker =
            firstSelected.GetComponent<WorkerAnt>();

        if (worker == null)
            return false;

        return firstSelected == gameObject;
    }

    void TryDigSelectedWorker(Vector3Int clickedCell)
    {
        if (UnitSelections.Instance == null)
            return;

        foreach (GameObject unit in UnitSelections.Instance.unitsSelected)
        {
            if (unit == null)
                continue;

            WorkerAnt ant = unit.GetComponent<WorkerAnt>();

            if (ant == null)
                continue;

            Vector3Int antCell =
                ant.tilemap.WorldToCell(ant.transform.position);

            int distance =
                Mathf.Abs(antCell.x - clickedCell.x) +
                Mathf.Abs(antCell.y - clickedCell.y);

            // Must be directly next to the ant.
            if (distance == 1)
            {
                tilemap.SetTile(clickedCell, null);

                Debug.Log("Worker dug tile: " + clickedCell);

                return;
            }
        }

        // Tile wasn't next to any selected worker.
        Debug.Log("Tile is too far away to dig.");
    }

    Vector3Int GetFormationOffset(int index)
    {
        switch (index)
        {
            case 0: return new Vector3Int(0, 0, 0);
            case 1: return new Vector3Int(1, 0, 0);
            case 2: return new Vector3Int(-1, 0, 0);
            case 3: return new Vector3Int(0, 1, 0);
            case 4: return new Vector3Int(0, -1, 0);
            case 5: return new Vector3Int(1, 1, 0);
            case 6: return new Vector3Int(-1, 1, 0);
            case 7: return new Vector3Int(1, -1, 0);
            case 8: return new Vector3Int(-1, -1, 0);

            default:
                return new Vector3Int(
                    (index % 3) - 1,
                    (index / 3) - 1,
                    0
                );
        }
    }

    void AttackSelectedEnemy(GameObject enemy)
    {
        if (UnitSelections.Instance == null)
            return;

        foreach (GameObject unit in UnitSelections.Instance.unitsSelected)
        {
            if (unit == null)
                continue;

            WorkerAnt ant = unit.GetComponent<WorkerAnt>();

            if (ant != null)
            {
                AttackScript attack = ant.GetComponent<AttackScript>();

                if (attack == null)
                    attack = ant.gameObject.AddComponent<AttackScript>();

                attack.SetTarget(enemy);

                // Move the ant toward the enemy
                Vector3Int enemyCell = ant.tilemap.WorldToCell(enemy.transform.position);
                ant.MoveTo(enemyCell);
            }
        }
    }
}
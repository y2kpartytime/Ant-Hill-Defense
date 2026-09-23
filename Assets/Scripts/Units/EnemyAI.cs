using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyAI : MonoBehaviour
{
    public Tilemap tilemap;
    public float moveSpeed = 2f;

    [Header("Combat")]
    public int attackDamage = 1;
    public float attackRange = 0.8f;
    public float attackCooldown = 1f;
    

    [Header("Target Selection")]
    [Range(0f, 1f)]
    public float attackAntChance = 0.33f;

    [Range(0f, 1f)]
    public float attackQueenChance = 0.33f;

    [Range(0f, 1f)]
    public float attackSpawnChance = 0.34f;

    private enum EnemyState
    {
        Searching,
        Moving,
        Attacking
    }

    private enum TargetType
    {
        Ant,
        Queen,
        Spawn
    }

    private EnemyState state = EnemyState.Searching;
    private TargetType targetType;

    private GameObject currentTarget;

    private List<Vector3> path = new List<Vector3>();
    private int pathIndex = 0;

    private float attackTimer = 0f;

    void Update()
    {
        attackTimer -= Time.deltaTime;

        // No target -> find one
        if (currentTarget == null)
        {
            ChooseNewTarget();
            return;
        }

        // Target disappeared
        if (currentTarget == null)
        {
            state = EnemyState.Searching;
            return;
        }

        float distance = Vector3.Distance(
            transform.position,
            currentTarget.transform.position
        );

        // Close enough to attack
        if (distance <= attackRange)
        {
            state = EnemyState.Attacking;
            AttackTarget();
            return;
        }

        // Otherwise move toward target
        if (state != EnemyState.Moving)
        {
            MoveTowardsTarget();
        }

        FollowPath();
    }

    // =========================================================
    // TARGET SELECTION
    // =========================================================

    void ChooseNewTarget()
    {
        state = EnemyState.Searching;

        float random = Random.value;

        if (random < attackAntChance)
        {
            targetType = TargetType.Ant;
        }
        else if (random < attackAntChance + attackQueenChance)
        {
            targetType = TargetType.Queen;
        }
        else
        {
            targetType = TargetType.Spawn;
        }

        currentTarget = FindTarget();

        if (currentTarget != null)
        {
            attackPositionIndex = Random.Range(0, 8);
            MoveTowardsTarget();
        }
    }

    GameObject FindTarget()
    {
        switch (targetType)
        {
            case TargetType.Ant:
                return FindClosestAnt();

            case TargetType.Queen:
                return FindQueen();

            case TargetType.Spawn:
                return FindClosestSpawn();
        }

        return null;
    }

    // =========================================================
    // ANT TARGET
    // =========================================================

    GameObject FindClosestAnt()
    {
        GameObject closest = null;
        float closestDistance = Mathf.Infinity;

        // Uses your existing UnitSelections list.
        if (UnitSelections.Instance == null)
            return null;

        foreach (GameObject unit in UnitSelections.Instance.unitList)
        {
            if (unit == null)
                continue;

            float distance =
                Vector3.Distance(transform.position, unit.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = unit;
            }
        }

        return closest;
    }

    // =========================================================
    // QUEEN TARGET
    // =========================================================

    GameObject FindQueen()
    {
        // For now, find the object tagged "Queen".
        GameObject queen = GameObject.FindGameObjectWithTag("Queen");

        return queen;
    }

    // =========================================================
    // SPAWN TARGET
    // =========================================================

    GameObject FindClosestSpawn()
    {
        GameObject[] spawns =
            GameObject.FindGameObjectsWithTag("Target");

        GameObject closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject spawn in spawns)
        {
            if (spawn == null)
                continue;

            float distance =
                Vector3.Distance(transform.position, spawn.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = spawn;
            }
        }

        return closest;
    }

    // =========================================================
    // PATHFINDING
    // =========================================================

    void MoveTowardsTarget()
    {
        if (currentTarget == null || tilemap == null)
            return;

        Vector3Int startCell =
            tilemap.WorldToCell(transform.position);

        Vector3Int targetCell =
        tilemap.WorldToCell(currentTarget.transform.position);

        targetCell += GetAttackOffset(attackPositionIndex);

        List<Vector3Int> cellPath =
            FindPath(startCell, targetCell);

        if (cellPath == null)
        {
            // Couldn't reach target.
            currentTarget = null;
            state = EnemyState.Searching;
            return;
        }

        path.Clear();

        foreach (Vector3Int cell in cellPath)
        {
            path.Add(
                tilemap.GetCellCenterWorld(cell)
            );
        }

        pathIndex = 0;
        state = EnemyState.Moving;
    }

    void FollowPath()
    {
        if (pathIndex >= path.Count)
        {
            state = EnemyState.Searching;
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

    // =========================================================
    // A*
    // =========================================================

    List<Vector3Int> FindPath(
        Vector3Int start,
        Vector3Int target)
    {
        if (!IsWalkable(target))
            return null;

        List<Vector3Int> open =
            new List<Vector3Int>();

        HashSet<Vector3Int> closed =
            new HashSet<Vector3Int>();

        Dictionary<Vector3Int, Vector3Int> cameFrom =
            new Dictionary<Vector3Int, Vector3Int>();

        Dictionary<Vector3Int, int> gScore =
            new Dictionary<Vector3Int, int>();

        Dictionary<Vector3Int, int> fScore =
            new Dictionary<Vector3Int, int>();

        open.Add(start);

        gScore[start] = 0;

        fScore[start] =
            Heuristic(start, target);

        while (open.Count > 0)
        {
            Vector3Int current =
                GetLowestFScore(open, fScore);

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

    bool IsWalkable(Vector3Int cell)
    {
        if (tilemap == null)
            return false;

        if (!tilemap.cellBounds.Contains(cell))
            return false;

        if (tilemap.HasTile(cell))
            return false;

        int groundDistance = 2;

        for (int x = -groundDistance; x <= groundDistance; x++)
        {
            for (int y = -groundDistance; y <= groundDistance; y++)
            {
                if (y > 0)
                    continue;

                Vector3Int nearbyCell =
                    cell + new Vector3Int(x, y, 0);

                if (tilemap.HasTile(nearbyCell))
                    return true;
            }
        }

        return false;
    }

    int Heuristic(
        Vector3Int a,
        Vector3Int b)
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

    List<Vector3Int> GetNeighbours(
        Vector3Int cell)
    {
        return new List<Vector3Int>
        {
            cell + Vector3Int.up,
            cell + Vector3Int.down,
            cell + Vector3Int.left,
            cell + Vector3Int.right
        };
    }

    // =========================================================
    // COMBAT
    // =========================================================

    void AttackTarget()
    {
        if (currentTarget == null)
            return;

        if (attackTimer > 0f)
            return;

        attackTimer = attackCooldown;

        Health targetHealth =
            currentTarget.GetComponent<Health>();

        if (targetHealth != null)
        {
            targetHealth.TakeDamage(attackDamage);

            Debug.Log(
                gameObject.name +
                " attacked " +
                currentTarget.name
            );
        }
        else
        {
            Debug.LogWarning(
                currentTarget.name +
                " does not have a Health component!"
            );
        }
    }

    private int attackPositionIndex;

    Vector3Int GetAttackOffset(int index)
{
    switch (index % 8)
    {
        case 0: return new Vector3Int(1, 0, 0);
        case 1: return new Vector3Int(-1, 0, 0);
        case 2: return new Vector3Int(0, 1, 0);
        case 3: return new Vector3Int(0, -1, 0);
        case 4: return new Vector3Int(1, 1, 0);
        case 5: return new Vector3Int(-1, 1, 0);
        case 6: return new Vector3Int(1, -1, 0);
        case 7: return new Vector3Int(-1, -1, 0);
    }

    return Vector3Int.zero;
}
}
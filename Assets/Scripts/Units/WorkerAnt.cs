using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorkerAnt : MonoBehaviour
{
    public Tilemap tilemap;
    public float moveSpeed = 3f;

    private List<Vector3> path = new List<Vector3>();
    private int pathIndex = 0;

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

        if (Input.GetMouseButtonDown(1))
        {
            MoveSelectedUnits();
        }

        if (Input.GetKeyDown(KeyCode.Space) && IsSelected())
        {
            Dig();
        }
    }

    void MoveSelectedUnits()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(
            Input.mousePosition
        );

        mousePosition.z = 0;

        Vector3Int targetCell = tilemap.WorldToCell(mousePosition);

        // Don't try to walk into a wall
        if (tilemap.GetTile(targetCell) != null)
            return;

        foreach (GameObject unit in UnitSelections.Instance.unitsSelected)
        {
            if (unit == null)
                continue;

            WorkerAnt ant = unit.GetComponent<WorkerAnt>();

            if (ant != null)
            {
                ant.MoveTo(targetCell);
            }
        }
    }

    public void MoveTo(Vector3Int targetCell)
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
    }

    void FollowPath()
    {
        if (pathIndex >= path.Count)
            return;

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
        // Empty tile = walkable
        return tilemap.GetTile(cell) == null;
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

    void Dig()
    {
        if (tilemap == null)
            return;

        Vector3Int npcCell =
            tilemap.WorldToCell(transform.position);

        Vector3Int blockCell =
            npcCell + Vector3Int.right;

        if (tilemap.GetTile(blockCell) != null)
        {
            tilemap.SetTile(blockCell, null);
        }
    }

    // Selection helper
    bool IsSelected()
    {
        return UnitSelections.Instance != null &&
               UnitSelections.Instance.unitsSelected.Contains(gameObject);
    }
}
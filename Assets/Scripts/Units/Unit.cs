using UnityEngine;
using UnityEngine.Tilemaps;

public class Unit : MonoBehaviour
{
    public Tilemap tilemap;

    void Start()
    {
        UnitSelections.Instance.unitList.Add(this.gameObject);
    }

    void OnDestroy()
    {
        UnitSelections.Instance.unitList.Remove(this.gameObject);
    }


    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Vector3Int cellPosition = tilemap.WorldToCell(mouseWorldPos);

            tilemap.SetTile(cellPosition, null);
        }
    }
}

using UnityEngine;
using System.Collections.Generic;

public class UnitSelections : MonoBehaviour
{
    public List<GameObject> unitList = new List<GameObject>();
    public List<GameObject> unitsSelected = new List<GameObject>();

    private static UnitSelections _instance;

    public static UnitSelections Instance
    {
        get { return _instance; }
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
    }

    public void ClickSelect(GameObject unitToAdd)
    {
        DeselectAll();

        unitsSelected.Add(unitToAdd);

        unitToAdd.transform.GetChild(0).gameObject.SetActive(true);
    }

    public void ShiftClickSelect(GameObject unitToAdd)
    {
        if (!unitsSelected.Contains(unitToAdd))
        {
            unitsSelected.Add(unitToAdd);
            unitToAdd.transform.GetChild(0).gameObject.SetActive(true);
        }
        else
        {
            unitsSelected.Remove(unitToAdd);
            unitToAdd.transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    public void DragSelect(GameObject unitToAdd)
    {
        if (!unitsSelected.Contains(unitToAdd))
        {
            unitsSelected.Add(unitToAdd);
            unitToAdd.transform.GetChild(0).gameObject.SetActive(true);
        }
    }

    public void DeselectAll()
    {
        foreach (GameObject unit in unitsSelected)
        {
            if (unit != null)
                unit.transform.GetChild(0).gameObject.SetActive(false);
        }

        unitsSelected.Clear();
    }

    public void Deselect(GameObject unitToDeselect)
    {
        if (unitsSelected.Contains(unitToDeselect))
        {
            unitsSelected.Remove(unitToDeselect);

            unitToDeselect.transform.GetChild(0).gameObject.SetActive(false);
        }
    }
}
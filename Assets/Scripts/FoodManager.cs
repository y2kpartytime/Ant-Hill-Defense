using UnityEngine;
using TMPro;

public class FoodManager : MonoBehaviour
{
    public static FoodManager Instance;
    public int food = 0;
    public TMP_Text foodText;
    public TMP_Text unitCountText;

    void Awake()
    {
        Instance = this;
        UpdateUI();
    }

    public void AddFood(int amount)
    {
        food += amount;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (foodText != null)
        {
            foodText.text = "" + food;
        }

        if (unitCountText != null)
        {
            unitCountText.text = "" + UnitSelections.Instance.unitsSelected.Count;
        }
    }
}
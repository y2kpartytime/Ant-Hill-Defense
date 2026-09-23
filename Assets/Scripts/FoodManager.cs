using UnityEngine;
using TMPro;

public class FoodManager : MonoBehaviour
{
    public static FoodManager Instance;
    public int food = 0;
    public int unitCount = 0;
    public TMP_Text foodText;
    public TMP_Text unitCountText;
    public FoodScript foodScript;

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

    public void RemoveFood(int amount)
    {
        foodScript.foodAmount -= amount;
    }

    public void AddUnit()
    {
        unitCount++;
        UpdateUI();
    }

    public void RemoveUnit()
    {
        unitCount--;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (foodText != null)
            foodText.text = "" + food;

        if (unitCountText != null)
            unitCountText.text = "" + unitCount;
    }
}
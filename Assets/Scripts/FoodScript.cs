using UnityEngine;

public class FoodScript : MonoBehaviour
{
    public int foodAmount = 20;

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            WorkerAnt.SelectFood(this);
            Debug.Log("Food clicked");
        }
    }

    public bool TakeFood(int amount)
    {
        if (foodAmount < amount)
            return false;

        foodAmount -= amount;

        if (foodAmount <= 0)
        {
            foodAmount = 0;
            Destroy(gameObject);
        }

        return true;
    }
}
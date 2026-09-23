using UnityEngine;

public class FoodScript : MonoBehaviour
{
    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            WorkerAnt.SelectFood(transform.position);
            Debug.Log("Food clicked");
        }
    }


}
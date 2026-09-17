using UnityEngine;

public class GameManager : MonoBehaviour
{
    void Update()
    {
        Quit();
    }

    void OnClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Mouse Clicked");
            
        }
    }

    void Quit()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Quit");
            Application.Quit();
        }
    }
}
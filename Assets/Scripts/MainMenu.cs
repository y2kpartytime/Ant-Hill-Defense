using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject controlsPanel;
    public GameObject infoPanel;

    public void StartGame ()
    {
        SceneManager.LoadScene("Main Level Scene");
    }

    public void OpenControls()
    {
        mainMenuPanel.SetActive(false);
        controlsPanel.SetActive(true);
    }

    public void OpenInfo()
    {
        mainMenuPanel.SetActive(false);
        infoPanel.SetActive(true);
    }

    public void BackToMenu()
    {
        controlsPanel.SetActive(false);
        infoPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

}

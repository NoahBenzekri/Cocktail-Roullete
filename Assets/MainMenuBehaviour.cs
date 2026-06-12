using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenuBehaviour : MonoBehaviour
{
    public string startGameSceneName = "Gameplay";

    public void OnStartGame()
    {
        SceneManager.LoadScene(startGameSceneName);
    }

    public void OnQuitGame()
    {
        Application.Quit();
    }
}

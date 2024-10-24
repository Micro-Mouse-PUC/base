using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader: MonoBehaviour
{
    // This method will load the specified scene by name
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // This method will quit the game (useful for Exit button)
    public void ExitGame()
    {
        Application.Quit();
    }
}

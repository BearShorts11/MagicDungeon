using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Just for menu interactions
/// </summary>
public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

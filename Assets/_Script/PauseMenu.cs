using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private Canvas pauseCanvas;
    [SerializeField] private InputAction pauseKey;

    private bool isPaused = false;

    private void Start()
    {
        pauseCanvas.enabled = false;
    }

    private void OnEnable()
    {
        pauseKey.Enable();
        pauseKey.performed += TogglePause;
    }

    private void OnDisable()
    {
        pauseKey.Disable();
        pauseKey.performed -= TogglePause;
    }

    /// <summary>
    ///Checks if the game is paused and either pauses or unpauses.
    /// </summary>
    /// <param name="ctx"></param>
    private void TogglePause(InputAction.CallbackContext ctx)
    {
        // Input actions don't allow for overloads unless using a delegate, so I just decided to make an individual method that does the checking itself, since it's only one variable we're tracking.

        if (isPaused) 
        {
            UnpauseGame(); 
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        pauseCanvas.enabled = true;

        Time.timeScale = 0f;   //freeze game
    }

    public void UnpauseGame()
    {
        isPaused = false;
        pauseCanvas.enabled = false;

        Time.timeScale = 1f;   //resuming game
    }

    //Specifically for button!
    public void RestartLevel()
    {
        SceneManager.LoadScene( SceneManager.GetActiveScene().buildIndex );
    }

    public void GoMainMenu()
    {
        SceneManager.LoadScene(0); //Main menu should always be index 0
    }
}
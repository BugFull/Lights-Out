using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Manages the menu canvases according to the Game State
/// </summary>
public class MenuManager : MonoBehaviour
{
    #region Fields

    [SerializeField] private GameEvent menuEvent;
    [SerializeField] private GameEvent gameStateChangeEvent;
    [SerializeField] private GameEvent newGameEvent;
    [SerializeField] private InputActionReference backInput;
    [SerializeField] private CountDownTimer waitMenuTime;

    [SerializeField] private Canvas mainMenu;
    [SerializeField] private Canvas pauseMenu;
    [SerializeField] private Canvas settingsMenu;
    [SerializeField] private Canvas waitMenu;
    [SerializeField] private Canvas endGameMenu;

    private bool isFromMainMenu;
    #endregion

    #region Unity Methods

    private void Awake()
    {
        gameStateChangeEvent.ActionEvent += EnableMenu;
        waitMenuTime.Done += ResumePlay;
    }

    private void OnDestroy()
    {
        gameStateChangeEvent.ActionEvent -= EnableMenu;
        waitMenuTime.Done -= ResumePlay;
    }
    #endregion

    #region Button Responses
    
    /// <summary>
    /// Resumes by raising event to change state to Playing
    /// </summary>
    private void ResumePlay()
    {
        menuEvent.ActionEvent?.Invoke(this, GameStates.Playing);
    }

    /// <summary>
    /// Raise Event To Change Game State to Playing
    /// </summary>
    public void ToPlay()
    {
        menuEvent.ActionEvent?.Invoke(this, GameStates.Playing);
        newGameEvent.ActionEvent?.Invoke(this, null);
    }

    /// <summary>
    /// Raise event To change game State to Wait
    /// </summary>
    public void Towait()
    {
        menuEvent.ActionEvent?.Invoke(this, GameStates.Wait);
    }

    /// <summary>
    /// Raise event to change game State to Settings
    /// </summary>
    /// <param name="fromMainMenu"> Is it from Main Menu?</param>
    public void ToSettings()
    {
        menuEvent.ActionEvent?.Invoke(this, GameStates.Settings);
    }

    /// <summary>
    /// Raise event to change game state to MainMenu
    /// </summary>
    public void ToMenu()
    {
        if (isFromMainMenu)
        {
            menuEvent.ActionEvent?.Invoke(this, GameStates.MainMenu);
        }
        else
        {
            menuEvent.ActionEvent?.Invoke(this, GameStates.PauseMenu);
        }
    }
    #endregion

    #region Event Handlers
    /// <summary>
    /// Event Handler to manage the menus with respect to change state event
    /// </summary>
    /// <param name="sender">Event From</param>
    /// <param name="data">Current Game State</param>
    private void EnableMenu(Component sender, object data)
    {
        if (sender is GameManager && data is GameStates gameState)
        {
            switch (gameState)
            {
                case GameStates.MainMenu:
                    isFromMainMenu = true;
                    pauseMenu.enabled = false;
                    settingsMenu.enabled = false;
                    mainMenu.enabled = true;
                    waitMenu.enabled = false;
                    endGameMenu.enabled = false;
                    backInput.action.performed -= Resume;
                    backInput.action.performed -= BackToMenu;
                    break;
                case GameStates.PauseMenu:
                    isFromMainMenu = false;
                    settingsMenu.enabled = false;
                    mainMenu.enabled = false;
                    pauseMenu.enabled = true;
                    waitMenu.enabled = false;
                    endGameMenu.enabled = false;
                    backInput.action.performed += Resume;
                    backInput.action.performed -= BackToMenu;
                    break;
                case GameStates.Settings:
                    mainMenu.enabled = false;
                    pauseMenu.enabled = false;
                    settingsMenu.enabled = true;
                    waitMenu.enabled = false;
                    endGameMenu.enabled = false;
                    backInput.action.performed -= Resume;
                    backInput.action.performed += BackToMenu;
                    break;
                case GameStates.Wait:
                    mainMenu.enabled = false;
                    pauseMenu.enabled = false;
                    settingsMenu.enabled = false;
                    waitMenu.enabled = true;
                    endGameMenu.enabled = false;
                    waitMenuTime.Begin();
                    backInput.action.performed -= Resume;
                    backInput.action.performed -= BackToMenu;
                    break;
                case GameStates.Playing:
                    mainMenu.enabled = false;
                    settingsMenu.enabled = false;
                    pauseMenu.enabled = false;
                    waitMenu.enabled = false;
                    endGameMenu.enabled = false;
                    backInput.action.performed -= Resume;
                    backInput.action.performed -= BackToMenu;
                    break;
                case GameStates.Finish:
                    mainMenu.enabled = false;
                    settingsMenu.enabled = false;
                    pauseMenu.enabled = false;
                    waitMenu.enabled = false;
                    endGameMenu.enabled = true;
                    backInput.action.performed -= Resume;
                    backInput.action.performed -= BackToMenu;
                    break;
                default:
                    break;
            }
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    #endregion

    #region Input Handlers
    /// <summary>
    /// Input handler to go back to menu
    /// </summary>
    /// <param name="context">Input</param>
    private void BackToMenu(InputAction.CallbackContext context)
    {
        ToMenu();
    }

    /// <summary>
    /// Input handler to go back to game
    /// </summary>
    /// <param name="context">Input</param>
    private void Resume(InputAction.CallbackContext context)
    {
        Towait();
    }
    #endregion

}

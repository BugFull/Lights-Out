using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Represents the different game states.
/// </summary>
public enum GameStates
{
    MainMenu,
    Settings,
    PauseMenu,
    Wait,
    Playing,
    Finish
}

/// <summary>
/// Manages the game flow and state.
/// </summary>
public class GameManager : MonoBehaviour
{
    #region Fields

    [SerializeField] private GameEvent gameStateChange;
    [SerializeField] private GameEvent menuRequestEvent;
    [SerializeField] private GameEvent gameEndEvent;
    [SerializeField] private InputActionReference pause;
    [SerializeField] private InputActionReference click;
    [SerializeField] private GameStates currentGameState;
    public GameStates CurrentGameState
    {
        get { return currentGameState; }
        set
        {
            currentGameState = value;
            gameStateChange.ActionEvent?.Invoke(this, currentGameState);
        }
    }
    #endregion

    #region Unity Methods

    private void Awake()
    {
        menuRequestEvent.ActionEvent += MenuRequest;
        gameEndEvent.ActionEvent += FinishCondition;
    }

    private void Start()
    {
        CurrentGameState = GameStates.MainMenu;
    }

    private void Update()
    {
        if(Keyboard.current.enterKey.wasPressedThisFrame)
        {
            Debug.Log(currentGameState.ToString());
        }
    }

    private void OnDestroy()
    {
        menuRequestEvent.ActionEvent -= MenuRequest;
        gameEndEvent.ActionEvent -= FinishCondition;
    }
    #endregion

    #region Input Handlers

    /// <summary>
    /// Handles the Pause action.
    /// </summary>
    /// <param name="context">The callback context.</param>
    private void ToPause(InputAction.CallbackContext context)
    {
        CurrentGameState = GameStates.PauseMenu;
    }

    /// <summary>
    /// Handles click to go to main menu
    /// </summary>
    /// <param name="context"></param>
    private void ToMainMenu(InputAction.CallbackContext context)
    {
        CurrentGameState = GameStates.MainMenu;
        click.action.performed -= ToMainMenu;
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Handles menu requests.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="data">The data.</param>
    private void MenuRequest(Component sender, object data)
    {
        if (sender is MenuManager menuManager && data is GameStates gameState)  // Remove menuManager if not needed in future
        {
            CurrentGameState = gameState;
            if (gameState == GameStates.Playing)
            {
                pause.action.performed += ToPause;
            }
            else
            {
                pause.action.performed -= ToPause;
            }

        }
    }

    /// <summary>
    /// Event Handler when game finishes (win or lose)
    /// </summary>
    /// <param name="sender">Whom sent the request</param>
    /// <param name="data">Win = true, Lose = False</param>
    private void FinishCondition(Component sender, object data)
    {
        if (sender is PlayerLOS && data is bool)
        {
            RaiseFinish();
        }
    }

    #endregion

    #region Utilities and Other Methods

    /// <summary>
    /// Changes State To Finish and subscribe to click to main menu
    /// </summary>
    private void RaiseFinish()
    {
        CurrentGameState = GameStates.Finish;
        pause.action.performed -= ToPause;
        click.action.performed += ToMainMenu;
    }
    #endregion

}

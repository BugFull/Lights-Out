using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class PlayerMovement : MonoBehaviour
{
    #region Fields

    [SerializeField] private float speed = 5f;
    [SerializeField] private AnimationCurve movementCurve; 
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private AudioClip[] footstepSounds;

    [SerializeField] private GameEvent gameStateChangeEvent;
    [SerializeField] private GameEvent onNewGameEvent;
    [SerializeField] private InputActionReference MovementActionRef;
    [SerializeField] private GameObjectVariable player;
    [SerializeField] private LayerMask walls;
    [SerializeField] private Camera mainCam;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip flashLight;
    [SerializeField] private Light2D Light;
     
    private Vector2 movementDirections;
    private Vector3 initialPosition;
    #endregion

    #region Unity Methods

    private void Awake()
    {
        gameStateChangeEvent.ActionEvent += EnableMovement;
        onNewGameEvent.ActionEvent += OnNewGame;
        player.Value = this.gameObject;
    }
    private void Start()
    {
        initialPosition = transform.position;
    }

    private void Update()
    {
        RotateTowardsCursor();
        Movement();
    }

    private void OnEnable()
    {
        MovementActionRef.action.performed += OnMove;
        MovementActionRef.action.canceled += OnMove;
    }

    private void OnDisable()
    {
        MovementActionRef.action.performed -= OnMove;
        MovementActionRef.action.canceled -= OnMove;
    }

    private void OnDestroy()
    {
        gameStateChangeEvent.ActionEvent -= EnableMovement;
        onNewGameEvent.ActionEvent -= OnNewGame;
    }

    #endregion

    #region Input handlers
    private void OnMove(InputAction.CallbackContext context)
    {
        movementDirections = context.ReadValue<Vector2>();
        if (movementDirections != Vector2.zero && !IsInvoking(nameof(PlayWalkingSound)))
        {
            InvokeRepeating(nameof(PlayWalkingSound), 0f, 0.5f);
        }
        // If player is not moving and sound is playing, stop it
        else if (movementDirections == Vector2.zero && IsInvoking(nameof(PlayWalkingSound)))
        {
            CancelInvoke(nameof(PlayWalkingSound));
        }
    }
    #endregion

    #region Methods

    /// <summary>
    /// Rotate player towards the Cursor
    /// </summary>
    private void RotateTowardsCursor()
    {
        Vector3 mousePos = mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 direction = mousePos - transform.position;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Moves player and avoid walls
    /// </summary>
    private void Movement()
    {
        Vector3 tempVect = new(movementDirections.x, movementDirections.y, 0);
        float easedTime = movementCurve.Evaluate(Time.time);
        tempVect = speed * easedTime * tempVect;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, tempVect, 0.2f, walls);
        if (!hit.collider)
        {
            // If the ray does not hit a wall, move the character
            this.transform.position += tempVect * Time.deltaTime;
        }
    }

    /// <summary>
    /// Plays walk sound
    /// </summary>
    private void PlayWalkingSound()
    {
        AudioClip footstepSound = footstepSounds[Random.Range(0, footstepSounds.Length)];
        audioSource.PlayOneShot(footstepSound, 0.3f);
    }

    /// <summary>
    /// Stops walk sound
    /// </summary>
    private void StopPlayingFootsstep()
    {
        if (IsInvoking(nameof(PlayWalkingSound)))
        {
            CancelInvoke(nameof(PlayWalkingSound));
        }
    }

    /// <summary>
    /// Turns On FlashLight
    /// </summary>
    /// <returns>Waits</returns>
    private IEnumerator TurnOnFlashLight()
    {
        yield return new WaitForSeconds(1f);
        audioSource.PlayOneShot(flashLight);
        yield return new WaitUntil(() => !audioSource.isPlaying);
        if (!Light.isActiveAndEnabled)
            Light.enabled = true;
    }

    #endregion

    #region Event Handlers
    public void EnableMovement(Component sender, object data)
    {
        if (sender is GameManager && data is GameStates state)
        {
            switch (state)
            {
                case GameStates.Playing: this.enabled = true; break;
                case GameStates.Finish: StopPlayingFootsstep(); Light.enabled = false; movementDirections = Vector3.zero; this.enabled = false; break;
                case GameStates.Wait:
                case GameStates.MainMenu:
                case GameStates.PauseMenu:
                case GameStates.Settings:
                default:
                    movementDirections = Vector3.zero;
                    this.enabled = false;
                    break;
            }
        }

    }

    public void OnNewGame(Component sender, object data)
    {
        if(sender is MenuManager && data is null)
        {
            transform.position = initialPosition;
            StartCoroutine(TurnOnFlashLight());
        }
    }
    #endregion
}



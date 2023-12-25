using UnityEngine;

/// <summary>
/// Follows the player
/// </summary>
public class PlayerFollowCamera : MonoBehaviour
{
    #region Fields

    [SerializeField] private GameEvent newGameEvent;
    [SerializeField] GameObjectVariable player; // Player's transform
    [SerializeField] float smoothSpeed = 0.125f;
    [SerializeField] Vector3 offset;
    private Vector3 initialPosition;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        newGameEvent.ActionEvent += OnNewGame;
    }
    private void Start()
    {
        initialPosition = transform.position;
    }

    private void LateUpdate()
    {
        if (player != null)
        {
            Vector3 desiredPosition = player.Value.transform.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y, transform.position.z);
        }
    }
    private void OnDestroy()
    {
        newGameEvent.ActionEvent -= OnNewGame;
    }
    #endregion

    #region Event Handler
    /// <summary>
    /// Set position to initial position
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="data"></param>
    private void OnNewGame(Component sender, object data)
    {
        if(sender is MenuManager && data is null)
        {
            transform.position = initialPosition;
        }
    }
    #endregion
}

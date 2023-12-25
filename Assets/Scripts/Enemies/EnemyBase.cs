using UnityEngine;

/// <summary>
/// Enemy States possible
/// </summary>
public enum EnemyState
{
    NotSeen,
    Seen,
    Lost
}

/// <summary>
/// Abstract class for enemies
/// </summary>
public abstract class EnemyBase : MonoBehaviour
{
    #region Fields
    [SerializeField] float speed;
    [SerializeField] AnimationCurve speedCurve;

    [SerializeField] GameObjectVariable player;
    [SerializeField] FloatVariable visionRange;
    [SerializeField] AudioSource chaseSound;
    [SerializeField] LayerMask obstacles;
    [SerializeField] GameEvent detectGameEvent;

    EnemyState currentState;
    protected EnemyState CurrentState
    {
        get { return currentState; }
        set
        {
            currentState = value;
        }
    }
    #endregion

    #region Unity Methods

    private void OnEnable()
    {
        detectGameEvent.ActionEvent += DetectionEventResponse;
    }

    private void OnDisable()
    {
        StopPlayingSound();
        CurrentState = EnemyState.NotSeen;
        detectGameEvent.ActionEvent -= DetectionEventResponse;
    }

    #endregion

    #region Behaviour Methods

    /// <summary>
    /// Enemy chases player 
    /// </summary>
    protected void PerformChase()
    {
        Vector3 directionVector = player.Value.transform.position - this.transform.position;
        if (!IsPlayerInRange(directionVector))
        {
            CurrentState = EnemyState.NotSeen;
            return;
        }
        PlayChaseSound();
        MoveTowardsPlayer(directionVector);
    }

    /// <summary>
    /// Checks if the enemy is within players view range
    /// </summary>
    /// <returns>True if is within view range else false</returns>
    bool IsPlayerInRange(Vector3 direction)
    {
        return direction.magnitude <= visionRange.Value;
    }

    /// <summary>
    /// Moves towards the player
    /// </summary>
    void MoveTowardsPlayer(Vector3 direction)
    {
        var noramlizedDirection = direction.normalized;
        if(GoingToHit(noramlizedDirection))
        {
            CurrentState = EnemyState.NotSeen;
            return;
        }
        float easedTime = speedCurve.Evaluate(Time.time);
        Vector3 velocityToPlayer = easedTime * speed * noramlizedDirection;   
        this.transform.position += velocityToPlayer * Time.deltaTime;
    }

    /// <summary>
    /// Checks if it moving stright to an obstacle
    /// </summary>
    /// <param name="normalizedDirection"></param>
    /// <returns>normalized direction moving towards</returns>
    bool GoingToHit(Vector3 normalizedDirection)
    {
        RaycastHit2D hit = Physics2D.Raycast(this.transform.position + normalizedDirection , normalizedDirection, 0.1f, obstacles);
        return hit.collider != null;
    }

    /// <summary>
    /// Rotate towards the player position
    /// </summary>
    protected void PerformRotateTowardsPlayer()
    {
        Vector3 direction = player.Value.transform.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, angle - 90f));
    }

    /// <summary>
    /// Plays sound if not playing
    /// </summary>
    void PlayChaseSound()
    {
        if (!chaseSound.isPlaying)
        {
            chaseSound.Play();
        }
    }

    /// <summary>
    /// Stops playing chase sound
    /// </summary>
    protected void StopPlayingSound()
    {
        if (chaseSound.isPlaying)
        {
            chaseSound.Stop();
        }
    }

    #endregion

    #region Event Handler
    /// <summary>
    /// Event handler when detected by player
    /// </summary>
    /// <param name="sender">From</param>
    /// <param name="data"></param>
    private void DetectionEventResponse(Component sender, object data)
    {
        if (data is DetectionData detection && detection.dataTo == this.gameObject && sender is PlayerLOS && detection.state is EnemyState state)
        {
            switch (state)
            {
                case EnemyState.NotSeen: CurrentState = EnemyState.NotSeen; break;
                case EnemyState.Seen: CurrentState = EnemyState.Seen; break;
                case EnemyState.Lost: CurrentState = EnemyState.Lost; break;
                default: break;
            }
        }
    }
    #endregion

}

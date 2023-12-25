using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Data to sent in Detection event
/// </summary>
public struct DetectionData
{
    public GameObject dataTo;
    public EnemyState state;
}

public class PlayerLOS : MonoBehaviour
{
    #region Fields

    [SerializeField] private FloatVariable range;
    [SerializeField] private int numberOfRays = 15;
    [SerializeField] private float maxAngle = 30f;

    [SerializeField] private GameEvent gameStateChangeEvent;
    [SerializeField] private GameEvent detectionEvent;
    [SerializeField] private GameEvent endGameEvent;

    [SerializeField] private LayerMask detectableLayer;
    [SerializeField] private LayerMask enemyLayer;

    private HashSet<GameObject> previousFrameDetectedEnemies;
    private HashSet<GameObject> currentFrameDetectedEnemies;

    private readonly string finish = "Finish";
    private readonly string enemy = "Enemy";

    #endregion

    #region Unity Methods
    private void Awake()
    {
        gameStateChangeEvent.ActionEvent += EnableLOS;
    }
    private void Start()
    {
        previousFrameDetectedEnemies = new HashSet<GameObject>();
        currentFrameDetectedEnemies = new HashSet<GameObject>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(finish))
            endGameEvent.ActionEvent?.Invoke(this, true);
        else if(collision.gameObject.CompareTag(enemy))
            endGameEvent.ActionEvent?.Invoke(this, false);
    }

    void Update()
    {       
        ToDetectEnemy();
        UndetectedEnemies();
        SetPreviousDetectedEnemies();
    }

    private void OnDestroy()
    {
        gameStateChangeEvent.ActionEvent -= EnableLOS;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Shoots raycasts to detect enemies in the LOS cone
    /// </summary>
    void ToDetectEnemy()
    {
        currentFrameDetectedEnemies.Clear();
        float angleIncrement = maxAngle / (numberOfRays - 1);
        for (int i = 0; i < numberOfRays; i++)
        {
            float currentAngle = -maxAngle / 2 + angleIncrement * i;
            Vector2 direction = Quaternion.Euler(0, 0, currentAngle) * transform.up;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, range.Value, detectableLayer);
            HandleHit(hit);
        }
    }

    /// <summary>
    /// Handles hit
    /// </summary>
    /// <param name="hit">Raycast hit</param>
    void HandleHit(RaycastHit2D hit)
    {
        if (hit.collider != null)
        {
            int objectLayer = hit.collider.gameObject.layer;
            if (((1 << objectLayer) & enemyLayer.value) != 0)
            {
                GameObject detectedObject = hit.collider.gameObject;
                DetectEnemy(detectedObject);
            }
        }
    }

    /// <summary>
    /// Detects an enemy and raises the DetectionEvent
    /// </summary>
    void DetectEnemy(GameObject obj)
    {
        if (obj == null || currentFrameDetectedEnemies.Contains(obj))
            return;
        currentFrameDetectedEnemies.Add(obj);
        detectionEvent.ActionEvent?.Invoke(this, new DetectionData { dataTo = obj, state = EnemyState.Seen});
    }

    /// <summary>
    /// Handles undetected enemies and raises the DetectionEvent
    /// </summary>
    void UndetectedEnemies()
    {
        foreach (var _ in previousFrameDetectedEnemies.Except(currentFrameDetectedEnemies))
        {
            detectionEvent.ActionEvent?.Invoke(this, new DetectionData { dataTo = _, state = EnemyState.Lost });
        }
    }

    /// <summary>
    /// Set the previous detected enemies to the current detected enemies
    /// </summary>
    private void SetPreviousDetectedEnemies()
    {
        previousFrameDetectedEnemies.Clear();
        previousFrameDetectedEnemies.UnionWith(currentFrameDetectedEnemies);
    }
    #endregion

    #region Event Handler
    /// <summary>
    /// Enables or disables the LOS component based on the game state
    /// </summary>
    private void EnableLOS(Component sender, object data)
    {
        if (sender is GameManager && data is GameStates state)
        {
            this.enabled = (state == GameStates.Playing);
        }
    }
    #endregion
}

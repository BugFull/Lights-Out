using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Manages the enemies in the scene (Spawning & Despawning)
/// </summary>
public class EnemyManager : MonoBehaviour
{
    #region Fields
    [SerializeField] private FloatVariable spawnInnerRadius;
    [SerializeField] private FloatVariable spawnOuterRadius;

    [SerializeField] private IntVariable difficultyLevel;
    [SerializeField] private GameObjectVariable playerObject;
    [SerializeField] private GameEvent gameStateChangeEvent;
    [SerializeField] private GameEvent enemyLeftEvent;
    [SerializeField] private GameEvent newGameEvent;

    [SerializeField] private GameObject watchMePrefab;
    [SerializeField] private GameObject dontWatchMePrefab;

    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private LayerMask floor;

    
    private readonly List<GameObject> activeEnemies = new();
    private ObjectPool<GameObject> watchMePool;
    private ObjectPool<GameObject> dontWatchMePool;

    private readonly Dictionary<int, (int, float)> difficultySettings = new()
    {
    { 0, (5, 6f) }, // Easy
    { 1, (7, 5f) }, // Normal
    { 2, (10, 4f) } // Hard
    };

    private int maxEnemyCount;
    private float enemySpawnTimeInterval;
    private int currentEnemyCount;
    private float nextSpawnTime;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        difficultyLevel.OnValueChanged += SetDifficulty;
        gameStateChangeEvent.ActionEvent += GameStateHandler;
        enemyLeftEvent.ActionEvent += EnemyLeft;
        newGameEvent.ActionEvent += OnNewGame;
    }
    private void Start()
    {
        currentEnemyCount = 0;
        CreateEnemyPools();
        SetDifficulty(difficultyLevel.Value);
    }

    private void OnDestroy()
    {
        difficultyLevel.OnValueChanged -= SetDifficulty;
        gameStateChangeEvent.ActionEvent -= GameStateHandler;
        enemyLeftEvent.ActionEvent -= EnemyLeft;
        newGameEvent.ActionEvent -= OnNewGame;
    }

    private void Update()
    {
        if (Time.time >= nextSpawnTime)
            SpawnEnemy();    
    }
    #endregion

    #region Enemy Spawn & DeSpawn

    /// <summary>
    /// Spawns an Enemy
    /// </summary>
    private void SpawnEnemy()
    {
        if(currentEnemyCount >= maxEnemyCount) { return; }

        Vector3 spawnPosition = GenerateSpawnPosition();
        if (!IsValidSpawnPoint(spawnPosition))
            return ;
        GameObject newEnemy;
        if (GenerateRandomBoolean())
            newEnemy = watchMePool.Get();
        else
            newEnemy = dontWatchMePool.Get();
        newEnemy.transform.position = spawnPosition;
        nextSpawnTime = Time.time + enemySpawnTimeInterval;
                      
    }

    /// <summary>
    /// Generates a random point in a donut region within inner and outer radius
    /// </summary>
    /// <returns>SpawnPoisition vector</returns>
    private Vector3 GenerateSpawnPosition()
    {
        float angle = UnityEngine.Random.Range(0f, 2f * Mathf.PI);
        float distance = UnityEngine.Random.Range(spawnInnerRadius.Value, spawnOuterRadius.Value);

        float x = Mathf.Cos(angle) * distance;
        float y = Mathf.Sin(angle) * distance;

        return new Vector3(x, y, 0f) + playerObject.Value.transform.position;
    }


    /// <summary>
    /// Check if an position is suitable to spawn an enemy
    /// </summary>
    /// <param name="position"> The position to check</param>
    /// <returns> True if valid else false</returns>
    private bool IsValidSpawnPoint(Vector2 position)
    {
        if (Physics2D.OverlapPoint(position, floor))
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(position, .5f, obstacleLayer);
            if (colliders.Length == 0)
                return true;
        }
        return false;
    }


    /// <summary>
    /// Despawns all enemies
    /// </summary>
    private void DeSpawnAllEnemy()
    {
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            DeSpawnEnemy(activeEnemies[i]);
        }
    }

    /// <summary>
    /// Despawns a given enemy
    /// </summary>
    /// <param name="enemy"> The enemy game object to despawn</param>
    private void DeSpawnEnemy(GameObject enemy)
    {
        if(enemy == null || !enemy.activeSelf) return;
        if(enemy.TryGetComponent<EnemyBase>(out var enemyScript))
        {
            if(enemyScript is WatchMeEnemy)
            {
                watchMePool.Release(enemy);
            }
            else if(enemyScript is DontWatchMeEnemy)
            {
                dontWatchMePool.Release(enemy);
            }
        }
    }
    #endregion

    #region ObjectPooling


    /// <summary>
    /// Create an object pool with the given Func as its Create function
    /// </summary>
    /// <param name="createFunc">Create Function</param>
    /// <returns>Object pool for the enemy</returns>
    ObjectPool<GameObject> CreateObjectPool(Func<GameObject> createFunc)
    {
        return new ObjectPool<GameObject>
        (
            createFunc,
            OnTakeEnemyFromPool,
            OnReturnEnemyToPool,
            OnDestroyPoolEnemy,
            false, 10, 15
        );
    }

    /// <summary>
    /// Creates all the object pool for each type of enemy
    /// </summary>
    private void CreateEnemyPools()
    {
        watchMePool = CreateObjectPool(CreateWatchMe);
        dontWatchMePool = CreateObjectPool(CreateDontSeeMe);
    }

    /// <summary>
    /// Instantiate WatchMePrefab
    /// </summary>
    /// <returns> The instance of the gameobject</returns>
    private GameObject CreateWatchMe()
    {    
        return Instantiate(watchMePrefab);
    }

    /// <summary>
    /// Instantiate DontWatchMePrefab
    /// </summary>
    /// <returns> The instance of the gameobject</returns>
    private GameObject CreateDontSeeMe()
    {
        return Instantiate(dontWatchMePrefab);
    }

    /// <summary>
    /// Sets the enemy to active and adds it into the active enemy list
    /// </summary>
    /// <param name="enemy"> The enemy to set</param>
    private void OnTakeEnemyFromPool(GameObject enemy)
    {
        enemy.SetActive(true);
        currentEnemyCount++;
        activeEnemies.Add(enemy);
    }

    /// <summary>
    /// Sets the enemy to inactive and remove it from the active enemy list
    /// </summary>
    /// <param name="enemy"> Enemy to set</param>
    private void OnReturnEnemyToPool(GameObject enemy)
    {
        enemy.SetActive(false);
        enemy.transform.position = Vector3.zero;
        currentEnemyCount--;
        activeEnemies.Remove(enemy);
    }

    /// <summary>
    /// If destroyed by pool remove it from active enemies
    /// </summary>
    /// <param name="enemy"> enemy destroyed</param>
    private void OnDestroyPoolEnemy(GameObject enemy)
    {
        if(activeEnemies.Contains(enemy))
            activeEnemies.Remove(enemy);
    }
    #endregion

    #region Event Handler and Other Methods
    /// <summary>
    /// Event handler when game state changes
    /// </summary>
    /// <param name="sender">Who sent it</param>
    /// <param name="data"> Content</param>
    private void GameStateHandler(Component sender, object data)
    {
        if (sender is GameManager && data is GameStates gameState)
        {
            switch (gameState)
            {
                case GameStates.Playing: this.enabled = true; break;
                case GameStates.Finish: DeSpawnAllEnemy(); this.enabled = false; break;
                case GameStates.MainMenu: 
                case GameStates.Settings:
                case GameStates.PauseMenu:
                case GameStates.Wait:this.enabled = false; break;
                default: break;
            }
        }
    }

    /// <summary>
    /// Destroys an enemy when it is too far.
    /// </summary>
    /// <param name="sender">From</param>
    /// <param name="data"> Which enemy gameObject</param>
    private void EnemyLeft(Component sender, object data)
    {
        if (sender is EnemyLeaveDetector && data is GameObject enemy)
        {
            DeSpawnEnemy(enemy);
        }
    }

    /// <summary>
    /// On New game reset the spawn timer
    /// </summary>
    /// <param name="sender">From</param>
    /// <param name="data">Null</param>
    private void OnNewGame(Component sender, object data)
    {
        if(sender is MenuManager && data is null)
        {
            nextSpawnTime = Time.time + enemySpawnTimeInterval;
        }
    }

    /// <summary>
    /// Sets the max enemy count and enemy spawn time according to the difficulty level
    /// </summary>
    /// <param name="difficultyLevel">The difficult level</param>
    private void SetDifficulty(int difficultyLevel)
    {
        if (difficultySettings.TryGetValue(difficultyLevel, out var settings))
        {
            maxEnemyCount = settings.Item1;
            enemySpawnTimeInterval = settings.Item2;
        }
    }
    #endregion

    #region Utilities Functions

    /// <summary>
    /// Generates a random bool value
    /// </summary>
    /// <param name="bias"> Bias for returning true</param>
    /// <returns></returns>
    private bool GenerateRandomBoolean(float bias = 0.5f)
    {
        float randomValue = UnityEngine.Random.value;
        return randomValue < bias;
    }
    #endregion

}

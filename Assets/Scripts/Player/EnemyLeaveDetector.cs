using UnityEngine;

public class EnemyLeaveDetector : MonoBehaviour
{
    #region Fields

    [SerializeField] private CircleCollider2D detector;
    [SerializeField] private FloatVariable outerRangeRadius;
    [SerializeField] private GameEvent enemyOutOfRangeEvent;
    [SerializeField] private LayerMask enemyMask;
    #endregion

    #region Unity Methods

    private void Start()
    {
        InitializeDetector();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsEnemyLayer(other.gameObject.layer))
        {
            enemyOutOfRangeEvent.ActionEvent?.Invoke(this, other.gameObject);
        }
    }
    #endregion

    #region Methods
    /// <summary>
    /// Initialize the detector
    /// </summary>
    private void InitializeDetector()
    {
        if (detector == null)
            detector = GetComponent<CircleCollider2D>();

        detector.radius = outerRangeRadius.Value;
        detector.isTrigger = true;
    }

    /// <summary>
    /// Checks if its in enemy layer
    /// </summary>
    /// <param name="layer">Layer of the object</param>
    /// <returns>True if it enemy</returns>
    private bool IsEnemyLayer(int layer)
    {
        return (enemyMask.value & (1 << layer)) != 0;
    }
    #endregion

}

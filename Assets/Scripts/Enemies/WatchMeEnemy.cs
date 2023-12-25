
/// <summary>
/// Chases when No LOS
/// </summary>
public class WatchMeEnemy : EnemyBase
{
    private void Update()
    {
        switch (CurrentState)
        {
            case EnemyState.NotSeen:
            case EnemyState.Seen: StopPlayingSound(); break;
            case EnemyState.Lost: PerformChase(); break;
        }
        PerformRotateTowardsPlayer();
    }
}

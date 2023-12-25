
/// <summary>
/// Chases when it's in LOS
/// </summary>
public class DontWatchMeEnemy : EnemyBase
{
    private void Update()
    {
        switch (CurrentState)
        {
            case EnemyState.NotSeen:
            case EnemyState.Lost: StopPlayingSound(); break;
            case EnemyState.Seen: PerformChase(); break;
        }
        PerformRotateTowardsPlayer();
    }
}

namespace BattleBuck.Core
{
    public enum MatchState
    {
        WaitingToStart,
        InProgress,
        Ended
    }

    public enum MatchEndReason
    {
        TimerExpired,
        ScoreLimitReached
    }

    public enum PlayerState
    {
        Alive,
        Dead,
        Respawning
    }

    public enum CombatState
    {
        Free,
        Engaged
    }
}

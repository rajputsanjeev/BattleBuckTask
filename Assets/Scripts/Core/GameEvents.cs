using System;
using UnityEngine;

namespace BattleBuck.Core
{
    public static class GameEvents
    {
        public static event Action OnMatchStarted;
        public static event Action<MatchEndReason, int> OnMatchEnded;

        public static event Action<int, int> OnPlayerKilled;
        public static event Action<int> OnPlayerRespawned;
        public static event Action<int> OnPlayerDied;
        public static event Action<int, Vector3> OnPlayerPositionChanged;
        public static event Action<int, int, int> OnPlayerHealthChanged;

        public static event Action<int, int, Vector3, Vector3> OnProjectileFired;

        public static event Action<int, int> OnScoreChanged;
        public static event Action OnLeaderboardChanged;

        public static event Action<float> OnTimerUpdated;
        public static event Action OnTimerExpired;

        public static void RaiseMatchStarted() => OnMatchStarted?.Invoke();
        public static void RaiseMatchEnded(MatchEndReason reason, int winnerId) => OnMatchEnded?.Invoke(reason, winnerId);
        public static void RaisePlayerKilled(int killerId, int victimId) => OnPlayerKilled?.Invoke(killerId, victimId);
        public static void RaisePlayerRespawned(int playerId) => OnPlayerRespawned?.Invoke(playerId);
        public static void RaisePlayerDied(int playerId) => OnPlayerDied?.Invoke(playerId);
        public static void RaisePlayerPositionChanged(int playerId, Vector3 pos) => OnPlayerPositionChanged?.Invoke(playerId, pos);
        public static void RaisePlayerHealthChanged(int playerId, int current, int max) => OnPlayerHealthChanged?.Invoke(playerId, current, max);
        public static void RaiseProjectileFired(int shooterId, int targetId, Vector3 from, Vector3 to) => OnProjectileFired?.Invoke(shooterId, targetId, from, to);
        public static void RaiseScoreChanged(int playerId, int newScore) => OnScoreChanged?.Invoke(playerId, newScore);
        public static void RaiseLeaderboardChanged() => OnLeaderboardChanged?.Invoke();
        public static void RaiseTimerUpdated(float remaining) => OnTimerUpdated?.Invoke(remaining);
        public static void RaiseTimerExpired() => OnTimerExpired?.Invoke();

        public static void ClearAll()
        {
            OnMatchStarted = null;
            OnMatchEnded = null;
            OnPlayerKilled = null;
            OnPlayerRespawned = null;
            OnPlayerDied = null;
            OnPlayerPositionChanged = null;
            OnPlayerHealthChanged = null;
            OnProjectileFired = null;
            OnScoreChanged = null;
            OnLeaderboardChanged = null;
            OnTimerUpdated = null;
            OnTimerExpired = null;
        }
    }
}

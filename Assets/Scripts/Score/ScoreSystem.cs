using System;
using BattleBuck.Core;
using BattleBuck.Player;

namespace BattleBuck.Score
{
    public class ScoreSystem : IDisposable
    {
        public int[] SortedIndices => _sortedIndices;

        private readonly PlayerData[] _players;
        private readonly int[] _sortedIndices;

        public ScoreSystem(PlayerData[] players)
        {
            _players = players;
            _sortedIndices = new int[players.Length];

            for (int i = 0; i < players.Length; i++)
            {
                _sortedIndices[i] = i;
            }

            GameEvents.OnScoreChanged += HandleScoreChanged;
        }

        public int GetPlayerIdAtRank(int rank)
        {
            if (rank < 0 || rank >= _sortedIndices.Length) return -1;
            return _sortedIndices[rank];
        }

        public PlayerData GetLeader()
        {
            return _players[_sortedIndices[0]];
        }

        public void Dispose()
        {
            GameEvents.OnScoreChanged -= HandleScoreChanged;
        }

        private void HandleScoreChanged(int playerId, int newScore)
        {
            int changedPos = -1;
            for (int i = 0; i < _sortedIndices.Length; i++)
            {
                if (_sortedIndices[i] == playerId)
                {
                    changedPos = i;
                    break;
                }
            }

            if (changedPos < 0) return;

            while (changedPos > 0)
            {
                int aboveIdx = _sortedIndices[changedPos - 1];
                if (_players[aboveIdx].Score < newScore)
                {
                    _sortedIndices[changedPos] = aboveIdx;
                    _sortedIndices[changedPos - 1] = playerId;
                    changedPos--;
                }
                else
                {
                    break;
                }
            }

            GameEvents.RaiseLeaderboardChanged();
        }
    }
}

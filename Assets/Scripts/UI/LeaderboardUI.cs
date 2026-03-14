using BattleBuck.Core;
using BattleBuck.Match;
using BattleBuck.Player;
using UnityEngine;

namespace BattleBuck.UI
{
    public class LeaderboardUI : MonoBehaviour
    {
        [SerializeField] private MatchController _matchController;
        [SerializeField] private LeaderboardEntry _entryPrefab;
        [SerializeField] private Transform _entryContainer;
        [SerializeField] private Color _goldColor = new Color(1f, 0.84f, 0f, 0.3f);
        [SerializeField] private Color _silverColor = new Color(0.75f, 0.75f, 0.75f, 0.2f);
        [SerializeField] private Color _bronzeColor = new Color(0.8f, 0.5f, 0.2f, 0.2f);
        [SerializeField] private Color _normalColor = new Color(1f, 1f, 1f, 0.08f);

        private LeaderboardEntry[] _entries;

        private void Awake()
        {
            if (_matchController == null)
            {
                _matchController = FindAnyObjectByType<MatchController>();
            }
            int count = _matchController.Config.playerCount;
            _entries = new LeaderboardEntry[count];

            for (int i = 0; i < count; i++)
            {
                _entries[i] = Instantiate(_entryPrefab, _entryContainer);
            }
        }

        private void OnEnable()
        {
            GameEvents.OnLeaderboardChanged += RefreshLeaderboard;
            GameEvents.OnMatchStarted += RefreshLeaderboard;
        }

        private void OnDisable()
        {
            GameEvents.OnLeaderboardChanged -= RefreshLeaderboard;
            GameEvents.OnMatchStarted -= RefreshLeaderboard;
        }

        private void RefreshLeaderboard()
        {
            var scoreSystem = _matchController.ScoreSystem;
            var playerManager = _matchController.PlayerManager;
            int[] sorted = scoreSystem.SortedIndices;

            for (int i = 0; i < _entries.Length; i++)
            {
                int playerId = sorted[i];
                PlayerData player = playerManager.GetPlayer(playerId);
                Color bg = GetRankColor(i);
                _entries[i].SetData(i, player, bg);
            }
        }

        private Color GetRankColor(int rank)
        {
            switch (rank)
            {
                case 0: return _goldColor;
                case 1: return _silverColor;
                case 2: return _bronzeColor;
                default: return _normalColor;
            }
        }
    }
}

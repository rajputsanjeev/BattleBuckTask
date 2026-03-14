using System;
using BattleBuck.Core;
using BattleBuck.Player;
using BattleBuck.Score;
using BattleBuck.Timer;
using UnityEngine;

namespace BattleBuck.Match
{
    [DefaultExecutionOrder(-50)]
    public class MatchController : MonoBehaviour
    {
        public PlayerManager PlayerManager => _playerManager;
        public ScoreSystem ScoreSystem => _scoreSystem;
        public MatchTimer Timer => _matchTimer;
        public MatchState State => _state;
        public MatchConfig Config => _config;

        [SerializeField] private MatchConfig _config;

        private PlayerManager _playerManager;
        private ScoreSystem _scoreSystem;
        private MatchTimer _matchTimer;
        private MatchState _state;

        private void Awake()
        {
            _state = MatchState.WaitingToStart;

            _playerManager = new PlayerManager(_config);
            _scoreSystem = new ScoreSystem(_playerManager.Players);
            _matchTimer = new MatchTimer(_config.matchDuration);

            GameEvents.OnTimerExpired += HandleTimerExpired;
            GameEvents.OnMatchEnded += HandleMatchEnded;
        }

        private void Start()
        {
            StartMatch();
        }

        private void Update()
        {
            if (_state != MatchState.InProgress) return;

            float dt = UnityEngine.Time.deltaTime;
            _matchTimer.Tick(dt);
            _playerManager.Tick(dt);
        }

        private void OnDestroy()
        {
            GameEvents.OnTimerExpired -= HandleTimerExpired;
            GameEvents.OnMatchEnded -= HandleMatchEnded;
            _scoreSystem?.Dispose();
            GameEvents.ClearAll();
        }

        private void StartMatch()
        {
            _state = MatchState.InProgress;
            _matchTimer.Start();
            _playerManager.StartMatch();
            GameEvents.RaiseMatchStarted();
        }

        private void HandleTimerExpired()
        {
            if (_state != MatchState.InProgress) return;

            _playerManager.StopMatch();
            _state = MatchState.Ended;

            PlayerData leader = _scoreSystem.GetLeader();
            GameEvents.RaiseMatchEnded(MatchEndReason.TimerExpired, leader.Id);
        }

        private void HandleMatchEnded(MatchEndReason reason, int winnerId)
        {
            _state = MatchState.Ended;
            _matchTimer.Stop();
            _playerManager.StopMatch();
        }
    }
}

using BattleBuck.Core;

namespace BattleBuck.Timer
{
    public class MatchTimer
    {
        public float Remaining => _remaining;
        public bool IsRunning => _isRunning;

        private float _remaining;
        private int _lastBroadcastSecond;
        private bool _isRunning;

        public MatchTimer(float durationSeconds)
        {
            _remaining = durationSeconds;
            _lastBroadcastSecond = (int)durationSeconds;
            _isRunning = false;
        }

        public void Start()
        {
            _isRunning = true;
            GameEvents.RaiseTimerUpdated(_remaining);
        }

        public void Stop()
        {
            _isRunning = false;
        }

        public void Tick(float deltaTime)
        {
            if (!_isRunning) return;

            _remaining -= deltaTime;

            if (_remaining <= 0f)
            {
                _remaining = 0f;
                _isRunning = false;
                GameEvents.RaiseTimerUpdated(0f);
                GameEvents.RaiseTimerExpired();
                return;
            }

            int currentSecond = (int)_remaining;
            if (currentSecond != _lastBroadcastSecond)
            {
                _lastBroadcastSecond = currentSecond;
                GameEvents.RaiseTimerUpdated(_remaining);
            }
        }
    }
}

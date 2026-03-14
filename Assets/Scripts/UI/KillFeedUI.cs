using BattleBuck.Core;
using BattleBuck.Match;
using BattleBuck.Player;
using TMPro;
using UnityEngine;

namespace BattleBuck.UI
{
    public class KillFeedUI : MonoBehaviour
    {
        private const string KILL_SEPARATOR = "  ✦  killed  ✦  ";

        [SerializeField] private MatchController _matchController;
        [SerializeField] private TextMeshProUGUI _killFeedText;
        [SerializeField] private float _displayDuration = 2.5f;

        private float _hideTimer;
        private bool _isShowing;

        private void OnEnable()
        {
            GameEvents.OnPlayerKilled += HandlePlayerKilled;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerKilled -= HandlePlayerKilled;
        }

        private void HandlePlayerKilled(int killerId, int victimId)
        {
            PlayerData killer = _matchController.PlayerManager.GetPlayer(killerId);
            PlayerData victim = _matchController.PlayerManager.GetPlayer(victimId);

            _killFeedText.text = killer.Name + KILL_SEPARATOR + victim.Name;
            _killFeedText.alpha = 1f;
            _hideTimer = _displayDuration;
            _isShowing = true;
        }

        private void Update()
        {
            if (!_isShowing) return;

            _hideTimer -= Time.deltaTime;
            if (_hideTimer <= 0f)
            {
                _killFeedText.alpha = 0f;
                _isShowing = false;
            }
            else if (_hideTimer < 0.5f)
            {
                _killFeedText.alpha = _hideTimer / 0.5f;
            }
        }
    }
}

using BattleBuck.Core;
using BattleBuck.Match;
using BattleBuck.Player;
using TMPro;
using UnityEngine;

namespace BattleBuck.UI
{
    public class WinnerScreenUI : MonoBehaviour
    {
        private const string REASON_TIME = "Time Expired";
        private const string REASON_SCORE = "Score Limit Reached!";
        private const string WINNER_PREFIX = "Winner: ";
        private const string KILLS_SUFFIX = " Kills";

        [SerializeField] private MatchController _matchController;
        [SerializeField] private GameObject _winnerPanel;
        [SerializeField] private TextMeshProUGUI _winnerNameText;
        [SerializeField] private TextMeshProUGUI _winnerScoreText;
        [SerializeField] private TextMeshProUGUI _reasonText;

        private void Awake()
        {
            GameEvents.OnMatchEnded += HandleMatchEnded;
        }

        private void Start()
        {
            _winnerPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            GameEvents.OnMatchEnded -= HandleMatchEnded;
        }

        private void HandleMatchEnded(MatchEndReason reason, int winnerId)
        {
            PlayerData winner = _matchController.PlayerManager.GetPlayer(winnerId);

            _winnerPanel.SetActive(true);

            _winnerNameText.text = WINNER_PREFIX + winner.Name;
            _winnerScoreText.text = winner.Score + KILLS_SUFFIX;
            _reasonText.text = (reason == MatchEndReason.TimerExpired) ? REASON_TIME : REASON_SCORE;
        }
    }
}

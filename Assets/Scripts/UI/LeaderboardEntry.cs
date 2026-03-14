using BattleBuck.Core;
using BattleBuck.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BattleBuck.UI
{
    public class LeaderboardEntry : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _rankText;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private Image _background;

        private static readonly string[] RANK_STRINGS = new string[]
        {
            "#1", "#2", "#3", "#4", "#5",
            "#6", "#7", "#8", "#9", "#10",
            "#11", "#12", "#13", "#14", "#15",
            "#16", "#17", "#18", "#19", "#20"
        };

        private static readonly string[] SCORE_STRINGS;

        private int _cachedScore = -1;
        private int _cachedPlayerId = -1;

        static LeaderboardEntry()
        {
            SCORE_STRINGS = new string[100];
            for (int i = 0; i < 100; i++)
            {
                SCORE_STRINGS[i] = i.ToString();
            }
        }

        public void SetData(int rank, PlayerData player, Color bgColor)
        {
            if (rank < RANK_STRINGS.Length)
            {
                _rankText.text = RANK_STRINGS[rank];
            }

            if (_cachedPlayerId != player.Id)
            {
                _cachedPlayerId = player.Id;
                _nameText.text = player.Name;
            }

            if (_cachedScore != player.Score)
            {
                _cachedScore = player.Score;
                _scoreText.text = (player.Score < SCORE_STRINGS.Length)
                    ? SCORE_STRINGS[player.Score]
                    : player.Score.ToString();
            }

            _background.color = bgColor;
        }
    }
}

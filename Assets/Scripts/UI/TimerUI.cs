using BattleBuck.Core;
using BattleBuck.Utils;
using TMPro;
using UnityEngine;

namespace BattleBuck.UI
{
    public class TimerUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _timerText;

        private void OnEnable()
        {
            GameEvents.OnTimerUpdated += HandleTimerUpdated;
        }

        private void OnDisable()
        {
            GameEvents.OnTimerUpdated -= HandleTimerUpdated;
        }

        private void HandleTimerUpdated(float remaining)
        {
            _timerText.text = TimeFormatter.GetTimeString(remaining);
        }
    }
}

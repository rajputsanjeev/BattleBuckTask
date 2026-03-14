using UnityEngine;

namespace BattleBuck.Utils
{
    public static class TimeFormatter
    {
        private static string[] _cachedStrings;
        private static bool _initialized;

        public static void Initialize(int maxSeconds)
        {
            _cachedStrings = new string[maxSeconds + 1];
            for (int i = 0; i <= maxSeconds; i++)
            {
                int m = i / 60;
                int s = i % 60;
                _cachedStrings[i] = $"{m:D2}:{s:D2}";
            }
            _initialized = true;
        }

        public static string GetTimeString(float seconds)
        {
            if (!_initialized)
            {
                Initialize(300);
            }

            int sec = Mathf.CeilToInt(seconds);
            if (sec < 0) sec = 0;
            if (sec >= _cachedStrings.Length) sec = _cachedStrings.Length - 1;
            return _cachedStrings[sec];
        }
    }
}

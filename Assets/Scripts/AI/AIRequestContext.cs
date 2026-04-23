using System;
using UnityEngine;

namespace Flowcube.AI
{
    /// <summary>
    /// Encapsulates the behavioural context that is packaged and sent to the
    /// AI therapist for personalised message generation (Feature 3).
    /// </summary>
    [Serializable]
    public class AIRequestContext
    {
        /// <summary>How many focus sessions the user has completed today.</summary>
        public int todayFocusCount;

        /// <summary>Total focus seconds accumulated today.</summary>
        public float todayFocusSeconds;

        /// <summary>Total exercise seconds accumulated today.</summary>
        public float todayExerciseSeconds;

        /// <summary>Current local hour (0-23).</summary>
        public int currentHour;

        /// <summary>Number of consecutive days with at least one focus session.</summary>
        public int consecutiveFocusDays;

        /// <summary>Duration in seconds of the session that just ended (0 if not applicable).</summary>
        public float lastSessionDurationSeconds;

        /// <summary>Type of the session that just ended.</summary>
        public string lastSessionType;

        // ─── Derived helpers used by the Prompt Builder ────────────────────────

        /// <summary>Returns a friendly time-of-day label for use in prompts.</summary>
        public string TimeOfDayLabel()
        {
            if (currentHour >= 23 || currentHour < 5)  return "深夜";
            if (currentHour >= 5  && currentHour < 9)  return "清晨";
            if (currentHour >= 9  && currentHour < 12) return "上午";
            if (currentHour >= 12 && currentHour < 14) return "正午";
            if (currentHour >= 14 && currentHour < 18) return "下午";
            return "傍晚";
        }

        /// <summary>Returns a human-readable string for the last session duration.</summary>
        public string LastSessionDurationLabel()
        {
            int minutes = Mathf.RoundToInt(lastSessionDurationSeconds / 60f);
            return minutes < 1 ? "不到一分钟" : $"{minutes} 分钟";
        }
    }
}

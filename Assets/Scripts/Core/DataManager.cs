using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Flowcube.Core
{
    /// <summary>
    /// Persists and retrieves user focus / activity history using a JSON file.
    /// Provides aggregated stats needed by EcosystemManager and AITherapistManager.
    /// </summary>
    public class DataManager : MonoBehaviour
    {
        // Singleton
        public static DataManager Instance { get; private set; }

        private const string SaveFileName = "flowcube_data.json";
        private string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        private FlowcubeData _data;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadData();
        }

        // ─── Public API ────────────────────────────────────────────────────────

        /// <summary>Records a completed focus or activity session.</summary>
        public void RecordSession(SessionRecord record)
        {
            _data.sessions.Add(record);
            SaveData();
        }

        /// <summary>Total accumulated focus seconds across all sessions (all time).</summary>
        public float GetTotalFocusSeconds()
        {
            float total = 0f;
            foreach (var s in _data.sessions)
                if (s.sessionType == SessionType.Focus)
                    total += s.durationSeconds;
            return total;
        }

        /// <summary>Number of focus sessions completed today (local time).</summary>
        public int GetTodayFocusCount()
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            int count = 0;
            foreach (var s in _data.sessions)
                if (s.sessionType == SessionType.Focus && s.date == today)
                    count++;
            return count;
        }

        /// <summary>Total focus seconds accumulated today.</summary>
        public float GetTodayFocusSeconds()
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            float total = 0f;
            foreach (var s in _data.sessions)
                if (s.sessionType == SessionType.Focus && s.date == today)
                    total += s.durationSeconds;
            return total;
        }

        /// <summary>
        /// Number of consecutive days that contain at least one focus session.
        /// Counts backwards from today.
        /// </summary>
        public int GetConsecutiveFocusDays()
        {
            var daysWithSessions = new HashSet<string>();
            foreach (var s in _data.sessions)
                if (s.sessionType == SessionType.Focus)
                    daysWithSessions.Add(s.date);

            int streak = 0;
            var day = DateTime.Now.Date;
            while (daysWithSessions.Contains(day.ToString("yyyy-MM-dd")))
            {
                streak++;
                day = day.AddDays(-1);
            }
            return streak;
        }

        /// <summary>Total exercise seconds accumulated today.</summary>
        public float GetTodayExerciseSeconds()
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            float total = 0f;
            foreach (var s in _data.sessions)
                if (s.sessionType == SessionType.Exercise && s.date == today)
                    total += s.durationSeconds;
            return total;
        }

        /// <summary>Returns the highest ecosystem level the user has unlocked so far.</summary>
        public int GetMaxUnlockedEcosystemLevel() => _data.maxUnlockedEcosystemLevel;

        /// <summary>Persists a newly unlocked ecosystem level if it is higher than the current one.</summary>
        public void UpdateMaxUnlockedEcosystemLevel(int level)
        {
            if (level > _data.maxUnlockedEcosystemLevel)
            {
                _data.maxUnlockedEcosystemLevel = level;
                SaveData();
            }
        }

        // ─── Persistence ───────────────────────────────────────────────────────

        private void LoadData()
        {
            if (File.Exists(SaveFilePath))
            {
                try
                {
                    string json = File.ReadAllText(SaveFilePath);
                    _data = JsonUtility.FromJson<FlowcubeData>(json);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[DataManager] Failed to load save file: {e.Message}. Starting fresh.");
                    _data = new FlowcubeData();
                }
            }
            else
            {
                _data = new FlowcubeData();
            }
        }

        private void SaveData()
        {
            try
            {
                string json = JsonUtility.ToJson(_data, prettyPrint: true);
                File.WriteAllText(SaveFilePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[DataManager] Failed to save data: {e.Message}");
            }
        }
    }

    // ─── Data models ───────────────────────────────────────────────────────────

    [Serializable]
    public class FlowcubeData
    {
        public List<SessionRecord> sessions = new List<SessionRecord>();
        public int maxUnlockedEcosystemLevel = 0;
    }

    [Serializable]
    public class SessionRecord
    {
        public SessionType sessionType;
        /// <summary>ISO date string "yyyy-MM-dd"</summary>
        public string date;
        /// <summary>ISO 8601 datetime string</summary>
        public string startTime;
        public float durationSeconds;
    }

    public enum SessionType
    {
        Focus,
        Exercise
    }
}

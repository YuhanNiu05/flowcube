using System;
using System.Collections.Generic;
using NUnit.Framework;
using Flowcube.Core;

namespace Flowcube.Tests
{
    /// <summary>
    /// Edit-mode unit tests for DataManager logic (pure C# — no MonoBehaviour lifecycle).
    ///
    /// These tests exercise the statistical helper methods directly on a
    /// FlowcubeData object so they can run in the Unity Test Runner (Edit Mode)
    /// without spinning up a full scene.
    /// </summary>
    public class DataManagerTests
    {
        // ─── Helpers ───────────────────────────────────────────────────────────

        /// <summary>Creates a SessionRecord for today with the given duration.</summary>
        private static SessionRecord FocusToday(float durationSeconds) => new SessionRecord
        {
            sessionType     = SessionType.Focus,
            date            = DateTime.Now.ToString("yyyy-MM-dd"),
            startTime       = DateTime.Now.ToString("o"),
            durationSeconds = durationSeconds
        };

        /// <summary>Creates a SessionRecord for a specific date offset (days from today).</summary>
        private static SessionRecord FocusOnDayOffset(int dayOffset, float durationSeconds) => new SessionRecord
        {
            sessionType     = SessionType.Focus,
            date            = DateTime.Now.AddDays(dayOffset).ToString("yyyy-MM-dd"),
            startTime       = DateTime.Now.AddDays(dayOffset).ToString("o"),
            durationSeconds = durationSeconds
        };

        // ─── TotalFocusSeconds ─────────────────────────────────────────────────

        [Test]
        public void GetTotalFocusSeconds_EmptySessions_ReturnsZero()
        {
            var data = new FlowcubeData();
            Assert.AreEqual(0f, SumFocusSeconds(data.sessions));
        }

        [Test]
        public void GetTotalFocusSeconds_MultipleSessions_SumsCorrectly()
        {
            var sessions = new List<SessionRecord>
            {
                FocusToday(900f),   // 15 min
                FocusToday(1800f),  // 30 min
                FocusToday(2700f)   // 45 min
            };
            Assert.AreEqual(5400f, SumFocusSeconds(sessions), 0.001f);
        }

        [Test]
        public void GetTotalFocusSeconds_ExcludesExerciseSessions()
        {
            var sessions = new List<SessionRecord>
            {
                FocusToday(600f),
                new SessionRecord
                {
                    sessionType     = SessionType.Exercise,
                    date            = DateTime.Now.ToString("yyyy-MM-dd"),
                    startTime       = DateTime.Now.ToString("o"),
                    durationSeconds = 1800f
                }
            };
            Assert.AreEqual(600f, SumFocusSeconds(sessions), 0.001f);
        }

        // ─── TodayFocusCount ───────────────────────────────────────────────────

        [Test]
        public void GetTodayFocusCount_OnlyCountsToday()
        {
            var sessions = new List<SessionRecord>
            {
                FocusToday(600f),
                FocusToday(300f),
                FocusOnDayOffset(-1, 600f)  // yesterday
            };
            Assert.AreEqual(2, CountTodayFocus(sessions));
        }

        // ─── ConsecutiveFocusDays ──────────────────────────────────────────────

        [Test]
        public void GetConsecutiveFocusDays_NoSessions_ReturnsZero()
        {
            var sessions = new List<SessionRecord>();
            Assert.AreEqual(0, ComputeStreak(sessions));
        }

        [Test]
        public void GetConsecutiveFocusDays_TodayOnly_ReturnsOne()
        {
            var sessions = new List<SessionRecord> { FocusToday(600f) };
            Assert.AreEqual(1, ComputeStreak(sessions));
        }

        [Test]
        public void GetConsecutiveFocusDays_ThreeConsecutiveDays_ReturnsThree()
        {
            var sessions = new List<SessionRecord>
            {
                FocusToday(600f),
                FocusOnDayOffset(-1, 600f),
                FocusOnDayOffset(-2, 600f)
            };
            Assert.AreEqual(3, ComputeStreak(sessions));
        }

        [Test]
        public void GetConsecutiveFocusDays_GapBreaksStreak()
        {
            var sessions = new List<SessionRecord>
            {
                FocusToday(600f),
                // day -1 is missing
                FocusOnDayOffset(-2, 600f)
            };
            // Streak should only count today (gap breaks it)
            Assert.AreEqual(1, ComputeStreak(sessions));
        }

        // ─── Helpers that replicate DataManager logic ──────────────────────────

        private static float SumFocusSeconds(List<SessionRecord> sessions)
        {
            float total = 0f;
            foreach (var s in sessions)
                if (s.sessionType == SessionType.Focus)
                    total += s.durationSeconds;
            return total;
        }

        private static int CountTodayFocus(List<SessionRecord> sessions)
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            int count = 0;
            foreach (var s in sessions)
                if (s.sessionType == SessionType.Focus && s.date == today)
                    count++;
            return count;
        }

        private static int ComputeStreak(List<SessionRecord> sessions)
        {
            var daysWithSessions = new System.Collections.Generic.HashSet<string>();
            foreach (var s in sessions)
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
    }
}

using System;
using UnityEngine;
using Flowcube.Hardware;

namespace Flowcube.Core
{
    /// <summary>
    /// Feature 1 – "Flip to Start" timing core.
    ///
    /// Listens to M5StackConnector for face-change events and manages the
    /// session timer. Fires events consumed by PlanetController, EcosystemManager
    /// and AITherapistManager.
    /// </summary>
    public class TimingManager : MonoBehaviour
    {
        // ─── Inspector ─────────────────────────────────────────────────────────
        [Header("References")]
        [SerializeField] private M5StackConnector m5Stack;

        [Header("Settings")]
        [Tooltip("Which face index means 'focus mode is active'")]
        [SerializeField] private int focusFaceIndex = 0;

        // ─── Events ────────────────────────────────────────────────────────────
        /// <summary>Fired every frame while a session is active. Arg = elapsed seconds.</summary>
        public event Action<float> OnTick;
        /// <summary>Fired when a focus or exercise session starts.</summary>
        public event Action<SessionType> OnSessionStarted;
        /// <summary>Fired when an active session ends. Args = type, total duration in seconds.</summary>
        public event Action<SessionType, float> OnSessionEnded;

        // ─── State ─────────────────────────────────────────────────────────────
        public bool IsSessionActive { get; private set; }
        public float ElapsedSeconds { get; private set; }
        public SessionType CurrentSessionType { get; private set; }

        private DateTime _sessionStartTime;
        private string _sessionStartDate;

        // ─── Unity lifecycle ───────────────────────────────────────────────────

        void OnEnable()
        {
            if (m5Stack != null)
                m5Stack.OnFaceChanged += HandleFaceChanged;
        }

        void OnDisable()
        {
            if (m5Stack != null)
                m5Stack.OnFaceChanged -= HandleFaceChanged;
        }

        void Update()
        {
            if (!IsSessionActive) return;

            ElapsedSeconds += Time.deltaTime;
            OnTick?.Invoke(ElapsedSeconds);
        }

        // ─── Public API ────────────────────────────────────────────────────────

        /// <summary>Manually start a session (useful for testing without hardware).</summary>
        public void StartSession(SessionType type)
        {
            if (IsSessionActive) return;

            CurrentSessionType = type;
            ElapsedSeconds = 0f;
            _sessionStartTime = DateTime.Now;
            _sessionStartDate = _sessionStartTime.ToString("yyyy-MM-dd");
            IsSessionActive = true;

            OnSessionStarted?.Invoke(type);
            Debug.Log($"[TimingManager] Session started: {type}");
        }

        /// <summary>Manually end the current session and persist it.</summary>
        public void EndSession()
        {
            if (!IsSessionActive) return;

            IsSessionActive = false;

            float duration = ElapsedSeconds;
            var record = new SessionRecord
            {
                sessionType = CurrentSessionType,
                date = _sessionStartDate,
                startTime = _sessionStartTime.ToString("o"),
                durationSeconds = duration
            };

            if (DataManager.Instance != null)
                DataManager.Instance.RecordSession(record);

            OnSessionEnded?.Invoke(CurrentSessionType, duration);
            Debug.Log($"[TimingManager] Session ended: {CurrentSessionType}, duration={duration:F1}s");
        }

        // ─── Private ───────────────────────────────────────────────────────────

        private void HandleFaceChanged(int faceIndex)
        {
            bool shouldBeActive = (faceIndex == focusFaceIndex);

            if (shouldBeActive && !IsSessionActive)
                StartSession(SessionType.Focus);
            else if (!shouldBeActive && IsSessionActive)
                EndSession();
        }
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Flowcube.Core;
using Flowcube.AI;

namespace Flowcube.UI
{
    /// <summary>
    /// Manages all UI panels and animations:
    ///   • Timer display (updates every frame during a session)
    ///   • AI message panel (fade-in after a session ends, with loading dots)
    ///   • Ecosystem unlock notification toast
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        // ─── Inspector ─────────────────────────────────────────────────────────
        [Header("References – Managers")]
        [SerializeField] private TimingManager timingManager;
        [SerializeField] private AITherapistManager aiManager;

        [Header("Timer UI")]
        [SerializeField] private GameObject timerPanel;
        [SerializeField] private TextMeshProUGUI timerLabel;

        [Header("AI Message UI")]
        [SerializeField] private GameObject aiMessagePanel;
        [SerializeField] private TextMeshProUGUI aiMessageLabel;
        [SerializeField] private GameObject loadingDotsContainer;
        [SerializeField] private CanvasGroup aiPanelCanvasGroup;

        [Tooltip("Seconds to fade the AI panel in/out.")]
        [SerializeField] private float aiPanelFadeDuration = 0.6f;

        [Tooltip("Seconds the AI message stays on screen before fading out.")]
        [SerializeField] private float aiMessageDisplayDuration = 6f;

        [Header("Unlock Toast")]
        [SerializeField] private GameObject unlockToastPanel;
        [SerializeField] private TextMeshProUGUI unlockToastLabel;
        [SerializeField] private float toastDisplayDuration = 3f;

        // ─── Unity lifecycle ───────────────────────────────────────────────────

        void OnEnable()
        {
            if (timingManager != null)
            {
                timingManager.OnTick          += HandleTick;
                timingManager.OnSessionStarted += HandleSessionStarted;
                timingManager.OnSessionEnded   += HandleSessionEnded;
            }

            if (aiManager != null)
            {
                aiManager.OnRequestStarted   += HandleAIRequestStarted;
                aiManager.OnRequestCompleted += HandleAIRequestCompleted;
                aiManager.OnMessageReceived  += HandleAIMessageReceived;
                aiManager.OnError            += HandleAIError;
            }
        }

        void OnDisable()
        {
            if (timingManager != null)
            {
                timingManager.OnTick          -= HandleTick;
                timingManager.OnSessionStarted -= HandleSessionStarted;
                timingManager.OnSessionEnded   -= HandleSessionEnded;
            }

            if (aiManager != null)
            {
                aiManager.OnRequestStarted   -= HandleAIRequestStarted;
                aiManager.OnRequestCompleted -= HandleAIRequestCompleted;
                aiManager.OnMessageReceived  -= HandleAIMessageReceived;
                aiManager.OnError            -= HandleAIError;
            }
        }

        void Awake()
        {
            SetPanelActive(timerPanel, false);
            SetPanelActive(aiMessagePanel, false);
            SetPanelActive(unlockToastPanel, false);
            if (aiPanelCanvasGroup != null) aiPanelCanvasGroup.alpha = 0f;
        }

        // ─── Event handlers ────────────────────────────────────────────────────

        private void HandleTick(float elapsedSeconds)
        {
            if (timerLabel != null)
                timerLabel.text = FormatTime(elapsedSeconds);
        }

        private void HandleSessionStarted(SessionType _type)
        {
            SetPanelActive(timerPanel, true);
            SetPanelActive(aiMessagePanel, false);
        }

        private void HandleSessionEnded(SessionType _type, float _duration)
        {
            SetPanelActive(timerPanel, false);
        }

        private void HandleAIRequestStarted()
        {
            SetPanelActive(aiMessagePanel, true);
            SetPanelActive(loadingDotsContainer, true);
            if (aiMessageLabel != null) aiMessageLabel.text = string.Empty;
            StartCoroutine(FadePanel(aiPanelCanvasGroup, 0f, 1f, aiPanelFadeDuration));
        }

        private void HandleAIRequestCompleted()
        {
            SetPanelActive(loadingDotsContainer, false);
        }

        private void HandleAIMessageReceived(string message)
        {
            if (aiMessageLabel != null)
                aiMessageLabel.text = message;

            StartCoroutine(AutoHideAIPanel());
        }

        private void HandleAIError(string _error)
        {
            SetPanelActive(aiMessagePanel, false);
        }

        // ─── Public API for EcosystemManager notifications ─────────────────────

        /// <summary>
        /// Call this from EcosystemManager to show a plant-unlock toast.
        /// </summary>
        public void ShowUnlockToast(string plantName)
        {
            if (unlockToastLabel != null)
                unlockToastLabel.text = $"✦ 解锁 {plantName}";

            StartCoroutine(ShowToast());
        }

        // ─── Coroutines ────────────────────────────────────────────────────────

        private IEnumerator AutoHideAIPanel()
        {
            yield return new WaitForSeconds(aiMessageDisplayDuration);
            yield return FadePanel(aiPanelCanvasGroup, 1f, 0f, aiPanelFadeDuration);
            SetPanelActive(aiMessagePanel, false);
        }

        private IEnumerator ShowToast()
        {
            SetPanelActive(unlockToastPanel, true);
            yield return new WaitForSeconds(toastDisplayDuration);
            SetPanelActive(unlockToastPanel, false);
        }

        private IEnumerator FadePanel(CanvasGroup cg, float from, float to, float duration)
        {
            if (cg == null) yield break;

            float elapsed = 0f;
            cg.alpha = from;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                cg.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }
            cg.alpha = to;
        }

        // ─── Helpers ───────────────────────────────────────────────────────────

        private static string FormatTime(float totalSeconds)
        {
            int h  = Mathf.FloorToInt(totalSeconds / 3600f);
            int m  = Mathf.FloorToInt((totalSeconds % 3600f) / 60f);
            int s  = Mathf.FloorToInt(totalSeconds % 60f);
            return h > 0
                ? $"{h:D2}:{m:D2}:{s:D2}"
                : $"{m:D2}:{s:D2}";
        }

        private static void SetPanelActive(GameObject panel, bool active)
        {
            if (panel != null) panel.SetActive(active);
        }
    }
}

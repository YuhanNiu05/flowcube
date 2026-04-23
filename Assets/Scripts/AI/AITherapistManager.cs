using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Flowcube.Core;

namespace Flowcube.AI
{
    /// <summary>
    /// Feature 3 – "Short-Memory AI Therapist"
    ///
    /// Collects behavioural context from DataManager, builds a Prompt, and
    /// sends it to the configured AI backend (DeepSeek / Kimi / ZhipuAI).
    ///
    /// The system prompt constrains the AI to:
    ///   • Reply in 30 Chinese characters or fewer
    ///   • Adopt a warm, gentle, restrained persona ("治愈、温暖、克制")
    ///   • Reference specific user context details in every message
    /// </summary>
    public class AITherapistManager : MonoBehaviour
    {
        // ─── Events ────────────────────────────────────────────────────────────
        /// <summary>Fired when the AI response is received successfully.</summary>
        public event Action<string> OnMessageReceived;
        /// <summary>Fired when the request starts (use to show loading UI).</summary>
        public event Action OnRequestStarted;
        /// <summary>Fired when the request completes, whether success or failure.</summary>
        public event Action OnRequestCompleted;
        /// <summary>Fired on network or API error.</summary>
        public event Action<string> OnError;

        // ─── Inspector ─────────────────────────────────────────────────────────
        [Header("API Configuration")]
        [Tooltip("Full URL of the AI completion endpoint.")]
        [SerializeField] private string apiEndpoint = "https://api.deepseek.com/v1/chat/completions";

        [Tooltip("API key. Store this securely; do NOT commit real keys.")]
        [SerializeField] private string apiKey = "YOUR_API_KEY_HERE";

        [Tooltip("Model identifier, e.g. deepseek-chat, moonshot-v1-8k, glm-4.")]
        [SerializeField] private string modelName = "deepseek-chat";

        [Tooltip("Maximum tokens for the AI response.")]
        [SerializeField] private int maxTokens = 60;

        [Tooltip("Request timeout in seconds.")]
        [SerializeField] private float requestTimeoutSeconds = 15f;

        // ─── System Prompt ─────────────────────────────────────────────────────
        private const string SystemPrompt =
            "你是一位来自星球的温柔守望者，名叫「星岚」。" +
            "你的回复必须满足以下所有要求：" +
            "1. 字数严格不超过30字；" +
            "2. 语气温暖、治愈、克制，不使用感叹号；" +
            "3. 必须自然地融入用户提供的至少一个具体情境细节；" +
            "4. 结尾不要有任何标点符号。";

        // ─── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Gathers current context from DataManager and fires the AI request.
        /// Call after a session ends to get a personalised message.
        /// </summary>
        /// <param name="lastSessionType">Type of the session that just ended.</param>
        /// <param name="lastSessionDuration">Duration in seconds of the last session.</param>
        public void RequestMessage(SessionType lastSessionType, float lastSessionDuration)
        {
            var context = BuildContext(lastSessionType, lastSessionDuration);
            StartCoroutine(SendRequest(context));
        }

        // ─── Context builder ───────────────────────────────────────────────────

        private AIRequestContext BuildContext(SessionType lastSessionType, float lastSessionDuration)
        {
            var dm = DataManager.Instance;
            return new AIRequestContext
            {
                todayFocusCount          = dm != null ? dm.GetTodayFocusCount()     : 0,
                todayFocusSeconds        = dm != null ? dm.GetTodayFocusSeconds()   : 0f,
                todayExerciseSeconds     = dm != null ? dm.GetTodayExerciseSeconds(): 0f,
                currentHour              = DateTime.Now.Hour,
                consecutiveFocusDays     = dm != null ? dm.GetConsecutiveFocusDays(): 0,
                lastSessionDurationSeconds = lastSessionDuration,
                lastSessionType          = lastSessionType.ToString()
            };
        }

        // ─── Prompt builder ────────────────────────────────────────────────────

        private string BuildUserMessage(AIRequestContext ctx)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"现在是{ctx.TimeOfDayLabel()}（{ctx.currentHour}点）。");
            sb.AppendLine($"这是用户今天第{ctx.todayFocusCount}次专注，本次持续了{ctx.LastSessionDurationLabel()}。");

            if (ctx.consecutiveFocusDays > 1)
                sb.AppendLine($"用户已经连续专注{ctx.consecutiveFocusDays}天。");

            if (ctx.todayExerciseSeconds > 60f)
            {
                int exMinutes = Mathf.RoundToInt(ctx.todayExerciseSeconds / 60f);
                sb.AppendLine($"用户今天还运动了{exMinutes}分钟。");
            }

            sb.AppendLine("请根据以上情境，对用户说一句话。");
            return sb.ToString().Trim();
        }

        // ─── HTTP Request ──────────────────────────────────────────────────────

        private IEnumerator SendRequest(AIRequestContext context)
        {
            OnRequestStarted?.Invoke();

            string userMessage = BuildUserMessage(context);

            // Build request body (OpenAI-compatible format used by DeepSeek / Kimi / GLM)
            var requestBody = new ChatCompletionRequest
            {
                model = modelName,
                max_tokens = maxTokens,
                messages = new[]
                {
                    new ChatMessage { role = "system", content = SystemPrompt },
                    new ChatMessage { role = "user",   content = userMessage  }
                }
            };

            string json = JsonUtility.ToJson(requestBody);
            byte[] bodyBytes = Encoding.UTF8.GetBytes(json);

            using var request = new UnityWebRequest(apiEndpoint, "POST");
            request.uploadHandler   = new UploadHandlerRaw(bodyBytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            request.timeout = Mathf.RoundToInt(requestTimeoutSeconds);

            yield return request.SendWebRequest();

            OnRequestCompleted?.Invoke();

            if (request.result != UnityWebRequest.Result.Success)
            {
                string err = $"[AITherapistManager] Request failed: {request.error}";
                Debug.LogError(err);
                OnError?.Invoke(request.error);
                yield break;
            }

            string responseJson = request.downloadHandler.text;
            string message = ParseMessage(responseJson);

            if (string.IsNullOrEmpty(message))
            {
                OnError?.Invoke("Empty or unreadable AI response.");
                yield break;
            }

            Debug.Log($"[AITherapistManager] AI says: {message}");
            OnMessageReceived?.Invoke(message);
        }

        // ─── Response parser ───────────────────────────────────────────────────

        private static string ParseMessage(string json)
        {
            try
            {
                var response = JsonUtility.FromJson<ChatCompletionResponse>(json);
                if (response?.choices != null && response.choices.Length > 0)
                    return response.choices[0].message?.content?.Trim();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[AITherapistManager] Failed to parse response: {e.Message}");
            }
            return null;
        }

        // ─── JSON models ───────────────────────────────────────────────────────

        [Serializable]
        private class ChatCompletionRequest
        {
            public string model;
            public int max_tokens;
            public ChatMessage[] messages;
        }

        [Serializable]
        private class ChatMessage
        {
            public string role;
            public string content;
        }

        [Serializable]
        private class ChatCompletionResponse
        {
            public Choice[] choices;
        }

        [Serializable]
        private class Choice
        {
            public ChatMessage message;
        }
    }
}

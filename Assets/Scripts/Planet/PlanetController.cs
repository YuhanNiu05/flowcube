using UnityEngine;
using Flowcube.Core;

namespace Flowcube.Planet
{
    /// <summary>
    /// Feature 1 – "Focus Lights the Planet"
    ///
    /// Drives the visual state of the virtual planet based on session
    /// elapsed time:
    ///   • Planet surface light/colour transitions from grey → vibrant
    ///   • Glowing grass appears progressively during the first session minutes
    ///
    /// Attach to the root GameObject of the planet prefab.
    /// Wire up references in the Inspector.
    /// </summary>
    public class PlanetController : MonoBehaviour
    {
        // ─── Inspector ─────────────────────────────────────────────────────────
        [Header("References")]
        [SerializeField] private TimingManager timingManager;
        [SerializeField] private Light planetAmbientLight;
        [SerializeField] private Renderer planetSurfaceRenderer;

        [Header("Visual – Idle State")]
        [SerializeField] private Color idleAmbientColor = new Color(0.15f, 0.15f, 0.2f);
        [SerializeField] private float idleAmbientIntensity = 0.3f;
        [SerializeField] private Color idleSurfaceColor = new Color(0.4f, 0.4f, 0.45f);

        [Header("Visual – Active State")]
        [SerializeField] private Color activeLightColor = new Color(0.6f, 0.9f, 1.0f);
        [SerializeField] private float activeAmbientIntensity = 1.2f;
        [SerializeField] private Color activeSurfaceColor = new Color(0.2f, 0.7f, 0.5f);

        [Header("Animation")]
        [Tooltip("Seconds to fully transition between idle and active states.")]
        [SerializeField] private float transitionDuration = 3f;

        [Tooltip("After how many seconds of focus the grass effect is fully visible.")]
        [SerializeField] private float grassFullGrowthSeconds = 120f;

        [Header("Grass Effect")]
        [SerializeField] private ParticleSystem grassParticleSystem;

        // ─── State ─────────────────────────────────────────────────────────────
        private float _blendTarget;   // 0 = idle, 1 = fully active
        private float _currentBlend;
        private static readonly int ColorProperty = Shader.PropertyToID("_BaseColor");

        // ─── Unity lifecycle ───────────────────────────────────────────────────

        void OnEnable()
        {
            if (timingManager != null)
            {
                timingManager.OnSessionStarted += HandleSessionStarted;
                timingManager.OnSessionEnded   += HandleSessionEnded;
                timingManager.OnTick           += HandleTick;
            }
        }

        void OnDisable()
        {
            if (timingManager != null)
            {
                timingManager.OnSessionStarted -= HandleSessionStarted;
                timingManager.OnSessionEnded   -= HandleSessionEnded;
                timingManager.OnTick           -= HandleTick;
            }
        }

        void Update()
        {
            // Smoothly interpolate towards the blend target
            if (!Mathf.Approximately(_currentBlend, _blendTarget))
            {
                float step = Time.deltaTime / transitionDuration;
                _currentBlend = Mathf.MoveTowards(_currentBlend, _blendTarget, step);
                ApplyBlend(_currentBlend);
            }
        }

        // ─── Event handlers ────────────────────────────────────────────────────

        private void HandleSessionStarted(SessionType type)
        {
            if (type == SessionType.Focus)
                _blendTarget = 1f;
        }

        private void HandleSessionEnded(SessionType type, float _duration)
        {
            _blendTarget = 0f;
            StopGrassEffect();
        }

        private void HandleTick(float elapsedSeconds)
        {
            // Gradually grow grass during a focus session
            if (grassParticleSystem != null)
            {
                float grassProgress = Mathf.Clamp01(elapsedSeconds / grassFullGrowthSeconds);
                var emission = grassParticleSystem.emission;
                emission.rateOverTime = Mathf.Lerp(0f, 50f, grassProgress);

                if (!grassParticleSystem.isPlaying && grassProgress > 0f)
                    grassParticleSystem.Play();
            }
        }

        // ─── Private helpers ───────────────────────────────────────────────────

        private void ApplyBlend(float t)
        {
            if (planetAmbientLight != null)
            {
                planetAmbientLight.color = Color.Lerp(idleAmbientColor, activeLightColor, t);
                planetAmbientLight.intensity = Mathf.Lerp(idleAmbientIntensity, activeAmbientIntensity, t);
            }

            if (planetSurfaceRenderer != null)
            {
                Color surface = Color.Lerp(idleSurfaceColor, activeSurfaceColor, t);
                planetSurfaceRenderer.material.SetColor(ColorProperty, surface);
            }
        }

        private void StopGrassEffect()
        {
            if (grassParticleSystem != null && grassParticleSystem.isPlaying)
                grassParticleSystem.Stop();
        }
    }
}

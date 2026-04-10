using System.Collections.Generic;
using UnityEngine;
using Flowcube.Core;

namespace Flowcube.Ecosystem
{
    /// <summary>
    /// Feature 2 – "Dynamic Ecosystem Generation"
    ///
    /// Listens for session-end events and evaluates which Low-Poly plants /
    /// relics should be instantiated on the planet based on the user's
    /// cumulative focus history.
    ///
    /// Milestone examples (configurable via PlantData ScriptableObjects):
    ///   • 15 min total  → glowing grass patch  (level 1)
    ///   • 45 min total  → glowing tree         (level 2)
    ///   • 3 consecutive days → ancient relic   (level 3)
    /// </summary>
    public class EcosystemManager : MonoBehaviour
    {
        // ─── Inspector ─────────────────────────────────────────────────────────
        [Header("References")]
        [SerializeField] private TimingManager timingManager;

        [Tooltip("Parent transform on the planet surface where plants are spawned.")]
        [SerializeField] private Transform planetSurfaceAnchor;

        [Header("Plant Catalogue")]
        [Tooltip("All PlantData assets, ordered by ascending ecosystemLevel.")]
        [SerializeField] private List<PlantData> plantCatalogue;

        // ─── State ─────────────────────────────────────────────────────────────
        /// <summary>Live instances currently on the planet (level → instance).</summary>
        private readonly Dictionary<int, GameObject> _spawnedPlants = new Dictionary<int, GameObject>();

        // ─── Unity lifecycle ───────────────────────────────────────────────────

        void OnEnable()
        {
            if (timingManager != null)
                timingManager.OnSessionEnded += HandleSessionEnded;
        }

        void OnDisable()
        {
            if (timingManager != null)
                timingManager.OnSessionEnded -= HandleSessionEnded;
        }

        void Start()
        {
            // Re-spawn any plants the user had already unlocked in a previous session.
            RebuildFromSavedProgress();
        }

        // ─── Event handlers ────────────────────────────────────────────────────

        private void HandleSessionEnded(SessionType type, float _duration)
        {
            // Only focus sessions drive ecosystem growth.
            if (type != SessionType.Focus) return;

            EvaluateAndSpawnPlants();
        }

        // ─── Core logic ────────────────────────────────────────────────────────

        /// <summary>
        /// Checks every PlantData entry and instantiates any whose unlock
        /// requirements are now met but haven't been spawned yet.
        /// </summary>
        public void EvaluateAndSpawnPlants()
        {
            if (DataManager.Instance == null) return;

            float totalFocusSeconds   = DataManager.Instance.GetTotalFocusSeconds();
            int   consecutiveDays     = DataManager.Instance.GetConsecutiveFocusDays();
            int   currentMaxLevel     = DataManager.Instance.GetMaxUnlockedEcosystemLevel();

            foreach (var plant in plantCatalogue)
            {
                if (plant == null) continue;
                if (_spawnedPlants.ContainsKey(plant.ecosystemLevel)) continue; // already on planet

                bool focusMet = totalFocusSeconds >= plant.requiredTotalFocusSeconds;
                bool streakMet = consecutiveDays >= plant.requiredConsecutiveDays;

                if (focusMet && streakMet)
                {
                    SpawnPlant(plant);
                    DataManager.Instance.UpdateMaxUnlockedEcosystemLevel(plant.ecosystemLevel);
                    Debug.Log($"[EcosystemManager] Unlocked '{plant.displayName}' (level {plant.ecosystemLevel})");
                }
            }
        }

        // ─── Private helpers ───────────────────────────────────────────────────

        private void SpawnPlant(PlantData data)
        {
            if (data.prefab == null)
            {
                Debug.LogWarning($"[EcosystemManager] PlantData '{data.displayName}' has no prefab assigned.");
                return;
            }

            Transform parent = planetSurfaceAnchor != null ? planetSurfaceAnchor : transform;
            GameObject instance = Instantiate(data.prefab, parent);
            instance.transform.localPosition = data.positionOffset;
            instance.transform.localRotation = Quaternion.Euler(0f, data.yRotation, 0f);
            instance.transform.localScale    = Vector3.one * data.scale;
            instance.name = $"Plant_{data.displayName}_L{data.ecosystemLevel}";

            _spawnedPlants[data.ecosystemLevel] = instance;
        }

        private void RebuildFromSavedProgress()
        {
            if (DataManager.Instance == null) return;

            float totalFocusSeconds   = DataManager.Instance.GetTotalFocusSeconds();
            int   consecutiveDays     = DataManager.Instance.GetConsecutiveFocusDays();

            foreach (var plant in plantCatalogue)
            {
                if (plant == null) continue;
                bool focusMet  = totalFocusSeconds >= plant.requiredTotalFocusSeconds;
                bool streakMet = consecutiveDays    >= plant.requiredConsecutiveDays;

                if (focusMet && streakMet && !_spawnedPlants.ContainsKey(plant.ecosystemLevel))
                    SpawnPlant(plant);
            }
        }
    }
}

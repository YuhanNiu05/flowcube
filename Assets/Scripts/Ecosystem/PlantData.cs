using System;
using UnityEngine;

namespace Flowcube.Ecosystem
{
    /// <summary>
    /// Data descriptor for a single Low-Poly plant / ecosystem milestone.
    ///
    /// These are created as ScriptableObject assets under
    /// Assets/Resources/PlantConfigs/ so designers can tune values without
    /// touching code.
    /// </summary>
    [CreateAssetMenu(fileName = "PlantData", menuName = "Flowcube/Plant Data", order = 1)]
    public class PlantData : ScriptableObject
    {
        [Tooltip("Human-readable name shown in the UI.")]
        public string displayName;

        [Tooltip("Prefab to instantiate on the planet surface.")]
        public GameObject prefab;

        [Header("Unlock Requirements")]
        [Tooltip("Minimum total focus seconds required (all-time) to unlock this plant.")]
        public float requiredTotalFocusSeconds;

        [Tooltip("Minimum consecutive focus days required to unlock this plant (0 = no streak needed).")]
        public int requiredConsecutiveDays;

        [Header("Placement")]
        [Tooltip("Local position offset relative to the planet surface anchor.")]
        public Vector3 positionOffset;

        [Tooltip("Local Y rotation applied when this plant is spawned.")]
        public float yRotation;

        [Tooltip("Uniform scale applied to the spawned prefab.")]
        public float scale = 1f;

        [Header("Ecosystem Level")]
        [Tooltip("Internal level index; higher = rarer. Used by EcosystemManager to track progress.")]
        public int ecosystemLevel;

        /// <summary>Human-readable description of what the user achieved to unlock this.</summary>
        [TextArea(2, 4)]
        public string unlockDescription;
    }
}

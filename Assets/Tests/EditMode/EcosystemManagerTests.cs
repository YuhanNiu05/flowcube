using System.Collections.Generic;
using NUnit.Framework;
using Flowcube.Core;
using Flowcube.Ecosystem;

namespace Flowcube.Tests
{
    /// <summary>
    /// Edit-mode tests for the EcosystemManager plant unlock logic.
    ///
    /// We test the pure eligibility check in isolation (no MonoBehaviour lifecycle).
    /// </summary>
    public class EcosystemManagerTests
    {
        // ─── Plant unlock eligibility helper ──────────────────────────────────
        // Mirrors the logic inside EcosystemManager.EvaluateAndSpawnPlants()

        private static bool IsPlantUnlocked(
            PlantDataDto plant,
            float totalFocusSeconds,
            int consecutiveDays)
        {
            return totalFocusSeconds >= plant.RequiredTotalFocusSeconds
                && consecutiveDays   >= plant.RequiredConsecutiveDays;
        }

        // ─── Helper DTO (avoids dependency on ScriptableObject in tests) ───────

        private class PlantDataDto
        {
            public string DisplayName                  { get; set; }
            public float  RequiredTotalFocusSeconds    { get; set; }
            public int    RequiredConsecutiveDays      { get; set; }
            public int    EcosystemLevel               { get; set; }
        }

        // ─── Tests ─────────────────────────────────────────────────────────────

        [Test]
        public void GlowingGrass_UnlocksAfter15Minutes()
        {
            var grass = new PlantDataDto
            {
                DisplayName               = "荧光草",
                RequiredTotalFocusSeconds = 15 * 60f,  // 900 s
                RequiredConsecutiveDays   = 0,
                EcosystemLevel            = 1
            };

            Assert.IsFalse(IsPlantUnlocked(grass, 899f, 1),  "Should not unlock at 899 s");
            Assert.IsTrue (IsPlantUnlocked(grass, 900f, 1),  "Should unlock exactly at 900 s");
            Assert.IsTrue (IsPlantUnlocked(grass, 1800f, 0), "Should unlock well past threshold");
        }

        [Test]
        public void GlowingTree_UnlocksAfter45Minutes()
        {
            var tree = new PlantDataDto
            {
                DisplayName               = "发光树",
                RequiredTotalFocusSeconds = 45 * 60f,  // 2700 s
                RequiredConsecutiveDays   = 0,
                EcosystemLevel            = 2
            };

            Assert.IsFalse(IsPlantUnlocked(tree, 2699f, 0), "Should not unlock at 2699 s");
            Assert.IsTrue (IsPlantUnlocked(tree, 2700f, 0), "Should unlock exactly at 2700 s");
        }

        [Test]
        public void AncientRelic_RequiresThreeConsecutiveDays()
        {
            var relic = new PlantDataDto
            {
                DisplayName               = "遗迹石碑",
                RequiredTotalFocusSeconds = 45 * 60f,  // need tree first
                RequiredConsecutiveDays   = 3,
                EcosystemLevel            = 3
            };

            // Enough focus but only 2 consecutive days → not unlocked
            Assert.IsFalse(IsPlantUnlocked(relic, 5400f, 2), "Streak of 2 is not enough");

            // Enough focus AND 3 consecutive days → unlocked
            Assert.IsTrue (IsPlantUnlocked(relic, 5400f, 3), "Should unlock with streak of 3");
        }

        [Test]
        public void NoPlants_UnlockedWithZeroFocusTime()
        {
            var plants = new List<PlantDataDto>
            {
                new PlantDataDto { RequiredTotalFocusSeconds = 900f,  RequiredConsecutiveDays = 0 },
                new PlantDataDto { RequiredTotalFocusSeconds = 2700f, RequiredConsecutiveDays = 0 },
                new PlantDataDto { RequiredTotalFocusSeconds = 2700f, RequiredConsecutiveDays = 3 }
            };

            foreach (var p in plants)
                Assert.IsFalse(IsPlantUnlocked(p, 0f, 0), "No plant should unlock with 0 s focus");
        }

        [Test]
        public void AllBasicPlants_UnlockedAfterLongSession()
        {
            // A single 2-hour session (7200 s) and a 3-day streak should unlock all.
            float focus = 7200f;
            int   streak = 5;

            var plants = new List<PlantDataDto>
            {
                new PlantDataDto { RequiredTotalFocusSeconds = 900f,  RequiredConsecutiveDays = 0 },
                new PlantDataDto { RequiredTotalFocusSeconds = 2700f, RequiredConsecutiveDays = 0 },
                new PlantDataDto { RequiredTotalFocusSeconds = 2700f, RequiredConsecutiveDays = 3 }
            };

            foreach (var p in plants)
                Assert.IsTrue(IsPlantUnlocked(p, focus, streak), $"Plant should be unlocked (req={p.RequiredTotalFocusSeconds}s, streak={p.RequiredConsecutiveDays})");
        }
    }
}

using UnityEngine;

namespace Combat {
    public static class UnitTest {
        /// <summary>
        /// Pass
        /// </summary>
        public static void TestUpdateFinal() {
            ref readonly FinalStats final = ref StageController.Player.Stats;
            float ogBase = StageController.Player.BaseStats.Atk;
            float ogScale = StageController.Player.ScalingStats.Atk;

            StageController.Player.GainStats(new(atk: 67), null);
            Debug.Assert(Mathf.Abs(final.Atk - (ogBase + 67) * (1+ogScale)) < 1e-4f, "Incorrect final stats upon reassingment with base change");

            StageController.Player.GainStats(null, new(atk: 0.2f));
            Debug.Assert(Mathf.Abs(final.Atk - (ogBase + 67) * (1+ogScale+.2f)) < 1E-4f, "Incorrect final stats upon reassignment with scaling change");

            StageController.Player.GainStats(new(atk:-7), new(atk: 0.1f));
            Debug.Assert(Mathf.Abs(final.Atk - (ogBase + 60) * (1+ogScale+.3f)) < 1E-4f, "Incorrect final stats upon reassignment with both base stats and scaling stats change");

        }
    }
}
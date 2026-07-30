using UnityEngine;

namespace Combat {
    public static class UnitTest {
        /// <summary>
        /// Pass
        /// </summary>
        public static void TestUpdateFinal() {
            FinalStats final = StageController.Player.Stats;
            float ogBase = StageController.Player.BaseStats[BaseAttribute.Atk];
            float ogScale = StageController.Player.ScalingStats[BaseAttribute.Atk];
            float ogDmgRed = StageController.Player.ScalingStats[ScalingAttribute.DmgReduction];


            float ogDef = StageController.Player.BaseStats[BaseAttribute.Def];
            float ogDefScale = StageController.Player.ScalingStats[BaseAttribute.Def];

            Debug.Log($"Attack: {ogBase} {ogScale}");

            StageController.Player.BaseStats.Gain((BaseAttribute.Atk, 67));
            Debug.Assert(Mathf.Abs(final[BaseAttribute.Atk] - (ogBase + 67) * (1+ogScale)) < 1e-4f, $"Incorrect final stats upon reassingment with base change; expected {(ogBase + 67) * (1 + ogScale)}, got {final[BaseAttribute.Atk]} instead");

            Debug.Log($"Attack: {ogBase} {ogScale}");

            StageController.Player.ScalingStats.Gain((BaseAttribute.Atk, 0.2f));
            Debug.Log(StageController.Player.ScalingStats[BaseAttribute.Atk]);
            Debug.Assert(Mathf.Abs(final[BaseAttribute.Atk] - (ogBase + 67) * (1+ogScale+.2f)) < 1E-4f, $"Incorrect final stats upon reassignment with scaling change expected {(ogBase + 67) * (1 + ogScale + .2f)}, got {final[BaseAttribute.Atk]}");

            StageController.Player.BaseStats.Gain((BaseAttribute.Atk, -7));
            StageController.Player.ScalingStats.Gain((BaseAttribute.Atk, 0.1f));
            Debug.Assert(Mathf.Abs(final[BaseAttribute.Atk] - (ogBase + 60) * (1+ogScale+.3f)) < 1E-4f, "Incorrect final stats upon reassignment with both base stats and scaling stats change");

            StageController.Player.ScalingStats.Gain((ScalingAttribute.DmgReduction, 0.3f));
            Debug.Assert(Mathf.Abs(final[ScalingAttribute.DmgReduction] - (ogDmgRed + 0.3f)) < 1E-4f, "Incorrect final stats upon reassignment of a scaling stat");

            StageController.Player.BaseStats.Gain((BaseAttribute.Def, 50));
            StageController.Player.ScalingStats.Lose((BaseAttribute.Def, 0.2f));
            Debug.Assert(Mathf.Abs(final[BaseAttribute.Def] - (ogDef + 50) * (1+ogDefScale-0.2f)) < 1E-4F, "");
        }
    }
}
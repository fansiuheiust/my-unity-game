using Combat;
using System;
using TMPro;
using UnityEngine;

namespace UI {
    public class StatsDisplay : MonoBehaviour {
        [SerializeField] TextMeshProUGUI info;

        public static readonly string ZEROSTAT = "#AAAAAA";
        static bool IsZero(float a) => Mathf.Abs(a) < 1e-5;
        static string Value(float f, bool percentage) => percentage ? $"{(f * 100):F0}%" : $"{f:F0}";
        static string AttributeValue(float f, bool percentage) => IsZero(f) ? $"<color={ZEROSTAT}>{(percentage? "0%": "0")}</color>":
            f > 0 ? $"<color={GearDisplay.GOODSTAT}>+{Value(f, percentage)}</color>"
            : $"<color={GearDisplay.BADSTAT}>{Value(f, percentage)}</color>";
        static string Stat(string name, float b, float s, float f) {
            return $"<color={GearDisplay.STATTEXT}>{name}</color><line-height=0em>\n<align=right>"
                + $"{AttributeValue(f, false)} ({AttributeValue(b, false)} {AttributeValue(s, true)})" 
                + "</align></line-height>\n";
        }
        static string Stat(string name, float s, float f) {
            return $"<color={GearDisplay.STATTEXT}>{name}</color><line-height=0em>\n<align=right>"
                + $"{AttributeValue(f, true)} ({AttributeValue(s, true)})"
                + "</align></line-height>\n";
        }
        public static string Stats(BaseStats b, ScalingStats s, FinalStats f) {
            string ri = "";
            foreach (BaseAttribute h in Enum.GetValues(typeof(BaseAttribute)))
                ri += $"{Stat(Global.HashedBaseStat(h), b[h], s[h], f[h])}";
            foreach (ScalingAttribute h in Enum.GetValues(typeof(ScalingAttribute)))
                ri += $"{Stat(Global.HashedScalingStat(h), s[h], f[h])}";

            return ri;
        }

        private void Start() {
            Player p = StageController.Player;
            info.text = Stats(p.BaseStats, p.ScalingStats, p.Stats);
        }
    }
}
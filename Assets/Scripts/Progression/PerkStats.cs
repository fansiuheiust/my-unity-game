using UnityEngine;


namespace Progression {
    public abstract class PerkStats {
        public readonly string name;
        public PerkStats(string name) {
            this.name = name;
        }
        public abstract string ValueInString { get; }
        public abstract float Value { get; }
    }

    public class IntStats: PerkStats {
        protected readonly int value;
        public IntStats(string name, int value): base(name) {
            this.value = value;
        }
        public override string ValueInString => value.ToString();
        public override float Value => Value;
    }

    public class DecimalStats : PerkStats {
        protected readonly float value;
        public DecimalStats(string name, float value) : base(name) {
            this.value = value;
        }
        public override string ValueInString => $"{value:F2}";
        public override float Value => Value;
    }

    public class PercentageStats: DecimalStats {
        public PercentageStats(string name, float value) : base(name, value) {
        }
        public override string ValueInString => $"{(value * 100):F2}%";
    }

}
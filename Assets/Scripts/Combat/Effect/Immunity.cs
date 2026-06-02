using System.Linq;
using UnityEngine;

namespace Combat {
    public class Immunity: Mob.Immunity { }
    public partial class Mob {
        public class Immunity : Effect {
            protected override void Begin() {
                Owner._isImmune = true;
            }
            public new void Apply(float duration) {
                base.Apply(duration);
            }

            protected override void End() {
                if (!Owner.Effects.Any(x=>x is Immunity && x != this))
                    Owner._isImmune = false;
            }
        }
    }
}

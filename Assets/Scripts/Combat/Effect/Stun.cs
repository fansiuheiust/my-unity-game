using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Combat {
    public class Stun : Effect {
        public bool IsInternal { get; private set; }

        public void Apply(float duration, bool isInternal) {
            IsInternal = isInternal;
            Apply(duration);
        }

        protected override void Begin() {
            Owner.IsStunned = true;
            if (!IsInternal) Owner.OnStunStart.Invoke(Owner);
        }
        protected override void End() {
            if (!Owner.Effects.Any(x => x is Stun && x != this)) {
                Owner.IsStunned = false;
            }

        }
    }
}

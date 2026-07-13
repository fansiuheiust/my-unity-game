
using UnityEngine;
using System.Collections;

namespace Combat {
    public abstract class Hostile : MobBehaviour {
        protected override bool Predicate(Mob m) => Owner.CanAttack(m);



        protected override void Awake() {
            base.Awake();
            Owner.OnAttackInterrupt.AddListener((m1, m2) => { m1.TakeStun(2, m2); });
        }

        
    }
}
using UnityEngine;
using Combat;


namespace Interactable {
    public abstract class Item<T> : MonoBehaviour, IInteractable {
        public T Value { get; private set; }
        public void Interact(Mob interacter) {
            Pick(interacter);
            Destroy(gameObject);
        }

        protected abstract void Pick(Mob picker);
        internal void Init(T item) { Value = item; }
    }
    public class GearItem: Item<Gear> {
        protected override void Pick(Mob picker) {
            picker.Equip(Value);
        }
    }
    public class StatBoostItem: Item<(BaseStats, ScalingStats)> {
        protected override void Pick(Mob picker) {
            picker.Stats.GainStats(Value.Item1, Value.Item2);
        }
    }
}
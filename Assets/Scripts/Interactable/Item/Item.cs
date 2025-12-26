using UnityEngine;
using Combat;


namespace Interactable {
    public abstract class Item<T> : MonoBehaviour, IInteractable {
        public T Value { get; private set; }
        public bool IsInteractable => true;
        public void Interact(Mob interacter) {
            Pick(interacter);
            Destroy(gameObject);
        }

        protected abstract void Pick(Mob picker);
        internal void Init(T item) { Value = item; }
    }
    /// <summary>
    /// T2: <c>Gear</c>
    /// </summary>
    public class GearItem: Item<Gear> {
        protected override void Pick(Mob picker) {
            picker.Equip(Value);
        }
    }
    /// <summary>
    /// T2: <c>(BaseStats, ScalingStats)</c>
    /// </summary>
    public class StatBoostItem: Item<(BaseStats, ScalingStats)> {
        protected override void Pick(Mob picker) {
            picker.Stats.GainStats(Value.Item1, Value.Item2);
        }
    }
}
using UnityEngine;
using Combat;
using Progression;

namespace BuildingBlocks {
    public abstract class ItemObject<T> : MonoBehaviour, IInteractable {
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
    public class GearObject: ItemObject<Gear> {
        protected override void Pick(Mob picker) {
            picker.Equip(Value);
        }
    }
    /// <summary>
    /// T2: <c>(BaseStats, ScalingStats)</c>
    /// </summary>
    public class BuffObject: ItemObject<(BaseStats, ScalingStats)> {
        protected override void Pick(Mob picker) {
            picker.Stats.GainStats(Value.Item1, Value.Item2);
        }
    }

    public class CoinObject: ItemObject<(CoinType, Rarity, uint)> {
        protected override void Pick(Mob picker) {
            if (picker is Player p) {
                p.PerkManager.GainCoin(Value.Item1, Value.Item2, Value.Item3);
            }
        }
    }
}
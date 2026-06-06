using UnityEngine;
using Combat;
using Progression;

namespace BuildingBlocks {
    public abstract class BaseItemObject : MonoBehaviour, IInteractable {
        // locking 
        bool _isInteractable = true;
        public bool IsInteractable => _isInteractable;
        public void Lock() => _isInteractable = false;
        public void Unlock() => _isInteractable = true;

        public abstract void Interact(Mob interacter);

    }
    public abstract class ItemObject<T> : BaseItemObject {
        public T Value { get; private set; }
        public override void Interact(Mob interacter) {
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
            picker.GainStats(Value.Item1, Value.Item2);
        }
    }

    public class CoinObject: ItemObject<(CoinType, uint, uint)> {
        protected override void Pick(Mob picker) {
            if (picker is Player p) {
                p.GainCoin(Value.Item1, Value.Item2, Value.Item3);
            }
        }
    }
}
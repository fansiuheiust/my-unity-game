using UnityEngine;
using Combat;
using BuildingBlocks;
using Progression;
using UnityEditor.SceneManagement;

namespace Loot {
    [System.Serializable]
    public abstract class Item {
        public GameObject Spawn(Vector3 coordinate) {
            GameObject go = MonoBehaviour.Instantiate((GameObject)Resources.Load("Prefabs/Interactable/Item"));
            go.transform.position = coordinate;
            Init(go);
            return go;
        }
        protected abstract void Init(GameObject go);
    }
    [System.Serializable]
    public class GearItem: Item {
        [field: SerializeField]
        public string GearId { get; private set; }
        public GearItem(string gearId) {
            GearId = gearId;
        }
        GearItem() { GearId = null; }
        protected override void Init(GameObject go) {
            GearObject obj = go.AddComponent<GearObject>();
            obj.Init(GearDatabase.GetById(GearId));
        }
    }
    [System.Serializable]
    public class Buff: Item {
        [field: SerializeField]
        public SerializedMobStats Stats { get; private set; }

        bool _isStatsInitialized = false;

        public Buff(BaseStats @base, ScalingStats scaling) {
            Stats = new() {
                @base = @base,
                scaling = scaling,
            };
            _isStatsInitialized = true;
        }

        Buff() {  }
        protected override void Init(GameObject go) {
            BuffObject obj = go.AddComponent<BuffObject>();
            if (!_isStatsInitialized) {
                Stats.InsertHasedStats();
                _isStatsInitialized = true;
            }
            obj.Init((Stats.@base.Clone(), Stats.scaling.Clone()));
        }
    }
    [System.Serializable]
    public class Coin: Item {
        [field: SerializeField]
        public CoinType Type { get; private set; }
        [field: SerializeField]
        public Rarity Rarity { get; private set; }
        [field: SerializeField]
        public uint Quantity { get; private set; }
        protected override void Init(GameObject go) {
            CoinObject obj = go.AddComponent<CoinObject>();
            obj.Init((Type, Rarity, Quantity));
        }
    }
}
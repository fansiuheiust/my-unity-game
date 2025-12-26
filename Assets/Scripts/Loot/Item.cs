using UnityEngine;
using Combat;
using Interactable;

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
        Buff() {  }
        protected override void Init(GameObject go) {
            BuffObject obj = go.AddComponent<BuffObject>();
            Stats.InsertHasedStats();
            obj.Init((Stats.@base, Stats.scaling));
        }
    }
}
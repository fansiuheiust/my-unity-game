using UnityEngine;

namespace Interactable {
    public static class InteractableSpawner {
        static readonly string path = "Prefabs/Interactable/";
        public static GameObject SpawnItem<T1, T2>(Vector3 pos, T2 item) where T1: Item<T2> {
            GameObject go = MonoBehaviour.Instantiate((GameObject)Resources.Load(path+"Item"));
            go.transform.position = pos;
            go.AddComponent<T1>().Init(item);
            return go;
        }
    }
}

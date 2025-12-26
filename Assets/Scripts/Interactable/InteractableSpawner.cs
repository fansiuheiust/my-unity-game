using UnityEngine;

namespace Interactable {
    public static class InteractableSpawner {
        static readonly string path = "Prefabs/Interactable/";
        
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T1">Item type</typeparam>
        /// <typeparam name="T2">Copy from what you get when hovering T1</typeparam>
        /// <param name="pos">Where the game object should be at</param>
        /// <param name="item">What the item should consist of</param>
        /// <returns></returns>
        public static GameObject SpawnItem<T1, T2>(Vector3 pos, T2 item) where T1: ItemObject<T2>, new() {
            GameObject go = MonoBehaviour.Instantiate((GameObject)Resources.Load(path+"Item"));
            go.transform.position = pos;
            go.AddComponent<T1>().Init(item);
            return go;
        }
    }
}

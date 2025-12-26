using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Profiling.Memory.Experimental;
using UnityEngine;
namespace Loot {
    [System.Serializable]
    public class Lootpool<T> {
        [System.Serializable]
        struct LootEntry {
            
            public T item;
            public float weight;
        }
        [field: SerializeField]
        LootEntry[] entries;
        public Lootpool((T loot, float weight)[] entries) {
            this.entries = entries.Select(e=>new LootEntry() { item=e.loot, weight=e.weight }).ToArray();
        }
        protected Lootpool() {
        }
        /// <summary>
        /// Draws a loot from the lootpool
        /// </summary>
        public T Draw() {
            float rng = Random.Range(0f, entries.Sum(e=>e.weight));
            float accumulator = 0f;
            for (int i = 0; i < entries.Length; i++) {
                if (accumulator <= rng && rng < accumulator + entries[i].weight)
                    return entries[i].item;
                accumulator += entries[i].weight;
            }
            return entries[^1].item;
        }



        /// <summary>
        /// Sorry for the DRY violation
        /// </summary>
        /// <returns></returns>
        public static T DrawFromTwo<T1, T2>(Lootpool<T1> a, Lootpool<T2> b) where T1: T where T2 : T {
            float rng = Random.Range(0f, a.entries.Sum(e => e.weight)+b.entries.Sum(e=>e.weight));
            float accumulator = 0f;
            foreach (var entry in a.entries) {
                if (accumulator <= rng && rng < accumulator + entry.weight)
                    return entry.item;
                accumulator += entry.weight;
            }
            foreach (var entry in b.entries) {
                if (accumulator <= rng && rng < accumulator + entry.weight)
                    return entry.item;
                accumulator += entry.weight;
            }
            return b.entries[^1].item;
        }
    }
}

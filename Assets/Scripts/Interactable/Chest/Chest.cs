using Interactable;
using UnityEngine;
using Loot;
using Combat;
using System.Collections;

namespace Interactable {
    public class Chest : MonoBehaviour, IInteractable {

        bool _isUntouched = true;
        Transform _lid;
        void Awake() {
            _lid = transform.Find("Lid");
        }

        [SerializeField] Lootpool<GearItem> weaponLootpool;
        [SerializeField] Lootpool<Buff> buffLootpool;

        public bool IsInteractable => _isUntouched;
        public void Interact(Mob _) {
            _isUntouched = false;
            Item chosen = Lootpool<Item>.DrawFromTwo(weaponLootpool, buffLootpool);
            GameObject item = chosen.Spawn(transform.position+0.4f*Vector3.up);
            StartCoroutine(CoolAnimation(item));
        }

        protected virtual IEnumerator CoolAnimation(GameObject lootedItem) {
            // move lid up by 1 in 0.33s
            yield return Move(_lid, Vector3.up, .33f);
            // move item up by 0.2 in 0.17s
            yield return Move(lootedItem.transform, Vector3.up * .2f, .17f);
            yield break;
        }
        static IEnumerator Move(Transform x, Vector3 dest, float time) {
            for (float f = 0; f < time; f += Time.deltaTime) {
                if (x == null) yield break;
                x.position += dest * Time.deltaTime / time;
                yield return new WaitForSeconds(Time.deltaTime);
            }
            yield break;
        }
    }
}
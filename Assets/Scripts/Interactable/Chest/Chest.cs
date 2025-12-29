using Interactable;
using UnityEngine;
using UnityEngine.Events;
using Loot;
using Combat;
using System.Collections;

namespace Interactable {
    public class Chest : MonoBehaviour, IInteractable {

        public UnityEvent OnUnlock;
        public UnityEvent OnOpen;

        [SerializeField] Lootpool<GearItem> weaponLootpool;
        [SerializeField] Lootpool<Buff> buffLootpool;
        [SerializeField] int numLoots = 1;
        [SerializeField] bool isLocked = false;
        
        bool _isUntouched = true;
        Transform _lidRotator;
        void Awake() {
            _lidRotator = transform.Find("LidRotator");
            
        }

        public bool IsInteractable => !isLocked && _isUntouched;
        public void Interact(Mob _) {
            _isUntouched = false;
            OnOpen.Invoke();
            GameObject[] items = new GameObject[numLoots];
            for (int i = 0; i < numLoots; i++) {
                items[i] =  Lootpool<Item>.DrawFromTwo(weaponLootpool, buffLootpool).Spawn(transform.position + 0.4f * Vector3.up);
            }
            StartCoroutine(CoolAnimation(items));
        }
        protected virtual IEnumerator CoolAnimation(GameObject[] lootedItems) {
            // open in 0.2s
            yield return SimpleAnimation.Rotate(_lidRotator, new Vector3(-90, 0, 0), .2f);
            // move item in 0.1s
            if (numLoots == 1)
                yield return SimpleAnimation.Move(lootedItems[0].transform, Vector3.up, .1f);

            else {
                float deg = 45f;
                foreach (GameObject item in lootedItems) {

                    yield return SimpleAnimation.Move(item.transform, Quaternion.Euler(new Vector3(0, 0, deg)) * Vector3.right, .1f);
                    deg += 90f / (numLoots-1);
                }
            }
            yield break;
        }

        public void Unlock() {
            if (!isLocked) return;
            isLocked = false;
            OnUnlock.Invoke();
        }
    }
}
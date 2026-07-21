using Combat;
using System.Collections;
using UnityEngine;

namespace BuildingBlocks {
    public class MinibossChooser : MonoBehaviour, IInteractable {

        public string bossID;
        public bool IsInteractable { get; private set; } = true;

        public void Interact(Mob m) {
            IsInteractable = false;
            StartCoroutine(Animation());
        }

        IEnumerator Animation() {
            yield return SimpleAnimation.Move(transform, new Vector3(0, 5, 0), 0.67f);
            yield return SimpleAnimation.Move(transform, new Vector3(0, -10, 0), 0.33f);
            Debug.Log("Spawn Miniboss here");

            Destroy(gameObject);
        }
    }
}
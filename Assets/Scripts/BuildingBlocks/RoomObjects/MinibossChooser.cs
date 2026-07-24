using Combat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuildingBlocks {
    public class MinibossChooser : MonoBehaviour, IInteractable {

        public string bossID;
        public bool IsInteractable { get; private set; } = true;

        public event System.Action<Mob> onMobSpawn;
        public event System.Action onStart;

        [SerializeField, Tooltip("Without the '/' at the end")]
        string minibossPrefabsPath = "Prefabs/Mobs/Minibosses";

        public void Interact(Mob m) {
            IsInteractable = false;
            StartCoroutine(Animation());
        }

        IEnumerator Animation() {
            onStart?.Invoke();
            yield return SimpleAnimation.Move(transform, new Vector3(0, 5, 0), 0.67f);
            yield return SimpleAnimation.Move(transform, new Vector3(0, -10, 0), 0.33f);
            GameObject miniboss = Instantiate((GameObject)Resources.Load($"{minibossPrefabsPath}/{bossID}"));
            miniboss.transform.position = new Vector3(transform.position.x, transform.position.y+5, transform.position.z);
            onMobSpawn?.Invoke(miniboss.GetComponent<Mob>());

            Destroy(gameObject);
        }
    }
}
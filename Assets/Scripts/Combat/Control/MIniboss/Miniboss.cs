using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Combat.Miniboss {
    public abstract class Miniboss : MonoBehaviour {
        /// <summary>
        /// Used for assigning abilities to the ability chooser
        /// </summary>
        protected abstract (System.Func<IEnumerator> ability, System.Func<bool> predicate)[] Abilities { get; }
        AbilityChooser abilityChooser;
        protected MobBehaviour Behaviour { get; private set; }
        protected Mob Owner { get; private set; }
        protected Mob Target => Behaviour.Target;

        protected virtual void Awake() {
            abilityChooser = new(Abilities);
            Owner = GetComponent<Mob>();
            Behaviour = GetComponent<MobBehaviour>();
            Behaviour.onTargetSwitch.AddListener(InterruptAbility);
            StartCoroutine(AbilityUnleasher());
        }

        protected virtual void OnDestroy() {
            InterruptAbility();
            StopAllCoroutines();
        }


        // ability stuff

        [SerializeField]
        float abilityIntervalMin = 4f, abilityIntervalMax = 20f;

        HashSet<GameObject> props = new();
        Queue<System.Func<IEnumerator>> queuedAbilities = new();
        Coroutine ability = null;
        public UnityEvent onAbilityStart, onAbilityEnd;

        bool _activeAbility = false;
        protected bool ActiveAbility {
            get => _activeAbility;
            private set {
                bool og = _activeAbility;
                _activeAbility = value;
                if (og != value) {
                    if (value)
                        onAbilityStart.Invoke();
                    else {
                        onAbilityEnd.Invoke();

                        ability = null;

                        // start an ability on queue if that exists
                        if (queuedAbilities.Count > 0)
                            StartNewAbility(queuedAbilities.Dequeue());
                    }
                }
            }
        }

        IEnumerator AbilityUnleasher() {
            while (true) {
                yield return new WaitForSeconds(Random.Range(abilityIntervalMin, abilityIntervalMax));
                if (!ActiveAbility && Target != null && abilityChooser.Next(out var f)) {
                    StartNewAbility(f);
                }
            }
        }

        protected virtual void InterruptAbility() {
            if (!ActiveAbility) return;
            StopCoroutine(ability);
            EndAbility();
            foreach (GameObject go in props)
                Destroy(go);
            props.Clear();
        }

        protected GameObject InstantiateProp(GameObject prefab) {
            GameObject ri = Instantiate(prefab);
            props.Add(ri);
            return ri;
        }

        protected void DestroyProp(GameObject gameObject) {
            props.Remove(gameObject);
            Destroy(gameObject);
        }

        /// <summary>
        /// Called when you want to start a new ability
        /// </summary>
        protected void StartNewAbility(System.Func<IEnumerator> ability) {
            if (!ActiveAbility) {
                this.ability = StartCoroutine(ability());
                ActiveAbility = true;
            } else {
                queuedAbilities.Enqueue(ability);
            }
        }

        /// <summary>
        /// Called every time when an ability ends, used at the end of all coroutines
        /// </summary>
        protected void EndAbility() {
            ActiveAbility = false;
        }
    }
}
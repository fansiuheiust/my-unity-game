using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;


namespace Combat {

    /// <summary>
    /// <para>What the mob is currentlly doing, add more as you implement more behaviour:</para>
    /// <c>Idle</c>: Nothing<br />
    /// <c>Charge</c>: Charging towards the target<br />
    /// <c>Attack</c>: Attacking the target<br />
    /// <c>Tank</c>: Tanking the target's attack<br />
    /// <c>Escape</c>: Running away from the target<br />
    /// </summary>
    public enum MobState {
        Idle, Charge, Attack, Tank, Escape
    }

    public abstract class MobBehaviour : MonoBehaviour {
        /// <summary>
        /// Second per target find
        /// </summary>
        [SerializeField] protected float findInterval = 1;
        /// <summary>
        /// Radius of target find
        /// </summary>
        [SerializeField] protected float searchRadius = 10;

        /// <summary>
        /// Self-documenting
        /// </summary>
        protected Mob Owner { get; private set; } = null;
        Mob _target;
        /// <summary>
        /// The mob the AI should act on, setting it to null will resume target finding
        /// </summary>
        protected virtual Mob Target {
            get {
                return _target;
            }
            set {
                if (value == null) {
                    TargetFinder = StartCoroutine(FindTarget());
                } else {
                    StopCoroutine(TargetFinder);
                }
                _target = value;
            }
        }

        /// <summary>
        /// Stores the ongoing coroutine that finds a target
        /// </summary>
        protected Coroutine TargetFinder { get; set; }
        MobState _state = MobState.Idle;
        public virtual MobState State {
            get => _state;
            protected set {
                _state = value;
            }
        }


        /// <summary>
        /// Criteria of the mob to be treated as a target
        /// </summary>
        protected abstract bool Predicate(Mob m);

        protected virtual void Awake() {
            Owner = GetComponent<Mob>();
            if (!Owner) throw new NullReferenceException($"{gameObject} does not have an attached Mob component.");

            Owner.OnAttackControlReset += OnAttackControlReset;
            Owner.OnBlockControlReset += OnBlockControlReset;
            TargetFinder = StartCoroutine(FindTarget());
        }

        IEnumerator FindTarget() {
            yield return new WaitForSeconds(0);
            while (true) {
                Collider[] candidates = Physics.OverlapSphere(transform.position, searchRadius);
                foreach (Collider x in candidates) {
                    if (x.TryGetComponent(out Mob m) && Predicate(m))
                        Target = m;
                }
                yield return new WaitForSeconds(findInterval);
            }
        }
        // Events
        /// <summary>
        /// Called every time when attack resets
        /// </summary>
        protected virtual void OnAttackControlReset() {

        }
        /// <summary>
        /// Called every time when block resets
        /// </summary>
        protected virtual void OnBlockControlReset() {

        }

        // Controls
        /// <summary>
        /// Sets the mob's move direction
        /// </summary>
        protected Vector3 MoveDirection {
            set {
                Owner.MoveDirection = value;
            }
        }
        protected void Jump() {
            Owner.Jump();
        }

        /// <summary>
        /// Used when attack 'key' should be 'clicked'
        /// </summary>
        protected void ClickAttack() {
            Owner.AttackClick();
        }
        /// <summary>
        /// Used when attack 'key' should be 'lifted'
        /// </summary>
        protected void LiftAttack() {
            Owner.AttackLift();
        }

        /// <summary>
        /// Used when block 'key' should be 'clicked'
        /// </summary>
        protected void ClickBlock() {
            Owner.BlockClick();
        }
        /// <summary>
        /// Used when block 'key' should be 'lifted'
        /// </summary>
        protected void LiftBlock() {
            Owner.BlockLift();
        }

    }
}
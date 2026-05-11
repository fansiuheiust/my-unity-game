
using UnityEngine;
using System.Collections;

namespace Combat {
    public abstract class Hostile : MobBehaviour {
        [SerializeField] protected float moveRadius = 2.5f;
        [SerializeField] protected float stateChangeInterval = 2;
        /// <summary>
        /// vector from self to target, updated at the start of <c>Update</c>
        /// </summary>
        protected Vector3 Delta { get; private set; }
        protected override Faction Faction => Faction.Enemy;
        protected override bool Predicate(Mob m) => m.Faction == Faction.Ally;

        protected override Mob Target {
            get => base.Target;
            set {
                if (value != null) {
                    State = MobState.Idle;
                    _stateChanger = StartCoroutine(StateChanger());
                } else {
                    StopCoroutine(_stateChanger);
                    _stateChanger = null;
                }
                base.Target = value;
            }
        }
        public override MobState State {
            get => base.State;
            protected set {
                switch (value) {
                    case MobState.Attack:
                        AttackTarget();
                        break;
                }
                base.State = value;
            }
        }


        protected override void Awake() {
            base.Awake();
            Owner.OnAttackInterrupt.AddListener((m1, m2) => { m1.TakeStun(2, m2); });
        }

        void Update() {
            if (Target == null) return;
            Delta = Target.transform.position - transform.position;
            FollowTarget();
        }

        /// <summary>
        /// Moves towards target every update
        /// </summary>
        void FollowTarget() {
            Vector3 scaledDelta = Vector3.Scale(Delta, new Vector3(1, 0, 1));
            MoveDirection = State switch {
                MobState.Charge => Delta.magnitude > moveRadius ? scaledDelta : Vector3.zero,
                MobState.Escape => -scaledDelta,
                _ => Vector3.zero
            };
        }

        /// <summary>
        /// indicates whether attack 'control' is reset s.t. the player can attack
        /// </summary>
        bool _canAttack = true;

        /// <summary>
        /// Uses attack if the target is close enough and the wepaon is not on cooldown
        /// </summary>
        void AttackTarget() {
            if (_canAttack) {
                _canAttack = false;
                AttackAction();
            }
        }

        protected abstract void AttackAction();

        protected override void OnAttackControlReset() {
            _canAttack = true;
        }

        Coroutine _stateChanger;

        /// <summary>
        /// Logic for alternating between different states
        /// </summary>
        IEnumerator StateChanger() {
            yield return new WaitForSeconds(0);

            while (true) {
                MobState s = State;
                yield return new WaitForSeconds(stateChangeInterval);

                if (State != s) continue;  // State was changed during execution, keep it for 1 cycle

                SwitchState();

            }
        }
        /// <summary>
        /// Function for deciding which state to switch to
        /// </summary>
        protected abstract void SwitchState();
    }
}
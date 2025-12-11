using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
namespace Combat {
    public class HostileMelee : MobBehaviour {
        [SerializeField] protected float moveRadius = 2.5f;
        [SerializeField] protected float attackRadius = 2;
        [SerializeField] protected float stateChangeInterval = 2;

        protected override Faction Faction => Faction.Enemy;
        protected override bool Predicate(Mob m) => m is Player;

        protected override Mob Target {
            get => base.Target;
            set {
                if (value != null) {
                    State = MobState.Idle;
                    _stateChanger = StartCoroutine(StateChanger());
                }
                base.Target = value;
            }
        }

        public override MobState State {
            get => base.State;
            protected set {
                switch (value) {
                    case MobState.Attack:
                        AttackCloseTarget();
                        break;
                }
                base.State = value;
            }
        }


        /// <summary>
        /// vector from self to target, updated at the start of <c>Update</c>
        /// </summary>
        protected Vector3 Delta { get; private set; }


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
        void AttackCloseTarget() {
            if (_canAttack) {
                _canAttack = false;
                ClickAttack();
            }
        }

        protected override void OnAttackControlReset() {
            _canAttack = true;
        }

        Coroutine _stateChanger;

        /// <summary>
        /// Self-documenting
        /// </summary>
        IEnumerator StateChanger() {
            yield return new WaitForSeconds(0);

            while (true) {
                MobState s = State;
                yield return new WaitForSeconds(stateChangeInterval);

                if (State != s) continue;  // State was changed during execution, keep it for 1 cycle

                if (State == MobState.Charge && Delta.magnitude <= attackRadius) {
                    State = MobState.Attack;
                    continue;
                }

                State = State switch {
                    MobState.Attack => MobState.Escape,
                    _ => MobState.Charge
                };


            }
        }
    }
}
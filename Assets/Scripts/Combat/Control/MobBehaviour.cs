using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
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
        [SerializeField] protected float searchRadius = 50;

        [SerializeField] protected float stateChangeInterval = 2;

        [field: SerializeField] public Faction Faction { get; private set; }

        /// <summary>
        /// <para>called when the target is switched</para>
        /// </summary>
        public UnityEvent onTargetSwitch;


        /// <summary>
        /// Self-documenting
        /// </summary>
        protected Mob Owner { get; private set; } = null;
        Mob _target;
        /// <summary>
        /// The mob the AI should act on, setting it to null will resume target finding
        /// </summary>
        public Mob Target {
            get {
                return _target;
            }
            protected set {
                if (value == null) {
                    if (_stateChanger != null)
                        StopCoroutine(_stateChanger);
                    _stateChanger = null;
                    State = MobState.Idle;
                    TargetFinder = StartCoroutine(FindTarget());
                } else {
                    if (TargetFinder != null)
                        StopCoroutine(TargetFinder);
                    TargetFinder = null;
                    _stateChanger = StartCoroutine(StateChanger());
                    value.OnDeath.AddListener((_, __) => OnTargetDead());
                }
                if (_target != value)
                    onTargetSwitch.Invoke();
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
            set {
                switch (value) {
                    case MobState.Attack:
                        AttackTarget();
                        break;
                }
                _state = value;
            }
        }


        /// <summary>
        /// Criteria of the mob to be treated as a target
        /// </summary>
        protected virtual bool Predicate(Mob m) => Owner.CanAttack(m);

        protected virtual void Awake() {
            Owner = GetComponent<Mob>();
            if (!Owner) throw new NullReferenceException($"{gameObject} does not have an attached Mob component.");

            Owner.Faction = Faction;
            Owner.OnAttackControlReset += OnAttackControlReset;
            Owner.OnBlockControlReset += OnBlockControlReset;
            TargetFinder = StartCoroutine(FindTarget());
            Owner.OnAttackInterrupt.AddListener((m1, m2) => { m1.TakeStun(2, m2); });
        }

        /// <summary>
        /// vector from self to target, updated at the start of <c>Update</c>
        /// </summary>
        public Vector3 Delta { get; private set; }

        void OnTargetDead() {
            Target = null;
        }

        void Update() {
            if (Target == null) {
                MoveDirection = Vector3.zero;
                return;
            }
            Delta = Target.transform.position - transform.position;
            FollowTarget();
        }

        /// <summary>
        /// Moves towards target every update
        /// </summary>
        protected virtual void FollowTarget() {
            Vector3 scaledDelta = Vector3.Scale(Delta, new Vector3(1, 0, 1));
            MoveDirection = State switch {
                MobState.Charge => (scaledDelta.magnitude-0.5f > AttackRange/2f * (Owner.EquippedWeapon is not null && Owner.EquippedWeapon is Ranged? 0.2f: 1f))? scaledDelta: Vector3.zero,
                MobState.Escape => -scaledDelta,
                _ => Vector3.zero
            };
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


        Coroutine _stateChanger = null;

        /// <summary>
        /// Logic for alternating between different states
        /// </summary>
        IEnumerator StateChanger() {
            yield return new WaitForSeconds(0);

            while (true) {
                MobState s = State;
                yield return new WaitForSeconds(stateChangeInterval);
                // target is too far away
                if (Target == null || (Target.transform.position - transform.position).magnitude >= 2 * searchRadius) {
                    Target = null;
                    State = MobState.Idle;
                    yield break;
                }
                if (State != s) continue;  // State was changed during execution, keep it for 1 cycle

                SwitchState();

            }
        }

        public void PauseStateSwitch() {
            if (_stateChanger != null)
                StopCoroutine(_stateChanger);
            _stateChanger = null;
        }
        public void ResumeStateSwitch(bool immediateSwitch = true) {
            if (_stateChanger == null) {
                if (immediateSwitch)
                    SwitchState();
                _stateChanger = StartCoroutine(StateChanger());
            }
        }



        /// <summary>
        /// indicates whether attack 'control' is reset s.t. the player can attack
        /// </summary>
        bool _canAttack = true;

        /// <summary>
        /// Uses attack if the wepaon is not on cooldown
        /// </summary>
        void AttackTarget() {
            if (_canAttack) {
                _canAttack = false;
                FaceTarget();
                AttackAction();
            }
        }

        protected virtual void AttackAction() {
            switch (Owner.EquippedWeapon) {
                case Melee:
                    ClickAttack();
                    LiftAttack();
                    break;
                case Ranged:
                    StartCoroutine(RangedAttack());
                    break;
            }
        }

        IEnumerator RangedAttack() {
            ClickAttack();
            float afkTime = 1 / ((1 + Owner.Stats[ScalingAttribute.AtkSpeed]) * Owner.EquippedWeapon.BaseAttackSpeed);
            for (float time = 0; time < afkTime; time += Time.deltaTime) {
                Owner.Rotatable.forward = Delta;
                yield return null;
            }
            Owner.Rotatable.forward = Vector3.Scale(Owner.Rotatable.forward, new Vector3(1, 0, 1));
            LiftAttack();
        }


        // helpers
        /// <summary>
        /// Current attack range of the owner
        /// </summary>
        protected float AttackRange => Owner.EquippedWeapon is not null ? Owner.EquippedWeapon.weaponRange * (1 + Owner.Stats[ScalingAttribute.AttackRange]) : 0;

        public float AttackTime => Owner.EquippedWeapon is not null ? 1/(Owner.EquippedWeapon.BaseAttackSpeed * (1 + Owner.Stats[ScalingAttribute.AtkSpeed])): 0;

        public void Face(Transform t) => Owner.Rotatable.forward = Vector3.Scale(t.position - transform.position, new(1, 0, 1));
        public void FaceTarget() => Face(Target.transform);

        protected bool ActiveStateSwitch => _stateChanger != null;

        /// <summary>
        /// Function for deciding which state to switch to
        /// </summary>
        protected abstract void SwitchState();

        // Events
        /// <summary>
        /// Called every time when attack resets
        /// </summary>
        protected virtual void OnAttackControlReset() {
            _canAttack = true;
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
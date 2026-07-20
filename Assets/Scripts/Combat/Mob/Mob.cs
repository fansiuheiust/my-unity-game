using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using System;
using JetBrains.Annotations;
using Loot;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditorInternal;

namespace Combat {
    /// <summary>
    /// <para>Determines what the mob can damage</para>
    /// <c>Ally</c>: Any non-ally<br />
    /// <c>Neutral</c>: Anyone <br />
    /// <c>Enemy</c>: Any Non-enemy
    /// </summary>
    public enum Faction {
        Ally, Neutral, Enemy, Indeterminate
    }
    public partial class Mob : MonoBehaviour {
        [SerializeField]
        protected MobStats stats;

        /// <summary>
        /// Base stats of the mob
        /// </summary>
        public ref readonly BaseStats BaseStats => ref stats.Base;
        /// <summary>
        /// Scaling stats of the mob
        /// </summary>
        public ref readonly ScalingStats ScalingStats => ref stats.Scaling;
        /// <summary>
        /// Stats after multiplying base stats with 1+scale and normalization
        /// </summary>
        public ref readonly FinalStats Stats => ref stats.Final;

        public float HP => stats.Hp;
        public float Mana => stats.Mana;

        [SerializeField, Tooltip("Whether the mob's stats should scale to the floor's level")]
        bool scalesToFloor = true;
        [SerializeField]
        SerializedMobStats initialStats;
        [SerializeField, Tooltip("ID of the initial gears")]
        string[] initialGears;



        Faction _faction = Faction.Indeterminate;

        /// <summary>
        /// Set by mob bevaiour script or Player script
        /// </summary>
        public Faction Faction {
            get {
                return _faction;
            }
            set {
                _faction = _faction == Faction.Indeterminate ? value : _faction;
            }
        }
        public bool CanAttack(Mob m) => Faction == Faction.Neutral || Faction != m.Faction;

        /// <summary>
        /// Parent of objects that store gameObject's that rotate about the mob
        /// </summary>
        public Transform Rotatable { get; private set; }

        /// <summary>
        /// The weapon equipped by the mob
        /// </summary>
        public Weapon EquippedWeapon { get; private set; } = null;
        /// <summary>
        /// The armors equipped by the mob
        /// </summary>
        public Dictionary<ArmorType, Armor> EquippedArmors { get; private set; } = new Dictionary<ArmorType, Armor>() {
        { ArmorType.Helmet, null },
        { ArmorType.Chestplate, null},
        { ArmorType.Leggings, null},
        { ArmorType.Boots, null}
    };

        MobMovement _movement;
        protected T CastMovement<T>() where T : MobMovement {
            if (_movement is T m)
                return m;
            throw new ArgumentException($"{gameObject}'s movement is not {nameof(T)}");
        }

        /// <summary>
        /// The list of effects affecting the mob
        /// </summary>
        public List<Effect> Effects { get; private set; } = new();

        bool _isStunned = false;

        /// <summary>
        /// Self-documenting
        /// </summary>
        public bool IsStunned {
            get => _isStunned;
            set {
                if (_isStunned != value) {
                    _isStunned = value;
                    _movement.IsStunned = value;
                }
            }
        }

        bool _isImmune = false;

        
        

        // Events
        /// <summary>
        /// <para>Raised when mob stats is changed</para>
        /// <c>float0</c>: The new attack range (not a multiplier)
        /// </summary>
        public event Action<float> OnAttackRangeChange;
        /// <summary>
        /// <para>Raised when mob consumes mana</para>
        /// <c>Mob</c>: mob who consumed mana <br />
        /// <c>float</c>: amount of mana consumed <br />
        /// </summary>
        public UnityEvent<Mob, float> OnManaConsumption;
        /// <summary>
        /// <para>Raised when mob takes damage</para>
        /// <c>Mob</c>: mob who deals damage <br />
        /// <c>float</c>: amount of damage <br />
        /// </summary>
        public UnityEvent<Mob, float> OnDamageTake;

        /// <summary>
        /// <para>Raised when a mob dies.</para>
        /// <c>Mob0</c>: the invoker, i.e. the soon-to-be dead mob. <br />
        /// <c>Mob1</c>: source, null if it does not exist.
        /// </summary>
        public UnityEvent<Mob, Mob> OnDeath;
        /// <summary>
        /// <para>Raised when an attack starts</para>
        /// <c>Mob</c>: the invoker
        /// </summary>
        public UnityEvent<Mob> OnAttackStart;
        /// <summary>
        /// <para>Raised when an attack ends</para>
        /// Mob0: the invoker
        /// </summary>
        public UnityEvent<Mob> OnAttackEnd;
        /// <summary>
        /// <para>Raised when an attack is interrupted (by blocking)</para>
        /// <c>Mob0</c>: Mob whose attack is interrupted<br />
        /// <c>Mob1</c>: Mob who interrupted the attack<br />
        /// </summary>
        public UnityEvent<Mob, Mob> OnAttackInterrupt;
        /// <summary>
        /// <para>Raised when a block starts</para>
        /// <c>Mob</c>: the invoker<br />
        /// </summary>
        public UnityEvent<Mob> OnBlockStart;
        /// <summary>
        /// <para>Raised when a block ends</para>
        /// <c>Mob</c>: the invoker<br />
        /// </summary>
        public UnityEvent<Mob> OnBlockEnd;

        /// <summary>
        /// Raised when attack control (e.g. player input changed due to attacking) should reset
        /// </summary>
        public event Action OnAttackControlReset;
        /// <summary>
        /// Raised when block control (e.g. player input changed due to blocking) should reset
        /// </summary>
        public event Action OnBlockControlReset;

        /// <summary>
        /// <para>Raised when a mob tries to unleash ability</para>
        /// <c>AbilityTriggerKey</c>: trigger key used
        /// </summary>
        public event Action<AbilityTriggerKey> OnAbilityUseAttempt;


        /// <summary>
        /// <para>Raised when the mob is stunned</para>
        /// <c>Mob</c>: the stunned mob <br />
        /// </summary>
        public UnityEvent<Mob> OnStunStart;
        /// <summary>
        /// Raised when a stun is lifted, even if there are more stuns
        /// </summary>
        public UnityEvent<Mob> OnStunEnd;
        /// <summary>
        /// Raised when a mob jumps
        /// </summary>
        public UnityEvent<Mob> OnJump;
        /// <summary>
        /// <para>Raised when a mob changes move direction</para>
        /// <para><c>Mob</c>: Invoker</para>
        /// <para><c>Vector3</c>: Direction</para>
        /// </summary>
        public UnityEvent<Mob, Vector3> OnMovementChange;

        




        protected virtual void Awake() {
            stats = new(initialStats);
            _movement = GetComponent<MobMovement>();
            Rotatable = transform.Find("Rotatable");
            stats.OnMovementSpeedChange += _movement.OnFinalStatsChanged;
            stats.OnAttackRangeChange += ChangeAttackRange;

            // raises all stats change events
            stats.ComputeFinalStats();

            // GC
            initialStats = null;
        }

        // Start is called before the first frame update
        void Start() {
            if (this is Player)
                foreach (string x in initialGears) {
                    Equip(GearDatabase.GetScaled(x));
                }
            else {
                foreach (string x in initialGears)
                    Equip(GearDatabase.Get(x));
            }
            if (scalesToFloor) {
                GainStats(( StageController.DungeonData.MobBaseStatsMultiplier.Evaluate(StageController.Floor)-1) * BaseStats, null);
            }
            ResetHp();
        }

        // damage-related
        /// <summary>
        /// Deals damage to a mob
        /// </summary>
        /// <param name="target">Mob to be damaged</param>
        /// <param name="damageType">Type of damage to be dealt</param>
        public void DealDamage(Mob target, DamageType damageType, float weaponMultiplier = 1f) {
            target.TakeDamage(this, damageType, weaponMultiplier);
        }

        /// <summary>
        /// Takes damage from a mob
        /// </summary>
        /// <param name="source">Mob that deals damage</param>
        /// <param name="damageType">Type of damage the mob dealt</param>
        /// <param name="weaponMultiplier">The damage multiplier based on the weapon's action</param>
        void TakeDamage(Mob source, DamageType damageType, float weaponMultiplier = 1f) {
            if (_isImmune) return;
            OnDamageTake.Invoke(source, stats.TakeDamage(source.stats, damageType, weaponMultiplier));
            DeathCheck(source);
        }

        /// <summary>
        /// Takes damage, damage dealt is purely dependent on the amount. <br />
        /// Damage resistance still applies
        /// </summary>
        /// <param name="amount">Amount of damage</param>
        /// <param name="source">Source of damage (null if not damaged by a mob)</param>
        /// <param name="damageType">type of damage</param>
        public void TakeDamage(float amount, Mob source, DamageType damageType) {
            if (_isImmune) return;
            OnDamageTake.Invoke(source, stats.TakeDamage(amount, damageType));
            DeathCheck(source);
        }

        // status related

        // effect
        /// <summary>
        /// Adds an UNAPPLIED effect to the mob, use Apply(...) to apply the effect
        /// </summary>
        /// <typeparam name="T">Type of effect</typeparam>
        public T AddEffect<T>() where T : Effect {
            T e = gameObject.AddComponent<T>();
            Effects.Add(e);
            return e;
        }


        /// <summary>
        /// Only for removing effect FROM Effect.cs
        /// </summary>
        /// <param name="e">Effect to be removed, must be equal reference</param>
        void RemoveExpiredEffect(Effect e) {
            Effects.Remove(e);
            Destroy(e);
        }



        // stun
        /// <summary>
        /// Applies stun to a mob. If the mob is already stunned, reset the countdown.
        /// Triggers OnStunEnd if the enemy is already stunned by a non-internal stun but the new one is internal.
        /// </summary>
        /// <param name="time">Self-documenting</param>
        /// <param name="source">The mob who inflicted the stun</param>
        /// <param name="isInternal">Whether this stun will invoke stun-related events</param>
        public void TakeStun(float time, Mob source, bool isInternal = false) {
            AddEffect<Stun>().Apply(time, isInternal);
        }
        /// <summary>
        /// Ends stun before the original duration
        /// </summary>
        public void InterruptStun() {
            if (!IsStunned) return;
            for (int i = 0; i < Effects.Count; i++) {
                if (Effects[i] is Stun) {
                    Effects[i].Cleanse();
                    i--;
                }
            }
        }

        // knockback
        /// <summary>
        /// Deals knockback to another mob
        /// </summary>
        /// <param name="target">self-documenting</param>
        /// <param name="origin">the position of the knockback</param>
        /// <param name="duration">How long should the mob not act (because of knockback) for</param>
        public void DealKnockback(Mob target, Vector3 origin, float duration) {
            target.TakeKnockback(this, origin, duration);
        }
        /// <summary>
        /// Deals knockback to another mob with the origin being self's position
        /// </summary>
        /// <param name="target">self-documenting</param>
        /// <param name="duration">How long should the mob not act (because of knockback) for</param>
        public void DealKnockback(Mob target, float duration) => DealKnockback(target, transform.position, duration);
        /// <summary>
        /// Takes knockback, does nothing if the duration is 0 (i.e. player has no knockback or mob has full knockback immunity)
        /// </summary>
        /// <param name="source">The mob who inflicted the knockback</param>
        /// <param name="origin">the position of the knockback</param>
        /// <param name="duration">How long should this mob not act (because of knockback) for</param>
        void TakeKnockback(Mob source, Vector3 origin, float duration) {
            if (_isImmune) return;
            duration *= (1 + source.stats.Final.Knockback) * (1 - stats.Final.KnockbackResistance);
            if (duration < 1e-3f) return;
            TakeStun(duration, source);
            _movement.TakeKnockback(origin, duration);
        }

        /// <summary>
        /// Takes knockback, does nothing if the duration is 0 (i.e. player has no knockback or mob has full knockback immunity)
        /// </summary>
        /// <param name="origin">the position of the knockback</param>
        /// <param name="duration">How long should this mob not act (because of knockback) for</param>
        /// <param name="isInternal">Whether the knockback is internal: internal knockback's duration is not modified by knockback resistance</param>
        /// <param name="magnitudeMultiplier">How much faster the speed should move when knocked back</param>
        public void TakeKnockback(Vector3 origin, float duration, bool isInternal = false, float magnitudeMultiplier = 1f) {
            if (_isImmune && !isInternal) return;
            if (!isInternal)
                duration *= (1 - stats.Final.KnockbackResistance);
            if (duration < 1e-3f) return;
            TakeStun(duration, null, isInternal);
            _movement.TakeKnockback(origin, duration, magnitudeMultiplier);
        }





        // death
        /// <returns>
        /// Default dead checker: Hp < 1
        /// </returns>
        public virtual bool IsDead => stats.IsDead;

        /// <summary>
        /// Called to check and act if the mob is dead
        /// </summary>
        void DeathCheck(Mob killer) {
            if (IsDead) {
                OnDeath.Invoke(this, killer);
                Die(killer);
            }
        }
        /// <summary>
        /// Deletes the gameObject
        /// </summary>
        protected virtual void Die(Mob killer) {
            Destroy(gameObject);
        }



        // Gears, stats, and ability
        /// <summary>
        /// Resets a mob's HP back to full (for changes in max HP)
        /// </summary>
        public void ResetHp() => stats.ResetHp();
        /// <summary>
        /// gains stats for the mob
        /// </summary>
        /// <param name="base">Base stats to gain, <c>null</c> if no change</param>
        /// <param name="scaling">Scaling stats to gain, <c>null</c> if no change</param>
        public void GainStats(BaseStats @base, ScalingStats scaling) {
            stats.GainStats(@base, scaling);
        }
        /// <summary>
        /// Loses stats for the mob
        /// </summary>
        /// <param name="base">Base stats to lose, <c>null</c> if no change</param>
        /// <param name="scaling">Scaling stats to lose, <c>null</c> if no change</param>
        public void LoseStats(BaseStats @base, ScalingStats scaling) {
            stats.LoseStats(@base, scaling);
        }
        /// <summary>
        /// Equips the mob with a Gear, and updates the Mob's stats. Unequips the mob's original gear if any.
        /// </summary>
        /// <param name="gear">Gear to be equipped, it will be owned by the mob</param>
        /// <exception cref="System.NotImplementedException">Equipment of gear of (type) is not implemented</exception>
        public void Equip(Gear gear) {
            switch (gear) {
                case Weapon weapon:
                    Equip(weapon);
                    break;
                case Armor armor:
                    Equip(armor);
                    break;
                default:
                    throw new System.NotImplementedException($"Equipment of gear of type \"{gear.GetType().Name}\" is not implemented.");
            }
            GainAbility(gear.ability);
        }
        /// <summary>
        /// Equips the mob with a Weapon, and updates the mob's stats. Unequips the mob's original weapon if any.
        /// </summary>
        /// <param name="weapon">Weapon to be equipped, it will be owned by the mob</param>
        void Equip(Weapon weapon) {
            if (EquippedWeapon is not null)
                UnequipWeapon();
            Instantiate(weapon.WeaponPrefab, Rotatable);
            EquippedWeapon = weapon;
            stats.GainStats(weapon.@base, weapon.scaling);
        }
        /// <summary>
        /// Equips the mob with an Armor, and updates the mob's stats. Unequips the mob's original armor if any.
        /// </summary>
        /// <param name="armor">Armor to be equipped, it will be owned by the mob</param>
        void Equip(Armor armor) {
            if (EquippedArmors[armor.type] is not null)
                UnequipArmor(armor.type);
            EquippedArmors[armor.type] = armor;
            stats.GainStats(armor.@base, armor.scaling);
        }

        /// <summary>
        /// Unequips the mob's Weapon, and updates the mob's stats.
        /// </summary>
        public void UnequipWeapon() {

            OnWeaponUnequip?.Invoke();

            stats.LoseStats(EquippedWeapon.@base, EquippedWeapon.scaling);
            LoseAbility(EquippedWeapon.ability);
            EquippedWeapon = null;
        }
        /// <summary>
        /// Unequips a mob's Armor, and updates the mob's stats
        /// </summary>
        /// <param name="type">Type of the armor to be unequipped</param>
        public void UnequipArmor(ArmorType type) {
            Armor ToLose = EquippedArmors[type];
            stats.LoseStats(ToLose.@base, ToLose.scaling);
            LoseAbility(ToLose.ability);
            EquippedArmors[type] = null;
        }

        void ChangeAttackRange(float multiplier) {
            if (EquippedWeapon is not null)
                OnAttackRangeChange?.Invoke(EquippedWeapon.weaponRange * (1+multiplier));
        }

        // Ability
        /// <summary>
        /// Consumes mana if possible
        /// </summary>
        /// <param name="mana">Amount of mana to be consumed</param>
        /// <returns>Whether mana is sufficient for the consumption</returns>
        public bool ConsumeMana(float mana) { 
            (bool consumed, float amount) = stats.ConsumeMana(mana);
            if (consumed)
                OnManaConsumption.Invoke(this, amount);
            return consumed;
        }

        /// <summary>
        /// Gains an ability, does nothing if ability is null
        /// </summary>
        /// <param name="ability">ability to be gained</param>
        public void GainAbility(Ability ability) {
            if (ability is null) return;
            AbilityObject a = (AbilityObject)gameObject.AddComponent(ability.abilityObject);
            a.Init(this, ability);
        }
        /// <summary>
        /// Lose an ability, does nothing if ability is null
        /// </summary>
        /// <param name="ability">ability to lose</param>
        public void LoseAbility(Ability ability) {
            if (ability is null) return;
            Destroy(gameObject.GetComponent(ability.abilityObject));
        }

        // Movement control
        /// <summary>
        /// For updating a mob's movement direction.
        /// </summary>
        public Vector3 MoveDirection {
            set {
                _movement.OnMovementTriggered(value);
            }
        }

        /// <summary>
        /// Makes the mob jump.
        /// </summary>
        public void Jump() {
            _movement.OnJumpTriggered();
            OnJump.Invoke(this);
        }


        // Weapon control
        public event Action<float> OnAttackClick, OnAttackLift;
        public event Action OnBlockClick, OnBlockLift;
        public event Action<float> OnBlockRotate;
        public event Action OnWeaponUnequip;
        /// <summary>
        /// indicate if the mob clicked attack button when being stunned
        /// </summary>
        bool _clickedAttackDuringStun = false;
        /// <summary>
        /// Handles mob "clicking" attack button
        /// </summary>
        public void AttackClick() {
            if (EquippedWeapon is null) {
                ResetAttackControl();
                return;
            }
            if (IsStunned) {
                _clickedAttackDuringStun = true;
                OnAttackControlReset?.Invoke();
                return;
            }
            OnAttackClick?.Invoke(1 / (EquippedWeapon.BaseAttackSpeed * (1 + stats.Final.AtkSpeed)));
        }
        /// <summary>
        /// Handles mob "lifting" attack button
        /// </summary>
        public void AttackLift() {
            // do nothing if attack was clicked when stunned
            if (_clickedAttackDuringStun) {
                _clickedAttackDuringStun = false;
                return;
            }
            OnAttackLift?.Invoke(1 / (EquippedWeapon.BaseAttackSpeed * (1 + stats.Final.AtkSpeed)));
        }

        /// <summary>
        /// indicate if the mob clicked block button when being stunned
        /// </summary>
        bool _clickedBlockDuringStun = false;
        /// <summary>
        /// Handles mob "clicking" block button
        /// </summary>
        public void BlockClick() {
            if (EquippedWeapon is null) {
                ResetBlockControl();
                return;
            }
            if (IsStunned) {
                _clickedBlockDuringStun = true;
                OnBlockControlReset?.Invoke();
                return;
            }
            OnBlockClick?.Invoke();
        }
        /// <summary>
        /// Handles mob "lifting" block button
        /// </summary>
        public void BlockLift() {
            if (_clickedBlockDuringStun) {
                _clickedBlockDuringStun = false;
                return;
            }
            OnBlockLift?.Invoke();
        }
        public void BlockRotate(float angle) {
            if (_clickedBlockDuringStun) return;
            OnBlockRotate?.Invoke(angle);
        }

        // event invokers 
        public void ResetAttackControl() {
            OnAttackControlReset?.Invoke();
        }
        public void ResetBlockControl() {
            OnBlockControlReset?.Invoke();
        }

        public void UseAbility(AbilityTriggerKey key) {
            OnAbilityUseAttempt?.Invoke(key);
        }



        // static
        public static Mob FindParentingMob(Transform t) {
            for (Transform t2 = t; t2 != null; t2 = t2.parent)
                if (t2.TryGetComponent(out Mob mob)) return mob;
            return null;
        }
    }
}
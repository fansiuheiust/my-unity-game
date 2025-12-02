using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using System;
using JetBrains.Annotations;

public class Mob : MonoBehaviour {
    [SerializeField]
    public MobStats Stats { get; private set; }

    [SerializeField]
    SerializedMobStats initialStats;

    /// <summary>
    /// Parent of objects that store gameObject's that rotate about the mob
    /// </summary>
    protected Transform _rotatable;

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
    protected T CastMovement<T>() where T: MobMovement {
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

    // Events
    /// <summary>
    /// <para>Raised when a mob dies.</para>
    /// <para>
    /// <c>Mob0</c>: the invoker, i.e. the soon-to-be dead mob.
    /// </para>
    /// <para>
    /// <c>Mob1</c>: source, null if it does not exist.
    /// </para>
    /// </summary>
    public UnityEvent<Mob, Mob> OnDeath;
    /// <summary>
    /// <para>Raised when an attack starts</para>
    /// <para>
    /// <c>Mob</c>: the invoker
    /// </para>
    /// </summary>
    public UnityEvent<Mob> OnAttackStart;
    /// <summary>
    /// <para>Raised when an attack ends</para>
    /// <para>
    /// Mob0: the invoker
    /// </para>
    /// </summary>
    public UnityEvent<Mob> OnAttackEnd;
    /// <summary>
    /// <para>Raised when an attack is interrupted (by blocking)</para>
    /// <para>Mob0: Mob whose attack is interrupted</para>
    /// <para>Mob1: Mob who interrupted the attack</para>
    /// </summary>
    public UnityEvent<Mob, Mob> OnAttackInterrupt;
    /// <summary>
    /// <para>Raised when a block starts</para>
    /// <para>
    /// <c>Mob</c>: the invoker
    /// </para>
    /// </summary>
    public UnityEvent<Mob> OnBlockStart;
    /// <summary>
    /// <para>Raised when a block ends</para>
    /// <para>
    /// <c>Mob</c>: the invoker
    /// </para>
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
    /// Raised when the mob is stunned
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
        Stats = new(initialStats);
        OnDeath = new();
        _movement = GetComponent<MobMovement>();
        _rotatable = transform.Find("Rotatable");
        Stats.OnMovementSpeedChange += _movement.OnFinalStatsChanged;

        // raises all stats change events
        Stats.ComputeFinalStats();

        // GC
        initialStats = null;
    }

    // Start is called before the first frame update
    void Start() {
        Equip(new Melee("Test sword", new BaseStats(atk: 4), new ScalingStats(atk: 0.1f), 0.9f, WeaponSpeed.Normal));
    }

    // damage-related
    /// <summary>
    /// Deals damage to a mob
    /// </summary>
    /// <param name="target">Mob to be damaged</param>
    /// <param name="damageType">Type of damage to be dealt</param>
    public void DealDamage(Mob target, DamageType damageType) {
        target.TakeDamage(this, damageType);
    }

    /// <summary>
    /// Takes damage from a mob
    /// </summary>
    /// <param name="source">Mob that deals damage</param>
    /// <param name="damageType">Type of damage the mob dealt</param>
    void TakeDamage(Mob source, DamageType damageType) {
        Stats.TakeDamage(source.Stats, damageType);
        DeathCheck(source);
    }

    /// <summary>
    /// Takes a fixed amount of damage
    /// </summary>
    /// <param name="amount">Amount of damage</param>
    /// <param name="source">Source of damage (null if not damaged by a mob)</param>
    /// <param name="damageType">type of damage</param>
    public void TakeDamage(float amount, Mob source, DamageType damageType) {
        Stats.TakeDamage(amount, damageType);
        DeathCheck(source);
    }

    // status related

    // effect
    /// <summary>
    /// Adds an UNAPPLIED effect to the mob, use Apply(...) to apply the effect
    /// </summary>
    /// <typeparam name="T">Type of effect</typeparam>
    public T AddEffect<T>() where T: Effect {
        T e = gameObject.AddComponent<T>();
        Effects.Add(e);
        return e;
    }

    /// <summary>
    /// Only for removing effect FROM Effect.cs
    /// </summary>
    /// <param name="e">Effect to be removed, must be equal reference</param>
    public void RemoveExpiredEffect(Effect e) {
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
    /// Takes knockback
    /// </summary>
    /// <param name="source">The mob who inflicted the knockback</param>
    /// <param name="origin">the position of the knockback</param>
    /// <param name="duration">How long should this mob not act (because of knockback) for</param>
    void TakeKnockback(Mob source, Vector3 origin, float duration) {
        duration *= (1+source.Stats.Final.Knockback) * (1-Stats.Final.KnockbackResistance);
        TakeStun(duration, source);
        _movement.TakeKnockback(origin, duration);
    }





    // death
    /// <returns>
    /// Default dead checker: Hp < 1
    /// </returns>
    protected virtual bool IsDead => Stats.IsDead;

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



    // Gears
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
    }
    /// <summary>
    /// Equips the mob with a Weapon, and updates the mob's stats. Unequips the mob's original weapon if any.
    /// </summary>
    /// <param name="weapon">Weapon to be equipped, it will be owned by the mob</param>
    public void Equip(Weapon weapon) {
        if (EquippedWeapon is not null)
            UnequipWeapon();
        Stats.GainStats(weapon.Base, weapon.Scaling, weapon.DmgRatio);
        EquippedWeapon = weapon;
        Instantiate(weapon.WeaponPrefab, _rotatable);
    }
    /// <summary>
    /// Equips the mob with an Armor, and updates the mob's stats. Unequips the mob's original armor if any.
    /// </summary>
    /// <param name="armor">Armor to be equipped, it will be owned by the mob</param>
    public void Equip(Armor armor) {
        if (EquippedArmors[armor.Type] is not null)
            UnequipArmor(armor.Type);
        Stats.GainStats(armor.Base, armor.Scaling);
        EquippedArmors[armor.Type] = armor;
    }

    /// <summary>
    /// Unequips the mob's Weapon, and updates the mob's stats.
    /// </summary>
    public void UnequipWeapon() {

        Stats.UnequipWeapon();

        OnWeaponUnequip();

        Stats.LoseStats(EquippedWeapon.Base, EquippedWeapon.Scaling);
        EquippedWeapon = null;
    }
    /// <summary>
    /// Unequips a mob's Armor, and updates the mob's stats
    /// </summary>
    /// <param name="type">Type of the armor to be unequipped</param>
    public void UnequipArmor(ArmorType type) {
        Armor ToLose = EquippedArmors[type];
        Stats.LoseStats(ToLose.Base, ToLose.Scaling);
        EquippedArmors[type] = null;
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
        if (IsStunned) {
            _clickedAttackDuringStun = true;
            OnAttackControlReset?.Invoke();
            return;
        }
        OnAttackClick?.Invoke(1 / (EquippedWeapon.BaseAttackSpeed * (1 + Stats.Final.AtkSpeed)));
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
        OnAttackLift?.Invoke(1 / (EquippedWeapon.BaseAttackSpeed * (1 + Stats.Final.AtkSpeed)));
    }

    /// <summary>
    /// indicate if the mob clicked block button when being stunned
    /// </summary>
    bool _clickedBlockDuringStun = false;
    /// <summary>
    /// Handles mob "clicking" block button
    /// </summary>
    public void BlockClick() {
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
}

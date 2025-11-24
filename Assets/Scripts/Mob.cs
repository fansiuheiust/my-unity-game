using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using System;

public class Mob : MonoBehaviour {
    [SerializeField]
    MobStats stats;

    [SerializeField]
    SerializedMobStats initialStats;

    /// <summary>
    /// Parent of objects that store gameObject's that rotate about the mob
    /// </summary>
    Transform _rotatable;

    /// <summary>
    /// The physical weapon
    /// </summary>
    WeaponObject _weaponObject = null;

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

    /// <summary>
    /// <para>Raised when a mob dies.</para>
    /// <para>
    /// arg0: the soon-to-be dead mob.
    /// arg1: source, null if it does not exist.
    /// </para>
    /// </summary>
    public UnityEvent<Mob, Mob> OnDeath;
    /// <summary>
    /// <para>Raised when an attack starts</para>
    /// <para>
    /// arg0: the invoker
    /// </para>
    /// </summary>
    public UnityEvent<Mob> OnAttackStart;
    /// <summary>
    /// <para>Raised when an attack ends</para>
    /// <para>
    /// arg0: the invoker
    /// </para>
    /// </summary>
    public UnityEvent<Mob> OnAttackEnd;
    /// <summary>
    /// <para>Raised when a block starts</para>
    /// <para>
    /// arg0: the invoker
    /// </para>
    /// </summary>
    public UnityEvent<Mob> OnBlockStart;
    /// <summary>
    /// <para>Raised when a block ends</para>
    /// <para>
    /// arg0: the invoker
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




    void Awake() {
        stats = new(initialStats);
        OnDeath = new();
        _movement = GetComponent<MobMovement>();
        _rotatable = transform.Find("Rotatable");
        stats.OnMovementSpeedChange += _movement.OnFinalStatsChanged;

        // raises all stats change events
        stats.ComputeFinalStats();

        // GC
        initialStats = null;
    }

    // Start is called before the first frame update
    void Start() {
        Equip(new Melee("Test sword", new BaseStats(atk: 7), new ScalingStats(atk: 0.1f, atkSpeed: -5f), 0.9f, WeaponSpeed.Slow));
    }

    // Update is called once per frame
    void Update() {
        
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
        stats.TakeDamage(source.stats, damageType);
        DeathCheck(source);
    }

    /// <summary>
    /// Takes a fixed amount of damage
    /// </summary>
    /// <param name="amount">Amount of damage</param>
    /// <param name="source">Source of damage (null if not damaged by a mob)</param>
    /// <param name="damageType">type of damage</param>
    public void TakeDamage(float amount, Mob source, DamageType damageType) {
        stats.TakeDamage(amount, damageType);
        DeathCheck(source);
    }




    // death
    /// <returns>
    /// Default dead checker: Hp < 1
    /// </returns>
    protected virtual bool IsDead => stats.IsDead;

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
        stats.GainStats(weapon.Base, weapon.Scaling, weapon.DmgRatio);
        EquippedWeapon = weapon;
        _weaponObject = Instantiate(weapon.WeaponPrefab, _rotatable).GetComponent<WeaponObject>();
        _weaponObject.OnAttackStart += OnAttackStarted;
        _weaponObject.OnAttackEnd += OnAttackEnded;
        _weaponObject.OnAttackControlReset += OnAttackControlResetted;
        _weaponObject.OnBlockStart += OnBlockStarted;
        _weaponObject.OnBlockEnd += OnBlockEnded;
        _weaponObject.OnBlockControlReset += OnBlockControlResetted;
    }
    /// <summary>
    /// Equips the mob with an Armor, and updates the mob's stats. Unequips the mob's original armor if any.
    /// </summary>
    /// <param name="armor">Armor to be equipped, it will be owned by the mob</param>
    public void Equip(Armor armor) {
        if (EquippedArmors[armor.Type] is not null)
            UnequipArmor(armor.Type);
        stats.GainStats(armor.Base, armor.Scaling);
        EquippedArmors[armor.Type] = armor;
    }

    /// <summary>
    /// Unequips the mob's Weapon, and updates the mob's stats.
    /// </summary>
    public void UnequipWeapon() {

        stats.UnequipWeapon();

        _weaponObject.OnAttackStart -= OnAttackStarted;
        _weaponObject.OnAttackEnd -= OnAttackEnded;
        _weaponObject.OnBlockStart -= OnBlockStarted;
        _weaponObject.OnBlockEnd -= OnBlockEnded;

        Destroy(_weaponObject);
        _weaponObject = null;

        stats.LoseStats(EquippedWeapon.Base, EquippedWeapon.Scaling);
        EquippedWeapon = null;
    }
    /// <summary>
    /// Unequips a mob's Armor, and updates the mob's stats
    /// </summary>
    /// <param name="type">Type of the armor to be unequipped</param>
    public void UnequipArmor(ArmorType type) {
        Armor ToLose = EquippedArmors[type];
        stats.LoseStats(ToLose.Base, ToLose.Scaling);
        EquippedArmors[type] = null;
    }

    // Attack
    /// <summary>
    /// Handles mob "clicking" attack button
    /// </summary>
    public void AttackClick() {
        _weaponObject?.AttackClicked(1 / (EquippedWeapon.BaseAttackSpeed * (1 + stats.Final.AtkSpeed)));
    }

    /// <summary>
    /// Handles mob "lifting" attack button
    /// </summary>
    public void AttackLift() {
        _weaponObject?.AttackLifted(1 / (EquippedWeapon.BaseAttackSpeed * (1 + stats.Final.AtkSpeed)));
    }

    void OnAttackStarted() {
        OnAttackStart.Invoke(this);
    }
    void OnAttackEnded() {
        OnAttackEnd.Invoke(this);
    }
    void OnAttackControlResetted() {
        OnAttackControlReset?.Invoke();
    }


    // block
    /// <summary>
    /// Handles mob "clicking" block button
    /// </summary>
    public void BlockClick() {
        _weaponObject?.BlockClicked();
    }

    /// <summary>
    /// Handles mob "lifting" block button
    /// </summary>
    public void BlockLift() {
        _weaponObject?.BlockLifted();
    }
    public void BlockRotate(float angle) {
        _weaponObject?.BlockRotated(angle);
    }

    void OnBlockStarted() {
        OnBlockStart.Invoke(this);
    }
    void OnBlockEnded() {
        OnBlockEnd.Invoke(this);
    }
    void OnBlockControlResetted() {
        OnBlockControlReset?.Invoke();
    }
}

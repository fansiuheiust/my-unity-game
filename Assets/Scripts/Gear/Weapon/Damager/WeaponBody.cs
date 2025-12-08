using System;
using UnityEngine;

/// <summary>
/// The object that deals damage when hitting someone
/// </summary>
public abstract class WeaponBody : MonoBehaviour {

    // basic info and their initialization
    public Mob Owner { get; private set; }
    protected Collider Collider { get; private set; }

    protected abstract DamageType DamageType { get; }

    private void Awake() {
        Owner = transform.root.GetComponent<Mob>();
        Collider = GetComponent<Collider>();
    }

    // blocking
    /// <summary>
    /// whether it can be blocked by certain objects (not piercing)
    /// </summary>
    public bool isBlockable = true;

    /// <summary>
    /// invoked when an attack is interrupted<br />
    /// Mob0: the mob who interrupted the attack
    /// </summary>
    public event Action<Mob> OnAttackInterrupted;

    /// <summary>
    /// check returns
    /// </summary>
    /// <param name="blocker">the weapon that is trying to block the attack</param>
    /// <returns>true if the blocker can block the weapon, base: true iff isBlockable</returns>
    public virtual bool IsBlockAvailable(WeaponBody blocker) => isBlockable;

    public void InterruptAttack(Mob interrupter) {
        OnAttackInterrupted?.Invoke(interrupter);
    }

    // attacking
    /// <summary>
    /// damage multiplier based on weapon action
    /// </summary>
    public float multiplier = 1f;

    /// <summary>
    /// handles collision, i.e. when the weapon hits someone
    /// </summary>
    /// <param name="collision"></param>
    void OnCollisionEnter(Collision collision) {
        Hit(collision.collider.gameObject);
    }

    /// <summary>
    /// base method for handling interaction with any gameObject, it only does damage dealing to mob targets
    /// </summary>
    /// <param name="target">the object that got hit</param>
    protected virtual void Hit(GameObject target) {
        if (target.TryGetComponent(out Mob m))
            Hit(m);
    }

    /// <summary>
    /// base method for handling mob interaction, only deals damage to the target mob
    /// </summary>
    /// <param name="target">the mob that it hit</param>
    protected virtual void Hit(Mob target) {
        Owner.DealDamage(target, DamageType);
    }

}

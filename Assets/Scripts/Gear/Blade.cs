using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <para><c>None</c>: currently not acting or undergoing any actions.</para>
/// <para><c>Attack</c>: self-documenting.</para>
/// <para><c>Block</c>: self-documenting.</para>
/// <para><c>Idle</c>: undergoing an action, but no need to trigger collision.</para>
/// </summary>
public enum BladeStance {
    None, Attack, Block, Idle
}
public class Blade : MonoBehaviour {
    BladeStance _stance = BladeStance.None;
    public float attackTime = 0f;
    public Mob Owner { get; private set; }
    Collider _collider;
    List<Mob> attackeds = new();
    /// <summary>
    /// Self-documenting, but comes with a setter for executing stuff when hopping from a stance
    /// </summary>
    public BladeStance Stance { 
        get =>_stance; 
        set {
            // x -> S
            switch (_stance) {
                // x = attack
                case BladeStance.Attack:
                    foreach (Mob m in attackeds) {
                        if (m != null)
                            Physics.IgnoreCollision(gameObject.GetComponent<Collider>(), m.GetComponent<Collider>(), false);
                    }
                    attackeds.Clear();
                    break;

            }
            
            // S -> y
            switch (value) {
                // y = {attack, block}
                case BladeStance.Attack:
                case BladeStance.Block:
                    _collider.isTrigger = false;
                    break;
                // y = {idle, none}
                case BladeStance.Idle:
                case BladeStance.None:
                    _collider.isTrigger = true;
                    break;
            }
            _stance = value;
        }
    }


    void Awake() {
        Owner = transform.root.GetComponent<Mob>();
        _collider = gameObject.GetComponent<Collider>();
    }


    /// <summary>
    /// Handles collision, when the blade is swinging
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision) {
        switch (_stance) {
            case BladeStance.Attack:
                OnSwordHit(collision.gameObject);
                break;
            case BladeStance.Block:
                OnBlockHit(collision.gameObject);
                break;
            default:
                break;
        }
    }

    void OnSwordHit(GameObject gameObject) {
        if (gameObject.GetComponent<Mob>() is not null) {
            Mob mob = gameObject.GetComponent<Mob>();
            Owner.DealKnockback(mob, attackTime / 2);
            Owner.DealDamage(mob, DamageType.Melee); // this line can kill mob
            Physics.IgnoreCollision(this.gameObject.GetComponent<Collider>(), gameObject.GetComponent<Collider>());
            attackeds.Add(mob);
        }
    }
    void OnBlockHit(GameObject gameObject) {
        Debug.Log("GG");
    }
}

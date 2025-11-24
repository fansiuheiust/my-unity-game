using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum BladeStance {
    None, Attack, Block
}
public class Blade : MonoBehaviour {
    BladeStance _stance = BladeStance.None;
    Mob _owner;
    List<Mob> attackeds = new();
    /// <summary>
    /// Self-documenting, but comes with a setter for executing stuff when hopping from a stance
    /// </summary>
    public BladeStance Stance { 
        get =>_stance; 
        set {
            // change from attack stance
            if (_stance == BladeStance.Attack) {
                foreach (Mob m in attackeds) {
                    if (m != null)
                        Physics.IgnoreCollision(gameObject.GetComponent<Collider>(), m.GetComponent<Collider>(), false);
                }
                attackeds.Clear();
            }
            _stance = value;
        }
    }


    void Awake() {
        _owner = transform.root.GetComponent<Mob>();
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
            _owner.DealDamage(mob, DamageType.Melee); // this line can kill mob
            Physics.IgnoreCollision(this.gameObject.GetComponent<Collider>(), gameObject.GetComponent<Collider>());
            attackeds.Add(mob);
        }
    }
    void OnBlockHit(GameObject gameObject) {

    }
}

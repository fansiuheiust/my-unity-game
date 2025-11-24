using System;
using UnityEngine;

public enum BladeStance {
    None, Attack, Block
}
public class Blade : MonoBehaviour {
    public BladeStance stance = BladeStance.None;
    Mob _owner;


    void Awake() {
        _owner = transform.root.GetComponent<Mob>();
    }


    /// <summary>
    /// Handles collision, when the blade is swinging
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision) {
        switch (stance) {
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
        if (gameObject.GetComponent<Mob>() != null) {
            _owner.DealDamage(gameObject.GetComponent<Mob>(), DamageType.Melee);
        }
    }
    void OnBlockHit(GameObject gameObject) {

    }
}

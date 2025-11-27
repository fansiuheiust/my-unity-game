using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Attached to the Melee weapon gameObject, used for handling the melee weapon's movement
/// </summary>
public class MeleeObject : WeaponObject {
    /// <summary>
    /// The rigid body ceneted around the weapon
    /// </summary>
    Transform _model;
    /// <summary>
    /// true if the object is already undergoing an animation
    /// </summary>
    bool _isActing = false;
    /// <summary>
    /// blade of the melee weapon
    /// </summary>
    Blade _blade;

    /// <summary>
    /// Change to the localPosition of model when started blocking
    /// </summary>
    Vector3 _blockChange = new(0.5f, -0.4f, 0);

    void Awake() {
        _blade = transform.Find("Model").Find("Blade").GetComponent<Blade>();
        _model = transform.Find("Model");
    }

    /// <summary>
    /// Performs an attack
    /// </summary>
    /// <param name="attackTime">self-documenting</param>
    public override void AttackClicked(float attackTime) {
        if (_blade.Stance != BladeStance.None) {
            base.AttackClicked(0);
            return;
        }
        InternalStun(attackTime);
        _blade.attackTime = attackTime;
        Swing(attackTime);
    }
    /// <summary>
    /// Nothing
    /// </summary>
    /// <param name="attackTime">self-documenting</param>
    public override void AttackLifted(float attackTime) {
        // blank
    }

    const float max_block_dur = 0.5f;
    /// <summary>
    /// indicates if action block is undergoing cooldown
    /// </summary>
    bool _blockUnderCd = false;
    public override void BlockClicked() {
        if (_blockUnderCd || _blade.Stance != BladeStance.None) {
            base.BlockClicked();
            return;
        }
        _model.transform.localPosition += _blockChange;
        BlockRotated(0);
        _blade.Stance = BladeStance.Block;
        StartBlock();
        InternalStun(max_block_dur);
        StartCoroutine(BlockTire(max_block_dur));
    }
    
    /// <summary>
    /// Note that this will be called if the mob is "tired" of blocking
    /// </summary>
    public override void BlockLifted() {
        if (_blade.Stance != BladeStance.Block) return;
        _model.transform.localPosition -= _blockChange;

        transform.localEulerAngles = Vector3.zero;
        _model.transform.localEulerAngles = Vector3.zero;

        _blade.Stance = BladeStance.None;
        InterruptStun();
        StartCoroutine(BlockCooldown(max_block_dur));
        EndBlock();
        ResetBlockControl();
    }
    /// <summary>
    /// Stops blocking after a set amount of time
    /// </summary>
    /// <param name="time">Time until blocking stops</param>
    IEnumerator BlockTire(float time) {
        yield return new WaitForSeconds(time);
        BlockLifted();
        yield break;
    }
    IEnumerator BlockCooldown(float time) {
        _blockUnderCd = true;
        yield return new WaitForSeconds(time);
        _blockUnderCd = false;
    }
    public override void BlockRotated(float angle) {
        transform.localEulerAngles = new Vector3(0, 0, angle);
        _model.transform.localEulerAngles = new Vector3(0 <= angle && angle < 90 || -180 <= angle && angle < -90? -90: 90, 0, 0);
    }




    /// <summary>
    /// Swing time distribution: 33.33333...% 180 degree movement (from right to left), 66.66666...% stay at left 
    /// </summary>
    /// <param name="time">Duration of the swing</param>
    public void Swing(float time) {
        StartCoroutine(SwingAnimation(time));
    }
    IEnumerator SwingAnimation(float time) {
        StartAttack();

        _blade.Stance = BladeStance.Attack;
        _blade.GetComponent<Collider>().isTrigger = false;

        _model.localEulerAngles = new Vector3(0, 90, 90);
        //  swing
        float vel = 0;
        float swingTime = time / 3;
        for (float timePassed = 0; timePassed < swingTime; timePassed += Time.deltaTime) {
            // 50% time: time*.5
            // theta target: \pi
            // find \alpha
            // for simplicity, i will use linear symbols instead
            // s = ut+1/2*at^2
            // 180 = 1/2*a*(swingTime)^2 
            // a = 360/(swingTime*swingTime)

            // v = at
            vel -= 360 / (swingTime * swingTime) * Time.deltaTime;

            transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y + vel * Time.deltaTime, 0);
            // _rb.AddTorque(new Vector3(0, 8 * Mathf.PI / (time * time), 0), ForceMode.Acceleration);
            yield return new WaitForSeconds(Time.deltaTime);
        }


        // disable blade collision and stay
        _blade.GetComponent<Collider>().isTrigger = true;
        _blade.Stance = BladeStance.Idle;
        yield return new WaitForSeconds(2 * time / 3);

        ResetPosition();
        _blade.Stance = BladeStance.None;

        EndAttack();
        ResetAttackControl();

        yield break;
    }
    void ResetPosition() {
        transform.localEulerAngles = Vector3.zero;
        _model.localEulerAngles = Vector3.zero;
    }


    // localEuler x of model:
    // [0, 90):     -90
    // [90, 180):   90
    // [-180, -90): -90
    // [-90, 0):    90
    // model position change:
    // x = 0.75->1.25
    // y = 0.4->0
}

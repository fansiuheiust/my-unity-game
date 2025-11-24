using System;
using System.Collections;
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

    public override void AttackClicked(float attackTime) {
        if (!_isActing)
            Swing(attackTime);
    }

    public override void AttackLifted(float attackTime) {
        // blank
    }

    public override void BlockClicked() {
        _model.transform.localPosition += _blockChange;
        BlockRotated(0);
        _blade.Stance = BladeStance.Block;
        StartBlock();
    }
    public override void BlockLifted() {
        _model.transform.localPosition -= _blockChange;

        transform.localEulerAngles = Vector3.zero;
        _model.transform.localEulerAngles = Vector3.zero;

        _blade.Stance = BladeStance.None;
        EndBlock();
        ResetBlockControl();
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

        _isActing = true;
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
        _blade.Stance = BladeStance.None;
        yield return new WaitForSeconds(2 * time / 3);

        ResetPosition();
        _isActing = false;

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

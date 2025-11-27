using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

/// <summary>
/// The class responsible for handling mob movements
/// </summary>
public class MobMovement : MonoBehaviour
{
    // Start is called before the first frame update

    [Header("Spring for Floating")]
    [SerializeField] float thresholdFloatDistance = 0.2f;
    [SerializeField] float springCoefficient = 100;
    [SerializeField] float dampCoefficient = 16;

    [Header("Movement")]
    [SerializeField] float maxSpeed = 10;
    [SerializeField] float jumpHeight = 1;
    [SerializeField] float acceleration = 35;
    [SerializeField] AnimationCurve accelerationFromRotation;


    Rigidbody _rb;

    /// <summary>
    /// The direction the mob moves to
    /// </summary>
    protected Vector3 _movement = Vector3.zero;
    /// <summary>
    /// The velocity the mob should have now
    /// </summary>
    Vector3 targetVelocity = Vector3.zero;
    /// <summary>
    /// Bonus speed from stats
    /// </summary>
    float _speedBonus = 1;

    /// <summary>
    /// Jump speed needded to jump <c>jumpHeight</c> units tall
    /// </summary>
    float JumpSpeed { get { return Mathf.Sqrt(-2*Physics.gravity.y*jumpHeight); } }
    /// <summary>
    /// Cast every fixed update, used to indicate the object below the mob for floating
    /// </summary>
    RaycastHit _downwardRay;
    /// <summary>
    /// True iff the object has landed AND settled
    /// </summary>
    public bool Grounded { get; private set; } = false;
    /// <summary>
    /// small number
    /// </summary>
    const float _epsilon = 0.0625f;

    bool _isStunned = false;
    float _prevMaxSpeed;
    public bool IsStunned { 
        get {
            return _isStunned;
        } 
        set {
            if (!IsStunned && value) {
                _prevMaxSpeed = maxSpeed;
                maxSpeed = 0;
            }
            if (IsStunned && !value) {
                maxSpeed = _prevMaxSpeed;
            }
            _isStunned = value;
        }
    }

    
    void Start()
    {
        _rb = gameObject.GetComponent<Rigidbody>();

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Float();
        Move(_movement);
    }
    
    /// <summary>
    /// Executed every frame to ensure that the player floats on top of the ground if any
    /// </summary>
    void Float() {
        Grounded = false;

        if (Physics.SphereCast(transform.position, transform.lossyScale.x/2, Vector3.down, out _downwardRay, thresholdFloatDistance + 1 - Physics.gravity.y / springCoefficient / 2)) {
            // distance: distance from equilibrium position w/o gravity
            float distance = transform.position.y - _downwardRay.point.y - (-Physics.gravity.y / springCoefficient + thresholdFloatDistance + 1);

            if (distance > Physics.gravity.y / springCoefficient / 2) 
                return;
            Grounded = Mathf.Abs((float)distance - Physics.gravity.y / springCoefficient) < _epsilon && -_rb.linearVelocity.y < JumpSpeed / 2;
            float acc = (-distance * springCoefficient + Vector3.Dot(Vector3.down, _rb.linearVelocity - (_downwardRay.rigidbody is not null? _downwardRay.rigidbody.linearVelocity: Vector3.zero)) * dampCoefficient);
            _rb.AddForce(acc * Vector3.up, ForceMode.Acceleration);

            _downwardRay.rigidbody?.AddForceAtPosition(_rb.mass * acc * Vector3.down, _downwardRay.point);
        }
    }

    /// <summary>
    /// Moves the character
    /// </summary>
    /// <param name="dir">normalized direction of the movement</param>
    void Move(Vector3 dir) {
        Vector3 currentVelocityWithoutY = Vector3.Scale(_rb.linearVelocity, new(1, 0, 1));
        // get the acceleration needed to go to targetVelocity in a frame
        targetVelocity = Vector3.MoveTowards(targetVelocity, _speedBonus * maxSpeed * dir, _speedBonus * acceleration * accelerationFromRotation.Evaluate(Vector3.Dot(dir, _rb.linearVelocity.normalized)) * Time.fixedDeltaTime);
        Vector3 acc = (targetVelocity - currentVelocityWithoutY)/Time.fixedDeltaTime;

        // obvious
        _rb.AddForce(acc, ForceMode.Acceleration);

        // make model face correctly
        if (targetVelocity.magnitude > _epsilon)
            transform.Find("Rotatable").transform.rotation = Quaternion.LookRotation(targetVelocity);
    }

    /// <summary>
    /// Triggered every time when the mob's movement direction should be changed
    /// </summary>
    /// <param name="value">Movement direction</param>
    public void OnMovementTriggered(Vector3 value) {
        _movement = value.normalized;
    }
    /// <summary>
    /// Triggered every time when the mob should jump
    /// </summary>
    public void OnJumpTriggered() {
        if (IsStunned) return;
        if (!Grounded) return;
        Jump();
    }

    /// <summary>
    /// self-explanatory
    /// </summary>
    void Jump() {
        // _canFloat = false;
        _rb.AddForce(Vector3.up * JumpSpeed, ForceMode.VelocityChange);
        // StartCoroutine(FloatCooldown());
    }

    public void OnFinalStatsChanged(float newSpeed) {
        _speedBonus = 1 + newSpeed;
    }



    // status related
    /// <summary>
    /// 
    /// </summary>
    /// <param name="orign">Where the knockback should be from</param>
    /// <param name="duration">How long the knockback should last, with mob's stats considered</param>
    public void TakeKnockback(Vector3 origin, float duration) {
        // apply force to rigid body
        _rb.linearVelocity = Vector3.zero;
        // v = u + at, u = 0 => v = at
        Vector3 dir = transform.position - origin;
        Vector2 xzV = acceleration * duration * (new Vector2(dir.x, dir.z)).normalized;
        Vector3 newV = new Vector3(xzV.x, 0, xzV.y);
        _rb.AddForce(newV, ForceMode.VelocityChange);
        targetVelocity = newV;
    }
}

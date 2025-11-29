using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System.Runtime.InteropServices;

/// <summary>
/// Controls the player's movement
/// </summary>
public class Controller : MonoBehaviour
{
    /// <summary>
    /// controls how fast look rotation is
    /// </summary>
    [SerializeField]
    float sensitivity = 0.1f;
    Mob _player;
    PlayerMovement _movement;
    PlayerInput _playerInput;
    public Transform _camera;
    UnityEvent<Vector3> onMovement;
    UnityEvent onJump;

    float _xAxisRotation = 0;


    void Awake() {
        _movement = GetComponent<PlayerMovement>();
        _player = GetComponent<Mob>();
        _camera = transform.Find("Camera");
        _playerInput = GetComponent<PlayerInput>();

        _playerInput.enabled = false;
        _playerInput.enabled = true;

        onMovement = new();
        onJump = new();

        onMovement.AddListener(_movement.OnMovementTriggered);
        onJump.AddListener(_movement.OnJumpTriggered);
        _player.OnAttackControlReset += OnAttackEnded;
        _player.OnBlockControlReset += OnBlockEnded;

        _playerInput.actions["walk"].performed += OnMovementInput;
        _playerInput.actions["walk"].canceled += OnMovementCancel;
        _playerInput.actions["jump"].performed += OnJumpInput;
        _playerInput.actions["rotate"].performed += OnRotate;
        _playerInput.actions["lockRotate"].performed += OnRotateLock;
        _playerInput.actions["lockRotate"].canceled += OnRotateLockCancel;
        _playerInput.actions["attack"].performed += OnAttackInput;
        _playerInput.actions["attack"].canceled += OnAttackInputCancel;
        _playerInput.actions["block"].performed += OnBlockInput;
        _playerInput.actions["block"].canceled += OnBlockInputCancel;
        _playerInput.actions["blockrotate"].performed += OnBlockRotation;
        _playerInput.actions["blockrotate"].Disable();

        // temp stuff
        _playerInput.actions["tempstun"].performed += x=> { _player.TakeStun(1, null); };
        _playerInput.actions["tempstuninterrupt"].performed += x => { _player.InterruptStun(); };
    }



    void OnEnable() {
        _playerInput.enabled = true;

    }

    void OnDisable() {
        _playerInput.enabled = false;
    }

    // player movement
    /// <summary>
    /// Invoked once per key press. Gives <code>PlayerMovement</code> the direction to move, accounting for camera's rotation
    /// </summary>
    /// <param name="context">WASD pressed</param>
    void OnMovementInput(InputAction.CallbackContext context) { onMovement.Invoke(Quaternion.Euler(0, transform.Find("Camera").localEulerAngles.y, 0) * context.ReadValue<Vector3>()); }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="context">WASD lifted</param>
    void OnMovementCancel(InputAction.CallbackContext context) { onMovement.Invoke(Vector3.zero); }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="context">Jump pressed</param>
    void OnJumpInput(InputAction.CallbackContext context) {
        onJump.Invoke();
    }

    // player rotation
    /// <summary>
    /// <para>
    /// Called every time the mouse moves away from the origin.
    /// Rotates the camera angle to the desired orientation, then resets the mouse's position.
    /// </para>
    /// <para>
    /// Also rotates the player's movement direction
    /// </para>
    /// </summary>
    /// <param name="context">Mouse Delta</param>
    void OnRotate(InputAction.CallbackContext context) {
        Vector2 v = context.ReadValue<Vector2>();
        _xAxisRotation += v.y * sensitivity;
        _camera.localEulerAngles = new Vector3(_xAxisRotation = Mathf.Min(Mathf.Max(_xAxisRotation, -75), 75), _camera.localEulerAngles.y + v.x*sensitivity,0);
        _movement.Rotate(Quaternion.Euler(0, v.x * sensitivity, 0));
        Mouse.current.WarpCursorPosition(new(Screen.width / 2f, Screen.height / 2f));
    }

    /// <summary>
    /// Locks the player's rotation, allowing them to use the cursor
    /// </summary>
    /// <param name="context"></param>
    void OnRotateLock(InputAction.CallbackContext context) {
        Cursor.visible = true;
        _playerInput.actions["rotate"].Disable();
    }

    /// <summary>
    /// Undoes player rotation lock, setting player back to movement 
    /// </summary>
    /// <param name="context"></param>
    void OnRotateLockCancel(InputAction.CallbackContext context) {
        ResetMouse();
        _playerInput.actions["rotate"].Enable();
    }

    void ResetMouse() {
        Cursor.visible = false;
        Mouse.current.WarpCursorPosition(new(Screen.width / 2f, Screen.height / 2f));
    }
    
    
    // attack
    void OnAttackInput(InputAction.CallbackContext context) {
        _player.AttackClick();
    }

    void OnAttackInputCancel(InputAction.CallbackContext context) {
        _player.AttackLift();
    }
    void OnAttackEnded() {
    }


    // block
    void OnBlockInput(InputAction.CallbackContext context) {
        _playerInput.actions["rotate"].Disable();
        _playerInput.actions["blockrotate"].Enable();
        Cursor.visible = true;
        _player.BlockClick();



    }
    void OnBlockInputCancel(InputAction.CallbackContext context) {
        _player.BlockLift();
    }
    void OnBlockEnded() {
        Cursor.visible = false;
        _playerInput.actions["blockrotate"].Disable();
        _playerInput.actions["rotate"].Enable();
    }

    void OnBlockRotation(InputAction.CallbackContext context) {
        Vector2 v = (context.ReadValue<Vector2>() - new Vector2(Screen.width/2f, Screen.height/2f));
        _player.BlockRotate(Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg);
    }

    void Start() {
        ResetMouse();
    }
}

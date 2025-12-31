using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System.Runtime.InteropServices;
using System;
using Dungeon.Generator;
using BuildingBlocks;
using System.Linq;
using Unity.VisualScripting;
using Loot;

namespace Combat {
    /// <summary>
    /// Controls the player's movement <br />
    /// For now, control keys are directly ripped off from the G-word game
    /// </summary>
    public class Controller : MonoBehaviour {
        /// <summary>
        /// controls how fast look rotation is
        /// </summary>
        [SerializeField] float sensitivity = 0.1f;
        /// <summary>
        /// How far the item can be if interacting with it
        /// </summary>
        [SerializeField] float interactionRange = 4f;



        Player _player;
        PlayerInput _playerInput;
        public Transform _camera;

        float _xAxisRotation = 0;


        void Awake() {
            if (!TryGetComponent(out _player))
                throw new NullReferenceException($"{gameObject} does not have an attached Mob script");
            _camera = transform.Find("Camera");
            _playerInput = GetComponent<PlayerInput>();

            _playerInput.enabled = false;
            _playerInput.enabled = true;

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
            _playerInput.actions["interact"].performed += OnInteraction;

            // temp stuff
            _playerInput.actions["tempstun"].performed += _ => { Driver.Main(); };
            _playerInput.actions["tempstuninterrupt"].performed += _ => {
                Buff b = new(new BaseStats(), new ScalingStats(walkSpeed: 0.125f));
                b.Spawn(transform.position);
            };
        }

        void Start() {
            ResetMouse();
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
        void OnMovementInput(InputAction.CallbackContext context) {
            _player.MoveDirection = Quaternion.Euler(0, transform.Find("Camera").localEulerAngles.y, 0) * context.ReadValue<Vector3>();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context">WASD lifted</param>
        void OnMovementCancel(InputAction.CallbackContext context) { _player.MoveDirection = Vector3.zero; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context">Jump pressed</param>
        void OnJumpInput(InputAction.CallbackContext context) {
            _player.Jump();
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
            _camera.localEulerAngles = new Vector3(_xAxisRotation = Mathf.Min(Mathf.Max(_xAxisRotation, -75), 75), _camera.localEulerAngles.y + v.x * sensitivity, 0);
            _player.RotateMovement(Quaternion.Euler(0, v.x * sensitivity, 0));
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
            Vector2 v = (context.ReadValue<Vector2>() - new Vector2(Screen.width / 2f, Screen.height / 2f));
            _player.BlockRotate(Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg);
        }

        /// <summary>
        /// Interacts with the item closest to where the player's camera is looking at
        /// </summary>
        void OnInteraction(InputAction.CallbackContext _) {
            // collection of interactable with available interaction in player's range that is in front of the camera, sorted by how close the camera is aiming at the interactable
            IEnumerable<IInteractable> hits = Physics.OverlapSphere(transform.position, interactionRange)
                .Where(h => h.TryGetComponent(out IInteractable inter) && inter.IsInteractable)
                .OrderByDescending(h => Vector3.Dot((h.transform.position - _camera.transform.position).normalized, _camera.forward))
                .Select(h => h.GetComponent<IInteractable>());
            if (hits.Count() != 0)
                hits.FirstOrDefault().Interact(_player);
        }
    }
}
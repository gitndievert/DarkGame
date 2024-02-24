// ********************************************************************
// CONFIDENTIAL - DO NOT DISTRIBUTE
// COPYRIGHT 2019-2024 Wacky Potato Games, LLC. All Rights Reserved.
// 
// If you send, receive, or use this file for any purpose other than
// internal use by Wacky Potato Games, it is without permission and an act of theft.
// Report any misuse of this file immediately to contact@wackypotato.com
// Misuse or failure to report misuse will subject you to legal action.
// 
// The intellectual and technical concepts contained herein are
// proprietary and are protected by trade secret and/or copyright law.
// Dissemination or reproduction of this material is forbidden.
// ********************************************************************

using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    const float DEFAULT_RUN_SPEED = 15f;
    
    public Camera PlayerCamera;

    [Range(0,100)]
    public float xForce;
    [Range(0, 100)]
    public float zForce;

    public float walkSpeed = 5;    
    public float runSpeed = DEFAULT_RUN_SPEED;
    public bool IsRunning = false;
    public bool IsMoving = false;
    public float LookSpeed = 2;
    public float MouseSpeed = 3;
    
    public float jumpHeight = 2f;
    public float gravity = -9.8f;

    public bool LiftModeActivate { get; set; }

    [SerializeField] 
    private Vector3 _boxSize;
    [SerializeField] 
    private float _maxDistance = 1;
    [SerializeField] 
    private LayerMask _layerMask;
    private CharacterController _controller;
    private Vector3 _velocity;
    [SerializeField]
    private float _fallSpeed = 2.5f;

    private Player _player;
    private Rigidbody _rb;

    //Paste stuff below
    private float _rotationYVelocity;
    private float _cameraXVelocity;
    private float _yRotationSpeed = 0.1f;
    private float _xCameraSpeed = 0.1f;        
    private float _wantedYRotation;    
    private float _currentYRotation;    
    private float _wantedCameraXRotation;    
    private float _currentCameraXRotation;

    

    [Tooltip("Top camera angle.")]
    public float topAngleView = 60;
    [Tooltip("Minimum camera angle.")]
    public float bottomAngleView = -45;


    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _controller = GetComponent<CharacterController>();
        _player = GetComponent<Player>();
        Cursor.visible = false;
        if(PlayerCamera == null)
        {
            PlayerCamera = Camera.main;
        }
        Cursor.lockState = CursorLockMode.Locked;
        LiftModeActivate = false;        
    }
        
    private void Update()
    {
        if (GameManager.Instance.GamePaused) return;
        IsRunning = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
        HandleMovementInput();
        HandleMouseLook();
        IsMoving = _controller.velocity.magnitude > 0.1f;        
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance.GamePaused) return;
        _currentYRotation = Mathf.SmoothDamp(_currentYRotation, _wantedYRotation, ref _rotationYVelocity, _yRotationSpeed);
        _currentCameraXRotation = Mathf.SmoothDamp(_currentCameraXRotation, _wantedCameraXRotation, ref _cameraXVelocity, _xCameraSpeed);
        PlayerCamera.transform.rotation = Quaternion.Euler(0, _currentYRotation, 0);
        PlayerCamera.transform.localRotation = Quaternion.Euler(_currentCameraXRotation, 0, 0);
    }

    public PowerUpType PlayerPowerup
    {
        get
        {
            return _player.ActivatedPlayerPowerup;
        }
    }

    public CharacterController GetCharacterController()
    {
        return _controller;
    }

    public Rigidbody GetRigidBody()
    {
        return _rb;
    }
    
    private void HandleMovementInput()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        if(PlayerPowerup == PowerUpType.Speed)
        {
            IsRunning = true;
            runSpeed = DEFAULT_RUN_SPEED * 2;
        }

        float playerSpeed = IsRunning ? runSpeed : walkSpeed;

        Vector3 movement = new Vector3(horizontal, 0f, vertical) * playerSpeed * Time.deltaTime;
        movement = transform.TransformDirection(movement);        
        
        ApplyGravity();
        _controller.Move(_velocity * Time.deltaTime + movement);
                
        //Removed Jump
        /*if (Input.GetButtonDown("Jump") && _controller.isGrounded)
        {
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }*/

        // Faster descent after jumping
        if (_velocity.y < 0)
        {
            _velocity.y += gravity * _fallSpeed * Time.deltaTime;
        }
    }
    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X");
        /*float mouseY = -Input.GetAxis("Mouse Y");
        transform.Rotate(Vector3.up * mouseX * MouseSpeed);
        cam.transform.Rotate(Vector3.right * mouseY * MouseSpeed);*/
        _wantedYRotation += Input.GetAxis("Mouse X") * MouseSpeed;
        _wantedCameraXRotation -= Input.GetAxis("Mouse Y") * MouseSpeed;
        _wantedCameraXRotation = Mathf.Clamp(_wantedCameraXRotation, bottomAngleView, topAngleView);
        transform.Rotate(Vector3.up * mouseX * MouseSpeed);
    }

    private void ApplyGravity()
    {
        if (LiftModeActivate) return;
        if (_controller.isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
        }        
        _velocity.y += gravity * Time.deltaTime;
    }

}

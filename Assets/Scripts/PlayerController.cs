using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerController : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] // Move speed of the character in m/s
    private float MoveSpeed = 2.0f;

    [SerializeField] // Sprint speed of the character in m/s
    private float SprintSpeed = 5.335f;

    [Space(10)]
    [SerializeField] // How fast the character turns to face movement direction
    [Range(0.0f, 0.3f)]
    private float RotationSmoothTime = 0.12f;

    [SerializeField] // Acceleration and deceleration
    private float SpeedChangeRate = 10.0f;

    [Space(10)]
    [SerializeField] // The height the player can jump
    private float JumpHeight = 1.2f;

    [SerializeField] // The character uses its own gravity value. The engine default is -9.81f
    private float Gravity = -15.0f;

    [SerializeField] // Time required to pass before being able to jump again. Set to 0f to instantly jump again
    private float JumpTimeout = 0.50f;

    [SerializeField] // Time required to pass before entering the fall state. Useful for walking down stairs
    private float FallTimeout = 0.15f;

    [Space(10)]
    [SerializeField] // Player model
    private SkinnedMeshRenderer MeshRenderer;

    [Header("Player Grounded")]
    [SerializeField] // If the character is grounded or not. Not part of the CharacterController built in grounded check
    private bool Grounded = true;

    [SerializeField] // Useful for rough ground
    private float GroundedOffset = -0.14f;

    [SerializeField] // The radius of the grounded check. Should match the radius of the CharacterController
    private float GroundedRadius = 0.28f;

    [SerializeField] // What layers the character uses as ground
    private LayerMask GroundLayers;

    [Header("Cinemachine")]
    [SerializeField] // Third person camera object
    private GameObject TPPCamera;

    [SerializeField] // First person camera object
    private GameObject FPPCamera;

    [SerializeField] // The follow target set in the Cinemachine Virtual Camera that the camera will follow
    private GameObject CinemachineCameraTarget;

    [Space(10)]
    [SerializeField] // How far in degrees can you move the camera up
    private float TopClamp = 70.0f;

    [SerializeField] // How far in degrees can you move the camera down
    private float BottomClamp = -30.0f;

    [SerializeField] // Additional degress to override the camera. Useful for fine tuning camera position when locked
    private float CameraAngleOverride = 0.0f;

    [SerializeField]  // For locking the camera position on all axis
    private bool LockCameraPosition = false;

    // Animator component
    private Animator animator;
    private bool hasAnimator;

    // Animation IDs
    private int animIDSpeed;
    private int animIDGrounded;
    private int animIDJump;
    private int animIDFreeFall;
    private int animIDMotionSpeed;
    private int animIDisFlying;
    private int animIDisWaving;
    private int animIDisDancing;

    // Input system
    private PlayerInput input;

    private Vector2 currentMovement;
    private Vector2 currentLook;

    private bool movementPressed;
    private bool lookPressed;
    private bool runPressed;
    private bool jumpPressed;
    private bool camTogglePressed;
    private bool analogMovement;
    private bool inAnimation = false;

    // Character controller
    private CharacterController controller;

    // Camera and cinemachine
    private Transform mainCamera;

    private float cinemachineTargetYaw;
    private float cinemachineTargetPitch;

    private int cameraMode;
    private bool CameraCourutineInProgress;


    // Player
    private float speed;
    private float animationBlend;
    private float targetRotation = 0.0f;
    private float rotationVelocity;
    private float verticalVelocity;
    private float terminalVelocity = 53.0f;
    private float targetSpeed;

    // Timeout deltatime
    private float jumpTimeoutDelta;
    private float fallTimeoutDelta;

    // Constant variables
    private const float threshold = 0.01f;
    private const float speedOffset = 0.1f;
    private const float RotationSpeed = 1.0f;

    // Gizmo colors for editor
    private Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
    private Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

    private void Awake()
    {
        // Get main camera transform
        mainCamera = Camera.main.transform;

        // Initialize input system
        input = new PlayerInput();

        // Subscribe to input
        // Movement
        input.Player.Move.performed += ctx =>
        {
            currentMovement = ctx.ReadValue<Vector2>();
            movementPressed = currentMovement.x != 0 || currentMovement.y != 0;
        };
        input.Player.Move.canceled += ctx => currentMovement = Vector2.zero;

        // Running
        input.Player.Run.performed += ctx => runPressed = ctx.ReadValueAsButton();
        input.Player.Run.canceled += ctx => runPressed = false;

        // Jumping
        input.Player.Jump.performed += ctx => jumpPressed = ctx.ReadValueAsButton();
        input.Player.Jump.canceled += ctx => jumpPressed = false;

        // Looking
        input.Player.Look.performed += ctx =>
        {
            currentLook = ctx.ReadValue<Vector2>();
            lookPressed = currentLook.x != 0 || currentLook.y != 0;
        };
        input.Player.Look.canceled += ctx => currentLook = Vector2.zero;

        // Camera toggle press
        input.Player.ToggleCamera.performed += ctx => camTogglePressed = ctx.ReadValueAsButton();
        input.Player.ToggleCamera.canceled += ctx => camTogglePressed = false;
    }

    private void Start()
    {
        // Animation setup
        hasAnimator = TryGetComponent(out animator);
        AssignAnimationIDs();

        // Get character controller
        controller = GetComponent<CharacterController>();

        // Reset timers on start
        jumpTimeoutDelta = JumpTimeout;
        fallTimeoutDelta = FallTimeout;
    }

    private void Update()
    {
        GroundedCheck();
        JumpAndGravity();
        Move();

        ControlAnimations();
        CameraToggle();
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    private void AssignAnimationIDs()
    {
        animIDSpeed = Animator.StringToHash("Speed");
        animIDGrounded = Animator.StringToHash("Grounded");
        animIDJump = Animator.StringToHash("Jump");
        animIDFreeFall = Animator.StringToHash("FreeFall");
        animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        animIDisFlying = Animator.StringToHash("isFlying");
        animIDisWaving = Animator.StringToHash("isWaving");
        animIDisDancing = Animator.StringToHash("isDancing");
    }

    private void GroundedCheck()
    {
        // Create sphere and check collision
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);

        // Update animator if using character
        if (hasAnimator)
        {
            animator.SetBool(animIDGrounded, Grounded);
        }
    }

    private void JumpAndGravity()
    {
        if (Grounded)
        {
            // Reset the fall timeout timer if grounded
            fallTimeoutDelta = FallTimeout;

            // Update animator if using character
            if (hasAnimator)
            {
                animator.SetBool(animIDJump, false);
                animator.SetBool(animIDFreeFall, false);
            }

            // Stop our velocity decreasing infinitely while grounded
            if (verticalVelocity < 0.0f)
            {
                verticalVelocity = -2f;
            }

            // Jump if ready to jump
            if (jumpPressed && jumpTimeoutDelta <= 0.0f && !inAnimation)
            {
                // The square root of H * -2 * G = how much velocity needed to reach desired height
                verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                // Update animator if using character
                if (hasAnimator)
                {
                    animator.SetBool(animIDJump, true);
                }
            }

            // Decrease timeout if not ready to jump
            if (jumpTimeoutDelta >= 0.0f)
            {
                jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else // If not grounded, in the air
        {
            // Reset the jump timeout as the character is in the air
            jumpTimeoutDelta = JumpTimeout;

            // Fall timeout for short falls
            if (fallTimeoutDelta >= 0.0f)
            {
                fallTimeoutDelta -= Time.deltaTime;
            }
            else // Not a small fall
            {
                // Update animator if using character
                if (hasAnimator)
                {
                    animator.SetBool(animIDFreeFall, true);
                }
            }
        }

        // Apply gravity over time if under terminal velocity (multiply by delta time twice to linearly speed up over time)
        if (verticalVelocity < terminalVelocity)
        {
            verticalVelocity += Gravity * Time.deltaTime;
        }
    }

    private void Move()
    {
        // Set target speed based on move speed, sprint speed and if sprint is pressed
        targetSpeed = runPressed ? SprintSpeed : MoveSpeed;

        // If there is no input, set the target speed to 0
        if (currentMovement == Vector2.zero) targetSpeed = 0.0f;

        // Reference to the players current horizontal velocity
        float currentHorizontalSpeed = new Vector3(controller.velocity.x, 0.0f, controller.velocity.z).magnitude;

        // Check if movement is analog - between 0 and 1
        analogMovement = (currentMovement.x > 0f && currentMovement.x < 1.0f) || (currentMovement.y > 0f && currentMovement.y < 1.0f);

        // If the movement isn't analog, then make the magnitude 1
        float inputMagnitude = analogMovement ? currentMovement.magnitude : 1f;

        // Accelerate or decelerate to target speed
        if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            // Creates curved result rather than a linear one giving a more organic speed change
            // Note T in Lerp is clamped, so we don't need to clamp our speed
            speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

            // Round speed to 3 decimal places
            speed = Mathf.Round(speed * 1000f) / 1000f;
        }
        else
        {
            speed = targetSpeed;
        }

        // Animation blend
        animationBlend = Mathf.Lerp(animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);

        // Normalize input direction
        Vector3 inputDirection = new Vector3(currentMovement.x, 0.0f, currentMovement.y).normalized;

        // Third person move
        // If there is a move input rotate player when the player is moving
        if (currentMovement != Vector2.zero)
        {
            targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + mainCamera.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, RotationSmoothTime);

            // Rotate to face input direction relative to camera position
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        Vector3 targetDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;

        // Move the player
        controller.Move(targetDirection.normalized * (speed * Time.deltaTime) + new Vector3(0.0f, verticalVelocity, 0.0f) * Time.deltaTime);

        //// First person move
        //if (_input.move != Vector2.zero)
        //{
        //    inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
        //}

        //_controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

        // Update animator if using character
        if (hasAnimator)
        {
            animator.SetFloat(animIDSpeed, animationBlend);
            animator.SetFloat(animIDMotionSpeed, inputMagnitude);
        }
    }

    private void CameraRotation()
    {
        // Third person
        // If there is an input and camera position is not fixed
        if (currentLook.sqrMagnitude >= threshold && !LockCameraPosition)
        {
            cinemachineTargetYaw += currentLook.x * Time.deltaTime;
            cinemachineTargetPitch += currentLook.y * Time.deltaTime;
        }

        // Clamp our rotations so our values are limited 360 degrees
        cinemachineTargetYaw = ClampAngle(cinemachineTargetYaw, float.MinValue, float.MaxValue);

        // Clamp pitch rotation
        cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, BottomClamp, TopClamp);

        // Cinemachine will follow this target
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(cinemachineTargetPitch + CameraAngleOverride, cinemachineTargetYaw, 0.0f);

        //// First person
        //// If there is an input
        //if (currentLook.sqrMagnitude >= threshold)
        //{
        //    cinemachineTargetPitch += currentLook.y * RotationSpeed * Time.deltaTime;
        //    rotationVelocity = currentLook.x * RotationSpeed * Time.deltaTime;

        //    // Clamp pitch rotation
        //    cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, BottomClamp, TopClamp);

        //    // Cinemachine will follow this target
        //    CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(cinemachineTargetPitch, 0.0f, 0.0f);

        //    // Rotate the player with the camera
        //    transform.Rotate(Vector3.up * rotationVelocity);
        //}
    }

    private void CameraToggle()
    {
        // Toggle camera mode on key press, for how many ever camera modes there will be
        if (camTogglePressed && !CameraCourutineInProgress)
        {
            // Final camera mode index case
            if (cameraMode == 1)
            {
                cameraMode = 0;
            }
            else
            {
                cameraMode += 1;
            }
            StartCoroutine(CameraChange());
        }
    }

    private IEnumerator CameraChange()
    {
        CameraCourutineInProgress = true;
        if (cameraMode == 0)
        {
            FPPCamera.SetActive(false);
            TPPCamera.SetActive(true);
            yield return new WaitForSeconds(0.2f);
            MeshRenderer.shadowCastingMode = ShadowCastingMode.On;
        }
        if (cameraMode == 1)
        {
            FPPCamera.SetActive(true);
            TPPCamera.SetActive(false);
            yield return new WaitForSeconds(0.8f);
            MeshRenderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
        }
        yield return new WaitForSeconds(1.0f);
        CameraCourutineInProgress = false;
    }

    private void ControlAnimations()
    {
        // Play animations, only if grounded and not already in animation
        if (Grounded && !inAnimation)
        {
            // Wave animation one shot
            if (Input.GetKeyDown(KeyCode.H))
            {
                StartCoroutine(OneShotAnimation(animIDisWaving));
            }

            // Dance animation loop start
            if (Input.GetKeyDown(KeyCode.J))
            {
                StartLoopAnimation(animIDisDancing);
            }
        }

        // Stop loop animations

        // Dance animation loop stop
        if (Input.GetKeyUp(KeyCode.J))
        {
            StartCoroutine(EndLoopAnimation(animIDisDancing));
        }
    }

    private IEnumerator OneShotAnimation(int animID)
    {
        inAnimation = true;
        // Disable movement while in animation
        movementDisable();
        // To register animation only once
        animator.SetBool(animID, true);
        yield return new WaitForEndOfFrame();
        animator.SetBool(animID, false);
        // Wait for the animation duration
        yield return new WaitForSeconds(5); // TODO: need to get animation clip length
        // Enable movement after animation
        movementEnable();
        inAnimation = false;
    }

    private void StartLoopAnimation(int animID)
    {
        inAnimation = true;
        movementDisable();
        animator.SetBool(animID, true);
    }

    private IEnumerator EndLoopAnimation(int animID)
    {
        animator.SetBool(animIDisDancing, false);
        yield return new WaitForSeconds(3);
        movementEnable();
        inAnimation = false;
    }

    private void movementDisable()
    {
        input.Player.Move.Disable();
        input.Player.Jump.Disable();
        // TODO: disable pointing
        // TODO: disable head look
    }

    private void movementEnable()
    {
        input.Player.Move.Enable();
        input.Player.Jump.Enable();
        // TODO: enable pointing
        // TODO: enable head look
    }
    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        // For clamping camera angle
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void OnEnable()
    {
        // Enable input when character is enabled
        input.Enable();
    }

    private void OnDisable()
    {
        // Disable input when character is disabled
        input.Disable();
    }


    private void OnDrawGizmosSelected()
    {
        // Draw grounded gizmo
        if (Grounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        // When selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
    }
}
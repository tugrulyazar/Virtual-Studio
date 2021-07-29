using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;
using Cinemachine;

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
    private float TopClamp = 60.0f;
    [SerializeField] // How far in degrees can you move the camera down
    private float BottomClamp = 60.0f;
    [SerializeField] // Additional degress to override the camera. Useful for fine tuning camera position when locked
    private float CameraAngleOverride = 0.0f;
    [SerializeField]  // For locking the camera position on all axis
    private bool LockCameraPosition = false;

    [Header("Pointing")]
    [SerializeField]
    private GameObject lookTarget;
    [SerializeField]
    private GameObject playerHead;
    [SerializeField]
    private GameObject playerPos;
    [SerializeField]
    private GameObject tagSphere;
    [SerializeField]
    private GameObject crosshair;
    [SerializeField]
    private Rig headRig;
    [SerializeField]
    private Rig rightHandRig;
    [SerializeField]
    private Rig leftHandRig;

    [Space(10)]
    [SerializeField]
    [Range(1f, 10f)]
    private float targetDistance = 4; // Look target distance from face
    [SerializeField]
    [Range(30f, 180f)]
    private float frontAngle = 120; // Front side angle range from body forward
    [SerializeField]
    [Range(5, 100)]
    private float raycastDistance = 20; // Raycast max distance for pointing

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
    private bool Grounded = true;

    // Head look and zoom
    private float transitionRate = 2.0f;
    private bool notLooking = false;
    private Vector3 heading;
    private CinemachineBrain cinemachineBrain;
    private CinemachineVirtualCamera activeCam;
    private Cinemachine3rdPersonFollow camTPF;
    private CinemachineBasicMultiChannelPerlin camNoise;
    private float cameraFov;
    private float cameraDistance;
    private float zoomFov = 30;
    private float originalFov;
    private float zoomDistance;
    private float originalDistance;
    private float RotationSpeed = 1.0f;
    private RaycastHit hit;
    GameObject mySphere;

    // Timeout deltatime
    private float jumpTimeoutDelta;
    private float fallTimeoutDelta;

    // Constant variables
    private const float threshold = 0.01f;
    private const float speedOffset = 0.1f;
    private const float ZoomRotationSpeed = 0.3f;
    private const float NormalRotationSpeed = 1f;

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

        // Get active cam
        cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
        StartCoroutine(getActiveCamera());

        // Reset timers on start
        jumpTimeoutDelta = JumpTimeout;
        fallTimeoutDelta = FallTimeout;
    }

    private void Update()
    {
        GroundedCheck();
        JumpAndGravity();
        Move();

        // Simple move target according to camera while not validly aiming
        if (!Input.GetKey(KeyCode.Mouse1) || !Physics.Raycast(mainCamera.position, mainCamera.forward, raycastDistance))
        {
            lookTarget.transform.position = playerHead.transform.position + mainCamera.forward * targetDistance;
        }

        // TODO: Move target funcs goes here
        CheckTargetingStatus();
        ManageHead();

        // Hand point and camera zoom goes here
        // ManageHandAndZoom();
        if (Input.GetKey(KeyCode.Mouse1) && !notLooking)
        {
            activateRig(rightHandRig, 3);
            zoomIn();

            // Move the pointing to the center of the screen
            if (Physics.Raycast(mainCamera.position, mainCamera.forward, out hit, raycastDistance))
            {
                lookTarget.transform.position = Vector3.Lerp(lookTarget.transform.position, hit.point, Time.deltaTime * transitionRate * 2);

                // Place sphere at the pointed location
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    if (!mySphere)
                    {
                        mySphere = Instantiate(tagSphere);
                        mySphere.transform.position = hit.point;
                    }
                    else
                    {
                        mySphere.transform.position = hit.point;
                    }
                }
            }
            // If you're not pointing to a valid location
            else if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Destroy(mySphere);
            }


        }
        else if (rightHandRig.weight != 0 && cameraFov != originalFov && cameraDistance != originalDistance)
        {
            deactivateRig(rightHandRig, 4);
            zoomOut();
        }

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
        // If there is an input and camera position is not fixed
        if (currentLook.sqrMagnitude >= threshold && !LockCameraPosition)
        {
            cinemachineTargetYaw += currentLook.x * Time.deltaTime * RotationSpeed;
            cinemachineTargetPitch += currentLook.y * Time.deltaTime * RotationSpeed;
        }

        // Clamp our rotations so our values are limited 360 degrees
        cinemachineTargetYaw = ClampAngle(cinemachineTargetYaw, float.MinValue, float.MaxValue);

        // Clamp pitch rotation
        cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, -TopClamp, BottomClamp);

        // Cinemachine will follow this target
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(cinemachineTargetPitch + CameraAngleOverride, cinemachineTargetYaw, 0.0f);

        //// Rotate the player with the camera
        //rotationVelocity = currentLook.x * Time.deltaTime;
        //transform.Rotate(Vector3.up * rotationVelocity);
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

    private void CheckTargetingStatus()
    {
        // Get the angle between the camera and player heading and target position in left/right
        heading = lookTarget.transform.position - playerPos.transform.position;
        float angle = Vector3.Angle(heading, playerPos.transform.forward);
        float angleDir = AngleDir(playerPos.transform.position, heading); // 1: left , -1: right

        // If the target is behind, don't look
        if (inAnimation)
        {
            notLooking = true;
        }
        else
        {
            notLooking = (angle > frontAngle) ? true : false;
        }
    }

    private void ManageHead()
    {
        // Decrease head constraint weights over time, disable unnecessary infinite lerping
        if (notLooking)
        {
            deactivateRig(headRig, 1);
        }
        else if (!notLooking)
        {
            activateRig(headRig, 1);
        }
    }

    private void activateRig(Rig rig, float rate)
    {
        if (rig.weight != 1)
        {
            // Lerp rig constraint weights
            rig.weight = (rig.weight > 0.99) ? 1 : Mathf.Lerp(rig.weight, 1, Time.deltaTime * transitionRate * rate);
        }
    }

    private void deactivateRig(Rig rig, float rate)
    {
        if (rig.weight != 0)
        {
            // Lerp rig constraint weights
            rig.weight = (rig.weight < 0.01) ? 0 : Mathf.Lerp(rig.weight, 0, Time.deltaTime * transitionRate * rate);
        }
    }

    private void zoomIn()
    {
        // Enable crosshair
        if (!crosshair.activeSelf)
        {
            crosshair.SetActive(true);
        }

        if (cameraFov != zoomFov && cameraDistance != zoomDistance)
        {
            // Lerp camera fov to zoom fov
            cameraFov = (cameraFov < zoomFov + 0.1f) ? zoomFov : Mathf.Lerp(cameraFov, zoomFov, Time.deltaTime * transitionRate * 2);
            activeCam.m_Lens.FieldOfView = cameraFov;

            // Lerp camera distance to zoom distance
            cameraDistance = (cameraDistance < zoomDistance + 0.01f) ? zoomDistance : Mathf.Lerp(cameraDistance, zoomDistance, Time.deltaTime * transitionRate * 2);
            camTPF.CameraDistance = cameraDistance;
        }

        // Change sensitivity
        if (RotationSpeed != ZoomRotationSpeed)
        {
            RotationSpeed = ZoomRotationSpeed;
        }

        // Stop perlin noise
        camNoise.m_FrequencyGain = 0;
    }

    private void zoomOut()
    {
        // Disable crosshair
        if (crosshair.activeSelf)
        {
            crosshair.SetActive(false);
        }

        // Change camera fov and distance
        if (cameraFov != originalFov && cameraDistance != originalDistance)
        {
            // Lerp camera fov to original
            cameraFov = (cameraFov > originalFov - 0.01f) ? originalFov : Mathf.Lerp(cameraFov, originalFov, Time.deltaTime * transitionRate * 2);
            activeCam.m_Lens.FieldOfView = cameraFov;

            // Lerp camera distance to original
            cameraDistance = (cameraDistance > originalDistance - 0.1f) ? originalDistance : Mathf.Lerp(cameraDistance, originalDistance, Time.deltaTime * transitionRate * 2);
            camTPF.CameraDistance = cameraDistance;
        }

        // Change sensitivity
        if (RotationSpeed != NormalRotationSpeed)
        {
            RotationSpeed = NormalRotationSpeed;
        }

        // Resume perlin noise
        camNoise.m_FrequencyGain = 0.3f;
    }

    private float AngleDir(Vector3 fwd, Vector3 targetDir)
    {
        Vector3 perp = Vector3.Cross(fwd, targetDir);
        float dir = Vector3.Dot(perp, Vector3.up);

        if (dir > 0f)
        {
            return 1f;
        }
        else if (dir < 0f)
        {
            return -1f;
        }
        else
        {
            return 0f;
        }
    }

    private IEnumerator CameraChange()
    {
        CameraCourutineInProgress = true;
        if (cameraMode == 0)
        {
            FPPCamera.SetActive(false);
            TPPCamera.SetActive(true);
            StartCoroutine(getActiveCamera());
            yield return new WaitForSeconds(0.2f);
            MeshRenderer.shadowCastingMode = ShadowCastingMode.On;
        }
        if (cameraMode == 1)
        {
            FPPCamera.SetActive(true);
            TPPCamera.SetActive(false);
            StartCoroutine(getActiveCamera());
            yield return new WaitForSeconds(0.8f);
            MeshRenderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
        }
        yield return new WaitForSeconds(1.0f);
        CameraCourutineInProgress = false;
    }

    private IEnumerator getActiveCamera()
    {
        yield return null;
        activeCam = cinemachineBrain.ActiveVirtualCamera as CinemachineVirtualCamera;
        originalFov = activeCam.m_Lens.FieldOfView;
        camNoise = activeCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        camTPF = activeCam.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        originalDistance = camTPF.CameraDistance;
        zoomDistance = originalDistance - 2;

        // Set dynamic camera values
        cameraFov = originalFov;
        cameraDistance = originalDistance;
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
        yield return new WaitForSeconds(4); // TODO: need to get animation clip length
        
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
    }

    private void movementEnable()
    {
        input.Player.Move.Enable();
        input.Player.Jump.Enable();
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
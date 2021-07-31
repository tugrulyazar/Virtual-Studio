using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Animations.Rigging;
using Cinemachine;
using TMPro;

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
    private Transform lookTarget;
    [SerializeField]
    private Transform playerHead;
    [SerializeField]
    private Transform playerPos;
    [SerializeField]
    private GameObject tagObject;
    [SerializeField]
    private GameObject permObject;
    [SerializeField]
    private GameObject distObject;
    [SerializeField]
    private GameObject distLine;
    [SerializeField]
    private GameObject textObject;
    [SerializeField]
    private GameObject crosshair;
    [SerializeField]
    private Rig headRig;
    [SerializeField]
    private Rig rightHandRig;
    [SerializeField]
    private Rig leftHandRig;
    [SerializeField]
    private LayerMask layerMask;

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
    private CinemachineBrain cinemachineBrain;
    private CinemachineVirtualCamera activeCam;
    private Cinemachine3rdPersonFollow camTPF;
    private CinemachineBasicMultiChannelPerlin camNoise;
    private Transform mainCamera;
    private float cinemachineTargetYaw;
    private float cinemachineTargetPitch;
    private int cameraMode;
    private bool CameraCoroutineInProgress;
    private float cameraFov;
    private float cameraDistance;
    private float zoomFov = 30;
    private float zoomOffset = 2;
    private float originalFov;
    private float zoomDistance;
    private float originalDistance;
    private float shoulderSide;

    // Player
    private float speed;
    private float animationBlend;
    private float targetRotation = 0.0f;
    private float rotationVelocity;
    private float verticalVelocity;
    private float terminalVelocity = 53.0f;
    private float targetSpeed;
    private bool Grounded = true;

    // Head look, hand point and zoom
    private float transitionRate = 2.0f;
    private bool notLooking = false;
    private bool rotateInProgress = false;
    private bool zoomedIn;
    private Vector3 heading;
    private float RotationSpeed = 1.0f;
    private const float lookTimeout = 3f;
    private float lookTimeoutDelta;
    private RaycastHit hit;
    GameObject myTag;
    GameObject myDistTag;
    private Rig handRig;

    // Timeout deltatime
    private float jumpTimeoutDelta;
    private float fallTimeoutDelta;

    // Constant variables
    private const float threshold = 0.01f;
    private const float speedOffset = 0.1f;
    private const float ZoomRotationSpeed = 0.3f;
    private const float NormalRotationSpeed = 1f;
    private const float minZoomOffset = 2f;
    private const float maxZoomOffset = 6f;

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
        StartCoroutine(GetActiveCamera());

        // Set active hand
        handRig = rightHandRig;

        // Reset timers on start
        jumpTimeoutDelta = JumpTimeout;
        fallTimeoutDelta = FallTimeout;
        lookTimeoutDelta = lookTimeout;
    }

    private void Update()
    {
        GroundedCheck();
        JumpAndGravity();
        MovePlayer();
        MoveLookTarget();
        CheckTargetingStatus();
        ManageHead();
        ManagePointAndZoom();
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

    private void MovePlayer()
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

        // Update animator if using character
        if (hasAnimator)
        {
            animator.SetFloat(animIDSpeed, animationBlend);
            animator.SetFloat(animIDMotionSpeed, inputMagnitude);
        }
    }

    private void MoveLookTarget()
    {
        // Simple move target according to camera while not validly aiming
        if (!zoomedIn || !Physics.Raycast(mainCamera.position, mainCamera.forward, raycastDistance))
        {
            Vector3 prevPos = lookTarget.position;
            Vector3 nextPos = playerHead.position + mainCamera.forward * targetDistance;

            if (prevPos != nextPos)
            {
                prevPos = Vector3.Lerp(prevPos, nextPos, Time.deltaTime * transitionRate * 2);
            }

            lookTarget.position = prevPos;
        }
    }

    private void ManagePointAndZoom()
    {
        // Rotate to target on mouse press if not looking
        if (Input.GetKeyDown(KeyCode.Mouse1) && notLooking && !rotateInProgress)
        {
            StartCoroutine(RotateToTarget(lookTarget));
        }

        // Zoom in
        if (Input.GetKey(KeyCode.Mouse1) && !notLooking)
        {
            ActivateRig(handRig, 3);
            DisableCamToggle();
            ZoomIn();

            // Move the pointing to the center of the screen
            if (Physics.Raycast(mainCamera.position, mainCamera.forward, out hit, raycastDistance, layerMask))
            {
                lookTarget.position = Vector3.Lerp(lookTarget.position, hit.point, Time.deltaTime * transitionRate * 3);

                // Place tag at pointed location
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    if (!myTag)
                    {
                        myTag = Instantiate(tagObject);
                        myTag.transform.position = hit.point;
                    }
                    else
                    {
                        myTag.transform.position = hit.point;
                    }
                }

                // Place perm tag at pointed location
                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (!hit.transform.CompareTag("PermObject"))
                    {
                        Instantiate(permObject, hit.point, Quaternion.identity);
                    }
                    else
                    {
                        Destroy(hit.transform.gameObject);
                    }
                }

                // Distance spheres
                if (Input.GetKeyDown(KeyCode.C))
                {
                    GameObject[] distObjects = GameObject.FindGameObjectsWithTag("DistObject");
                    if (distObjects.Length == 2)
                    {
                        foreach (GameObject obj in distObjects)
                        {
                            Destroy(obj);
                        }
                    }
                    else if (distObjects.Length == 1)
                    {
                        myDistTag = Instantiate(distObject);
                        myDistTag.transform.position = hit.point;

                        Vector3 start = distObjects[0].transform.position;
                        Vector3 end = myDistTag.transform.position;
                        Vector3 mid = Vector3.Lerp(start, end, 0.5f);
                        float distance = Vector3.Distance(start, end);
                        distance = Mathf.Round(distance * 100f) / 100f;
                        Color color = Color.yellow;

                        DrawLine(start, end);
                        GameObject distText = Instantiate(textObject);
                        distText.transform.position = mid;
                        TextMeshPro tmp = distText.GetComponent<TextMeshPro>();
                        tmp.SetText(distance + "m");

                        Vector3 dir = (distText.transform.position - transform.position).normalized;
                        Vector3 targetDirection = new Vector3(dir.x, 0, dir.z);
                        Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
                        distText.transform.rotation = Quaternion.RotateTowards(distText.transform.rotation, targetRotation, 360);
                    }
                    else
                    {
                        myDistTag = Instantiate(distObject);
                        myDistTag.transform.position = hit.point;
                    }

                }

            }
            // If you're not clicking to a valid location
            else if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Destroy(myTag);
            }

            // Destroy perm objects
            if (Input.GetKeyDown(KeyCode.Mouse2))
            {
                GameObject[] permObjects = GameObject.FindGameObjectsWithTag("PermObject");
                foreach (GameObject obj in permObjects)
                {
                    Destroy(obj);
                }
            }

            // Scroll wheel zoom set
            if (Input.mouseScrollDelta.y > 0)
            {
                if (cameraMode == 0)
                {
                    zoomOffset = Mathf.Clamp(zoomOffset + 0.5f, minZoomOffset, maxZoomOffset);
                }
                else if (cameraMode == 1)
                {
                    zoomOffset = Mathf.Clamp(zoomOffset + 0.5f, 0, maxZoomOffset - minZoomOffset);
                }
                zoomDistance = originalDistance - zoomOffset;
            }

            if (Input.mouseScrollDelta.y < 0)
            {
                if (cameraMode == 0)
                {
                    zoomOffset = Mathf.Clamp(zoomOffset - 0.5f, minZoomOffset, maxZoomOffset);
                }
                else if (cameraMode == 1)
                {
                    zoomOffset = Mathf.Clamp(zoomOffset - 0.5f, 0, maxZoomOffset - minZoomOffset);
                }
                zoomDistance = originalDistance - zoomOffset;
            }
        }

        // Zoom out
        else if (handRig.weight != 0 || cameraFov != originalFov || cameraDistance != originalDistance)
        {
            if (cameraMode == 0)
            {
                // TPP zoom distance
                zoomDistance = originalDistance - minZoomOffset;
            }
            else if (cameraMode == 1)
            {
                // FPP zoom distance
                zoomDistance = 0;
            }

            DeactivateRig(handRig, 4);
            EnableCamToggle();
            ZoomOut();
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

        // First person character rotation
        if (cameraMode == 1)
        {
            // Get rotation velocity
            rotationVelocity = currentLook.x * RotationSpeed * Time.deltaTime;

            // Rotate the player with the camera
            transform.Rotate(Vector3.up * rotationVelocity);
        }

        // Third person delayed character rotation
        if (cameraMode == 0)
        {
            if (lookTimeoutDelta < 0 && !rotateInProgress)
            {
                StartCoroutine(RotateToTarget(lookTarget));
            }
        }
    }

    private IEnumerator RotateToTarget(Transform target)
    {
        rotateInProgress = true;
        MovementDisable();
        Vector3 dir = (target.transform.position - transform.position).normalized;
        Vector3 targetDirection = new Vector3(dir.x, 0, dir.z);
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);

        do
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 3);
            yield return null;
        } while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f);

        lookTimeoutDelta = lookTimeout;
        MovementEnable();
        rotateInProgress = false;
    }

    private void CameraToggle()
    {
        // Toggle camera mode on key press, for how many ever camera modes there will be
        if (camTogglePressed && !CameraCoroutineInProgress && cameraDistance == originalDistance)
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

        // Shoulder toggle
        if (Input.GetKeyDown(KeyCode.R) && !CameraCoroutineInProgress && cameraMode == 0 && !zoomedIn)
        {
            StartCoroutine(ShoulderChange());
        }
    }

    private void DisableCamToggle()
    {
        if (input.Player.ToggleCamera.enabled) input.Player.ToggleCamera.Disable();
    }

    private void EnableCamToggle()
    {
        if (!input.Player.ToggleCamera.enabled) input.Player.ToggleCamera.Enable();
    }

    private void CheckTargetingStatus()
    {
        // Get the angle between the camera and player heading and target position in left/right
        heading = lookTarget.position - playerPos.position;
        float angle = Vector3.Angle(heading, playerPos.forward);
        float angleDir = AngleDir(playerPos.position, heading); // 1: left , -1: right

        if (inAnimation)
        {
            // If in animation, don't look
            notLooking = true;
        }
        else
        {
            // If the target is behind and not player isn't moving, decrease timer
            if (angle > frontAngle)
            {
                notLooking = true;
                if (speed == 0)
                {
                    if (lookTimeoutDelta >= 0)
                    {
                        lookTimeoutDelta -= Time.deltaTime;
                    }
                }
                else
                {
                    lookTimeoutDelta = lookTimeout;
                }
            }
            else
            {
                notLooking = false;
                lookTimeoutDelta = lookTimeout;
            }
        }
    }

    private void ManageHead()
    {
        // Decrease head constraint weights over time, disable unnecessary infinite lerping
        if (notLooking)
        {
            DeactivateRig(headRig, 1);
        }
        else if (!notLooking)
        {
            ActivateRig(headRig, 1);
        }
    }

    private void ActivateRig(Rig rig, float rate)
    {
        if (rig.weight != 1)
        {
            // Lerp rig constraint weights
            rig.weight = (rig.weight > 0.99) ? 1 : Mathf.Lerp(rig.weight, 1, Time.deltaTime * transitionRate * rate);
        }
    }

    private void DeactivateRig(Rig rig, float rate)
    {
        if (rig.weight != 0)
        {
            // Lerp rig constraint weights
            rig.weight = (rig.weight < 0.01) ? 0 : Mathf.Lerp(rig.weight, 0, Time.deltaTime * transitionRate * rate);
        }
    }

    private void ZoomIn()
    {
        zoomedIn = true;

        // Enable crosshair
        if (!crosshair.activeSelf)
        {
            crosshair.SetActive(true);
        }

        if (cameraFov != zoomFov)
        {
            // Lerp camera fov to zoom fov
            cameraFov = (cameraFov < zoomFov + 0.01f) ? zoomFov : Mathf.Lerp(cameraFov, zoomFov, Time.deltaTime * transitionRate * 2);
            activeCam.m_Lens.FieldOfView = cameraFov;
        }

        // Lerp camera distance to zoom distance, kept dynamic because of scroll
        cameraDistance = (cameraDistance < zoomDistance + 0.01f) ? zoomDistance : Mathf.Lerp(cameraDistance, zoomDistance, Time.deltaTime * transitionRate * 2);
        camTPF.CameraDistance = cameraDistance;

        // Change sensitivity
        if (RotationSpeed != ZoomRotationSpeed) RotationSpeed = ZoomRotationSpeed;

        // Stop perlin noise, unless in FPP
        if (cameraMode != 1 && camNoise.m_FrequencyGain != 0) camNoise.m_FrequencyGain = 0;
    }

    void DrawLine(Vector3 start, Vector3 end)
    {
        GameObject myLine = Instantiate(distLine);
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
    }

    private void ZoomOut()
    {
        zoomedIn = false;

        // Disable crosshair
        if (crosshair.activeSelf)
        {
            crosshair.SetActive(false);
        }

        // Change camera fov and distance
        if (cameraFov != originalFov)
        {
            // Lerp camera fov to original
            cameraFov = (cameraFov > originalFov - 0.01f) ? originalFov : Mathf.Lerp(cameraFov, originalFov, Time.deltaTime * transitionRate * 2);
            activeCam.m_Lens.FieldOfView = cameraFov;
        }

        if (cameraDistance != originalDistance)
        {
            // Lerp camera distance to original
            cameraDistance = (cameraDistance > originalDistance - 0.01f) ? originalDistance : Mathf.Lerp(cameraDistance, originalDistance, Time.deltaTime * transitionRate * 2);
            camTPF.CameraDistance = cameraDistance;
        }

        // Change sensitivity
        if (RotationSpeed != NormalRotationSpeed) RotationSpeed = NormalRotationSpeed;

        // Resume perlin noise, unless in FPP
        if (cameraMode != 1 && camNoise.m_FrequencyGain != 0.3f) camNoise.m_FrequencyGain = 0.3f;
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
        CameraCoroutineInProgress = true;
        if (cameraMode == 0)
        {
            // Third-person perspective
            FPPCamera.SetActive(false);
            TPPCamera.SetActive(true);
            yield return null;
            StartCoroutine(GetActiveCamera());
            yield return new WaitForSeconds(0.2f);
            // Resume perlin noise
            if (camNoise.m_FrequencyGain != 0.3f) camNoise.m_FrequencyGain = 0.3f;
            MeshRenderer.shadowCastingMode = ShadowCastingMode.On;
        }
        if (cameraMode == 1)
        {
            // First-person perspective
            FPPCamera.SetActive(true);
            TPPCamera.SetActive(false);
            yield return null;
            StartCoroutine(GetActiveCamera());
            yield return new WaitForSeconds(0.8f);
            // Stop perlin noise
            if (camNoise.m_FrequencyGain != 0) camNoise.m_FrequencyGain = 0;
            MeshRenderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
        }
        yield return new WaitForSeconds(1.0f);
        CameraCoroutineInProgress = false;
    }

    private IEnumerator ShoulderChange()
    {
        CameraCoroutineInProgress = true;

        // Shoulder side is between 0 (left) and 1 (right)
        float camSide = shoulderSide;
        float targetSide = 1f - shoulderSide;

        // Switch hands
        if (camSide > targetSide)
        {
            handRig = leftHandRig;
        }
        else
        {
            handRig = rightHandRig;
        }

        // Lerp shoulder side
        do
        {
            camSide = Mathf.Lerp(camSide, targetSide, Time.deltaTime * transitionRate * 2);
            camTPF.CameraSide = camSide;
            yield return null;
        } while (Mathf.Abs(camSide - targetSide) > 0.001f);

        // Zero side
        camTPF.CameraSide = targetSide;
        // Set new shoulder side
        shoulderSide = targetSide;
        CameraCoroutineInProgress = false;
    }

    private IEnumerator GetActiveCamera()
    {
        yield return null;
        activeCam = cinemachineBrain.ActiveVirtualCamera as CinemachineVirtualCamera;
        originalFov = activeCam.m_Lens.FieldOfView;
        camNoise = activeCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        camTPF = activeCam.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        originalDistance = camTPF.CameraDistance;
        shoulderSide = camTPF.CameraSide;

        if (cameraMode == 0)
        {
            // TPP zoom distance
            zoomDistance = originalDistance - minZoomOffset;
        }
        else if (cameraMode == 1)
        {
            // FPP zoom distance
            zoomDistance = 0;
        }

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
        MovementDisable();

        // To register animation only once
        animator.SetBool(animID, true);
        yield return new WaitForEndOfFrame();
        animator.SetBool(animID, false);

        // Wait for the animation duration
        yield return new WaitForSeconds(4); // TODO: need to get animation clip length

        // Enable movement after animation
        MovementEnable();
        inAnimation = false;
    }

    private void StartLoopAnimation(int animID)
    {
        inAnimation = true;
        MovementDisable();
        animator.SetBool(animID, true);
    }

    private IEnumerator EndLoopAnimation(int animID)
    {
        animator.SetBool(animIDisDancing, false);
        yield return new WaitForSeconds(3);
        MovementEnable();
        inAnimation = false;
    }

    private void MovementDisable()
    {
        input.Player.Move.Disable();
        input.Player.Jump.Disable();
    }

    private void MovementEnable()
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
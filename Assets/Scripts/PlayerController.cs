using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace UserBehaviour
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Player")]
        [SerializeField] // Character mesh renderer
        private SkinnedMeshRenderer meshRenderer;
        [SerializeField] // Player head transform
        private Transform playerHead;
        [SerializeField] // Wheelchair model
        private GameObject wheelchair;

        [Header("Movement")]
        [SerializeField] // Move speed of the character in m/s
        private float moveSpeed = 2.0f;
        [SerializeField] // Sprint speed of the character in m/s
        private float sprintSpeed = 6f;
        [SerializeField] // Flying speed of the character in m/s
        private float flySpeed = 4f;
        [SerializeField] // Fast flying speed of the character in m/s
        private float fastFlySpeed = 20f;
        [SerializeField] // Acceleration and deceleration rate
        private float groundAcceleration = 10.0f;
        [SerializeField] // Flight acceleration and deceleration rate
        private float flyAcceleration = 3.0f;

        [Header("Jump")]
        [SerializeField] // The height the player can jump
        private float jumpHeight = 1.2f;
        [SerializeField] // The height the player jumps when initiating flight
        private float flyJumpHeight = 2f;
        [SerializeField] // The character uses its own gravity value. The engine default is -9.81f
        private float gravity = -15.0f;
        [SerializeField] // Time required to pass before being able to jump again. Set to 0f to instantly jump again
        private float jumpTimeout = 0.50f;
        [SerializeField] // Time required to pass before entering the fall state. Useful for walking down stairs
        private float fallTimeout = 0.15f;

        [Space(10)]
        [SerializeField] // Useful for rough ground
        public float groundedOffset = -0.14f;
        [SerializeField] // The radius of the grounded check. Should match the radius of the CharacterController
        public float groundedRadius = 0.15f;
        [SerializeField] // What layers the character uses as ground
        private LayerMask groundLayers;

        [Header("Cinemachine")]
        [SerializeField] // Third person camera object
        private GameObject TPPCamera;
        [SerializeField] // First person camera object
        private GameObject FPPCamera;
        [SerializeField] // The follow target set in the Cinemachine Virtual Camera that the camera will follow
        private GameObject cinemachineCameraTarget;
        [SerializeField] // How fast the character turns to face movement direction
        [Range(0.0f, 0.3f)]
        private float rotationSmoothTime = 0.12f;
        [SerializeField] // Camera layer mask for walking
        private LayerMask walkingLayermask;
        [SerializeField] // Camera layer mask for flying/noclip
        private LayerMask flyingLayermask;

        [Space(10)]
        [SerializeField] // How far in degrees can you move the camera up
        private float topClamp = 60.0f;
        [SerializeField] // How far in degrees can you move the camera down
        private float bottomClamp = 70.0f;
        [SerializeField] // Additional degress to override the camera. Useful for fine tuning camera position when locked
        private float cameraAngleOverride = 0.0f;
        [SerializeField]  // For locking the camera position on all axis
        private bool lockCameraPosition = false;

        [Header("Pointing")]
        [SerializeField] // Head rig for looking at
        private Rig headRig;
        [SerializeField] // Head rig for looking at
        private Rig lookRig;
        [SerializeField] // Right hand rig for pointing
        private Rig rightHandRig;
        [SerializeField] // Left hand rig for pointing
        private Rig leftHandRig;
        [SerializeField] // Target for looking and pointing
        private Transform lookTarget;

        [Space(10)]
        [SerializeField] // Look target distance from face
        [Range(1f, 10f)]
        private float targetDistance = 4;
        [SerializeField] // Front side angle range from body forward
        [Range(30f, 180f)]
        private float frontAngle = 100;
        [SerializeField] // Raycast max distance for pointing
        [Range(5, 100)]
        private float raycastDistance = 30;
        [SerializeField] // Raycast layer mask
        private LayerMask raycastLayerMask;

        [Header("Objects")]
        [SerializeField] // Pointing tag object
        private GameObject tagObject;
        [SerializeField] // Pointing permanent tag object
        private GameObject permObject;
        [SerializeField] // Distance measuring tag object
        private GameObject distObject;
        [SerializeField] // Distance measuring line object
        private GameObject distLine;
        [SerializeField] // Text object
        private GameObject textObject;
        [SerializeField] // Distance measuring tag object
        private GameObject elevationObject;
        [SerializeField] // Pointing mode crosshair
        private GameObject crosshair;
        [SerializeField] // Permanent tag object context menu
        private Image tagMenu;

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
        private int animIDisSitting;
        private int animIDisWaving;
        private int animIDisDancing;
        private int animIDonWheelchair;
        private int animIDturnLeft;
        private int animIDturnRight;

        // Input system
        private PlayerInput input;

        private Vector2 currentMovement;
        private int currentMovementDir;
        private Vector2 currentLook;
        private float currentAscension;

        private bool movementPressed;
        private bool lookPressed;
        private bool runPressed;
        private bool jumpPressed;
        private bool sitPressed;
        private bool flightTogglePressed;
        private bool ascendPressed;
        private bool camTogglePressed;
        private bool shoulderTogglePressed;
        private bool analogMovement;
        private bool wavePressed;
        private bool dancePressed;
        private bool danceReleased;

        // Player
        private float speed;
        private float horizontalSpeed;
        private float verticalSpeed;
        private float targetRotation = 0.0f;
        private float targetSpeed;
        private float targetHorizontalSpeed;
        private float targetVerticalSpeed;
        private float rotationVelocity;
        private float verticalVelocity;
        private float terminalVelocity = 53.0f;

        // Character controller
        private CharacterController controller;

        // Cinemachine
        private Transform mainCamera;
        private CinemachineBrain cinemachineBrain;
        private CinemachineVirtualCamera activeCam;
        private Cinemachine3rdPersonFollow camTPF;
        private CinemachineBasicMultiChannelPerlin camNoise;

        private float cinemachineTargetYaw;
        private float cinemachineTargetPitch;

        // Camera fov and distances
        private float originalFov;
        private float originalDistance;

        private const float zoomFov = 30;
        private const float minZoomOffset = 2f;
        private const float maxZoomOffset = 6f;
        private const float zoomRotationSpeed = 0.3f;
        private const float normalRotationSpeed = 1f;

        // Dynamic camera variables
        private float cameraFov;
        private float cameraDistance;
        private float zoomDistance;
        private float zoomOffset;
        private float shoulderSide;
        private float rotationSpeed = 1.0f;

        // Camera states
        private int cameraMode;
        private bool CameraCoroutineInProgress;

        // Targeting: head look, hand point and zoom
        private const float handActivate_TRate = 6;
        private const float handDeactivate_TRate = 10;
        private const float headActivate_TRate = 2;
        private const float headDeactivate_TRate = 1;
        private const float lookTarget_TRate = 5;
        private const float zoom_TRate = 6;
        private const float wheelchairRotate_TRate = 10;
        private const float shoulderSwitch_TRate = 5;
        private const float lookTimeout = 2f;

        // Dynamic targeting variables
        private Rig handRig;

        private RaycastHit aimHit;
        private RaycastHit wheelchairHit;
        private float angleDir;

        // Tag objects
        private const float deleteHoldTimeout = 2f;

        private GameObject tempTag;
        private GameObject myDistTag;

        // Timeout deltatime
        private float jumpTimeoutDelta;
        private float fallTimeoutDelta;
        private float lookTimeoutDelta;
        private float holdTimeoutDelta;

        // Constant variables
        private const float camRotateThreshold = 0.01f;
        private const float speedOffset = 0.1f;
        private const float smoothTime = 1f;
        private const float smoothCount = smoothTime * 60;


        // States
        public bool grounded;

        private bool isFlying;
        private bool isSitting;
        private bool onWheelchair;
        private bool inRotation;
        private bool inAnimation;
        private bool inStaticAnimation;
        private bool inLoopAnimation;
        private bool isTargetValid;
        private bool notLooking;
        private bool zoomedIn;
        private bool zoomedOut;
        private bool sitDelayed;

        private void Awake()
        {
            // Get main camera transform
            mainCamera = Camera.main.transform;

            // Initialize input system
            input = new PlayerInput();

            // Input System
            SubscribeToInput();
        }

        private void Start()
        {
            // Mouse lock
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            // Animation setup
            hasAnimator = TryGetComponent(out animator);
            AssignAnimationIDs();

            // Get character controller
            controller = GetComponent<CharacterController>();

            // Get active cam
            cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
            StartCoroutine(GetActiveCamera());

            // Set default states
            grounded = true;
            isFlying = false;
            isSitting = false;
            onWheelchair = false;
            inAnimation = false;
            inStaticAnimation = false;
            inLoopAnimation = false;
            isTargetValid = false;
            notLooking = false;
            zoomedIn = false;
            zoomedOut = true;
            sitDelayed = false;

            // Set active hand
            handRig = rightHandRig;

            // Reset timers on start
            jumpTimeoutDelta = jumpTimeout;
            fallTimeoutDelta = fallTimeout;
            lookTimeoutDelta = lookTimeout;
            holdTimeoutDelta = deleteHoldTimeout;
        }

        private void Update()
        {
            if (!isFlying)
            {
                GroundedCheck();
                JumpAndGravity();
                MovePlayer();
            }
            else
            {
                FlyPlayer();
            }

            ManageFlight();
            MoveLookTarget();
            CheckTargetingStatus();
            ManageHead();
            ManagePointAndZoom();

            if (hasAnimator)
            {
                ControlAnimations();
            }

            if (onWheelchair)
            {
                wheelchairRotate();
            }
        }

        private void LateUpdate()
        {
            CameraRotation();
            CameraToggle();
        }

        private void GroundedCheck()
        {
            // Create sphere and check collision
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z);
            grounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);

            // Update animator if using character
            if (hasAnimator)
            {
                animator.SetBool(animIDGrounded, grounded);
            }
        }

        private void JumpAndGravity()
        {
            if (grounded)
            {
                // Reset the fall timeout timer if grounded
                fallTimeoutDelta = fallTimeout;

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
                if (jumpPressed && jumpTimeoutDelta <= 0.0f && !inAnimation && !onWheelchair)
                {
                    // The square root of H * -2 * G = how much velocity needed to reach desired height
                    verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

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
                jumpTimeoutDelta = jumpTimeout;

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
                verticalVelocity += gravity * Time.deltaTime;
            }
        }

        private void MovePlayer()
        {
            // Set target speed based on move speed, sprint speed and if sprint is pressed
            targetSpeed = runPressed ? sprintSpeed : moveSpeed;

            // If there is no input, set the target speed to 0
            if (currentMovement == Vector2.zero) targetSpeed = 0.0f;

            // Check if movement is analog (for controller input) - between 0 and 1
            analogMovement = (currentMovement.x > 0f && currentMovement.x < 1.0f) || (currentMovement.y > 0f && currentMovement.y < 1.0f);

            // If the movement isn't analog, then make the magnitude 1
            float inputMagnitude = analogMovement ? currentMovement.magnitude : 1f;

            // Reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(controller.velocity.x, 0.0f, controller.velocity.z).magnitude;

            // Accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // Creates curved result rather than a linear one giving a more organic speed change
                // Note T in Lerp is clamped, so we don't need to clamp our speed
                speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * groundAcceleration);

                // Round speed to 3 decimal places
                speed = Mathf.Round(speed * 1000f) / 1000f;
            }
            else
            {
                speed = targetSpeed;
            }

            // Normalize input direction
            Vector3 inputDirection = new Vector3(currentMovement.x, 0.0f, currentMovement.y).normalized;

            // Third person move
            if (cameraMode == 0)
            {
                // If there is a move input rotate player when the player is moving
                if (currentMovement != Vector2.zero)
                {
                    targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + mainCamera.eulerAngles.y;
                    float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, rotationSmoothTime);

                    // Rotate to face input direction relative to camera position
                    transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                }

                Vector3 targetDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;

                // Move the player
                controller.Move(targetDirection.normalized * (speed * Time.deltaTime) + new Vector3(0.0f, verticalVelocity, 0.0f) * Time.deltaTime);
            }

            // First person move
            else if (cameraMode == 1)
            {
                if (currentMovement != Vector2.zero)
                {
                    inputDirection = transform.right * currentMovement.x + transform.forward * currentMovement.y;
                }

                controller.Move(inputDirection.normalized * (speed * Time.deltaTime) + new Vector3(0.0f, verticalVelocity, 0.0f) * Time.deltaTime);
            }

            // Update animator if using character
            if (hasAnimator)
            {
                animator.SetFloat(animIDSpeed, speed);
                animator.SetFloat(animIDMotionSpeed, inputMagnitude);
            }
        }

        private void ManageFlight()
        {
            // Initiate flight if ready to jump
            if (!isFlying && flightTogglePressed && jumpTimeoutDelta <= 0.0f && !inAnimation && !onWheelchair)
            {
                StartCoroutine(StartFlight());
            }

            // Stop flight if flying
            if (isFlying && flightTogglePressed && !inAnimation)
            {
                StartCoroutine(EndFlight());
            }
        }

        private IEnumerator StartFlight()
        {
            // Set states
            isFlying = true;
            inAnimation = true;
            grounded = false;

            // Change collision filter
            camTPF.CameraCollisionFilter = flyingLayermask;

            // Turn collider/controller off
            controller.enabled = false;

            // Zero ground speed
            speed = 0;

            // Zero flying speed
            horizontalSpeed = 0f;
            verticalSpeed = 0f;

            if (hasAnimator)
            {
                // Stop ground animations
                animator.SetFloat(animIDSpeed, 0);
                animator.SetFloat(animIDMotionSpeed, 0);

                // Flying state
                animator.SetBool(animIDisFlying, true);
            }

            // Get current pos and target pos
            Vector3 targetPos = transform.position + new Vector3(0, flyJumpHeight, 0);

            // Lerp to target pos
            do
            {
                transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 4);
                yield return null;
            } while (transform.position.y < targetPos.y - 0.01f && !movementPressed);

            // Set state
            inAnimation = false;
        }

        private IEnumerator EndFlight()
        {
            isFlying = false;
            camTPF.CameraCollisionFilter = walkingLayermask;
            if (hasAnimator)
            {
                animator.SetBool(animIDisFlying, false);
            }
            controller.enabled = true;
            yield return null;
        }

        private void FlyPlayer()
        {
            // Set target speed based on move speed, sprint speed and if sprint is pressed
            targetSpeed = runPressed ? fastFlySpeed : flySpeed;
            targetHorizontalSpeed = targetSpeed;

            // Get target vertical speed through input
            targetVerticalSpeed = targetSpeed * currentAscension;

            // If there is no input, set the target speeds to 0
            if (currentMovement == Vector2.zero) targetHorizontalSpeed = 0.0f;

            // Check if movement is analog (for controller input) - between 0 and 1
            analogMovement = (currentMovement.x > 0f && currentMovement.x < 1.0f) || (currentMovement.y > 0f && currentMovement.y < 1.0f);

            // If the movement isn't analog, make the magnitude 1
            float inputMagnitude = analogMovement ? currentMovement.magnitude : 1f;

            // Accelerate or decelerate to target horizontal speed
            if (horizontalSpeed < targetHorizontalSpeed - speedOffset || horizontalSpeed > targetHorizontalSpeed + speedOffset)
            {
                // Lerp speed to target speed
                horizontalSpeed = Mathf.Lerp(horizontalSpeed, targetHorizontalSpeed * inputMagnitude, Time.deltaTime * flyAcceleration);

                // Round speed to 3 decimal places
                horizontalSpeed = Mathf.Round(horizontalSpeed * 1000f) / 1000f;
            }
            else
            {
                horizontalSpeed = targetHorizontalSpeed;
            }

            // Accelerate or decelerate to target vertical speed
            if (verticalSpeed < targetVerticalSpeed - speedOffset || verticalSpeed > targetVerticalSpeed + speedOffset)
            {
                // Lerp speed to target speed
                verticalSpeed = Mathf.Lerp(verticalSpeed, targetVerticalSpeed * inputMagnitude, Time.deltaTime * flyAcceleration);

                // Round speed to 3 decimal places
                verticalSpeed = Mathf.Round(verticalSpeed * 1000f) / 1000f;
            }
            else
            {
                verticalSpeed = targetVerticalSpeed;
            }

            // Normalize input horizontal direction
            Vector3 inputDirection = new Vector3(currentMovement.x, 0.0f, currentMovement.y).normalized;

            // If there is a move input rotate player when the player is moving
            if (currentMovement != Vector2.zero || currentAscension != 0.0f)
            {
                targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + mainCamera.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, rotationSmoothTime);

                // Third person character rotation
                if (cameraMode == 0)
                {
                    // Rotate to face input direction relative to camera position
                    transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                }
            }

            // Get movement direction for reversing flight ascension and culling vertical movement on strafe
            if (currentMovement.y == 0) currentMovementDir = 0;
            else if (currentMovement.y > 0) currentMovementDir = 1;
            else if (currentMovement.y < 0) currentMovementDir = -1;

            // Get look direction
            Vector3 lookDirection = Quaternion.Euler(mainCamera.eulerAngles.x * currentMovementDir, targetRotation, 0.0f) * Vector3.forward;

            // Move the player
            Vector3 moveDirection = Vector3.ClampMagnitude((horizontalSpeed * lookDirection + verticalSpeed * Vector3.up), fastFlySpeed);
            transform.position += moveDirection * Time.deltaTime;

            // Update animator if using character
            if (hasAnimator)
            {
                // Use animation blend only with horizontal movement
                animator.SetFloat(animIDSpeed, horizontalSpeed);
            }
        }

        private void MoveLookTarget()
        {
            // Simple move target according to camera while not zoomed in or validly aiming
            if (!zoomedIn || !isTargetValid)
            {
                Vector3 prevPos = lookTarget.position;
                Vector3 nextPos = playerHead.position + (mainCamera.forward * targetDistance);

                if (prevPos != nextPos)
                {
                    prevPos = Vector3.Lerp(prevPos, nextPos, Time.deltaTime * lookTarget_TRate);
                }

                lookTarget.position = prevPos;
            }
        }

        private void ManagePointAndZoom()
        {
            // Rotate to target on mouse press if not looking or in animation
            if (Input.GetKeyDown(KeyCode.Mouse1) && notLooking && !inAnimation)
            {
                StartCoroutine(RotateToTarget(lookTarget));
            }

            // Zoom in
            if (Input.GetKey(KeyCode.Mouse1) && !notLooking && !inAnimation)
            {
                ActivateRig(handRig, handActivate_TRate);
                DisableCamToggle();
                ZoomIn();

                // Move the pointing to the center of the screen
                if (Physics.Raycast(mainCamera.position, mainCamera.forward, out aimHit, raycastDistance, raycastLayerMask))
                {
                    isTargetValid = true;
                    lookTarget.position = Vector3.Lerp(lookTarget.position, aimHit.point, Time.deltaTime * lookTarget_TRate);

                    // Move temp tag
                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        if (!tempTag)
                        {
                            tempTag = Instantiate(tagObject);
                            tempTag.transform.position = aimHit.point;
                        }
                        else
                        {
                            tempTag.transform.position = aimHit.point;
                        }

                        // Open object context menu
                        //if (hit.transform.CompareTag("PermObject"))
                        //{
                        //    TagMenu.gameObject.SetActive(true);
                        //}
                    }

                    // Spawn perm tag
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        if (!aimHit.transform.CompareTag("PermObject"))
                        {
                            Instantiate(permObject, aimHit.point, Quaternion.identity);
                        }
                        else
                        {
                            Destroy(aimHit.transform.gameObject);
                        }
                    }

                    // Spawn distance measuring objects
                    if (Input.GetKeyUp(KeyCode.C))
                    {
                        // Check if key up didn't come after holding down to delete
                        if (holdTimeoutDelta > 0)
                        {
                            // Get distance objects
                            GameObject[] distObjects = GameObject.FindGameObjectsWithTag("DistObject");

                            if (distObjects.Length == 2)
                            {
                                // If there already are two objects, destroy them
                                foreach (GameObject obj in distObjects)
                                {
                                    Destroy(obj);
                                }
                                // Instantiate a new one
                                Instantiate(distObject, aimHit.point, Quaternion.identity);
                            }
                            else if (distObjects.Length == 1)
                            {
                                // Instantiate second object
                                myDistTag = Instantiate(distObject);
                                myDistTag.transform.position = aimHit.point;

                                // Get distance
                                Vector3 start = distObjects[0].transform.position;
                                Vector3 end = myDistTag.transform.position;
                                Vector3 mid = Vector3.Lerp(start, end, 0.5f);
                                float distance = Vector3.Distance(start, end);
                                distance = Mathf.Round(distance * 100f) / 100f;

                                // Draw line and place text
                                DrawLine(start, end);
                                GameObject distText = Instantiate(textObject);
                                distText.transform.position = mid;
                                TextMeshPro tmp = distText.GetComponent<TextMeshPro>();
                                tmp.SetText(distance + "m");

                                // Make the text look at player
                                Vector3 dir = (distText.transform.position - transform.position).normalized;
                                Vector3 targetDirection = new Vector3(dir.x, 0, dir.z);
                                Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
                                distText.transform.rotation = Quaternion.RotateTowards(distText.transform.rotation, targetRotation, 360);
                            }
                            else
                            {
                                // If there isn't any, spawn first object
                                myDistTag = Instantiate(distObject);
                                myDistTag.transform.position = aimHit.point;
                            }
                        }
                        // Reset timer
                        holdTimeoutDelta = deleteHoldTimeout;
                    }

                    // Spawn elevation tag
                    if (Input.GetKeyDown(KeyCode.T))
                    {
                        if (!aimHit.transform.CompareTag("ElevationObject"))
                        {
                            // If the pointed location isn't an elevation object, spawn
                            Instantiate(elevationObject, aimHit.point, Quaternion.identity);
                        }
                        else
                        {
                            // Else, destroy and reduce marker count
                            Destroy(aimHit.transform.gameObject);
                            SetElevation.markerCount--;
                        }
                    }
                }
                else
                {
                    isTargetValid = false;
                }

                // If you're not clicking to a valid location, destroy temp tag
                if (!isTargetValid && Input.GetKeyDown(KeyCode.Mouse0))
                {
                    Destroy(tempTag);
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

                // Destroy distance texts and lines
                if (Input.GetKey(KeyCode.C))
                {
                    // Decrease timer while held
                    if (holdTimeoutDelta >= 0.0f)
                    {
                        holdTimeoutDelta -= 2 * Time.deltaTime;
                    }

                    // Destroy distance tags, texts and lines
                    else
                    {
                        GameObject[] distPermObjects = GameObject.FindGameObjectsWithTag("DistPermObject");
                        if (distPermObjects.Length > 0)
                        {
                            GameObject[] distObjects = GameObject.FindGameObjectsWithTag("DistObject");
                            var objList = new List<GameObject>();
                            objList.AddRange(distPermObjects);
                            objList.AddRange(distObjects);

                            foreach (GameObject obj in objList)
                            {
                                Destroy(obj);
                            }
                        }
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
            else if (!zoomedOut)
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

                DeactivateRig(handRig, handDeactivate_TRate);

                if (!onWheelchair)
                {
                    EnableCamToggle();
                }

                ZoomOut();
            }

            // Zoomed out flag
            zoomedOut = !(handRig.weight != 0 || cameraFov != originalFov || cameraDistance != originalDistance);
        }

        private void CameraRotation()
        {
            // Third person camera - follow
            if (cameraMode == 0)
            {
                // If there is an input and camera position is not fixed
                if (currentLook.sqrMagnitude >= camRotateThreshold && !lockCameraPosition)
                {
                    cinemachineTargetYaw += currentLook.x * Time.deltaTime * rotationSpeed;
                    cinemachineTargetPitch += currentLook.y * Time.deltaTime * rotationSpeed;
                }

                // Clamp our rotations so our values are limited 360 degrees
                cinemachineTargetYaw = ClampAngle(cinemachineTargetYaw, float.MinValue, float.MaxValue);

                // Clamp pitch rotation
                cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, -topClamp, bottomClamp);

                // Cinemachine will follow this target
                cinemachineCameraTarget.transform.rotation = Quaternion.Euler(cinemachineTargetPitch + cameraAngleOverride, cinemachineTargetYaw, 0.0f);

                // Delayed character rotation if not in animation or wheelchair
                if (lookTimeoutDelta < 0 && !inAnimation && !onWheelchair)
                {
                    StartCoroutine(RotateToTarget(lookTarget));
                }

                // Rotation interrupt
                if (inRotation && Input.anyKeyDown)
                {
                    StopCoroutine("RotateToTarget");

                    // Back to locomotion blend
                    if (hasAnimator)
                    {
                        animator.SetBool(animIDturnLeft, false);
                        animator.SetBool(animIDturnRight, false);
                    }

                    // Reset
                    lookTimeoutDelta = lookTimeout;
                    MovementEnable();
                    inRotation = false;
                    inAnimation = false;
                }
            }

            // First person camera - rotate
            if (cameraMode == 1)
            {
                if (currentLook.sqrMagnitude >= camRotateThreshold && !lockCameraPosition)
                {
                    cinemachineTargetPitch += currentLook.y * Time.deltaTime * rotationSpeed;
                    rotationVelocity = currentLook.x * Time.deltaTime * rotationSpeed;

                    cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, -topClamp, bottomClamp);
                    cinemachineCameraTarget.transform.localRotation = Quaternion.Euler(cinemachineTargetPitch, 0f, 0f);

                    transform.Rotate(Vector3.up * rotationVelocity);
                }
            }
        }

        private IEnumerator RotateToTarget(Transform target)
        {
            inRotation = true;
            inAnimation = true;
            MovementDisable();
            Vector3 dir = (target.transform.position - transform.position).normalized;
            Vector3 targetDirection = new Vector3(dir.x, 0, dir.z);
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);

            if (hasAnimator && !isFlying)
            {
                // Do left/right standing turning animation
                if (angleDir == -1)
                {
                    animator.SetBool(animIDturnLeft, true);
                }
                else if (angleDir == 1)
                {
                    animator.SetBool(animIDturnRight, true);
                }
            }

            // Rotate player body
            do
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * 180);
                yield return null;
            } while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f);

            // Back to locomotion blend
            if (hasAnimator)
            {
                animator.SetBool(animIDturnLeft, false);
                animator.SetBool(animIDturnRight, false);
            }

            // Reset
            lookTimeoutDelta = lookTimeout;
            MovementEnable();
            inRotation = false;
            inAnimation = false;
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
            if (shoulderTogglePressed && !CameraCoroutineInProgress && cameraMode == 0 && zoomedOut)
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

        private IEnumerator CameraChange()
        {
            CameraCoroutineInProgress = true;
            if (cameraMode == 0)
            {
                // Third-person perspective
                TPPCamera.SetActive(true);
                FPPCamera.SetActive(false);
                yield return null;
                StartCoroutine(GetActiveCamera());
                yield return new WaitForSeconds(0.2f);
                // Resume perlin noise
                if (camNoise.m_FrequencyGain != 0.3f) camNoise.m_FrequencyGain = 0.3f;
                meshRenderer.shadowCastingMode = ShadowCastingMode.On;
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
                meshRenderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            }

            // Set collision filter
            if (isFlying)
            {
                camTPF.CameraCollisionFilter = flyingLayermask;
            }
            else
            {
                camTPF.CameraCollisionFilter = walkingLayermask;
            }

            yield return new WaitForSeconds(1.0f);
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
            zoomOffset = minZoomOffset;
            cameraFov = originalFov;
            cameraDistance = originalDistance;
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
                camSide = Mathf.Lerp(camSide, targetSide, Time.deltaTime * shoulderSwitch_TRate);
                camTPF.CameraSide = camSide;
                yield return null;
            } while (Mathf.Abs(camSide - targetSide) > 0.01f);

            // Zero side
            camTPF.CameraSide = targetSide;
            // Set new shoulder side
            shoulderSide = targetSide;
            CameraCoroutineInProgress = false;
        }

        private void CheckTargetingStatus()
        {
            // Get the angle between the camera and player heading and target position in left/right
            Vector3 flatTargetPos = new Vector3(lookTarget.position.x, transform.position.y, lookTarget.position.z);
            Vector3 heading = flatTargetPos - transform.position;
            float angle = Vector3.Angle(heading, transform.forward);

            angleDir = AngleDir(transform.forward, heading); // 1: right, -1: left

            if (inStaticAnimation)
            {
                // If in static animation, don't look
                notLooking = true;
            }
            else if (isFlying && horizontalSpeed > 1)
            {
                // If flying horizontally in high speed, don't try to look
                notLooking = true;
            }
            else
            {
                // If the target is behind
                if (angle > frontAngle)
                {
                    notLooking = true;

                    // And the player isn't moving or looking around or sitting, decrease timer
                    if (!movementPressed && !ascendPressed && !lookPressed && !isSitting)
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
            if (isSitting || onWheelchair)
            {
                if (notLooking)
                {
                    DeactivateRig(lookRig, headActivate_TRate);
                }
                else if (!notLooking)
                {
                    ActivateRig(lookRig, headDeactivate_TRate);
                }
            }
            else
            {
                if (notLooking)
                {
                    DeactivateRig(headRig, headActivate_TRate);
                }
                else if (!notLooking)
                {
                    ActivateRig(headRig, headDeactivate_TRate);
                }
            }
        }

        private void ActivateRig(Rig rig, float rate)
        {
            if (rig.weight != 1)
            {
                // Lerp rig constraint weights
                rig.weight = (rig.weight > 0.999) ? 1 : Mathf.Lerp(rig.weight, 1, Time.deltaTime * rate);
            }
        }

        private void DeactivateRig(Rig rig, float rate)
        {
            if (rig.weight != 0)
            {
                // Lerp rig constraint weights
                rig.weight = (rig.weight < 0.001) ? 0 : Mathf.Lerp(rig.weight, 0, Time.deltaTime * rate); // TODO: Fix deactivation - reactivation in rig is causing head to twitch
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
                cameraFov = (cameraFov < zoomFov + 0.01f) ? zoomFov : Mathf.Lerp(cameraFov, zoomFov, Time.deltaTime * zoom_TRate);
                activeCam.m_Lens.FieldOfView = cameraFov;
            }

            if (cameraDistance != zoomDistance)
            {
                // Lerp camera distance to zoom distance
                cameraDistance = (cameraDistance < zoomDistance + 0.001f) ? zoomDistance : Mathf.Lerp(cameraDistance, zoomDistance, Time.deltaTime * zoom_TRate);
                camTPF.CameraDistance = cameraDistance;
            }

            // Change sensitivity
            if (rotationSpeed != zoomRotationSpeed) rotationSpeed = zoomRotationSpeed;

            // Stop perlin noise, unless in FPP
            if (cameraMode != 1 && camNoise.m_FrequencyGain != 0) camNoise.m_FrequencyGain = 0;
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
                cameraFov = (cameraFov > originalFov - 0.01f) ? originalFov : Mathf.Lerp(cameraFov, originalFov, Time.deltaTime * zoom_TRate);
                activeCam.m_Lens.FieldOfView = cameraFov;
            }

            if (cameraDistance != originalDistance)
            {
                // Lerp camera distance to original
                cameraDistance = (cameraDistance > originalDistance - 0.001f) ? originalDistance : Mathf.Lerp(cameraDistance, originalDistance, Time.deltaTime * zoom_TRate);
                camTPF.CameraDistance = cameraDistance;
            }

            // Change sensitivity
            if (rotationSpeed != normalRotationSpeed) rotationSpeed = normalRotationSpeed;

            // Resume perlin noise, unless in FPP
            if (cameraMode != 1 && camNoise.m_FrequencyGain != 0.3f) camNoise.m_FrequencyGain = 0.3f;
        }

        private void DrawLine(Vector3 start, Vector3 end)
        {
            GameObject myLine = Instantiate(distLine);
            LineRenderer lr = myLine.GetComponent<LineRenderer>();
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
        }

        private void ControlAnimations()
        {
            // Play animations, only if grounded and not already in animation
            if (!inAnimation)
            {
                // Wave animation one shot
                if (wavePressed)
                {
                    StartCoroutine(WaveAnimation());
                }

                if (grounded && !onWheelchair && !sitDelayed && sitPressed)
                {
                    StartCoroutine(SitEnter());
                }

                // Dance animation loop start
                if (grounded && !onWheelchair && dancePressed)
                {
                    StartLoopAnimation(animIDisDancing);
                }

                if (grounded && !onWheelchair && cameraMode != 1 && Input.GetKeyDown(KeyCode.L))
                {
                    StartCoroutine(WheelchairEnter());
                }

                if (onWheelchair && Input.GetKeyDown(KeyCode.L))
                {
                    StartCoroutine(WheelchairExit());
                }
            }
            else
            {
                // Stop loop animations
                // Dance animation loop stop
                if (inLoopAnimation && danceReleased)
                {
                    StartCoroutine(EndLoopAnimation(animIDisDancing));
                }

                if (sitDelayed && sitPressed)
                {
                    StartCoroutine(SitExit());
                }
            }
        }

        private IEnumerator WaveAnimation()
        {
            inAnimation = true;

            // Disable jump while waving
            input.Player.Jump.Disable();

            // To register animation only once
            animator.SetBool(animIDisWaving, true);
            yield return new WaitForEndOfFrame();
            animator.SetBool(animIDisWaving, false);

            // Wait for the animation duration
            yield return new WaitForSeconds(4);

            // Enable jump after waving
            input.Player.Jump.Enable();
            inAnimation = false;
        }

        private IEnumerator SitEnter()
        {
            inAnimation = true;
            isSitting = true;
            MovementDisable();
            animator.SetBool(animIDisSitting, true);

            for (int i = 0; i < smoothCount; i++)
            {
                headRig.weight = Mathf.Lerp(headRig.weight, 0, i / smoothCount);
                yield return new WaitForSeconds(smoothTime / smoothCount);
            }

            sitDelayed = true;
        }

        private IEnumerator SitExit()
        {
            inAnimation = false;
            isSitting = false;
            animator.SetBool(animIDisSitting, false);
            MovementEnable();

            for (int i = 0; i < smoothCount; i++)
            {
                lookRig.weight = Mathf.Lerp(lookRig.weight, 0, i / smoothCount);
                yield return new WaitForSeconds(smoothTime / smoothCount);
            }

            sitDelayed = false;
        }

        private IEnumerator WheelchairEnter()
        {
            yield return new WaitForEndOfFrame();
            inAnimation = true;
            onWheelchair = true;
            DisableCamToggle();
            input.Player.Run.Disable();
            animator.SetBool(animIDonWheelchair, true);
            controller.slopeLimit = 9.5f;
            controller.stepOffset = 0.03f;

            wheelchair.SetActive(true);

            for (int i = 0; i < smoothCount; i++)
            {
                headRig.weight = Mathf.Lerp(headRig.weight, 0, i / smoothCount);
                yield return new WaitForSeconds(smoothTime / smoothCount);
            }

            inAnimation = false;
        }

        private IEnumerator WheelchairExit()
        {
            inAnimation = true;
            onWheelchair = false;
            EnableCamToggle();
            input.Player.Run.Enable();
            animator.SetBool(animIDonWheelchair, false);
            controller.slopeLimit = 45f;
            controller.stepOffset = 0.25f;

            wheelchair.SetActive(false);

            for (int i = 0; i < smoothCount; i++)
            {
                lookRig.weight = Mathf.Lerp(lookRig.weight, 0, i / smoothCount);
                yield return new WaitForSeconds(smoothTime / smoothCount);
            }

            inAnimation = false;
        }

        private void wheelchairRotate()
        {
            if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out wheelchairHit, 1.1f))
            {
                Vector3 front = Vector3.Cross(transform.right, wheelchairHit.normal);
                Quaternion targetRotation = Quaternion.LookRotation(front, wheelchairHit.normal);
                wheelchair.transform.rotation = Quaternion.Lerp(wheelchair.transform.rotation, targetRotation, Time.deltaTime * wheelchairRotate_TRate);
                // transform.rotation = Quaternion.Euler(targetRotation.eulerAngles.x, transform.rotation.eulerAngles.y, targetWRotation.eulerAngles.z);
            }
        }

        private void StartLoopAnimation(int animID)
        {
            inAnimation = true;
            inStaticAnimation = true;
            inLoopAnimation = true;
            MovementDisable();
            animator.SetBool(animID, true);
        }

        private IEnumerator EndLoopAnimation(int animID)
        {
            animator.SetBool(animID, false);
            yield return new WaitForSeconds(3.5f);
            MovementEnable();
            inAnimation = false;
            inStaticAnimation = false;
            inLoopAnimation = false;
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

        private void SubscribeToInput()
        {
            // Movement
            input.Player.Move.performed += ctx =>
            {
                currentMovement = ctx.ReadValue<Vector2>();
                movementPressed = currentMovement.x != 0 || currentMovement.y != 0;
            };
            input.Player.Move.canceled += ctx =>
            {
                currentMovement = Vector2.zero;
                movementPressed = false;
            };

            // Looking
            input.Player.Look.performed += ctx =>
            {
                currentLook = ctx.ReadValue<Vector2>();
                lookPressed = currentLook.x != 0 || currentLook.y != 0;
            };
            input.Player.Look.canceled += ctx =>
            {
                currentLook = Vector2.zero;
                lookPressed = false;
            };

            // Flight toggle
            input.Player.ToggleFlight.performed += ctx => flightTogglePressed = ctx.ReadValueAsButton();
            input.Player.ToggleFlight.canceled += ctx => flightTogglePressed = false;

            // Ascension
            input.Player.Ascend.performed += ctx =>
            {
                currentAscension = ctx.ReadValue<float>();
                ascendPressed = currentAscension != 0;
            };
            input.Player.Ascend.canceled += ctx =>
            {
                currentAscension = 0f;
                ascendPressed = false;
            };

            // Running
            input.Player.Run.performed += ctx => runPressed = ctx.ReadValueAsButton();
            input.Player.Run.canceled += ctx => runPressed = false;

            // Jumping
            input.Player.Jump.performed += ctx => jumpPressed = ctx.ReadValueAsButton();
            input.Player.Jump.canceled += ctx => jumpPressed = false;

            // Sitting
            input.Player.Sit.performed += ctx => sitPressed = ctx.ReadValueAsButton();
            input.Player.Sit.canceled += ctx => sitPressed = false;

            // Camera toggle
            input.Player.ToggleCamera.performed += ctx => camTogglePressed = ctx.ReadValueAsButton();
            input.Player.ToggleCamera.canceled += ctx => camTogglePressed = false;

            // Camera toggle
            input.Player.ToggleShoulder.performed += ctx => shoulderTogglePressed = ctx.ReadValueAsButton();
            input.Player.ToggleShoulder.canceled += ctx => shoulderTogglePressed = false;

            // Waving
            input.Player.Wave.performed += ctx => wavePressed = ctx.ReadValueAsButton();
            input.Player.Wave.canceled += ctx => wavePressed = false;

            // Dancing
            input.Player.Dance.performed += ctx =>
            {
                dancePressed = true;
                danceReleased = false;
            };
            input.Player.Dance.canceled += ctx =>
            {
                dancePressed = false;
                danceReleased = true;
            };
        }

        private void AssignAnimationIDs()
        {
            animIDSpeed = Animator.StringToHash("Speed");
            animIDGrounded = Animator.StringToHash("Grounded");
            animIDJump = Animator.StringToHash("Jump");
            animIDFreeFall = Animator.StringToHash("FreeFall");
            animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
            animIDisFlying = Animator.StringToHash("isFlying");
            animIDisSitting = Animator.StringToHash("isSitting");
            animIDisWaving = Animator.StringToHash("isWaving");
            animIDisDancing = Animator.StringToHash("isDancing");
            animIDonWheelchair = Animator.StringToHash("onWheelchair");
            animIDturnLeft = Animator.StringToHash("turnLeft");
            animIDturnRight = Animator.StringToHash("turnRight");
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
    }
}
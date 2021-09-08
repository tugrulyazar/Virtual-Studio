using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace UserBehaviour
{
    public class PlayerSizeManipulation : MonoBehaviour
    {
        [SerializeField]
        private GameObject playerCharacter;

        private PlayerController playerController;
        private IKFootPlacement ikFootPlacement;
        private CharacterController characterController;
        private Cinemachine3rdPersonFollow camTPF;

        // States
        private bool superSize;
        private bool microSize;
        private bool sizeAdjusted;
        private bool inScaleCoroutine;

        // Scale factor
        private float superSizeScale = 8f;
        private float microSizeScale = 0.1f;
        private float scaleSpeed = 2f;

        // Default properties
        private float groundedRadius;
        private float groundedOffset;
        private float distanceToGround;
        private float stepOffset;
        private float raycastDistance;
        private float gravity;
        private float targetDistance;

        private void Start()
        {
            playerController = playerCharacter.GetComponent<PlayerController>();
            ikFootPlacement = playerCharacter.GetComponent<IKFootPlacement>();
            characterController = playerCharacter.GetComponent<CharacterController>();

            groundedRadius = playerController.groundedRadius;
            groundedOffset = playerController.groundedOffset;
            distanceToGround = ikFootPlacement.distanceToGround;
            stepOffset = characterController.stepOffset;
            raycastDistance = playerController.raycastDistance;
            gravity = playerController.gravity;
            targetDistance = playerController.targetDistance;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.PageUp) && !microSize && !inScaleCoroutine && !playerController.inAnimation)
            {
                if (!superSize)
                {
                    superSize = true;
                    adjustPlayerSize(superSizeScale);
                }
                else if (superSize)
                {
                    superSize = false;
                    adjustPlayerSize(superSizeScale);
                }
            }

            if (Input.GetKeyDown(KeyCode.PageDown) && !superSize && !inScaleCoroutine && !playerController.inAnimation)
            {
                if (!microSize)
                {
                    microSize = true;
                    adjustPlayerSize(microSizeScale);
                }
                else if (microSize)
                {
                    microSize = false;
                    adjustPlayerSize(microSizeScale);
                }
            }
        }

        private void adjustPlayerSize(float scale)
        {
            if (!sizeAdjusted)
            {
                // Set state
                sizeAdjusted = true;

                // Set grounded props
                if (superSize)
                {
                    playerController.groundedOffset = -1f;
                }
                else if (microSize)
                {
                    playerController.groundedOffset = 0f;
                }
                
                playerController.groundedRadius *= scale;

                // Set IK distance
                ikFootPlacement.distanceToGround *= scale;

                // Get cam TPF
                camTPF = playerController.camTPF;

                // Adjust scale
                StartCoroutine(scaleCoroutine(scale));

                // Adjust movement
                playerController.animSpeedMultiplier /= scale;
                playerController.moveSpeed *= scale;
                playerController.sprintSpeed *= scale;
                playerController.jumpHeight *= scale;
                playerController.flyJumpHeight *= scale;
                playerController.flySpeed *= scale;
                playerController.fastFlySpeed *= scale;

                if (superSize)
                {
                    playerController.gravity *= 3f;
                }
                else if (microSize)
                {
                    playerController.gravity *= scale;
                }

                // Adjust camera properties
                playerController.minZoomOffset *= scale;
                playerController.maxZoomOffset *= scale;
                playerController.zoomOffset *= scale;
                playerController.zoomDistance *= scale;
                playerController.originalDistance *= scale;
                playerController.cameraDistance *= scale;

                // Adjust targeting distance
                playerController.targetDistance *= scale;

                // Adjust raycast distance
                if (superSize)
                {
                    playerController.raycastDistance *= scale;
                }
                

                // Adjust step offset
                if (superSize)
                {
                    characterController.stepOffset = 1.7f;
                }
                else if (microSize)
                {
                    characterController.stepOffset *= scale;
                }
                
            }
            else if (sizeAdjusted)
            {
                // Exit state
                sizeAdjusted = false;

                // Reset grounded props
                playerController.groundedOffset = groundedOffset;
                playerController.groundedRadius = groundedRadius;

                // Reset IK distance
                ikFootPlacement.distanceToGround = distanceToGround;

                // Get cam TPF
                camTPF = playerController.camTPF;

                // Reset scale
                StartCoroutine(scaleCoroutine(1f / scale));

                // Reset movement
                playerController.animSpeedMultiplier *= scale;
                playerController.moveSpeed /= scale;
                playerController.sprintSpeed /= scale;
                playerController.jumpHeight /= scale;
                playerController.flyJumpHeight /= scale;
                playerController.flySpeed /= scale;
                playerController.fastFlySpeed /= scale;
                playerController.gravity = gravity;

                // Reset camera properties
                playerController.minZoomOffset /= scale;
                playerController.maxZoomOffset /= scale;
                playerController.zoomOffset /= scale;
                playerController.zoomDistance /= scale;
                playerController.originalDistance /= scale;
                playerController.cameraDistance /= scale;

                // Adjust targeting distance
                playerController.targetDistance = targetDistance;

                // Adjust raycast distance
                playerController.raycastDistance = raycastDistance;

                // Reset step offset
                characterController.stepOffset = stepOffset;
            }
        }

        private IEnumerator scaleCoroutine(float targetScale)
        {
            // Enter state
            inScaleCoroutine = true;
            playerController.inAnimation = true;

            // Get current cam properties
            Vector3 playerScale = playerCharacter.transform.localScale;
            float cameraDistance = camTPF.CameraDistance;
            Vector3 shoulderOffset = camTPF.ShoulderOffset;

            float currentScale = 1f;

            // Lerp scale
            do
            {
                currentScale = Mathf.Lerp(currentScale, targetScale, Time.deltaTime * scaleSpeed);

                playerCharacter.transform.localScale = playerScale * currentScale;
                camTPF.CameraDistance = cameraDistance * currentScale;
                camTPF.ShoulderOffset = shoulderOffset * currentScale;

                yield return null;
            } while (currentScale < targetScale - 0.001f || currentScale > targetScale + 0.001f);

            // Finally set target scale
            playerCharacter.transform.localScale = playerScale * targetScale;
            camTPF.CameraDistance = cameraDistance * targetScale;
            camTPF.ShoulderOffset = shoulderOffset * targetScale;

            // Exit state
            inScaleCoroutine = false;
            playerController.inAnimation = false;
        }
    }
}


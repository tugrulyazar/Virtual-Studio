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
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.PageUp) && !microSize && !inScaleCoroutine && !playerController.inAnimation)
            {
                superSizePlayer(superSizeScale);
            }

            if (Input.GetKeyDown(KeyCode.PageDown) && !superSize && !inScaleCoroutine && !playerController.inAnimation)
            {
                microSizePlayer(microSizeScale);
            }
        }

        private void superSizePlayer(float scale)
        {
            if (!superSize)
            {
                superSize = true;

                // Set grounded props
                playerController.groundedOffset *= 1f;
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
                playerController.gravity *= 3;

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
                playerController.raycastDistance *= scale;

                // Adjust step offset
                characterController.stepOffset = 1.5f;
            }
            else if (superSize)
            {
                superSize = false;

                // Reset grounded props
                playerController.groundedOffset = groundedOffset;
                playerController.groundedRadius = groundedRadius;

                // Reset IK distance
                ikFootPlacement.distanceToGround = distanceToGround;

                // Get cam TPF
                camTPF = playerController.camTPF;

                // Adjust scale
                StartCoroutine(scaleCoroutine(1f / scale));

                // Adjust movement
                playerController.animSpeedMultiplier *= scale;
                playerController.moveSpeed /= scale;
                playerController.sprintSpeed /= scale;
                playerController.jumpHeight /= scale;
                playerController.flyJumpHeight /= scale;
                playerController.flySpeed /= scale;
                playerController.fastFlySpeed /= scale;
                playerController.gravity /= 3;

                // Adjust camera properties
                playerController.minZoomOffset /= scale;
                playerController.maxZoomOffset /= scale;
                playerController.zoomOffset /= scale;
                playerController.zoomDistance /= scale;
                playerController.originalDistance /= scale;
                playerController.cameraDistance /= scale;

                // Adjust targeting distance
                playerController.targetDistance /= 2f;

                // Adjust raycast distance
                playerController.raycastDistance /= raycastDistance;

                // Reset step offset
                characterController.stepOffset = stepOffset;
            }
        }

        private void microSizePlayer(float scale)
        {
            if (!microSize)
            {
                microSize = true;

                // Set grounded props
                playerController.groundedOffset = 0.005f;
                playerController.groundedRadius *= scale;

                // Zero IK distance
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
                playerController.gravity *= scale;

                // Adjust camera properties
                playerController.minZoomOffset *= scale;
                playerController.maxZoomOffset *= scale;
                playerController.zoomOffset *= scale;
                playerController.zoomDistance *= scale;
                playerController.originalDistance *= scale;
                playerController.cameraDistance *= scale;

                // Adjust step offset
                characterController.stepOffset *= scale;
            }
            else if (microSize)
            {
                microSize = false;

                // Reset grounded props
                playerController.groundedOffset = groundedOffset;
                playerController.groundedRadius = groundedRadius;

                // Reset IK distance
                ikFootPlacement.distanceToGround = distanceToGround;

                // Get cam TPF
                camTPF = playerController.camTPF;

                // Adjust scale
                StartCoroutine(scaleCoroutine(1f / scale));

                // Adjust movement
                playerController.animSpeedMultiplier *= scale;
                playerController.moveSpeed /= scale;
                playerController.sprintSpeed /= scale;
                playerController.jumpHeight /= scale;
                playerController.flyJumpHeight /= scale;
                playerController.flySpeed /= scale;
                playerController.fastFlySpeed /= scale;
                playerController.gravity /= scale;

                // Adjust camera properties
                playerController.minZoomOffset /= scale;
                playerController.maxZoomOffset /= scale;
                playerController.zoomOffset /= scale;
                playerController.zoomDistance /= scale;
                playerController.originalDistance /= scale;
                playerController.cameraDistance /= scale;

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


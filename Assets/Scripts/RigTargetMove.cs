using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Cinemachine;

public class RigTargetMove : MonoBehaviour
{
    [SerializeField]
    [Range(1f, 10f)]
    private float targetDistance = 4;
    [SerializeField]
    [Range(30f, 180f)]
    private float frontAngle = 120;
    [SerializeField]
    [Range(5, 100)]
    private float raycastDistance = 20;

    [SerializeField]
    private GameObject playerHead;

    [SerializeField]
    private GameObject playerPos;

    [SerializeField]
    private GameObject tagSphere;

    [SerializeField]
    private Rig headRig;
    [SerializeField]
    private Rig rightHandRig;
    [SerializeField]
    private Rig leftHandRig;

    private Rig handRig;

    private Transform mainCamera;

    private float transitionRate = 2.0f;
    private bool notLooking = false;
    private Vector3 heading;

    private CinemachineBrain cinemachineBrain;
    private CinemachineVirtualCamera activeCam;
    private Cinemachine3rdPersonFollow camTPF;
    private float cameraFov;
    private float cameraDistance;
    private float zoomFov = 30;
    private float originalFov;
    private float zoomDistance = 1;
    private float originalDistance;

    private RaycastHit hit;

    GameObject mySphere;

    private void Awake()
    {
        // Get main camera transform
        mainCamera = Camera.main.transform;
    }

    private void Start()
    {
        cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
        StartCoroutine(getActiveCamera());
    }

    void Update()
    {
        // Move target according to camera
        transform.position = playerHead.transform.position + mainCamera.forward * targetDistance;

        // Get the angle between the camera and player heading and target position in left/right
        heading = transform.position - playerPos.transform.position;
        float angle = Vector3.Angle(heading, playerPos.transform.forward);
        float angleDir = AngleDir(playerPos.transform.position, heading);

        // If the target is behind, don't look
        notLooking = (angle > frontAngle) ? true : false;

        // Decrease head constraint weights over time, disable unnecessary infinite lerping
        if (notLooking)
        {
            deactivateRig(headRig, 0.5f);
        }
        else if (!notLooking)
        {
            activateRig(headRig, 1);
        }

        // TODO: Carry this over to camera switch script
        if (Input.GetKeyDown(KeyCode.F)) StartCoroutine(getActiveCamera());

        // TODO: improve code to release previous hand
        // Switch hands based on player side
        // switch (angleDir)
        // {
        //     case 1:
        //         handRig = leftHandRig;
        //         break;
        //     case -1:
        //         handRig = rightHandRig;
        //         break;
        // }

        // Right hand point and camera zoom
        if (Input.GetKey(KeyCode.Mouse1) && !notLooking)
        {
            activateRig(rightHandRig, 3);
            zoomIn();

            // Move the pointing to the center of the screen
            if (Physics.Raycast(mainCamera.position, mainCamera.forward, out hit, raycastDistance))
            {
                transform.position = Vector3.Lerp(transform.position, hit.point, Time.deltaTime * transitionRate);

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


        }
        else if (rightHandRig.weight != 0 && cameraFov != originalFov && cameraDistance != originalDistance)
        {
            deactivateRig(rightHandRig, 4);
            zoomOut();
        }
    }

    // Need an IEnumerator to get the active camera, takes at least a frame to set after start
    private IEnumerator getActiveCamera()
    {
        yield return null;
        activeCam = cinemachineBrain.ActiveVirtualCamera as CinemachineVirtualCamera;
        camTPF = activeCam.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        originalFov = activeCam.m_Lens.FieldOfView;
        originalDistance = camTPF.CameraDistance;

        // Temp camera values
        cameraFov = originalFov;
        cameraDistance = originalDistance;
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
        if (cameraFov != zoomFov && cameraDistance != zoomDistance)
        {
            // Lerp camera fov to zoom fov
            cameraFov = (cameraFov < zoomFov + 0.1f) ? zoomFov : Mathf.Lerp(cameraFov, zoomFov, Time.deltaTime * transitionRate * 2);
            activeCam.m_Lens.FieldOfView = cameraFov;

            // Lerp camera distance to zoom distance
            cameraDistance = (cameraDistance < zoomDistance + 0.1f) ? zoomDistance : Mathf.Lerp(cameraDistance, zoomDistance, Time.deltaTime * transitionRate * 2);
            camTPF.CameraDistance = cameraDistance;
        }
    }

    private void zoomOut()
    {
        if (cameraFov != originalFov && cameraDistance != originalDistance)
        {
            // Lerp camera fov to original
            cameraFov = (cameraFov > originalFov - 0.1f) ? originalFov : Mathf.Lerp(cameraFov, originalFov, Time.deltaTime * transitionRate * 2);
            activeCam.m_Lens.FieldOfView = cameraFov;

            // Lerp camera distance to original
            cameraDistance = (cameraDistance > originalDistance - 0.1f) ? originalDistance : Mathf.Lerp(cameraDistance, originalDistance, Time.deltaTime * transitionRate * 2);
            camTPF.CameraDistance = cameraDistance;
        }
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
}

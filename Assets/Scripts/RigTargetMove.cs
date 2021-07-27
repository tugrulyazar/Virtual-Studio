using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Cinemachine;

public class RigTargetMove : MonoBehaviour
{
    [SerializeField]
    [Range(0f, 10f)]
    private float distance;
    [SerializeField]
    [Range(30f, 180f)]
    private float frontAngle = 120;

    [SerializeField]
    private GameObject head;

    [SerializeField]
    private GameObject playerPos;

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
    CinemachineVirtualCamera activeCam;
    private float cameraFov;
    private float zoomFov = 20;
    private float originalCameraFov;


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
        transform.position = head.transform.position + mainCamera.forward * distance;

        // Get the angle between the camera and player heading and target position in left/right
        heading = transform.position - playerPos.transform.position;
        float angle = Vector3.Angle(heading, playerPos.transform.forward);
        float angleDir = AngleDir(playerPos.transform.position, heading);

        // If the target is behind, don't look
        notLooking = (angle > frontAngle) ? true : false;

        // Decrease head constraint weights over time, disable unnecessary infinite lerping
        if (notLooking && headRig.weight != 0)
        {
            headRig.weight = (headRig.weight < 0.01) ? 0 : Mathf.Lerp(headRig.weight, 0, Time.deltaTime * transitionRate);
        }
        else if (!notLooking && headRig.weight != 1)
        {
            headRig.weight = (headRig.weight > 0.99) ? 1 : Mathf.Lerp(headRig.weight, 1, Time.deltaTime * transitionRate);
        }

        // TODO: Carry this over to camera switch script
        if (Input.GetKeyDown(KeyCode.F)) StartCoroutine(getActiveCamera());

        // Switch hands based on player side
        switch (angleDir)
        {
            case 1:
                handRig = leftHandRig;
                break;
            case -1:
                handRig = rightHandRig;
                break;
        }

        // KEEP!!!
        // Right hand point and camera zoom
        if (Input.GetKey(KeyCode.Mouse0) && !notLooking)
        {
            raiseHand(rightHandRig);
            zoomIn();
        }
        else if (rightHandRig.weight != 0 && cameraFov != originalCameraFov)
        {
            lowerHand(rightHandRig);
            zoomOut();
        }
    }

    // Need an IEnumerator to get the active camera, takes at least a frame to set after start
    private IEnumerator getActiveCamera()
    {
        yield return null;
        activeCam = cinemachineBrain.ActiveVirtualCamera as CinemachineVirtualCamera;
        originalCameraFov = activeCam.m_Lens.FieldOfView;
        // Temp camera fov
        cameraFov = originalCameraFov;
    }

    private void raiseHand(Rig rig)
    {
        if (rig.weight != 1)
        {
            // Lerp hand constraint weights
            rig.weight = (rig.weight > 0.99) ? 1 : Mathf.Lerp(rig.weight, 1, Time.deltaTime * transitionRate * 3);
        }
    }

    private void lowerHand(Rig rig)
    {
        if (rig.weight != 0)
        {
            // Lerp hand constraint weights
            rig.weight = (rig.weight < 0.01) ? 0 : Mathf.Lerp(rig.weight, 0, Time.deltaTime * transitionRate * 4);
        }
    }
    private void zoomIn()
    {
        if (cameraFov != zoomFov)
        {
            // Lerp camera fov to zoom fov
            cameraFov = (cameraFov < zoomFov + 0.1f) ? zoomFov : Mathf.Lerp(cameraFov, zoomFov, Time.deltaTime * transitionRate * 2);
            activeCam.m_Lens.FieldOfView = cameraFov;
        }
    }

    private void zoomOut()
    {
        if (cameraFov != originalCameraFov)
        {
            // Lerp camera fov to original
            cameraFov = (cameraFov > originalCameraFov - 0.1f) ? originalCameraFov : Mathf.Lerp(cameraFov, originalCameraFov, Time.deltaTime * transitionRate * 2);
            activeCam.m_Lens.FieldOfView = cameraFov;
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

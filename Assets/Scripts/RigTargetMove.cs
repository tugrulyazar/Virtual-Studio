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
    private Rig handRig;

    private Transform mainCamera;

    private float transitionRate = 2.0f;
    public bool notLooking = false;

    private float dot;
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

        // Get the angle between the camera and player heading
        heading = transform.position - playerPos.transform.position;
        float angle = Vector3.Angle(heading, playerPos.transform.forward);

        // If the target is behind, don't look
        notLooking = (angle > frontAngle) ? true : false;
        Debug.Log("checking angle");


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

        // TODO: Switch hands based on player side
        if (Input.GetKey(KeyCode.Mouse0) && !notLooking)
        {
            // Increase hand constraint weights
            if (handRig.weight != 1)
            {
                handRig.weight = (handRig.weight > 0.99) ? 1 : Mathf.Lerp(handRig.weight, 1, Time.deltaTime * transitionRate * 3);
            }

            // Lerp camera fov to zoom
            if (cameraFov != zoomFov)
            {
                cameraFov = (cameraFov < zoomFov + 0.1f) ? zoomFov : Mathf.Lerp(cameraFov, zoomFov, Time.deltaTime * transitionRate * 2);
                activeCam.m_Lens.FieldOfView = cameraFov;
            }
        }
        else if (handRig.weight != 0 && cameraFov != originalCameraFov)
        {
            // Decrease hand constraint weights
            if (handRig.weight != 0)
            {
                handRig.weight = (handRig.weight < 0.01) ? 0 : Mathf.Lerp(handRig.weight, 0, Time.deltaTime * transitionRate * 4);
            }

            // Lerp camera fov to original
            if (cameraFov != originalCameraFov)
            {
                cameraFov = (cameraFov > originalCameraFov - 0.1f) ? originalCameraFov : Mathf.Lerp(cameraFov, originalCameraFov, Time.deltaTime * transitionRate * 2);
                activeCam.m_Lens.FieldOfView = cameraFov;
            }

        }
    }

    IEnumerator getActiveCamera()
    {
        yield return null;
        activeCam = cinemachineBrain.ActiveVirtualCamera as CinemachineVirtualCamera;
        originalCameraFov = activeCam.m_Lens.FieldOfView;
        // Temp camera fov
        cameraFov = originalCameraFov;
    }
}

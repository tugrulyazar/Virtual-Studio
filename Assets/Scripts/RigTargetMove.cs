using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class RigTargetMove : MonoBehaviour
{
    [SerializeField]
    [Range(0.0f, 10.0f)]
    private float distance;

    [SerializeField]
    private GameObject head;
    private Transform headTransform;
    private Transform mainCamera;

    private void Awake()
    {
        // Get head transform
        headTransform = head.transform;
        // Get main camera transform
        mainCamera = Camera.main.transform;
    }

    void Update()
    {    
        // Move target according to camera
        transform.position = headTransform.position + mainCamera.forward * distance;
    }
}

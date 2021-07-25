using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class RigTargetMove : MonoBehaviour
{
    [SerializeField]
    [Range(0.0f, 10.0f)]
    private float distance;

    [SerializeField]
    private GameObject head;

    [SerializeField]
    private GameObject playerPos;

    [SerializeField]
    private GameObject[] AimConstraints;

    private List<MultiAimConstraint> MultiAimConstraints = new List<MultiAimConstraint>();
    private List<float> originalWeights = new List<float>();
    private Transform headTransform;
    private Transform mainCamera;

    private float transitionRate = 2.0f;
    public bool lookingBehind = false;

    private float dot;
    private Vector3 heading;

    private void Awake()
    {
        // Get main camera transform
        mainCamera = Camera.main.transform;
    }

    
    private void Start()
    {
        // Get original weights
        foreach (GameObject aim in AimConstraints)
        {
            MultiAimConstraint mac = aim.GetComponent<MultiAimConstraint>();
            MultiAimConstraints.Add(mac);
            originalWeights.Add(mac.weight);  
        }
    }

    void Update()
    {    
        // Move target according to camera
        transform.position = head.transform.position + mainCamera.forward * distance;

        // Check if target is behind the player
        heading = transform.position - playerPos.transform.position;
        float angle = Vector3.Angle(heading, playerPos.transform.forward);
        Debug.Log(angle);

        // If the target is behind, decrease constraint weight over time
        if (angle > 100)
        {
            foreach (MultiAimConstraint mac in MultiAimConstraints)
            {
                mac.weight = Mathf.Lerp(mac.weight, 0, Time.deltaTime * transitionRate);
            }
        }
        else // look at the target
        {
            for (int i = 0; i < MultiAimConstraints.Count; i++)
            {
                MultiAimConstraints[i].weight = Mathf.Lerp(MultiAimConstraints[i].weight, originalWeights[i], Time.deltaTime * transitionRate);
            }
        }
        
    }
}

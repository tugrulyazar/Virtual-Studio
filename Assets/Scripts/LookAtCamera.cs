using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private Transform mainCamera;
    private void Start()
    {
        mainCamera = Camera.main.transform;
    }

    void Update()
    {
        // Make the text look at camera
        Vector3 dir = (transform.position - mainCamera.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(dir, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 360);
    }
}

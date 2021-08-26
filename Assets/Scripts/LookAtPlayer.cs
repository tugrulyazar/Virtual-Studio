using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    private GameObject player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        // Make the text look at player
        Vector3 dir = (transform.position - player.transform.position).normalized;
        Vector3 targetDirection = new Vector3(dir.x, 0, dir.z);
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 360);
    }
}

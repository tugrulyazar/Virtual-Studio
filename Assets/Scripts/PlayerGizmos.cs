using UnityEngine;

public class PlayerGizmos : MonoBehaviour
{
    private UserBehaviour.PlayerController playerController;

    // Colors
    private Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
    private Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

    private bool grounded;
    private float groundedOffset;
    private float groundedRadius;

    private void Awake()
    {
        playerController = GetComponent<UserBehaviour.PlayerController>();
    }

    private void Update()
    {
        groundedOffset = playerController.groundedOffset;
        groundedRadius = playerController.groundedRadius;
        grounded = playerController.grounded;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw grounded gizmo
        if (grounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        // When selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z), groundedRadius);
    }
}
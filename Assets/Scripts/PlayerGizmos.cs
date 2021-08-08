using UnityEngine;

namespace UserBehaviour
{
    public class PlayerGizmos : MonoBehaviour
    {
        private PlayerController playerController;

        // Colors
        private Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        private Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        private bool grounded;
        private float groundedOffset;
        private float groundedRadius;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
        }

        private void Start()
        {
            groundedOffset = playerController.groundedOffset;
            groundedRadius = playerController.groundedRadius;
        }

        private void Update()
        {
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
}
using UnityEngine;
using TMPro;

namespace UserBehaviour
{
    public class DetectSlopeAngle : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI angleText;

        private PlayerController playerController;

        private float slopeAngle;
        private float slopePercent;
        private string angleToString;

        private void Start()
        {
            playerController = GetComponent<PlayerController>();
        }
        private void Update()
        {
            slopePercent = Mathf.Round((slopeAngle / 90f) * 100);
            playerController.slopeAngle = slopePercent;

            angleToString = slopePercent.ToString();
            angleText.text = "%" + angleToString;
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            slopeAngle = Vector3.Angle(Vector3.up, hit.normal);
        }
    }
}


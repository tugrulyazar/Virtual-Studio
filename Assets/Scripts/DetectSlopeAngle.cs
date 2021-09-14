using UnityEngine;
using TMPro;

public class DetectSlopeAngle : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI angleText;

    private UserBehaviour.PlayerController playerController;

    private float slopeAngle;
    private float slopePercent;
    private string angleToString;

    private void Start()
    {
        playerController = GetComponent<UserBehaviour.PlayerController>();
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


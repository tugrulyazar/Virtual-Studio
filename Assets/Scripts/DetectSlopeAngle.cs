using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DetectSlopeAngle : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI angleText;
    private float slopeAngle;
    private float slopePercent;
    private string angleToString;

    private void Update()
    {
        slopePercent = Mathf.Round((slopeAngle / 90f) * 100);
        angleToString = slopePercent.ToString();
        angleText.text = "%" + angleToString;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        slopeAngle = Vector3.Angle(Vector3.up, hit.normal);
    }
}

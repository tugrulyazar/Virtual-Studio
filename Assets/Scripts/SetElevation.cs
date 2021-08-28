using UnityEngine;
using TMPro;

namespace UserBehaviour
{
    public class SetElevation : MonoBehaviour
    {
        public static int markerCount;
        private static float zeroElevation;

        void Start()
        {
            // Declare variables, get tmp component
            float elevation;
            TextMeshPro tmp = GetComponent<TextMeshPro>();

            // If it's the first elevation marker, set it as the zero point
            if (markerCount == 0)
            {
                zeroElevation = gameObject.transform.parent.position.y;
                elevation = 0f;
            }
            else
            {
                // Find the elevation relative to the zero point
                elevation = gameObject.transform.parent.position.y - zeroElevation;
                elevation = Mathf.Round(elevation * 100f) / 100f;
            }


            if (elevation == 0)
            {
                tmp.SetText("±0.00");
            }
            else if (elevation > 0)
            {
                tmp.SetText("+" + elevation.ToString("#0.00"));
            }
            else
            {
                tmp.SetText(elevation.ToString("#0.00"));
            }

            // Increase marker count in this scope to get proper reading
            markerCount++;
        }
    }
}


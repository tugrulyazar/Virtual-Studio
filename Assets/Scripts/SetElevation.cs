using UnityEngine;
using TMPro;

public class SetElevation : MonoBehaviour
{
    void Start()
    {
        TextMeshPro tmp = GetComponent<TextMeshPro>();
        float elevation = gameObject.transform.parent.position.y;
        elevation = Mathf.Round(elevation * 100f) / 100f;

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
    }
}

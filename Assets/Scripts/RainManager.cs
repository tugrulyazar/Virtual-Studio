using System.Collections;
using UnityEngine;

public class RainManager : MonoBehaviour
{
    [SerializeField]
    private GameObject rainObject;

    private bool isRaining;
    private float xRange = 10f;
    private float zRange = 10f;
    private float height = 20f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Comma))
        {
            if (isRaining)
            {
                isRaining = false;
            }
            else
            {
                isRaining = true;
                StartCoroutine(MakeItRain());
            }
        }
    }

    private IEnumerator MakeItRain()
    {
        while (isRaining)
        {
            Vector3 pos = new Vector3(Random.Range(0f, xRange), height, Random.Range(-zRange, 0f));
            Instantiate(rainObject, pos, Quaternion.identity, gameObject.transform);
            yield return new WaitForSeconds(0.005f);
        }
        yield return null;
    }
}


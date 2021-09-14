using UnityEngine;

public class RainBehaviour : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Ground")
        {
            Destroy(gameObject, 3f);
        }
        else
        {
            Destroy(gameObject, 10f);
        }
    }
}

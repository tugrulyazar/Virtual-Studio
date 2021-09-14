using UnityEngine;

public class RainBehaviour : MonoBehaviour
{
    private float groundDisappearTime = 3f;
    private float modelDisappearTime = 20f;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Ground")
        {
            Destroy(gameObject, groundDisappearTime);
        }
        else
        {
            Destroy(gameObject, modelDisappearTime);
        }
    }
}

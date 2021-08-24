using UnityEngine;

public class IKFootPlacement : MonoBehaviour
{
    Animator anim;
    [SerializeField]
    private LayerMask IKLayerMask;

    [SerializeField]
    [Range(0, 1f)]
    private float distanceToGround;

    [SerializeField]
    [Range(0f, 1f)]
    private float distanceOffset;

    private int animIDleftFootWeight;
    private int animIDrightFootWeight;
    void Start()
    {
        anim = GetComponent<Animator>();
        animIDleftFootWeight = Animator.StringToHash("leftFootIKWeight");
        animIDrightFootWeight = Animator.StringToHash("rightFootIKWeight");
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (anim)
        {
            // Set weights
            float leftFootWeight = anim.GetFloat(animIDleftFootWeight);
            float rightFootWeight = anim.GetFloat(animIDrightFootWeight);

            anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, leftFootWeight);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, leftFootWeight);
            anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, rightFootWeight);
            anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, rightFootWeight);

            // Left foot
            RaycastHit hit;

            // Start the ray one unit above the actual position to account for clipping
            Ray ray = new Ray(anim.GetIKPosition(AvatarIKGoal.LeftFoot) + Vector3.up, Vector3.down);

            // Account for the extra unit set above
            if (Physics.Raycast(ray, out hit, distanceToGround + 1f + distanceOffset, IKLayerMask))
            {
                Vector3 footPosition = hit.point;
                footPosition.y += distanceToGround;
                anim.SetIKPosition(AvatarIKGoal.LeftFoot, footPosition);
                anim.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.FromToRotation(Vector3.up, hit.normal) * transform.rotation);
            }

            // Right foot
            ray = new Ray(anim.GetIKPosition(AvatarIKGoal.RightFoot) + Vector3.up, Vector3.down);

            if (Physics.Raycast(ray, out hit, distanceToGround + 1f + distanceOffset, IKLayerMask))
            {
                Vector3 footPosition = hit.point;
                footPosition.y += distanceToGround;
                anim.SetIKPosition(AvatarIKGoal.RightFoot, footPosition);
                anim.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.FromToRotation(Vector3.up, hit.normal) * transform.rotation);
            }
        }
    }
}

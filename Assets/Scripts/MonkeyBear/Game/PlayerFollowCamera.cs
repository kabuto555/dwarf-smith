using UnityEngine;

public class PlayerFollowCamera : MonoBehaviour
{
    public GameObject FollowTarget;

    void Update()
    {
        var targetPosition = FollowTarget.transform.position;
        var tf = transform;
        tf.position = new Vector3(targetPosition.x, targetPosition.y, tf.position.z);
    }
}

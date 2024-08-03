using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFollowCamera : MonoBehaviour
{
    public GameObject FollowTarget;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var targetPosition = FollowTarget.transform.position;
        var tf = transform;
        tf.position = new Vector3(targetPosition.x, targetPosition.y, tf.position.z);
    }
}

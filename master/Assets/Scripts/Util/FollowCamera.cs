using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target;

    public float linStrength = 1.0f;
    public float angStrength = 1.0f;

    void Start()
    {
        
    }

    void Update()
    {
        float dT = Time.deltaTime;

        transform.position = Vector3.Lerp(transform.position, target.position, dT * linStrength);
        transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, dT * angStrength);
    }
}

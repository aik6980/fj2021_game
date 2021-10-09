using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target;
	public Vector3 relPosition = Vector3.zero;
	public Quaternion relRotation = Quaternion.identity;

    public float linStrength = 1.0f;
    public float angStrength = 1.0f;

    void Start()
    {
        
    }

    void Update()
    {
        float dT = Time.deltaTime;

        transform.position = Vector3.Lerp(transform.position, target.TransformPoint(relPosition), dT * linStrength);
        transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation * relRotation, dT * angStrength);
    }
}

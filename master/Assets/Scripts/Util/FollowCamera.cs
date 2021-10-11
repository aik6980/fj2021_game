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
	public float extrapolation = 0.0f;
	public bool local;
	public float velocityLerp = 1.0f;

	public Vector3 lastPos;
	public Vector3 velocity;

    void Start()
    {
		lastPos = target.TransformPoint(relPosition);
		if (local && transform.parent!=null)
			lastPos = transform.parent.InverseTransformPoint(lastPos);
	}

	void Update()
    {
        float dT = Time.deltaTime;

		Vector3 newPos = target.TransformPoint(relPosition);
		if (local && transform.parent != null)
			newPos = transform.parent.InverseTransformPoint(newPos);
		Vector3 vel = (newPos - lastPos) / dT;
		velocity = Vector3.Lerp(velocity, vel, velocityLerp * Time.deltaTime);
		Vector3 pos = newPos + velocity * extrapolation;
		lastPos = newPos;
		if (local && transform.parent != null)
			pos = transform.parent.TransformPoint(pos);

		transform.position = Vector3.Lerp(transform.position, pos, dT * linStrength);
        transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation * relRotation, dT * angStrength);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipMovement : MonoBehaviour
{
	public Transform planetRoot;
	public float radius;

	public Vector3 velocity;

	public ParticleSystem waterSplosh;
	public Transform sploshTrans;
	public float emissionRateScale = 0.1f;

	public float maxHeight = 1.0f;
	public float maxDistance = 2.0f;
	public float collisionDistance = 0.2f;
	public float pushDistance = 0.3f;
	public float dragDistance = 0.4f;
	public Vector3[] collisionPoints;
	public Vector3[] rayDir;
	public float[] depth;
	public Vector3[] hitNormal;
	public Collider[] hitCollider;


	// Start is called before the first frame update
	void Start()
    {
		depth = new float[collisionPoints.Length];
		hitNormal =  new Vector3[collisionPoints.Length];
		hitCollider = new Collider[collisionPoints.Length];
	}

    // Update is called once per frame
    void Update()
    {
		if (Input.GetKeyDown(KeyCode.W))
			velocity.z += 5.0f;
		if (Input.GetKeyDown(KeyCode.S))
			velocity.z -= 5.0f;
		if (Input.GetKeyDown(KeyCode.A))
			velocity.y -= 5.0f;
		if (Input.GetKeyDown(KeyCode.D))
			velocity.y += 5.0f;

		Vector3 angVel;
		angVel.x = -velocity.z / radius;
		angVel.z = velocity.x / radius;
		angVel.y = -velocity.y;

		//How do we "collide" with islands? we are not using physics on the ship (and really shouldn't)
		//idea: use a raycast to MEASURE DEPTH, and use that to slow down (drag the bottom) and push away :) 
		IslandCollision();

		if (angVel.magnitude > 0.001f)
		{
			Quaternion turn = Quaternion.AngleAxis(angVel.magnitude * Time.deltaTime, angVel.normalized);
			planetRoot.rotation = turn * planetRoot.rotation;
		}

		if (waterSplosh)
		{
			waterSplosh.emissionRate = velocity.sqrMagnitude * emissionRateScale;
			//var emitParams = new ParticleSystem.EmitParams();
			var shape = waterSplosh.shape;
			shape.position = waterSplosh.transform.InverseTransformPoint(sploshTrans.transform.position);
			shape.rotation = (Quaternion.Inverse(waterSplosh.transform.rotation) * sploshTrans.transform.rotation).eulerAngles;
		}
	}

	void IslandCollision()
	{
		RaycastHit hit;
		for (int i = 0; i < collisionPoints.Length; i++)
		{
			Vector3 p1 = this.transform.TransformPoint(collisionPoints[i]);
			Vector3 dir = this.transform.TransformDirection(rayDir[i].normalized);
			p1 -= dir * maxHeight;
			if (Physics.Raycast(p1, dir, out hit, maxDistance))
			{
				depth[i] = hit.distance;
				hitNormal[i] = hit.normal;
				hitCollider[i] = hit.collider;
			} else
			{
				depth[i] = maxDistance;
				hitNormal[i] = Vector3.up;
				hitCollider[i] = null;
			}
			Debug.DrawLine(p1, p1 + dir * depth[i], Color.Lerp(Color.red, Color.green, depth[i]/maxDistance));
		}

		Vector3 F = Vector3.zero;
		for (int i = 0; i < collisionPoints.Length; i++)
		{
			if (depth[i] < maxDistance)
			{
				Vector3 normal = (hitNormal[i] - Vector3.up * hitNormal[i].y).normalized;

				Vector3 p1 = this.transform.TransformPoint(collisionPoints[i]);
				Vector3 dir = this.transform.TransformDirection(rayDir[i].normalized);
				Debug.DrawRay(p1 + dir * (depth[i] - maxHeight), normal);

				float v = Vector3.Dot(velocity, normal);

				if (depth[i] < maxHeight + collisionDistance)
				{
					velocity += normal * Mathf.Clamp(-v, 0, 100.0f);
				} else
				if (depth[i] < maxHeight + dragDistance)
				{
					velocity += -velocity * 0.1f * Time.deltaTime;
				} else
				if (depth[i] < maxHeight + pushDistance)
				{
					float f = (maxHeight + pushDistance - depth[i]);
					velocity += normal * f;
					velocity.y += Vector3.Cross(normal, p1 - this.transform.position).y * f * -5.0f;
				}
			}
		}
	}
}

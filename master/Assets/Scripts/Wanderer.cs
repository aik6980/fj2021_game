using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wanderer : MonoBehaviour
{
	public Transform planetRoot;
	public float radius;

	public float moveSpeed = 1.0f;
	public Vector3 velocity = Vector3.forward;
	public float turnTimeMin = 1.0f;
	public float turnTimeMax = 10.0f;
	public float maxTurnSpeed = 5.0f;
	public float lateralDrag = 1.0f;

	public float turnTimer;

	public Vector3 worldVel;

	// Start is called before the first frame update
	void Start()
    {
		PlanetTurner Tina = PlanetTurner.singleton;
		planetRoot = Tina.planetRoot;
		radius = Tina.radius;
    }

    // Update is called once per frame
    void Update()
    {
		Vector3 localUp = (this.transform.position - planetRoot.position).normalized;

		worldVel = this.transform.rotation * velocity;
		worldVel -= localUp * Vector3.Dot(worldVel, localUp);

		//lateral drag
		worldVel -= transform.right * Vector3.Dot(worldVel, transform.right) * lateralDrag * Time.deltaTime;

		this.transform.position += worldVel * Time.deltaTime;// * Mathf.Deg2Rad;

		Vector3 lv = Quaternion.Inverse(this.transform.rotation) * worldVel;
		velocity.x = lv.x;
		//keep y!
		//velocity.z = lv.z;
		velocity.z = moveSpeed;

		//ToDo: gentle turn every now and then to avoid them just circling the planet
		if (turnTimer > 0)
			turnTimer -= Time.deltaTime;
		else
		{
			turnTimer = Random.Range(turnTimeMin, turnTimeMax);
			velocity.y = Random.Range(-maxTurnSpeed, maxTurnSpeed);
		}

		Quaternion deltaYaw = Quaternion.AngleAxis(velocity.y * Time.deltaTime, Vector3.up);
		this.transform.rotation = deltaYaw * this.transform.rotation;
		velocity = Quaternion.Inverse(deltaYaw) * velocity;

		//eliminate numerical drift in altitude
		localUp = (this.transform.position - planetRoot.position).normalized;
		this.transform.position = planetRoot.position + localUp * radius;
		//stay upright
		Vector3 cross = Vector3.Cross(this.transform.up, localUp);
		if (cross != Vector3.zero)
		{
			Quaternion turnQ = Quaternion.AngleAxis(cross.magnitude * Mathf.Rad2Deg, cross.normalized);
			this.transform.rotation = turnQ * this.transform.rotation;
		}

	}
}

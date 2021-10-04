using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wanderer : MonoBehaviour
{
	public Transform planetRoot;
	public float radius;

	public Vector3 velocity = Vector3.forward;

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
		Vector3 worldVel = this.transform.rotation * velocity;
		worldVel.y = 0.0f;
		this.transform.position += worldVel * Time.deltaTime;// * Mathf.Deg2Rad;

		//eliminate numerical drift in altitude
		Vector3 planetUp = (this.transform.position - planetRoot.position).normalized;
		this.transform.position = planetRoot.position + planetUp * radius;
		//stay upright
		Vector3 cross = Vector3.Cross(this.transform.up, planetUp);
		if (cross != Vector3.zero)
		{
			Quaternion turnQ = Quaternion.AngleAxis(cross.magnitude * Mathf.Rad2Deg, cross.normalized);
			this.transform.rotation = turnQ * this.transform.rotation;
		}

	}
}

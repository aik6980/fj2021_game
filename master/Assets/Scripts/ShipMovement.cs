using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipMovement : MonoBehaviour
{
	public Transform planetRoot;
	public float radius;

	public Vector3 velocity;

	public Vector3 angVel;

    // Start is called before the first frame update
    void Start()
    {
        
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

		angVel.x = -velocity.z / radius;
		angVel.z = velocity.x / radius;
		angVel.y = -velocity.y;

		if (angVel.magnitude > 0.001f)
		{
			Quaternion turn = Quaternion.AngleAxis(angVel.magnitude * Time.deltaTime, angVel.normalized);
			planetRoot.rotation = turn * planetRoot.rotation;
		}

		//How do we "collide" with islands? we are not using physics on the ship (and really shouldn't)
		//idea: use a raycast to MEASURE DEPTH, and use that to slow down (drag the bottom) and push away :) 
	}
}

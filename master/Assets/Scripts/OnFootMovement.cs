using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnFootMovement : MonoBehaviour
{
	public CameraControl camCon;

	public float walkLevel = 0.1f;
	public float maxHeight = 1.0f;
	public float maxDistance = 1.2f;
	public float depth;
	public Vector3 hitPoint;
	public Vector3 hitNormal;
	public Collider hitCollider;
	public LayerMask mask;

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		if (camCon.mode != CameraControl.Mode.LandWalk) return;

		//ToDo: point-and click (detect if i clicked on valid ground and move to where)
		// use depth tests to avoid walking into the water
		// detect encounters (probably triggers on landmarks, collider on me) and trigger narrative
		// must work nicely with cameracontrol and UI

		//temp
		Vector3 oldPos = transform.position - Vector3.up * walkLevel;
		Vector3 newPos = oldPos;

		if (Input.GetKeyDown(KeyCode.W))
			newPos += camCon.transform.forward * 0.1f;
		if (Input.GetKeyDown(KeyCode.S))
			newPos += camCon.transform.forward * -0.1f;
		if (Input.GetKeyDown(KeyCode.A))
			newPos += camCon.transform.right * -0.1f;
		if (Input.GetKeyDown(KeyCode.D))
			newPos += camCon.transform.right * 0.1f;


		if (newPos != oldPos)
		{
			RaycastHit hit;

			Vector3 p1 = newPos;
			Vector3 dir = Vector3.down;
			p1 -= dir * maxHeight;
			Debug.DrawRay(p1, dir * maxDistance);
			if (Physics.Raycast(p1, dir, out hit, maxDistance, mask))
			{
				depth = hit.distance;
				hitPoint = hit.point;
				hitNormal = hit.normal;
				hitCollider = hit.collider;
				//also don't go under the water level
				//if ((hitPoint - camCon.Tina.planetRoot.position).magnitude > camCon.Tina.radius)	//not accurate enough
				{
					transform.rotation = Quaternion.LookRotation(newPos - transform.position);
					transform.position = hitPoint + Vector3.up * walkLevel;
				}
			} else
			{
				depth = maxDistance;
				hitPoint = p1;
				hitNormal = Vector3.up;
				hitCollider = null;
			}
		}

		//stay upright
		Vector3 cross = Vector3.Cross(this.transform.up, Vector3.up);
		if (cross != Vector3.zero)
		{
			Quaternion turnQ = Quaternion.AngleAxis(cross.magnitude * Mathf.Rad2Deg, cross.normalized);
			this.transform.rotation = turnQ * this.transform.rotation;
		}

	}

	private void OnTriggerEnter(Collider other)
	{
		Debug.Log(other.name);
	}
}

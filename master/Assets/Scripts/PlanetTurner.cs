using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetTurner : MonoBehaviour
{
	public Transform planetRoot;
	public float radius;

	public Transform whatToFollow;

	public Quaternion turnQ;

	public static PlanetTurner singleton;

	private void Awake()
	{
		singleton = this;
	}

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		//turnQ = Quaternion.FromToRotation(currentVector, Vector3.up);
		Vector3 cross = Vector3.Cross((whatToFollow.position - planetRoot.position).normalized, Vector3.up);
		if (cross != Vector3.zero)
		{
			turnQ = Quaternion.AngleAxis(cross.magnitude * Mathf.Rad2Deg, cross.normalized);
			planetRoot.rotation = turnQ * planetRoot.rotation;
			//keep it upright in case it doesn't do it itself
			whatToFollow.rotation = Quaternion.Inverse(turnQ) * whatToFollow.rotation;
			CameraControl.singleton.transform.rotation = Quaternion.Inverse(turnQ) * CameraControl.singleton.transform.rotation;
		}

		//
		//deltaTurn.x = -worldVel.z / radius;
		//deltaTurn.z = worldVel.x / radius;
		//deltaTurn.y = 0;// -velocity.y;

		//deltaTurn *= Time.deltaTime;
/*
		if (deltaTurn.magnitude > 0.001f)
		{
			Quaternion turnQ = Quaternion.AngleAxis(deltaTurn.magnitude, deltaTurn.normalized);
			planetRoot.rotation = turnQ * planetRoot.rotation;
		}
*/
	}
}

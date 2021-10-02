using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
	public Vector3 angVel;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		Quaternion turn = Quaternion.AngleAxis(angVel.magnitude * Time.deltaTime, angVel.normalized);

		transform.localRotation = turn * transform.localRotation;
	}
}

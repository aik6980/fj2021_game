using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeagullMovement : MonoBehaviour
{
	public CameraControl camCon;
	public OnFootMovement footMove;

	public Transform tiltable;
	public float tiltMax = -90.0f;

	public float drag = 1.0f;
	public float acceleration = 5.0f;
	public float liftSpeed = 1.0f;
	public float slerpRate = 1.0f;
	public float lift = 1.0f;

	public Vector3 worldVel;




	// Start is called before the first frame update
	void Start()
    {
		if (!footMove)
			footMove = GetComponent<OnFootMovement>();
		if (footMove)
			camCon = footMove.camCon;
    }

    // Update is called once per frame
    void Update()
    {
		//ToDo: add falling and flying (WASD, a bit like the ship movement, plus SPC), and simply revert to onfootmovement when on ground :)
		//actually onfoot could handle falling... it knows if we have ground or not

		Transform planetRoot = PlanetTurner.singleton ? PlanetTurner.singleton.transform : null;
		Vector3 localUp = PlanetTurner.singleton ? (this.transform.position - planetRoot.position).normalized : Vector3.up;

		if (Input.GetKey(KeyCode.Space))
		{
			//transform.position += localUp * liftSpeed * Time.deltaTime;
			worldVel += localUp * liftSpeed * Time.deltaTime;
			footMove.hitCollider = null;
			footMove.onGround = false;
			footMove.jumping = true;
		} else
		{
			footMove.jumping = false;
		}

		if (footMove.onGround && !footMove.jumping)
		{
			worldVel = Vector3.zero;
		} else
		{
			//lift
			worldVel += transform.up * Mathf.Abs(Vector3.Dot(worldVel, transform.forward)) * lift * Time.deltaTime;
			//drag
			worldVel += Vector3.ClampMagnitude(worldVel * -drag * Time.deltaTime, worldVel.magnitude);

			Vector3 velocity = camCon.transform.InverseTransformVector(worldVel);

			//input
			if (Input.GetKey(KeyCode.W))
				velocity.z += acceleration * Time.deltaTime;
			if (Input.GetKey(KeyCode.A))
				velocity.x -= acceleration * Time.deltaTime;
			if (Input.GetKey(KeyCode.S))
				velocity.z -= acceleration * Time.deltaTime;
			if (Input.GetKey(KeyCode.D))
				velocity.x += acceleration * Time.deltaTime;

			worldVel = camCon.transform.TransformVector(velocity);

			transform.position += worldVel * Time.deltaTime;

			//transform.rotation = Quaternion.LookRotation(worldVel);
			Vector3 fwd = worldVel - localUp * footMove.fallSpeed;
			Vector3 xy = fwd;
			xy.y = 0.0f;
			float q = Mathf.Clamp01(xy.magnitude / fwd.magnitude);
			Vector3 up = Vector3.Lerp(transform.up, localUp, q * q);
			//ToDo: suppress pitch at low vertical speeds
			Vector3 flatFwd = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0) * Vector3.forward;
			fwd = Vector3.Lerp(flatFwd, fwd, q*q);
			//fwd = Vector3.Lerp(, fwd, q * q);
			Quaternion look = Quaternion.LookRotation(fwd, up);

			//ToDo: roll (based on acceleration)

			Debug.DrawRay(transform.position, worldVel, Color.green);
			Debug.DrawRay(transform.position, fwd, Color.red);

			transform.rotation = Quaternion.Slerp(transform.rotation, look, slerpRate * Time.deltaTime);

			if (tiltable)
			{
				tiltable.localRotation = Quaternion.Euler(0, 0, Vector3.Dot(worldVel, transform.right) * tiltMax);
			}
		}
	}
}

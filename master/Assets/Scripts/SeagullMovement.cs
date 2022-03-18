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
	public float liftAcceleration = 1.0f;
	public float slerpRate = 1.0f;
	public float lift = 1.0f;
	public float gravity = 0.5f;

	public Vector3 worldVel;

	public LayerMask mask;
	public Vector3 hitPoint;
	public Vector3 hitNormal;
	public Collider hitCollider;

	public float liftTimer;
	public GameObject poop;
	//public float cameraSlerpRate = 1.0f;


	// Start is called before the first frame update
	void Start()
    {
		if (!footMove)
			footMove = GetComponent<OnFootMovement>();
		if (footMove)
		{
			if (camCon)
				footMove.camCon = camCon;
			else
				camCon = footMove.camCon;
		}
    }

    // Update is called once per frame
    void Update()
    {
		if ((ConstellationMgr.Instance && ConstellationMgr.Instance.is_canvas_mode_enabled())
			|| (camCon && camCon.mode != CameraControl.Mode.LandWalk && camCon.mode != CameraControl.Mode.Seagull)
			)//|| stopped)
		{
			return;
		}


		//ToDo: add falling and flying (WASD, a bit like the ship movement, plus SPC), and simply revert to onfootmovement when on ground :)
		//actually onfoot could handle falling... it knows if we have ground or not

		Transform planetRoot = PlanetTurner.singleton ? PlanetTurner.singleton.transform : null;
		Vector3 localUp = PlanetTurner.singleton ? (this.transform.position - planetRoot.position).normalized : Vector3.up;

		if (Input.GetKey(KeyCode.Space))
		{
			//transform.position += localUp * liftSpeed * Time.deltaTime;
			worldVel += localUp * liftAcceleration * Time.deltaTime;

			if (liftTimer == 0 && footMove)
			{// MIGHT be a takeoff, but might be just start flapping in air
				hitCollider = footMove.hitCollider;
				hitPoint = footMove.hitPoint;
				hitNormal = footMove.hitNormal;
			}

			footMove.hitCollider = null;
			footMove.onGround = false;
			footMove.jumping = true;

			liftTimer += Time.deltaTime;
		} else
		{
			footMove.jumping = false;
			if (liftTimer > 0 && liftTimer < 0.2f)
			{//was a tap, not hold
				Poop();
			}
			liftTimer = 0;
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

			//gravity
			worldVel -= localUp * gravity * Time.deltaTime;


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
			Vector3 fwd = worldVel;
			if (!footMove.jumping) fwd -= localUp * footMove.fallSpeed;
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

			//Debug.DrawRay(transform.position, worldVel, Color.green);
			//Debug.DrawRay(transform.position, fwd, Color.red);

			transform.rotation = Quaternion.Slerp(transform.rotation, look, slerpRate * Time.deltaTime);

			//Debug.DrawRay(transform.position, transform.right, Color.blue);

			if (tiltable)
			{
				tiltable.localRotation = Quaternion.Slerp(tiltable.localRotation,
										Quaternion.Euler(0, 0, Vector3.Dot(worldVel, transform.right) * tiltMax),
										slerpRate * Time.deltaTime);
			}

			//camCon.transform.rotation = Quaternion.Slerp(camCon.transform.rotation, tiltable.rotation, cameraSlerpRate * Time.deltaTime);
		}
	}

	void OnCollisionEnter(Collision collision)
	{
		Debug.Log("bang " + collision.collider.name);
	}

	void Poop()
	{
		//could be a tap mid air!
		Debug.Log("Mine!");

		Vector3 p1 = transform.TransformPoint(0, 0, -0.05f);
		Vector3 dir = -transform.up;
		float maxDistance = 0.2f;
		RaycastHit hit;
		if (Physics.Raycast(p1, dir, out hit, maxDistance, mask))
		{
			hitPoint = hit.point;
			hitNormal = hit.normal;
			hitCollider = hit.collider;

			if (hitCollider)
			{
				if (poop)
				{
					Instantiate(poop
								, hitPoint, Quaternion.LookRotation(hitNormal, transform.forward) * Quaternion.AngleAxis(90.0f, Vector3.right)
								, transform.parent);
				}
			}
		}
	}
}

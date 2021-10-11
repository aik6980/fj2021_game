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

	public float stepLength = 0.1f;
	public float stepTime = 0.3f;

	public LayerMask mask;
	public Vector3 hitPoint;
	public Vector3 hitNormal;
	public Collider hitCollider;

	public bool onGround;   //hitCollider!=null;
	public bool jumping;
	public float fallSpeed = 1.0f;	// m/sec

	public bool moveTargetValid;
	public Collider moveTarget;
	public Vector3 moveRelPos;
	public float stepTimer;

	public float maxGroundSlopeUp = 45.0f;

	public float currentLevel;
	public float seaLevelMin = 195.0f;

	public Vector3 modelOffset = Vector3.zero;
	public Quaternion modelRotationOffset = Quaternion.identity;
	public Transform model;

	public bool stopped;


	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		if ((ConstellationMgr.Instance && ConstellationMgr.Instance.is_canvas_mode_enabled())
			|| (camCon && camCon.mode != CameraControl.Mode.LandWalk && camCon.mode != CameraControl.Mode.Seagull)
			|| stopped)
		{
			stepTimer = 0;
			return;
		}

		//ToDo: point-and click (detect if i clicked on valid ground and move to where)
		// use depth tests to avoid walking into the water
		// detect encounters (probably triggers on landmarks, collider on me) and trigger narrative
		// must work nicely with cameracontrol and UI

		//temp
		Vector3 oldPos = transform.position - Vector3.up * walkLevel;
		Vector3 newPos = oldPos;

		bool stepped = false;

		//move on WASD press (allows faster ;)
/*		if (Input.GetKeyDown(KeyCode.W))
			newPos += camCon.transform.forward * stepLength;
		if (Input.GetKeyDown(KeyCode.S))
			newPos += camCon.transform.forward * -stepLength;
		if (Input.GetKeyDown(KeyCode.A))
			newPos += camCon.transform.right * -stepLength;
		if (Input.GetKeyDown(KeyCode.D))
			newPos += camCon.transform.right * stepLength;
		stepped = newPos != oldPos;
*/
		if (stepTimer > 0)
		{
			stepTimer -= Time.deltaTime;
			//if (model)
			//	model.transform.localPosition = modelOffset * Mathf.Clamp01(stepTimer / stepTime);
		}

		Transform planetRoot = PlanetTurner.singleton ? PlanetTurner.singleton.transform : null;
		Vector3 localUp = PlanetTurner.singleton ? (this.transform.position - planetRoot.position).normalized : Vector3.up;

		if (!onGround && !jumping)
		{
			transform.position += localUp * -fallSpeed * Time.deltaTime;	// NOTE: constant speed! :oP

			RaycastHit hit;

			Vector3 p1 = newPos;
			Vector3 dir = Vector3.down;
			p1 -= dir * maxHeight;
			Debug.DrawRay(p1, dir * maxDistance, Color.cyan, 10.0f);

			if (Physics.Raycast(p1, dir, out hit, maxDistance, mask))
			{
				depth = hit.distance;
				hitPoint = hit.point;
				hitNormal = hit.normal;
				hitCollider = hit.collider;
				onGround = hitCollider != null;
				//ToDo: set the appropriate height (snap down)
				transform.position = hitPoint + Vector3.up * walkLevel;
			}
			return;
		}

		onGround = hitCollider != null;

		//ToDo: if we have ground but it's too steep, SLIDE =o)

		if (onGround)
		{
			if (!stepped && stepTimer <= 0)
			{//move on WASD hold
				if (Input.GetKey(KeyCode.W))
					newPos += camCon.transform.forward * stepLength;
				if (Input.GetKey(KeyCode.S))
					newPos += camCon.transform.forward * -stepLength;
				if (Input.GetKey(KeyCode.A))
					newPos += camCon.transform.right * -stepLength;
				if (Input.GetKey(KeyCode.D))
					newPos += camCon.transform.right * stepLength;

				if (newPos != oldPos)
				{
					stepped = true;
					moveTargetValid = false;
					stepTimer = stepTime;
					modelOffset = Vector3.zero;
					modelRotationOffset = Quaternion.identity;
					AudioManager.Instance.PlaySFX("footstep");
				}
			}

			if (!stepped && moveTargetValid && stepTimer <= 0)
			{
				Vector3 wp = moveTarget.transform.TransformPoint(moveRelPos);
				newPos += Vector3.ClampMagnitude(wp - newPos, 0.1f);
				stepTimer = stepTime;
				modelOffset = Vector3.zero;
				modelRotationOffset = Quaternion.identity;

				Debug.DrawLine(transform.position, wp, Color.white, 5.0f);
				if ((wp - newPos).magnitude < stepLength)
				{
					moveTargetValid = false;
				}
			}

			//just for debug
			if (planetRoot)
				currentLevel = (this.transform.position - planetRoot.position).magnitude;

			if (newPos != oldPos)
			{
				RaycastHit hit;

				Vector3 p1 = newPos;
				Vector3 dir = Vector3.down;
				p1 -= dir * maxHeight;
				Debug.DrawRay(p1, dir * maxDistance, Color.cyan, 10.0f);

				if (Physics.Raycast(p1, dir, out hit, maxDistance, mask))
				{
					depth = hit.distance;
					hitPoint = hit.point;
					hitNormal = hit.normal;
					hitCollider = hit.collider;
					onGround = hitCollider != null;

					Vector3 nextPos = hitPoint + Vector3.up * walkLevel;
					Vector3 delta = nextPos - transform.position;

					bool stepUp = Vector3.Dot(delta, localUp) >= 0;
					float groundSlopeThere = Mathf.Acos(Vector3.Dot(hitNormal, localUp)) * Mathf.Rad2Deg;

					//do not step on too steep ground
					if (groundSlopeThere >= maxGroundSlopeUp)
					{//too steep
						Debug.Log("too steep");
					} else
					//do not step DOWN into water (but allow coming up out)
					if (!stepUp && planetRoot && (nextPos - planetRoot.position).magnitude < seaLevelMin) //not accurate enough but cheap early test
					{
						Debug.Log("water");
					} else
					//also check if there's no collider blocking my way there; e.g. rocks, trees, etc
					{
						Debug.DrawLine(transform.position, nextPos, Color.green, 10);

						if (Physics.Raycast(transform.position, delta.normalized, out hit, delta.magnitude, mask))
						{//hit something; cancel
						 //FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Ouch");
							Debug.Log("obstacle " + hit.collider.name, hit.collider);
						} else
						{
							//Debug.Log(transform.position.ToString() + "->" + nextPos.ToString());
							Vector3 lookEuler = Quaternion.LookRotation(nextPos - transform.position).eulerAngles;
							lookEuler.x = 0.0f;
							lookEuler.z = 0.0f;

							Vector3 prevPos = transform.position;
							Quaternion prevRot = transform.rotation;
							transform.position = nextPos;
							transform.rotation = Quaternion.Euler(lookEuler);
							modelOffset = this.transform.InverseTransformPoint(prevPos);
							modelRotationOffset = Quaternion.Inverse(transform.rotation) * prevRot;

							//FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Footsteps");
						}
					}
				} else
				{
					depth = maxDistance;
					hitPoint = p1;
					hitNormal = Vector3.up;
					hitCollider = null;
					onGround = hitCollider != null;
				}
			}
		}

		//stay upright
		localUp = planetRoot ? (this.transform.position - planetRoot.position).normalized : Vector3.up;
		Vector3 cross = Vector3.Cross(this.transform.up, localUp);
		if (cross != Vector3.zero)
		{
			Quaternion turnQ = Quaternion.AngleAxis(cross.magnitude * Mathf.Rad2Deg, cross.normalized);
			this.transform.rotation = turnQ * this.transform.rotation;
		}

		if (stepTimer > 0)
		{
			if (model)
			{
				model.transform.localPosition = modelOffset * Mathf.Clamp01(stepTimer / stepTime);
				model.transform.localRotation = Quaternion.Slerp(Quaternion.identity, modelRotationOffset, Mathf.Clamp01(stepTimer / stepTime));
				//model.transform.localRotation = Quaternion.Slerp(model.transform.localRotation,
				//								Quaternion.Euler(0, Quaternion.LookRotation(-modelOffset).eulerAngles.y, 0),
				//								1.0f - Mathf.Clamp01(stepTimer / stepTime));
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		Debug.Log(other.name);
	}

	public void OnClicked(Collider coll, Vector3 relPos)
	{
		//since we clicked on, and move on, a rotating planet, the collider is important
		moveTargetValid = coll!=null;
		moveTarget = coll;
		moveRelPos = relPos;
	}

	public void OnHold(Collider coll, Vector3 relPos)
	{
		moveTargetValid = coll != null;
		moveTarget = coll;
		moveRelPos = relPos;
	}


	public void StopMove()
	{
		stopped = true;
	}

	public void Continue()
	{
		stopped = false;
	}
}

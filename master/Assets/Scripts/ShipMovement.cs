using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipMovement : MonoBehaviour
{
	public Transform planetRoot;
	public float radius;

	public bool anchored;

	public Transform tiltable;

	public Vector3 velocity;
	public Vector3 worldVel;

	public ParticleSystem waterSplosh;
	public Transform sploshTrans;
	public float emissionRateScale = 0.1f;

	public LayerMask mask;
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
	public Transform shipTransform;

	public float lateralDrag = 0.1f;
	public float tiltScale = 10.0f;

	public float max_speed = 400;
	[Range(0, 1)]
	public float drag = 0.1f;

	public Image steeringWheelUI;
	public bool hoveringOverWheel;

	public bool isSteeringDragging;
	public Vector2 screenspaceShipPoint;
	public Vector2 startDraggingDir, draggingDir;
	public float dragAngle;

	public float steerClickRadius = 10f;
	public AnimationCurve steeringSpeed = AnimationCurve.Linear(0f, 0f, Mathf.PI / 2, 1f);
	public float steeringSpeedMultiplier = 5f;
	public float steeringAngle = 0.0f;      //degrees
	public float steeringAngleToAngVel = -0.1f;
	public float steeringReturnSpeed = 30.0f;
	public float steeringSpeedKeyboard = 100.0f;

	public float steerPushScale = 1.0f;
	public float steerPushSpeed = 0.0f;
	public float steerPushForce = 1.0f;

	public float steerBaseSpeed = 1.0f;
	public float steerBaseForce = 1.0f;

	// Start is called before the first frame update
	void Start()
    {
		depth = new float[collisionPoints.Length];
		hitNormal =  new Vector3[collisionPoints.Length];
		hitCollider = new Collider[collisionPoints.Length];

		screenspaceShipPoint = RectTransformUtility.WorldToScreenPoint(null, steeringWheelUI.rectTransform.transform.position);
		//steerClickRadius = steeringWheelUI.rectTransform.rect.height * steeringWheelUI.canvas.scaleFactor;
		steerClickRadius = Screen.height * 0.2f;
	}

	[ExecuteInEditMode]
	private float SuperSpeed()
    {
		if (Input.GetKey(KeyCode.LeftShift))
        {
			Time.timeScale = 5f;
			return 0f;
        }
		else
        {
			Time.timeScale = 1f;
			return drag;
        }
    }

    // Update is called once per frame
    void Update()
    {
		float update_drag = SuperSpeed();

		if (anchored)
		{
			velocity = Vector3.zero;
			if (waterSplosh)
			{
				ParticleSystem.EmissionModule emission = waterSplosh.emission;
				emission.rateOverTimeMultiplier = 0.1f * emissionRateScale;
			}

			steeringWheelUI.gameObject.SetActive(false);
			return;
		}

		steeringWheelUI.gameObject.SetActive(true);

		worldVel = this.transform.rotation * velocity;
		//lateral drag
		worldVel -= transform.right * Vector3.Dot(worldVel, transform.right) * lateralDrag * Time.deltaTime;
		velocity = Quaternion.Inverse(this.transform.rotation) * worldVel;
		//decoration
		if (tiltable)
			tiltable.localRotation = Quaternion.Slerp(tiltable.localRotation, Quaternion.Euler(0, 0, Mathf.Clamp(-Vector3.Dot(worldVel, transform.right) * tiltScale, -45.0f, 45.0f)), Time.deltaTime / 0.1f);
		//longitudinal drag
		velocity.z = Mathf.Clamp(velocity.z, -max_speed, max_speed);
		velocity.z *= 1f - (update_drag * Time.deltaTime);

		// Player input
		Vector2 screenPoint2d = Input.mousePosition;

		//this is actually the bottom centre of the screen
		// should be the center of steeringwheelUI
		//screenspaceShipPoint = new Vector2(Camera.main.pixelRect.center.x, Camera.main.pixelRect.yMin);//Camera.main.WorldToScreenPoint(transform.position);
		//screenspaceShipPoint = steeringWheelUI.rectTransform.rect.center;// .anchoredPosition;

		if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
		{
			Debug.Log(Vector2.Distance(screenPoint2d, screenspaceShipPoint));
			if (Vector2.Distance(screenPoint2d, screenspaceShipPoint) <= steerClickRadius)
			{
				startDraggingDir = (screenPoint2d - screenspaceShipPoint).normalized;
				isSteeringDragging = true;
			}
		}

		if(isSteeringDragging && (Input.GetMouseButton(0) || Input.GetMouseButton(1)))
        {
			draggingDir = (screenPoint2d - screenspaceShipPoint).normalized;
			//Debug.Log($"{startDraggingDir}, {draggingDir}");
			dragAngle = Vector2.SignedAngle(startDraggingDir, draggingDir);// * Mathf.Deg2Rad;
			//Debug.Log(angle);
			//var angularVelocityChange = Mathf.Sign(angle) * steeringSpeed.Evaluate(Mathf.Abs(angle / Mathf.PI)) * steeringSpeedMultiplier;
			//velocity.y -= angularVelocityChange * Time.deltaTime;
			steeringAngle -= dragAngle;
			startDraggingDir = draggingDir;

			//new, test :)
			if (Mathf.Abs(dragAngle) > 0)
			{
				steerPushSpeed = Mathf.Clamp((Mathf.Abs(dragAngle) / Time.deltaTime) * steerPushScale, 0, max_speed);
				velocity.z += (steerPushSpeed - velocity.z) * steerPushForce * Time.deltaTime;
			} else
			{
				velocity.z += (steerBaseSpeed - velocity.z) * steerBaseForce * Time.deltaTime;
			}
		}
        else
        {
			startDraggingDir = Vector2.zero;
			isSteeringDragging = false;
			steerPushSpeed = 0.0f;
		}

		velocity.y = steeringAngle * steeringAngleToAngVel;
		//steering return
		if (!isSteeringDragging)
		{
			float str = steeringReturnSpeed * Time.deltaTime;
			//velocity.y += Mathf.Clamp(-velocity.y, -str, str);
			steeringAngle += Mathf.Clamp(-steeringAngle, -str, str);
		}

		// Steering wheel UI
		//hoveringOverWheel = steeringWheelUI.Raycast(screenPoint2d, null);	//this doesn't work
		hoveringOverWheel = Vector2.Distance(screenPoint2d, screenspaceShipPoint) <= steerClickRadius;
		Color wheelColor = steeringWheelUI.color;
		wheelColor.a = (hoveringOverWheel || isSteeringDragging) ? 1.0f : 0.4f;
		steeringWheelUI.color = wheelColor;

		steeringWheelUI.rectTransform.rotation = Quaternion.Euler(0, 0, -steeringAngle);

		if (Input.GetKeyDown(KeyCode.W))
			velocity.z += 5.0f;
		if (Input.GetKeyDown(KeyCode.S))
			velocity.z -= 5.0f;
		if (Input.GetKey(KeyCode.A))
			steeringAngle -= steeringSpeedKeyboard * Time.deltaTime;
		if (Input.GetKey(KeyCode.D))
			steeringAngle += steeringSpeedKeyboard * Time.deltaTime;

		//How do we "collide" with islands? we are not using physics on the ship (and really shouldn't)
		//idea: use a raycast to MEASURE DEPTH, and use that to slow down (drag the bottom) and push away :) 
		IslandCollision();

		worldVel = this.transform.rotation * velocity;
		worldVel.y = 0.0f;
		this.transform.position += worldVel * Time.deltaTime * Mathf.Deg2Rad;
		
		//eliminate numerical drift in altitude
		this.transform.position = planetRoot.position + (this.transform.position - planetRoot.position).normalized * radius;
		//stay upright
		Vector3 cross = Vector3.Cross(this.transform.up, Vector3.up);
		if (cross != Vector3.zero)
		{
			Quaternion turnQ = Quaternion.AngleAxis(cross.magnitude * Mathf.Rad2Deg, cross.normalized);
			this.transform.rotation = turnQ * this.transform.rotation;
		}

		/*
				Vector3 angVel;
				//angVel.x = -velocity.z / radius;
				//angVel.z = velocity.x / radius;
				//angVel.y = 0;// -velocity.y;
				worldVel = this.transform.rotation * velocity;

				angVel.x = -worldVel.z / radius;
				angVel.z = worldVel.x / radius;
				angVel.y = 0;// -velocity.y;


				if (angVel.magnitude > 0.001f)
				{
					Quaternion turn = Quaternion.AngleAxis(angVel.magnitude * Time.deltaTime, angVel.normalized);
					planetRoot.rotation = turn * planetRoot.rotation;
				}
		*/
		Quaternion deltaYaw = Quaternion.AngleAxis(velocity.y * Time.deltaTime, Vector3.up);
		this.transform.rotation = deltaYaw * this.transform.rotation;
		velocity = deltaYaw * velocity;

		if (waterSplosh)
		{
			//waterSplosh.emissionRate = velocity.sqrMagnitude * emissionRateScale;
			ParticleSystem.EmissionModule emission = waterSplosh.emission;
			emission.rateOverTimeMultiplier = velocity.sqrMagnitude * emissionRateScale;

			//var emitParams = new ParticleSystem.EmitParams();
			var shape = waterSplosh.shape;
			shape.position = waterSplosh.transform.InverseTransformPoint(sploshTrans.transform.position);
			shape.rotation = (Quaternion.Inverse(waterSplosh.transform.rotation) * sploshTrans.transform.rotation).eulerAngles;
		}

/*		//FOIL RAISE ABOVE WATER EFFECT PROTO
		if (velocity.z > 10) //THRESHOLD VELOCITY
		{
			//transform.position = new Vector3(0f,0.1f,0f); //RAISE LEVEL
			tiltable.localPosition = Vector3.up * 0.1f;
		}
		else
		{
			//transform.position = new Vector3(0f,0f,0f);
			tiltable.localPosition = Vector3.up * 0.0f;
		}
*/


	}

	void IslandCollision()
	{
		RaycastHit hit;
		for (int i = 0; i < collisionPoints.Length; i++)
		{
			Vector3 p1 = this.transform.TransformPoint(collisionPoints[i]);
			Vector3 dir = this.transform.TransformDirection(rayDir[i].normalized);
			p1 -= dir * maxHeight;
			if (Physics.Raycast(p1, dir, out hit, maxDistance, mask))
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

		Vector3 worldVel = this.transform.rotation * velocity;

		Vector3 F = Vector3.zero;
		for (int i = 0; i < collisionPoints.Length; i++)
		{
			if (depth[i] < maxDistance)
			{
				Vector3 normal = (hitNormal[i] - Vector3.up * hitNormal[i].y).normalized;

				Vector3 p1 = this.transform.TransformPoint(collisionPoints[i]);
				Vector3 dir = this.transform.TransformDirection(rayDir[i].normalized);
				Debug.DrawRay(p1 + dir * (depth[i] - maxHeight), normal);

				float v = Vector3.Dot(worldVel, normal);

				if (depth[i] < maxHeight + collisionDistance)
				{
					worldVel += normal * Mathf.Clamp(-v, 0, 100.0f);
					worldVel += -worldVel * 0.1f * Time.deltaTime;	//also drag so we can stop and get out
				} else
				if (depth[i] < maxHeight + dragDistance)
				{
					worldVel += -worldVel * 0.1f * Time.deltaTime;
				} else
				if (depth[i] < maxHeight + pushDistance)
				{
					float f = (maxHeight + pushDistance - depth[i]);
					worldVel += normal * f;
					worldVel.y += Vector3.Cross(normal, p1 - this.transform.position).y * f * -5.0f;
				}
			}
		}
		velocity = Quaternion.Inverse(this.transform.rotation) * worldVel;

	}
}

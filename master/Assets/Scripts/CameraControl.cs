using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CameraControl : MonoBehaviour
{
	public GameObject player;
	public ShipMovement shipMove;
	public OnFootMovement footMove;
	public Transform shipSeat;
	public PlanetTurner Tina;

	public Camera cam;
	public float CamDistance = 1.0f;
	public float CamHeight = 1.0f;
	public AnimationCurve CamZ;
	public AnimationCurve CamY;

	public float sensitivityX = 10.0f;
	public float sensitivityY = 10.0f;

	public float pitchMin = -70;
	public float pitchMax = 89;

	public Vector3 euler;
	[Range(-1, 1)]
	public float pitch = 0.0f;

	public float mx, my;

	public Vector3 shoreDetector = Vector3.forward;
	public LayerMask mask;
	public float maxHeight = 1.0f;
	public float maxDistance = 1.2f;
	public float depth;
	public Vector3 hitPoint;
	public Vector3 hitNormal;
	public Collider hitCollider;
	public bool canDisembark;

	public bool canEmbark = true;

	public Button disembark;
	public Button embark;

	public enum Mode
	{
		ShipNav,
		LandWalk,
		SkyDraw
	}
	public Mode mode = Mode.ShipNav;



	// Start is called before the first frame update
	void Start()
    {
		euler = transform.rotation.eulerAngles;
		UpdateCam();

		disembark.onClick.AddListener(this.OnPressDisembark);
		embark.onClick.AddListener(this.OnPressEmbark);

		footMove.camCon = this;

		if (mode == Mode.ShipNav)
		{// Start on the boat; attach the player
			mode = Mode.LandWalk;
			canEmbark = true;
			OnPressEmbark();
		} else
		{
			player.transform.SetParent(Tina.planetRoot, true);
			shipMove.anchored = true;
			Tina.whatToFollow = player.transform;
		}
	}

	// Update is called once per frame
	void Update()
    {
		//ToDo: on foot controls
		// detect if we can get off the boat (say, depth test just off the boat in the view direction),
		// detect if we can get back on,
		// activate ship controls on onfoot controls

		switch(mode)
		{
			case Mode.ShipNav:
				DetectShore();
				disembark.interactable = canDisembark;
				embark.interactable = false;
				break;
			case Mode.LandWalk:
				disembark.interactable = false;
				embark.interactable = true;
				break;
		}

		if (Cursor.lockState == CursorLockMode.Locked)
		{
			float x = Input.GetAxisRaw("Mouse X");
			float y = Input.GetAxisRaw("Mouse Y");

			//Vector3 euler = transform.rotation.eulerAngles;
			if (euler.x > 90.0f) euler.x -= 360.0f;
			euler.y += x * sensitivityX;
			euler.x -= y * sensitivityY;
			euler.y = (euler.y + 360.0f) % 360.0f;
			euler.x = Mathf.Clamp(euler.x, pitchMin, pitchMax);
			transform.rotation = Quaternion.Euler(euler);

			//pitch = euler.x / 90.0f;
			//Vector3 camPos = cam.transform.localPosition;
			//camPos.z = CamZ.Evaluate(pitch) * CamDistance;
			//camPos.y = CamY.Evaluate(pitch) * CamHeight;
			//cam.transform.localPosition = camPos;
			UpdateCam();

			if (Input.GetMouseButtonUp(0))
			{
				//release
				Cursor.lockState = CursorLockMode.None;
				//ToDo: restore mousepos
				//https://answers.unity.com/questions/330661/setting-the-mouse-position-to-specific-coordinates.html
			}
		} else
		{
			transform.rotation = Quaternion.Euler(euler);

			//ToDo: check if it's over UI!
			// ...or anything we might want to click on
			// and ONLY take the mouse if not
			if (EventSystem.current.IsPointerOverGameObject())
			{

			} else
			if (Input.GetMouseButtonDown(0))
			{
				//ToDo: save mousepos
				Cursor.lockState = CursorLockMode.Locked;
			}
		}
	}

	void UpdateCam()
	{
		Vector3 euler = transform.rotation.eulerAngles;
		if (euler.x > 90.0f) euler.x -= 360.0f;

		pitch = euler.x / 90.0f;
		Vector3 camPos = cam.transform.localPosition;
		camPos.z = CamZ.Evaluate(pitch) * CamDistance;
		camPos.y = CamY.Evaluate(pitch) * CamHeight;
		cam.transform.localPosition = camPos;
	}

	void DetectShore()
	{
		RaycastHit hit;

		Vector3 p1 = this.transform.TransformPoint(shoreDetector);
		Vector3 dir = this.transform.TransformDirection(Vector3.down);
		p1 -= dir * maxHeight;
		if (Physics.Raycast(p1, dir, out hit, maxDistance, mask))
		{
			depth = hit.distance;
			hitPoint = hit.point;
			hitNormal = hit.normal;
			hitCollider = hit.collider;
		} else
		{
			depth = maxDistance;
			hitPoint = p1;
			hitNormal = Vector3.up;
			hitCollider = null;
		}

		canDisembark = depth < maxHeight;
		Debug.DrawLine(p1, p1 + dir * depth, depth < maxHeight ? Color.green : Color.red);
	}

	public void OnPressDisembark()
	{
		if (mode == Mode.ShipNav && canDisembark)
		{
			mode = Mode.LandWalk;
			player.transform.SetParent(Tina.planetRoot, true);
			player.transform.position = hitPoint;
			player.transform.rotation = this.transform.rotation;
			shipMove.anchored = true;
			Tina.whatToFollow = player.transform;
		}
	}

	public void OnPressEmbark()
	{
		if (mode == Mode.LandWalk && canEmbark)
		{
			mode = Mode.ShipNav;
			player.transform.SetParent(shipSeat, true);
			player.transform.position = shipSeat.position;
			player.transform.rotation = shipSeat.rotation;
			shipMove.anchored = false;
			Tina.whatToFollow = shipMove.transform;
		}
	}
}

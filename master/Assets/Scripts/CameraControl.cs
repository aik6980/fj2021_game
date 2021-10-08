using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using System.Runtime.InteropServices;

public class CameraControl : MonoBehaviour
{
	public static CameraControl singleton;

	[DllImport("user32.dll")]
	static extern bool SetCursorPos(int X, int Y);

	public GameObject player;
	public ShipMovement shipMove;
	public OnFootMovement footMove;
	public Transform shipSeat;
	public PlanetTurner Tina;
	public Collider embarkTrigger;
	public FollowCamera follower;

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
	public float msx, msy;
	public Vector3 mousePosPress;
	public float mousePressTime = 0;

	public Vector3 shoreDetector = Vector3.forward;
	public LayerMask mask;
	public float maxHeight = 1.0f;
	public float maxDisembarkDepth = 1.0f;
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
		LandWalk
		//SkyDraw
	}
	public Mode mode = Mode.ShipNav;

	public float clickRange = 1.0f;
	public LayerMask clickMask;

	public float baseFOV;

	public bool lookTargetValid;
	public bool lookTargetIsRoot;
	public Transform lookTarget;
	public float lookStrength = 0.0f;
	public float targetFOV;
	//public Vector3 lookEuler;

	//public bool moveTargetValid;
	//public Transform moveTarget;
	//public float moveStrength = 0.0f;

	public ConstellationMgr constellationManager;


	private void Awake()
	{
		singleton = this;
	}

	// Start is called before the first frame update
	void Start()
    {
		follower = GetComponent<FollowCamera>();

		euler = transform.rotation.eulerAngles;
		UpdateCam();

		disembark.onClick.AddListener(this.OnPressDisembark);
		embark.onClick.AddListener(this.OnPressEmbark);

		footMove.camCon = this;
		shipMove.planetRoot = Tina.planetRoot;

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

		if (!embarkTrigger)
			embarkTrigger = shipMove.transform.Find("EmbarkTrigger").GetComponent<Collider>();

		EventListener.Get(player).OnTriggerEnterDelegate += CameraControl_OnTriggerEnterDelegate;
		EventListener.Get(player).OnTriggerExitDelegate += CameraControl_OnTriggerExitDelegate;

		baseFOV = cam.fieldOfView;
	}

	private void CameraControl_OnTriggerExitDelegate(Collider col)
	{
		if (col == embarkTrigger)
			canEmbark = false;
	}

	private void CameraControl_OnTriggerEnterDelegate(Collider col)
	{
		//Debug.Log("OTE " + col.name);
		if (col == embarkTrigger)
			canEmbark = true;
	}

	// Update is called once per frame
	public void Update()
    {
		//ToDo: on foot controls
		// detect if we can get off the boat (say, depth test just off the boat in the view direction),
		// detect if we can get back on,
		// activate ship controls on onfoot controls


		if (lookTargetValid)
		{   //mini "cutscene", look at and optionally zoom in
			float f = Mathf.Clamp01(lookStrength * Time.deltaTime);
			Vector3 fwd = lookTargetIsRoot ? lookTarget.position - this.transform.position : lookTarget.position - cam.transform.position;
			Vector3 xy = fwd;
			xy.z = 0.0f;
			float q = Mathf.Clamp01(xy.magnitude / fwd.magnitude);
			Vector3 up = Vector3.Lerp(transform.up, Vector3.up, q*q);
			Quaternion look = Quaternion.LookRotation(fwd, up);
			//lookEuler = look.eulerAngles;
			//euler.x = Mathf.LerpAngle(euler.x, look.eulerAngles.x, f);
			//euler.y = Mathf.LerpAngle(euler.y, look.eulerAngles.y, f);
			//transform.rotation = Quaternion.Euler(euler);
			transform.rotation = Quaternion.Slerp(transform.rotation, look, f);

			cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, f);

			UpdateCam();
		}

		//Debug.Log(ConstellationMgr.Instance.is_canvas_mode_enabled());
		if (ConstellationMgr.Instance.is_canvas_mode_enabled())
		{
			disembark.gameObject.SetActive(false);
			embark.gameObject.SetActive(false);
			if (Cursor.lockState == CursorLockMode.Locked)
			{
				Cursor.lockState = CursorLockMode.None;
				mousePressTime = 0;
			}
			return;
		}

		if (lookTargetValid)
			return;

		switch (mode)
		{
			case Mode.ShipNav:
				DetectShore();
				disembark.interactable = canDisembark;
				embark.interactable = false;
				disembark.gameObject.SetActive(canDisembark);
				embark.gameObject.SetActive(false);
				break;
			case Mode.LandWalk:
				disembark.interactable = false;
				embark.interactable = canEmbark;
				disembark.gameObject.SetActive(false);
				embark.gameObject.SetActive(canEmbark);
				break;
		}

		mx = Input.GetAxisRaw("Mouse X");
		my = Input.GetAxisRaw("Mouse Y");

		if (!lookTargetValid && cam.fieldOfView != baseFOV)
			cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, baseFOV, 1.0f * Time.deltaTime);

		//euler = transform.rotation.eulerAngles;
		Vector3 localUp = (this.transform.position - Tina.planetRoot.position).normalized;
		Quaternion noRoll = Quaternion.LookRotation(transform.forward, localUp);
		euler = noRoll.eulerAngles;


		if (Cursor.lockState == CursorLockMode.Locked)
		{
			//float x = Input.GetAxisRaw("Mouse X");
			//float y = Input.GetAxisRaw("Mouse Y");

			//Vector3 euler = transform.rotation.eulerAngles;
			if (euler.x > 90.0f) euler.x -= 360.0f;
			euler.y += mx * sensitivityX;
			euler.x -= my * sensitivityY;
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
				//this expects fullscreen (windows) pixel coordinates (top left->right&down)
				//SetCursorPos((int)msx, (int)msy);
				mousePressTime = 0;
			}
		} else
		{
			transform.rotation = Quaternion.Euler(euler);

			//ToDo: check if it's over UI!
			// ...or anything we might want to click on
			// and ONLY take the mouse if not
			if (shipMove.isSteeringDragging)
			{
				euler.y = Mathf.LerpAngle(euler.y, shipMove.transform.rotation.eulerAngles.y, 2.0f * Time.deltaTime);
				transform.rotation = Quaternion.Euler(euler);
				UpdateCam();
			} else
			if (EventSystem.current.IsPointerOverGameObject())
			{

			} else
			{
				if (Input.GetMouseButtonDown(0))
					mousePressTime = 0;

				if (Input.GetMouseButton(0))
				{
					//ToDo: save mousepos
					// these are pixel coordinates in window space (bottom left->right&up)
					if (mousePressTime > 0 && (Input.mousePosition - mousePosPress).sqrMagnitude > 1.0f)
					{
						Cursor.lockState = CursorLockMode.Locked;
					} else
					{//holding without moving; detect a click and pass it to onfoot movement
						if (mousePressTime >= 0.2f)
							OnMouseHold(Input.mousePosition);
					}

					mousePosPress = Input.mousePosition;
					msx = mousePosPress.x;
					msy = mousePosPress.y;

					mousePressTime += Time.deltaTime;
				} else
				{
					if (mousePressTime > 0)
					{
						Debug.Log("MPT " + mousePressTime);
						if (mousePressTime < 0.2f)
						{//click (as in, not hold)
							OnMouseClick(Input.mousePosition);
						}
						mousePressTime = 0;
					}
				}
			}
		}
	}

	void OnMouseClick(Vector3 mousePos)
	{
		RaycastHit hit;

		Ray ray = cam.ScreenPointToRay(mousePos);
		Vector3 p1 = ray.origin;
		Vector3 dir = ray.direction;
		Debug.DrawRay(p1, dir * clickRange, Color.yellow, 1.0f);
		if (Physics.Raycast(p1, dir, out hit, clickRange, clickMask))
		{
			Debug.Log(hit.point);
			if (mode == Mode.LandWalk)
			{
				footMove.OnClicked(hit.collider, hit.collider.transform.InverseTransformPoint(hit.point));
				AudioManager.Instance.PlaySFX("footstep");
			}
		}
	}

	void OnMouseHold(Vector3 mousePos)
	{
		RaycastHit hit;

		Ray ray = cam.ScreenPointToRay(mousePos);
		Vector3 p1 = ray.origin;
		Vector3 dir = ray.direction;
		Debug.DrawRay(p1, dir * clickRange, Color.yellow, 0.0f);
		if (Physics.Raycast(p1, dir, out hit, clickRange, clickMask))
		{
			Debug.Log(hit.point);
			if (mode == Mode.LandWalk)
			{
				footMove.OnHold(hit.collider, hit.collider.transform.InverseTransformPoint(hit.point));
				AudioManager.Instance.PlaySFX("footstep");
			}
		}
	}

	public void UpdateCam()
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

		Vector3 euler = this.transform.rotation.eulerAngles;
		euler.x = euler.z = 0.0f;

		Vector3 p1 = this.transform.position + Quaternion.Euler(euler) * shoreDetector;// this.transform.TransformPoint(shoreDetector);
		Vector3 dir = Vector3.down; //this.transform.TransformDirection(Vector3.down);
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

		canDisembark = depth < maxDisembarkDepth;
		//ToDo: check with onfootmovement if it also accepts it
		{
			Vector3 localUp = (hitPoint - Tina.planetRoot.position).normalized;
			float groundSlopeThere = Mathf.Acos(Vector3.Dot(hitNormal, localUp)) * Mathf.Rad2Deg;

			//do not step on too steep ground
			if (groundSlopeThere >= footMove.maxGroundSlopeUp)
			{//too steep
				//Debug.Log("too steep");
				canDisembark = false;
			} else
			//do not step DOWN into water (but allow coming up out)
			if ((hitPoint - Tina.planetRoot.position).magnitude < footMove.seaLevelMin) //not accurate enough but cheap early test
			{
				//Debug.Log("water");
				canDisembark = false;
			}
		}
		Debug.DrawLine(p1, p1 + dir * depth, canDisembark ? Color.green : Color.red);
	}

	public void OnPressDisembark()
	{
		if (mode == Mode.ShipNav && canDisembark)
		{
			mode = Mode.LandWalk;
			player.transform.SetParent(Tina.planetRoot, true);
			player.transform.position = hitPoint + Vector3.up * footMove.walkLevel;
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

	public void ZoomTo(Transform target)
    {
		lookTarget = target;
		lookStrength = 1f;
		targetFOV = baseFOV;
		lookTargetValid = true;
	}

	public void LookAt(Transform target, float strength = 2.0f, float FoV = 30.0f, bool raw = false)
	{
		if (strength < 0)
		{//instant
			Quaternion look = raw
							? Quaternion.LookRotation(target.position - this.transform.position)
							: Quaternion.LookRotation(target.position - cam.transform.position);
			euler.x = look.eulerAngles.x;
			euler.y = look.eulerAngles.y;
			transform.rotation = Quaternion.Euler(euler);
			if (FoV > 0)
				cam.fieldOfView = FoV;
			UpdateCam();
			return;
		}

		lookTargetValid = target != null;
		lookTarget = target;
		lookStrength = strength;
		lookTargetIsRoot = raw;
		targetFOV = FoV > 0 ? FoV : baseFOV * 0.5f;
	}

	public void StopLook()
	{
		lookTargetValid = false;
	}

	public void StopMove()
	{
		follower.enabled = false;
	}
	public void Continue()
	{
		follower.enabled = true;
	}
	public void Follow(Transform target)
	{
		follower.target = target;
		follower.enabled = true;
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
	public Camera cam;
	public float CamDistance = 1.0f;
	public float CamHeight = 1.0f;
	public AnimationCurve CamZ;
	public AnimationCurve CamY;

	public float sensitivityX = 10.0f;
	public float sensitivityY = 10.0f;

	public float pitchMin = -70;
	public float pitchMax = 89;

	[Range(-1, 1)]
	public float pitch = 0.0f;

	public float mx, my;

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		if (Cursor.lockState == CursorLockMode.Locked)
		{
			float x = Input.GetAxisRaw("Mouse X");
			float y = Input.GetAxisRaw("Mouse Y");

			Vector3 euler = transform.rotation.eulerAngles;
			if (euler.x > 90.0f) euler.x -= 360.0f;
			euler.y += x * sensitivityX;
			euler.x -= y * sensitivityY;
			euler.y = (euler.y + 360.0f) % 360.0f;
			euler.x = Mathf.Clamp(euler.x, pitchMin, pitchMax);
			transform.rotation = Quaternion.Euler(euler);

			pitch = euler.x / 90.0f;
			Vector3 camPos = cam.transform.localPosition;
			camPos.z = CamZ.Evaluate(pitch) * CamDistance;
			camPos.y = CamY.Evaluate(pitch) * CamHeight;
			cam.transform.localPosition = camPos;

			if (Input.GetMouseButtonUp(0))
			{
				//release
				Cursor.lockState = CursorLockMode.None;
				//ToDo: restore mousepos
				//https://answers.unity.com/questions/330661/setting-the-mouse-position-to-specific-coordinates.html
			}
		} else
		{
			if (Input.GetMouseButtonDown(0))
			{
				//ToDo: save mousepos
				Cursor.lockState = CursorLockMode.Locked;
			}
		}
	}
}

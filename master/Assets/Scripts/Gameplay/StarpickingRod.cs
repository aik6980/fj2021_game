using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StarpickingRod : MonoBehaviour {

    public CameraControl m_camera_control;

  // Start is called before the first frame update
  void Start() 
  {

  }

  // Update is called once per frame
  void Update() 
  {

        if(ConstellationMgr.Instance.is_canvas_mode_enabled())
        {
            // disable main camera control
            m_camera_control.enabled = false;

            // enable picking rod
            var rod_collider = GetComponent<Collider>();
            rod_collider.enabled = true;


            var mouse_pos = Input.mousePosition;
            var ray = Camera.main.ScreenPointToRay(mouse_pos);

            transform.position = ray.origin;
            transform.rotation = Quaternion.LookRotation(ray.direction);
        }
        else
        {
            // enable main cam
            m_camera_control.enabled = true;

            // disbale rod
            var rod_collider = GetComponent<Collider>();
            rod_collider.enabled = false;
        }
  }

  public void ToggleConstellationCanvas() 
  {
        bool toggle_constellation_canvas = !ConstellationMgr.Instance.is_canvas_mode_enabled();
        ConstellationMgr.Instance.enable_canvas_mode(toggle_constellation_canvas);
  }
}

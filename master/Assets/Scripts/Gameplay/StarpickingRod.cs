using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StarpickingRod : MonoBehaviour {

    public CameraControl m_camera_control;

    bool m_enable_constellation_canvas = false;

  // Start is called before the first frame update
  void Start() 
  {
        m_enable_constellation_canvas = false;
  }

  // Update is called once per frame
  void Update() 
  {

        if(m_enable_constellation_canvas)
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
        m_enable_constellation_canvas = !m_enable_constellation_canvas;
  }
}

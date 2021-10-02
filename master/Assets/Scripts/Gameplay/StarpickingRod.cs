using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarpickingRod : MonoBehaviour {
  // Start is called before the first frame update
  void Start() {}

  // Update is called once per frame
  void Update() {
    var mouse_pos = Input.mousePosition;
    var ray = Camera.main.ScreenPointToRay(mouse_pos);

    transform.position = ray.origin;
    transform.rotation = Quaternion.LookRotation(ray.direction);
  }
}

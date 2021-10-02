using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnFootMovement : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //ToDo: point-and click (detect if i clicked on valid ground and move to where)
		// use depth tests to avoid walking into the water
		// detect encounters (probably triggers on landmarks, collider on me) and trigger narrative
		// must work nicely with cameracontrol and UI
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShip : MonoBehaviour
{
    [Header("Properties")]
    public float steerRadius = 10f;
    public int sailModes = 3;
    public float maxSailSpeed = 10f;

    [Header("References")]
    public GameObject sailGameObject;
    public GameObject environmentGameObject;

    private new Camera camera;
    private Vector3? rotateActionMouseDownPosition;
    private int sailThrottle = 0;

    void Start()
    {
        camera = Camera.main;
    }

    void Update()
    {
        HandleSteering();
        HandleSailThrottle();
        HandleMovement();
    }

    private void HandleSteering()
    {
        if(!Input.GetMouseButton(0))
        {
            rotateActionMouseDownPosition = null;
            return;
        }

        var ray = camera.ScreenPointToRay(Input.mousePosition);

        var plane = GetEnvironmentPlane();
        if (plane.Raycast(ray, out float enter))
        {
            var pos = ray.GetPoint(enter);
            if (Vector3.Distance(transform.position, pos) <= steerRadius)
            {
                var dirToLastPos = (rotateActionMouseDownPosition ?? pos - transform.position).normalized;
                var dirToCurrentPos = (pos - transform.position).normalized;

                var angle = Vector3.SignedAngle(dirToLastPos, dirToCurrentPos, plane.normal);
                transform.Rotate(Vector3.up, angle);

                rotateActionMouseDownPosition = pos;
            }
        }
    }

    private void HandleSailThrottle()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = camera.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out RaycastHit hit))
            {
                var playerShip = hit.collider.GetComponentInParent<PlayerShip>();
                if(playerShip)
                {
                    sailThrottle = (sailThrottle + 1) % sailModes;
                }
            }
        }

        var scale = sailGameObject.transform.localScale;
        scale.z = 0.1f + 0.2f * sailThrottle;
        sailGameObject.transform.localScale = scale;
    }

    private void HandleMovement()
    {
        //TODO: Smooth velocity
        var velocity = transform.forward * sailThrottle * (maxSailSpeed / sailModes);
        environmentGameObject.transform.Translate(-velocity * Time.deltaTime);
    }

    /// <summary>
    /// Calculate the plane which we use for mouse interactions.
    /// 
    /// Currently this is the ship's up normal. 
    /// But in case we want to add some sway to the ship this is the custimisation point to define a more consistent interaction plane.
    /// </summary>
    /// <returns>The plane to be used for mouse interactions for the ship</returns>
    private Plane GetEnvironmentPlane()
    {
        return new Plane(transform.up, transform.position);
    }
}

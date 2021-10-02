using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShip : MonoBehaviour
{
    [Header("Properties")]
    public float steerClickRadius = 10f;
    public float steeringInputDampening = 0.8f;
    public float steeringSpeedMultiplier = 500f;
    public AnimationCurve steeringSpeedCurve = AnimationCurve.Linear(0, 20, 0, 1);
    public int sailModes = 3;
    public float maxSailSpeed = 10f;
    public float dragSquaredCoeffecient = 1f;
    public float dragLinearDampening = 0.1f;
    public float dragSidewaysCoefficient = 1f;

    [Header("References")]
    public GameObject sailGameObject;
    public GameObject environmentGameObject;

    private new Camera camera;
    private Vector3? rotateActionMouseDownPosition;
    private int sailThrottle = 0;
    private Vector3 velocity;
    private Vector3 targetDirection;


    void Start()
    {
        camera = Camera.main;
        targetDirection = transform.forward;
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
            targetDirection = transform.forward;
            return;
        }

        var ray = camera.ScreenPointToRay(Input.mousePosition);

        var plane = GetEnvironmentPlane();
        if (plane.Raycast(ray, out float enter))
        {
            var pos = ray.GetPoint(enter);
            if (Vector3.Distance(transform.position, pos) <= steerClickRadius)
            {
                var dirToLastPos = (rotateActionMouseDownPosition ?? pos - transform.position).normalized;
                var dirToCurrentPos = (pos - transform.position).normalized;

                var angle = Vector3.SignedAngle(dirToLastPos, dirToCurrentPos, plane.normal);
                targetDirection = Quaternion.AngleAxis(angle * steeringInputDampening, Vector3.up) * targetDirection;
                //transform.Rotate(Vector3.up, angle * steeringSpeed);

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
        scale.z = 1f + 0.2f * sailThrottle;
        sailGameObject.transform.localScale = scale;
    }

    private void HandleMovement()
    {
        var forwardVelocity = Vector3.Project(velocity, transform.forward);
        Debug.DrawLine(transform.position, transform.position + velocity * 5f, Color.red, 0.2f);
        Debug.DrawLine(transform.position, transform.position + forwardVelocity * 5f, Color.green, 0.2f);
        var forwardVelocityMag = Vector3.Project(velocity, transform.forward).magnitude;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(targetDirection, Vector3.up), steeringSpeedMultiplier * steeringSpeedCurve.Evaluate(forwardVelocityMag) * Time.deltaTime);

        var sidewaysDrag = velocity.magnitude * dragSidewaysCoefficient *(1f - Mathf.Abs(Vector3.Dot(velocity.normalized, transform.forward))) * Time.deltaTime;
        var drag = (Vector3.Magnitude(velocity) + sidewaysDrag) * dragLinearDampening + 0.5f * (Vector3.SqrMagnitude(velocity) + sidewaysDrag) * dragSquaredCoeffecient;
        velocity -= velocity * Mathf.Clamp01(drag);
        velocity += transform.forward * sailThrottle * (maxSailSpeed / sailModes) * Time.deltaTime;

        environmentGameObject.transform.Translate(-velocity * Time.deltaTime);
    }

    /// <summary>
    /// Calculate the plane which we use for mouse interactions.
    /// 
    /// Currently this is the ship's up normal. 
    /// But in case we want to add some sway to the ship this is the customization point to define a more consistent interaction plane.
    /// </summary>
    /// <returns>The plane to be used for mouse interactions for the ship</returns>
    private Plane GetEnvironmentPlane()
    {
        return new Plane(transform.up, transform.position);
    }
}

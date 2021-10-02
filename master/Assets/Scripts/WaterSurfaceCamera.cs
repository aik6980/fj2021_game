using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSurfaceCamera : MonoBehaviour
{
    public MeshRenderer water_surface;
    private Camera ortho_camera;

    void Awake()
    {
        ortho_camera = GetComponent<Camera>();
    }

    void Update()
    {
        water_surface.sharedMaterial.SetVector("_CamPosition", transform.position);
        water_surface.sharedMaterial.SetFloat("_OrthograhicCamSize", ortho_camera.orthographicSize);
    }
}

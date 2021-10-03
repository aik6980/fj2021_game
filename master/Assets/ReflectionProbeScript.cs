using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReflectionProbeScript : MonoBehaviour
{
    public RenderTexture probe_target;
    public MeshRenderer water_surface;
    private ReflectionProbe probe;

    void Start()
    {
        probe = GetComponent<ReflectionProbe>();
    }

    void Update()
    {
        probe.RenderProbe(probe_target);
        //water_surface.sharedMaterial.SetTexture("_Cube", probe.texture);
    }
}

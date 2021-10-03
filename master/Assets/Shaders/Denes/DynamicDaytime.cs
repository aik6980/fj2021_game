using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicDaytime : MonoBehaviour
{
    public Light sun;
    public ParticleSystem stars;
    public ParticleSystem clouds;

    public float dayTimer;
    public float dayTime = 20.0f;   //seconds
    public Gradient ambientColour;
    public Gradient fogColour;

    // Use this for initialization
    void Start ()
    {
		
	}
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        dayTimer += Time.fixedDeltaTime / dayTime;
        dayTimer = dayTimer % 1.0f;
        sun.transform.rotation = Quaternion.AngleAxis(360.0f * Time.fixedDeltaTime / dayTime, Vector3.right) * sun.transform.rotation;

        Color amb = ambientColour.Evaluate(dayTimer);
        RenderSettings.ambientIntensity = amb.a;
        RenderSettings.reflectionIntensity = amb.a;
        RenderSettings.fogColor = fogColour.Evaluate(dayTimer);
    }
}

using UnityEngine;
using System.Collections;

public class DayLight : MonoBehaviour 
{
	public float dayLength = 60.0f;

	public Transform sunRoot;
	public Light sunLight;
	public float sunIntensityMax = 1.0f;
	public Quaternion sunStartRotation = Quaternion.identity;
	public Vector3 axis = Vector3.right;

	//public Transform skyLight;

	public float normalisedDayTime;
	public Gradient sunColour;
	public AnimationCurve sunIntensity;
	public AnimationCurve ambientIntensity;
	public float ambientIntensityMax = 1.0f;

	public Gradient fogColour;
	public AnimationCurve fogIntensity;

	public float moonOrbitLength = 65.0f;
	public Transform moonRoot;
	public Light moonLight;
	public float normalisedMoonTime;
	public Quaternion moonStartRotation = Quaternion.identity;


	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (dayLength > 0) 
		{
			normalisedDayTime += Time.deltaTime / dayLength;
			normalisedDayTime = normalisedDayTime % 1.0f;
		}

		if (moonOrbitLength > 0)
		{
			normalisedMoonTime += Time.deltaTime / moonOrbitLength;
			normalisedMoonTime = normalisedMoonTime % 1.0f;
		}

		if (sunRoot)
		{
			sunRoot.localRotation = Quaternion.AngleAxis(360.0f * normalisedDayTime, axis) * sunStartRotation;
		}
		if (sunLight)
		{
			sunLight.color = sunColour.Evaluate(normalisedDayTime);
			sunLight.intensity = sunIntensity.Evaluate(normalisedDayTime) * sunIntensityMax;
		}
		RenderSettings.ambientIntensity = ambientIntensity.Evaluate(normalisedDayTime) * ambientIntensityMax;

		RenderSettings.fogColor = fogColour.Evaluate(normalisedDayTime);

		if (moonRoot)
		{
			moonRoot.localRotation = Quaternion.AngleAxis(360.0f * normalisedMoonTime, axis) * moonStartRotation;
		}

	}
}

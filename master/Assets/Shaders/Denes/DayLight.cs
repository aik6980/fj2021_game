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

	[Range(0, 1)]
	public float normalisedRotationTime;
	[Range(0, 1)]
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

	//debug
	public float dot;
	public float dot2;

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (dayLength > 0) 
		{
			normalisedRotationTime += Time.deltaTime / dayLength;
			normalisedRotationTime = normalisedRotationTime % 1.0f;
		}
		if (sunRoot)
		{
			sunRoot.localRotation = Quaternion.AngleAxis(360.0f * normalisedRotationTime, axis) * sunStartRotation;
		}


		if (sunLight)
		{
			dot = Vector3.Dot(sunLight.transform.forward, Vector3.down);
			dot2 = Vector3.Dot(Vector3.Cross(sunLight.transform.forward, sunRoot.rotation * axis), Vector3.up);
			normalisedDayTime = dot2 > 0 ? 0.25f + dot * 0.25f : 0.75f - dot * 0.25f;

			sunLight.color = sunColour.Evaluate(normalisedDayTime);
			sunLight.intensity = sunIntensity.Evaluate(normalisedDayTime) * sunIntensityMax;
		} else
		{
			normalisedDayTime = normalisedRotationTime;
		}

		RenderSettings.ambientIntensity = ambientIntensity.Evaluate(normalisedDayTime) * ambientIntensityMax;

		RenderSettings.fogColor = fogColour.Evaluate(normalisedDayTime);

		if (moonOrbitLength > 0)
		{
			normalisedMoonTime += Time.deltaTime / moonOrbitLength;
			normalisedMoonTime = normalisedMoonTime % 1.0f;
		}

		if (moonRoot)
		{
			moonRoot.localRotation = Quaternion.AngleAxis(360.0f * normalisedMoonTime, axis) * moonStartRotation;
		}

	}
}

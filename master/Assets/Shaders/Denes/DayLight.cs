using UnityEngine;
using System.Collections;

public class DayLight : MonoBehaviour 
{
	public float dayLength;

	public Transform sun;
	public float sunIntensityMax = 1.0f;
	public Quaternion sunStartRotation = Quaternion.identity;
	public Vector3 axis = Vector3.right;


	public Transform skyLight;

	public float normalisedDayTime;
	public Gradient sunColour;
	public AnimationCurve sunIntensity;
	public AnimationCurve ambientIntensity;
	public float ambientIntensityMax = 1.0f;

	public Gradient fogColour;
	public AnimationCurve fogIntensity;

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

		//transform.Rotate (Vector3.right, 360 / (dayLength * 60));
		sun.rotation = Quaternion.AngleAxis(360.0f * normalisedDayTime, axis) * sunStartRotation;
		sun.GetComponent<Light> ().color = sunColour.Evaluate(normalisedDayTime);
		sun.GetComponent<Light> ().intensity = sunIntensity.Evaluate(normalisedDayTime) * sunIntensityMax;
		RenderSettings.ambientIntensity = ambientIntensity.Evaluate(normalisedDayTime) * ambientIntensityMax;

		RenderSettings.fogColor = fogColour.Evaluate(normalisedDayTime);
	}
}

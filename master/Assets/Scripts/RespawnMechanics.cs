using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnMechanics : MonoBehaviour
{
	public GameObject player;
	public GameObject ship;

	public Vector3 respawnLocation_Player;
	public Vector3 respawnLocation_Ship;
	public Quaternion respawnRotation_Player;
	public Quaternion respawnRotation_Ship;

	public float ageYearLengthInSeconds = 5;	//~8 minutes to reach 100 years
	public float currentAge = 0;
	public float maxAge = 100;

	public int years = 0;

	private void Awake()
	{
		if (player)
		{
			respawnLocation_Player = player.transform.localPosition;
			respawnRotation_Player = player.transform.localRotation;
		}
		if (ship)
		{
			respawnLocation_Ship = ship.transform.localPosition;
			respawnRotation_Ship = ship.transform.localRotation;
		}
	}


	// Start is called before the first frame update
	void Start()
    {
		if (player)
			EventListener.Get(player).OnTriggerEnterDelegate += RespawnMechanics_OnTriggerEnterDelegate;

	}

	private void RespawnMechanics_OnTriggerEnterDelegate(Collider col)
	{
	}

	// Update is called once per frame
	void Update()
    {
		currentAge += Time.deltaTime / ageYearLengthInSeconds;

		int _years = (int)currentAge;
		if (_years != years)
		{
			years = _years;
			StoryTeller.singleton.fc.SendFungusMessage("Age_" + years.ToString());
		}

		//debug
		if (Input.GetKeyDown(KeyCode.T) && Input.GetKey(KeyCode.LeftControl))
		{
			ExecuteRespawn();
		}

		if (currentAge >= maxAge)
		{
			ExecuteRespawn();
		}
    }

	public void SaveRespawnPoint()
	{
		Debug.Log("SaveRespawnPoint");

		if (player)
		{
			respawnLocation_Player = player.transform.localPosition;
			respawnRotation_Player = player.transform.localRotation;
		}
		if (ship)
		{
			respawnLocation_Ship = ship.transform.localPosition;
			respawnRotation_Ship = ship.transform.localRotation;
		}
	}

	public void ExecuteRespawn()
	{
		Debug.Log("Respawn");
		StartCoroutine(RespawnProcess());
	}

	public IEnumerator RespawnProcess()
	{
		//ToDo:
		//notify fungus
		StoryTeller.singleton.fc.SendFungusMessage("Died");

		//FADE DOWN
		yield return new WaitForSeconds(3.0f);
		//notify fungus (it should actually come back FROM it)
		StoryTeller.singleton.fc.SendFungusMessage("RespawnFadedDown");
		
		//RESPAWN,
		PlanetTurner Tina = PlanetTurner.singleton;

		CameraControl camcon = CameraControl.singleton;
		camcon.mode = CameraControl.Mode.LandWalk;

		if (player)
		{
			OnFootMovement footMove = player.GetComponent<OnFootMovement>();
			player.transform.SetParent(Tina.planetRoot, true);
			player.transform.localPosition = respawnLocation_Player;
			player.transform.localRotation = respawnRotation_Player;
		}
		if (ship)
		{
			ship.transform.localPosition = respawnLocation_Ship;
			ship.transform.localRotation = respawnRotation_Ship;

			ShipMovement shipMove = ship.GetComponent<ShipMovement>();
			shipMove.anchored = true;
		}

		Tina.whatToFollow = player.transform;
		Tina.Update();

		//there should be a function to do this
		camcon.transform.localPosition = player.transform.localPosition;
		camcon.transform.localRotation = player.transform.localRotation;

		camcon.euler = camcon.transform.rotation.eulerAngles;

		Cursor.lockState = CursorLockMode.Locked;
		camcon.Update();
		camcon.UpdateCam();
		Cursor.lockState = CursorLockMode.None;

		Debug.Log(camcon.euler);
		Debug.Log(camcon.transform.localRotation.eulerAngles);

		currentAge = 0;
		years = (int)currentAge;

		//notify fungus
		StoryTeller.singleton.fc.SendFungusMessage("Respawned");

		//FADE UP
		yield return new WaitForSeconds(3.0f);
		//notify fungus
		StoryTeller.singleton.fc.SendFungusMessage("RespawFadedUp");

		yield return new WaitForSeconds(.1f);
	}
}

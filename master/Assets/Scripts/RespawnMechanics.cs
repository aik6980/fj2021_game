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

	private void Awake()
	{
		if (player)
		{
			respawnLocation_Player = player.transform.position;
			respawnRotation_Player = player.transform.rotation;
		}
		if (ship)
		{
			respawnLocation_Ship = ship.transform.position;
			respawnRotation_Ship = ship.transform.rotation;
		}
	}


	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		currentAge += Time.deltaTime / ageYearLengthInSeconds;

		//debug
		if (Input.GetKeyDown(KeyCode.R) && Input.GetKey(KeyCode.LeftControl))
		{
			ExecuteRespawn();
		}
    }

	public void ExecuteRespawn()
	{
		currentAge = 0;
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryTeller : MonoBehaviour
{
	public GameObject player;
	Fungus.Flowchart fc;

	public GameObject[] triggers;

    // Start is called before the first frame update
    void Start()
    {
		EventListener.Get(player).OnTriggerEnterDelegate2 += StoryTeller_OnTriggerEnterDelegate2;
    }

	private void StoryTeller_OnTriggerEnterDelegate2(GameObject a, GameObject b)
	{
		Debug.Log(a.name + " ENTER " + b.name);
	}

	// Update is called once per frame
	void Update()
    {
        
    }
}

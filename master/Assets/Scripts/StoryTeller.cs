using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryTeller : MonoBehaviour
{
	public static StoryTeller singleton;

	public GameObject player;
	public Fungus.Flowchart fc;

	//public GameObject[] triggers;

	private void Awake()
	{
		singleton = this;
	}

	// Start is called before the first frame update
	void Start()
    {
		EventListener.Get(player).OnTriggerEnterDelegate2 += StoryTeller_OnTriggerEnterDelegate2;

		if (!fc)
			fc = this.gameObject.GetComponent<Fungus.Flowchart>();

	}

	private void StoryTeller_OnTriggerEnterDelegate2(GameObject a, GameObject b)
	{
		//Debug.Log(a.name + " ENTER " + b.name + " of " + b.transform.parent.name);
		this.gameObject.SendMessage(b.name, SendMessageOptions.DontRequireReceiver);
		fc.SendFungusMessage(b.name);
		fc.SendFungusMessage(b.transform.parent.name);
		fc.SendFungusMessage(b.tag);
		fc.SendFungusMessage(b.transform.parent.tag);
		//this.gameObject.OnTriggerEnter(b.GetComponent<Collider>());
	}

	// Update is called once per frame
	void Update()
    {
        
    }
}

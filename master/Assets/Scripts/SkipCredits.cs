using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkipCredits : MonoBehaviour
{
    public Transform credit_go;
    public float Delay_amount = 8.0f;

    bool first_time = true;

    // Start is called before the first frame update
    void Start()
    {
        first_time = true;
        this.GetComponent<Text>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(first_time == true && credit_go.gameObject.activeSelf)
        {
            StartCoroutine(ExampleCoroutine());
        }


        if(credit_go.gameObject.activeSelf == false)
        {
            this.GetComponent<Text>().enabled = false;
        }
    }

    IEnumerator ExampleCoroutine()
    {
        //Print the time of when the function is first called.
        //Debug.Log("Started Coroutine at timestamp : " + Time.time);

        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(Delay_amount);


        if(credit_go.gameObject.activeSelf)
        {
            this.GetComponent<Text>().enabled = true;
        }

        //After we have waited 5 seconds print the time again.
        //Debug.Log("Finished Coroutine at timestamp : " + Time.time);
    }
}
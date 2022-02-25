using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerAudioCollider : MonoBehaviour
{
    public static bool inWater, inBush, inGrass = false;
    public static float waterDepth;
    float altitude;

    [SerializeField] GameObject worldRoot;

    
    void Update() 
    {
        // Debug.DrawLine(worldRoot.transform.localPosition, transform.position);
        altitude = Vector3.Distance(worldRoot.transform.localPosition, transform.position);
        if (altitude < 199.9756f) //got value by standing in water
        {
            inWater = true;
        }
        else 
        {
            inWater = false;
        }
        
        waterDepth = (Mathf.InverseLerp(199.9759f, 199.9557f, altitude));

        //Debug.Log(altitude);
        // Debug.Log("depth = " + waterDepth);
        // Debug.Log("in water = " + inWater);
    }

    void OnTriggerStay(Collider other) 
    {
        if (other.gameObject.tag == "Bush") 
        { 
            inBush = true;
        }
        if (other.gameObject.tag == "Grass") 
        { 
            inGrass = true;
        }
    }
    void OnTriggerExit(Collider other) 
    {
        if (other.gameObject.tag == "Bush") 
        { 
            inBush = false;
        }
        if (other.gameObject.tag == "Grass") 
        { 
            inGrass = false;
        }
    }
}

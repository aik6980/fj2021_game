using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class DynamicFoley : MonoBehaviour
{
    EventInstance playerFoleyEvent;
    Vector3 oldPos, newPos;
    float limbSpeed;  
    [SerializeField] Transform mainBodyPos; 


    void Start() 
    {
        oldPos = transform.position;
        //inGrass = GameObject.FindGameObjectWithTag("Grass").GetComponent<ParticleTrigger>().inGrass;
        
        playerFoleyEvent = RuntimeManager.CreateInstance("event:/SFX/dynamic_foley");
        playerFoleyEvent.start();
        RuntimeManager.AttachInstanceToGameObject(playerFoleyEvent, GetComponent<Transform>(), GetComponent<Rigidbody>()); 
    }

    void Update() 
    {
        newPos = transform.position;
        //limbSpeed = (oldPos - newPos).magnitude * Time.deltaTime;
        limbSpeed = Mathf.Sqrt(Mathf.Pow((oldPos.x - newPos.x), 2) + Mathf.Pow((oldPos.z - newPos.z), 2)) / Time.deltaTime;

        //Debug.Log("limb speed " + limbSpeed);

        
        playerFoleyEvent.setParameterByName("ClothesMove", limbSpeed);
        if(PlayerAudioCollider.inGrass) { playerFoleyEvent.setParameterByName("GrassMove", limbSpeed); }
        if(PlayerAudioCollider.inBush) { playerFoleyEvent.setParameterByName("BushMove", limbSpeed); }
        if(PlayerAudioCollider.inWater) 
        { 
            playerFoleyEvent.setParameterByName("WaterMove", limbSpeed);
            playerFoleyEvent.setParameterByName("WaterDepth", PlayerAudioCollider.waterDepth);
        }
        
        // Debug.Log("In bush = " + PlayerAudioCollider.inBush);
        // Debug.Log("In grass = " + PlayerAudioCollider.inGrass);
        // Debug.Log("In shallow water = " + PlayerAudioCollider.inShallowWater);
        // Debug.Log("In deep water = " + PlayerAudioCollider.inDeepWater);
        
        oldPos = transform.position;

    }

}

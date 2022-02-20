using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonSolidObjects : MonoBehaviour
{
    [SerializeField] bool inShallowWater, inDeepWater, inGrass, inBush;
    Rigidbody rb;
    float nonSolidTriggerVelocity = 0.1f;
    FMOD.Studio.EventInstance grassMoveSound;

    void Start() 
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnTriggerEnter(Collider other) 
    {
        float waterDepthCheck = other.ClosestPoint(gameObject.transform.position).y;
        if (other.gameObject.tag == "Water") 
        { 
            inShallowWater = true; 
        }
        if (other.gameObject.tag == "Water" && waterDepthCheck < -0.055f) 
        { 
            inShallowWater = false;
            inDeepWater = true;
        }
        if (other.gameObject.tag == "Bush") 
        { 
            inBush = true; 
        }
    }
    void OnTriggerExit(Collider other) 
    {
        float waterDepthCheck = other.ClosestPoint(gameObject.transform.position).y;
        if (other.gameObject.tag == "Water") 
        { 
            inShallowWater = false; 
        }
        if (other.gameObject.tag == "Water" && waterDepthCheck < -0.055f) 
        { 
            inShallowWater = true;
            inDeepWater = false;
        }
        if (other.gameObject.tag == "Bush") 
        { 
            inBush = false; 
        }
    }

    void OnParticleCollision(GameObject other) 
    {
        if (rb.velocity.x > nonSolidTriggerVelocity)
        {
            grassMoveSound = FMODUnity.RuntimeManager.CreateInstance("event:/SFX/Movement/Grass");
            grassMoveSound.start();
            FMODUnity.RuntimeManager.AttachInstanceToGameObject(grassMoveSound, GetComponent<Transform>(), GetComponent<Rigidbody>());
            grassMoveSound.release();
        }
    }
}

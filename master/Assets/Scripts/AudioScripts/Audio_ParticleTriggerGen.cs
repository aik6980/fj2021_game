using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Audio_ParticleTriggerGen
 : MonoBehaviour
{
    public GameObject colliderGO;
    ParticleSystem partSys;
    ParticleSystem.Particle[] allParticles;
    Vector3 startPos;
    bool collidersGenerated = false;

    void Start()
    {
        partSys = this.gameObject.GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        if (partSys.particleCount > 1 && collidersGenerated == false)
        {
            allParticles = new ParticleSystem.Particle[partSys.particleCount];
            partSys.GetParticles(allParticles);

            foreach (ParticleSystem.Particle p in allParticles)
            {
                GameObject clone;
                startPos = transform.TransformPoint(p.position);
                clone = Instantiate(colliderGO, startPos, Quaternion.identity);
                clone.transform.SetParent(this.gameObject.transform);
                clone.SetActive(true);
            }

            collidersGenerated = true;
        }
    }
}

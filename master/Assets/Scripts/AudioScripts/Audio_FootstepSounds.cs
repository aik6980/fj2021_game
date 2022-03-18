using UnityEngine;
using System.Collections.Generic;
using FMODUnity;
using FMOD.Studio;

public class Audio_FootstepSounds : MonoBehaviour
{
    enum CURRENT_TERRAIN { GRAVEL, ROCK, GRASSPATH, GRASS, SAND };
    [SerializeField] private CURRENT_TERRAIN currentTerrain;
    [SerializeField] Texture islandTexture, rockTexture, sandTexture, monolithTexture;
    EventInstance footstepSound;
    [SerializeField] GameObject leftFoot, rightFoot;
    

    void DetectGroundType()
    {
        float groundCoordX, groundCoordY;
        Texture textureCheck;
        RaycastHit[] hit;

        if(AnimatePlayer.leftStep)
            hit = Physics.RaycastAll(leftFoot.transform.position, -transform.up, 0.05f);
        else
            hit = Physics.RaycastAll(rightFoot.transform.position, -transform.up, 0.05f);

        System.Array.Sort(hit, (x,y) => x.distance.CompareTo(y.distance));

        foreach (RaycastHit rayhit in hit)
        {
            groundCoordX = rayhit.textureCoord.x;
            groundCoordY = rayhit.textureCoord.y;

            if(rayhit.collider.gameObject.GetComponent<Renderer>())
            {
                textureCheck = rayhit.transform.GetComponent<Renderer>().sharedMaterial.mainTexture;
            }
            else textureCheck = islandTexture;

            if (textureCheck == islandTexture)
            {
                if (groundCoordX > 0.7f && groundCoordY > 0.5f)
                {
                    currentTerrain = CURRENT_TERRAIN.GRAVEL;
                    return;
                }
                else if (groundCoordX > 0.7f)
                {
                    currentTerrain = CURRENT_TERRAIN.ROCK;
                    return;
                }
                else if (groundCoordX > 0.3f && groundCoordY > 0.5f)
                {
                    currentTerrain = CURRENT_TERRAIN.GRASSPATH;
                    return;
                }
                else if (groundCoordX >= 0.3f)
                {
                    currentTerrain = CURRENT_TERRAIN.GRASS;
                    return;
                }   
                else
                {
                    currentTerrain = CURRENT_TERRAIN.SAND;
                    return;
                }
            }
            else if (textureCheck == (rockTexture || monolithTexture))
            {
                currentTerrain = CURRENT_TERRAIN.ROCK;
                return;
            }
            else if (textureCheck == sandTexture)
            {
                currentTerrain = CURRENT_TERRAIN.SAND;
                return;
            }
            else
            {
                currentTerrain = CURRENT_TERRAIN.GRASSPATH;
                return;
            }
        }
    }

    int GroundTypeParameter()
    {
        DetectGroundType();
        switch (currentTerrain)
        {
            case CURRENT_TERRAIN.GRAVEL: { return 0; }
            case CURRENT_TERRAIN.ROCK: { return 1; }
            case CURRENT_TERRAIN.GRASSPATH: { return 2; }
            case CURRENT_TERRAIN.GRASS: { return 3; }
            case CURRENT_TERRAIN.SAND: { return 4; }
            default: { return 2; }
        }
    }

    public void PlayFootstepSound()
    {

        footstepSound = RuntimeManager.CreateInstance("event:/SFX/footsteps");
        PARAMETER_ID groundTypeParameterID;
        EventDescription groundTypeEventDescription;
        footstepSound.getDescription(out groundTypeEventDescription);

        PARAMETER_DESCRIPTION groundTypeParameterDescription;
        groundTypeEventDescription.getParameterDescriptionByName("Footstep_GroundType", out groundTypeParameterDescription);
        groundTypeParameterID = groundTypeParameterDescription.id;

        footstepSound.setParameterByID(groundTypeParameterID, GroundTypeParameter());

        if (Audio_DynamicFoley.altitude < 199.9756f)
        {
            footstepSound.setParameterByName("Footstep_Water", 1);
        }
        else footstepSound.setParameterByName("Footstep_Water", 0);
        footstepSound.setParameterByName("WaterDepth", Audio_DynamicFoley.waterDepth);
        
        footstepSound.start();
        if(AnimatePlayer.leftStep)
            RuntimeManager.AttachInstanceToGameObject(footstepSound, leftFoot.GetComponent<Transform>());
        else 
            RuntimeManager.AttachInstanceToGameObject(footstepSound, rightFoot.GetComponent<Transform>());

        footstepSound.release();
    }
}

using UnityEngine;

public class FootstepSounds : MonoBehaviour
{
    enum CURRENT_TERRAIN { GRAVEL, ROCK, GRASSPATH, GRASS, SAND };
    [SerializeField] private CURRENT_TERRAIN currentTerrain;
    [SerializeField] Texture islandTexture, rockTexture, sandTexture, monolithTexture;
    FMOD.Studio.EventInstance footstepSound;
    

    void DetectGroundType()
        {
            float groundCoordX, groundCoordY;
            Texture textureCheck;
            RaycastHit[] hit;
            
            hit = Physics.RaycastAll(transform.position, -transform.up, 0.05f);
            System.Array.Sort(hit, (x,y) => x.distance.CompareTo(y.distance));

            foreach (RaycastHit rayhit in hit)
            {
                groundCoordX = rayhit.textureCoord.x;
                groundCoordY = rayhit.textureCoord.y;
                textureCheck = rayhit.transform.GetComponent<Renderer>().sharedMaterial.mainTexture;

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

            footstepSound = FMODUnity.RuntimeManager.CreateInstance("event:/SFX/footsteps");
            FMOD.Studio.PARAMETER_ID groundTypeParameterID;
            FMOD.Studio.EventDescription groundTypeEventDescription;
            footstepSound.getDescription(out groundTypeEventDescription);

            FMOD.Studio.PARAMETER_DESCRIPTION groundTypeParameterDescription;
            groundTypeEventDescription.getParameterDescriptionByName("Footstep_GroundType", out groundTypeParameterDescription);
            groundTypeParameterID = groundTypeParameterDescription.id;

            footstepSound.setParameterByID(groundTypeParameterID, GroundTypeParameter());
            Debug.Log ("ground type = " + GroundTypeParameter());

            footstepSound.start();
            FMODUnity.RuntimeManager.AttachInstanceToGameObject(footstepSound, GetComponent<Transform>(), GetComponent<Rigidbody>());
            footstepSound.release();
        }

    
}

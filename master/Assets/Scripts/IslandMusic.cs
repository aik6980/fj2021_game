using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class IslandMusic : MonoBehaviour
{
    public CameraControl camControl;
    FMOD.Studio.EventInstance psIslandMusicWalking;
    FMOD.Studio.PARAMETER_ID walkSail;
    [FMODUnity.EventRef] public string islandMusicWalking;

    // Start is called before the first frame update
    void Start()
    {

    }

    void update()
    {
        if (camControl.footMove)
        {
            Debug.Log("on foot");
           // psIslandMusicWalking.setParameterByID(walkSail, 0);
           // psIslandMusicWalking = RuntimeManager.CreateInstance(islandMusicWalking);
           // psIslandMusicWalking.start();
        }
        else
        {
            Debug.Log("on ship");
            psIslandMusicWalking.setParameterByID(walkSail, 1);
        }
        
    }


    FMOD.Studio.PLAYBACK_STATE PlaybackState(FMOD.Studio.EventInstance islandMusicWalking)
    {
        FMOD.Studio.PLAYBACK_STATE pS;
        psIslandMusicWalking.getPlaybackState(out pS);
        return pS;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class Audio_IslandMusic : MonoBehaviour
{
    public CameraControl camControl;
    EventInstance psIslandMusicWalking;
    PARAMETER_ID walkSail;
    EventReference islandMusicWalking;

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


    PLAYBACK_STATE PlaybackState(EventInstance islandMusicWalking)
    {
        PLAYBACK_STATE pS;
        psIslandMusicWalking.getPlaybackState(out pS);
        return pS;
    }
}

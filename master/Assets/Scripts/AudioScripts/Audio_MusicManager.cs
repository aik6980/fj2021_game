using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class Audio_MusicManager : MonoBehaviour
{

    public GameObject creditsObject, cameraObject;
    private EventInstance music_stargazing, music_introstart, music_introend, music_sailing;

    // Start is called before the first frame update
    void Start()
    {
        music_introstart = RuntimeManager.CreateInstance("event:/Music/music_introstart");
        music_introstart.start();
        RuntimeManager.StudioSystem.setParameterByName("CreditsSequence", 1.0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (creditsObject.activeSelf != true)
        {
            RuntimeManager.StudioSystem.setParameterByName("CreditsSequence", 0.0f);

            if (GetPlaybackState(music_introstart) == PLAYBACK_STATE.PLAYING)
            {
                music_introstart.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                music_introstart.release();
            }
            if (GetPlaybackState(music_introend) != PLAYBACK_STATE.PLAYING)
            {
                music_introend = RuntimeManager.CreateInstance("event:/Music/music_introend");
                music_introend.start();
                music_introend.release();
            }
            // if (GetPlaybackState(music_sailing) != PLAYBACK_STATE.PLAYING && cameraObject.GetComponent<CameraControl>().mode == CameraControl.Mode.ShipNav)
            // {
            //     music_sailing = FMODUnity.RuntimeManager.CreateInstance("event:/Music/music_sailing");
            //     music_sailing.start();
            //     music_sailing.release();
            // }
        }

        if(ConstellationMgr.Instance.is_canvas_mode_enabled())
        {
            RuntimeManager.StudioSystem.setParameterByName("ConstellationView", 1.0f);

            if (GetPlaybackState(music_stargazing) != PLAYBACK_STATE.PLAYING)
                {
                    music_stargazing = RuntimeManager.CreateInstance("event:/Music/music_stargazing");
                    music_stargazing.start();
                }
        }
        else
        {
            RuntimeManager.StudioSystem.setParameterByName("ConstellationView", 0.0f);
        }
    }
    
    PLAYBACK_STATE GetPlaybackState(EventInstance instance)
    {
        PLAYBACK_STATE pS;
        instance.getPlaybackState(out pS);
        return pS;        
    }


}


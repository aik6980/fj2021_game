using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FMODMusicManager : MonoBehaviour
{

    public GameObject creditsObject;


    private FMOD.Studio.EventInstance music_stargazing, music_introstart, music_introend;

    FMOD.Studio.PLAYBACK_STATE PlaybackState_IntroStart(FMOD.Studio.EventInstance music_introstart)
        {
            FMOD.Studio.PLAYBACK_STATE pS;
            music_introstart.getPlaybackState(out pS);
            return pS;        
        }
    FMOD.Studio.PLAYBACK_STATE PlaybackState_IntroEnd(FMOD.Studio.EventInstance music_introend)
        {
            FMOD.Studio.PLAYBACK_STATE pS;
            music_introend.getPlaybackState(out pS);
            return pS;        
        }
    FMOD.Studio.PLAYBACK_STATE PlaybackState_Stargazing(FMOD.Studio.EventInstance music_stargazing)
        {
            FMOD.Studio.PLAYBACK_STATE pS;
            music_stargazing.getPlaybackState(out pS);
            return pS;        
        }

    // Start is called before the first frame update
    void Start()
    {
            music_introstart = FMODUnity.RuntimeManager.CreateInstance("event:/Music/music_introstart");
            music_introstart.start();
    }

    // Update is called once per frame
    void Update()
    {
        if (creditsObject.activeSelf != true)
        {
            if (PlaybackState_IntroStart(music_introstart) == FMOD.Studio.PLAYBACK_STATE.PLAYING)
            {
                music_introstart.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                music_introstart.release();
            }
            if (PlaybackState_IntroEnd(music_introend) != FMOD.Studio.PLAYBACK_STATE.PLAYING)
            {
                music_introend = FMODUnity.RuntimeManager.CreateInstance("event:/Music/music_introend");
                music_introend.start();
                music_introend.release();
            }
        }

        if(ConstellationMgr.Instance.is_canvas_mode_enabled())
        {
            if (PlaybackState_Stargazing(music_stargazing) != FMOD.Studio.PLAYBACK_STATE.PLAYING)
                {
                            music_stargazing = FMODUnity.RuntimeManager.CreateInstance("event:/Music/music_stargazing");
                            music_stargazing.start();
                }
        }
        if(ConstellationMgr.Instance.is_canvas_mode_enabled())
        {
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("constellation_view", 1.0f);
        }
        else
        {
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("constellation_view", 0.0f);
        }



    }
}


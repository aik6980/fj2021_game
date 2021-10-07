using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FMODMusicManager : MonoBehaviour
{

    public CameraControl camera_control;

    private FMOD.Studio.EventInstance music_stargazing;
    FMOD.Studio.PLAYBACK_STATE PlaybackState(FMOD.Studio.EventInstance music_stargazing)
        {
            FMOD.Studio.PLAYBACK_STATE pS;
            music_stargazing.getPlaybackState(out pS);
            return pS;        
        }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(ConstellationMgr.Instance.is_canvas_mode_enabled())
        {
            if (PlaybackState(music_stargazing) != FMOD.Studio.PLAYBACK_STATE.PLAYING)
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


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FMODMusicManager : MonoBehaviour
{

    public CameraControl camera_control;
    public Vector3 velocity;

    private FMOD.Studio.EventInstance music_stargazing, music_sailing;
    FMOD.Studio.PLAYBACK_STATE PlaybackState_Stargazing(FMOD.Studio.EventInstance music_stargazing)
        {
            FMOD.Studio.PLAYBACK_STATE pS;
            music_stargazing.getPlaybackState(out pS);
            return pS;        
        }
    FMOD.Studio.PLAYBACK_STATE PlaybackState_Sailing(FMOD.Studio.EventInstance music_sailing)
        {
            FMOD.Studio.PLAYBACK_STATE pS;
            music_sailing.getPlaybackState(out pS);
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
        
        if (velocity.z > 10.0f)
        {
            if (PlaybackState_Sailing(music_sailing) != FMOD.Studio.PLAYBACK_STATE.PLAYING)
            {
                music_sailing = FMODUnity.RuntimeManager.CreateInstance("event:/Music/music_sailing");
                music_sailing.start();
            }
        }

    }
}


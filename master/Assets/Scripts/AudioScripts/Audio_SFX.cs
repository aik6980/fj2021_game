using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class Audio_SFX : MonoBehaviour
{
    static EventInstance stargazing_drawing;
    public static void BoatSplashStart(GameObject boat)
    {
        EventInstance boatsplash = RuntimeManager.CreateInstance("event:/SFX/boatsplash");
        boatsplash.start();
		RuntimeManager.AttachInstanceToGameObject(boatsplash, boat.GetComponent<Transform>());
    }

    public static void BoatSplashUpdate(Vector3 velocity)
    {
        RuntimeManager.StudioSystem.setParameterByName("sailing_speed", velocity.z);
		RuntimeManager.StudioSystem.setParameterByName("is_turning", velocity.y);
    }

    public EventInstance stargazing_constellation;
    
    public static void StarDrawingStart()
    {
        stargazing_drawing = RuntimeManager.CreateInstance("event:/SFX/stargazing_drawing");
        if (GetPlaybackState(stargazing_drawing) != FMOD.Studio.PLAYBACK_STATE.PLAYING)
        {
            stargazing_drawing.start();
        }
    }

    public static void StarDrawingEnd(bool endOnStar)
    {
        if(endOnStar) { RuntimeManager.StudioSystem.setParameterByName("end_on_star", 1f); }
        else  { RuntimeManager.StudioSystem.setParameterByName("end_on_star", 0f); }
        stargazing_drawing.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        stargazing_drawing.release();
        Debug.Log("end_on_star " + endOnStar);
    }

    public static void StarDrawingConstellation()
    {
        EventInstance stargazing_constellation = FMODUnity.RuntimeManager.CreateInstance("event:/SFX/stargazing_constellation");
        stargazing_constellation.start();
        stargazing_constellation.release();
    }

    public static PLAYBACK_STATE GetPlaybackState(EventInstance instance)
    {
        PLAYBACK_STATE pS;
        instance.getPlaybackState(out pS);
        return pS;        
    }
}

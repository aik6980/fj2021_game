using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class Audio_SFX : MonoBehaviour
{
    public static void BoatSplashStart(GameObject boat)
    {
        EventInstance boatsplash = RuntimeManager.CreateInstance("event:/SFX/boatsplash");
        boatsplash.start();
        RuntimeManager.AttachInstanceToGameObject(boatsplash, boat.GetComponent<Transform>());
    }

    public static void BoatSplashUpdate(Vector3 velocity)
    {
        RuntimeManager.StudioSystem.setParameterByName("SailingSpeed", velocity.z);
        RuntimeManager.StudioSystem.setParameterByName("IsTurning", velocity.y);
    }

    static EventInstance stargazing_drawing;
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
        if (endOnStar) { RuntimeManager.StudioSystem.setParameterByName("EndOnStar", 1f); }
        else { RuntimeManager.StudioSystem.setParameterByName("EndOnStar", 0f); }
        stargazing_drawing.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        stargazing_drawing.release();
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

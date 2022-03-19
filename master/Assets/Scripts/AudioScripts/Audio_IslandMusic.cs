using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class Audio_IslandMusic : MonoBehaviour
{
    [SerializeField] GameObject mainCamera;
    [SerializeField] EventReference musicEvent;
    EventInstance musicInstance;

    void Start()
    {
        musicInstance = RuntimeManager.CreateInstance(musicEvent);
        RuntimeManager.AttachInstanceToGameObject(musicInstance, transform);
    }

    void update()
    {
        if (mainCamera.GetComponent<CameraControl>().footMove)
        {
            musicInstance.setParameterByNameWithLabel("WalkSail", "Walk");
        }
        else
        {
            float distanceToIsland = Vector3.Distance(transform.position, mainCamera.transform.position);
            Debug.Log("distance to island: " + distanceToIsland);
            musicInstance.setParameterByNameWithLabel("WalkSail", "Sail");
            musicInstance.setParameterByName("DistanceToIsland", distanceToIsland);
        }
        
    }

    PLAYBACK_STATE GetPlaybackState(EventInstance instance)
    {
        PLAYBACK_STATE pS;
        instance.getPlaybackState(out pS);
        return pS;        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class Audio_DayNightDetector : MonoBehaviour
{

    private EventInstance instance;
    private PARAMETER_ID day_nightParameterID;
    
    [SerializeField] private EventReference fmodEvent;

    // Start is called before the first frame update
    void Start()
    {
        instance = RuntimeManager.CreateInstance(fmodEvent);

        EventDescription day_nightEventDescription;
        instance.getDescription(out day_nightEventDescription);
        PARAMETER_DESCRIPTION day_nightParameterDescription;
        day_nightEventDescription.getParameterDescriptionByName("day_night", out day_nightParameterDescription);
        day_nightParameterID = day_nightParameterDescription.id;
        
        instance.start();

        RuntimeManager.AttachInstanceToGameObject(instance, GetComponent<Transform>(), GetComponent<Rigidbody>());
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(DayLight.singleton.normalisedDayTime);
        instance.setParameterByID(day_nightParameterID, DayLight.singleton.normalisedDayTime);
    }
}

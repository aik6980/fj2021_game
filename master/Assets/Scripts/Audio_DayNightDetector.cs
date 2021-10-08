using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Audio_DayNightDetector : MonoBehaviour
{

    private FMOD.Studio.EventInstance instance;
    private FMOD.Studio.PARAMETER_ID day_nightParameterID;
    
    [FMODUnity.EventRef]
    public string fmodEvent;
    

    // Start is called before the first frame update
    void Start()
    {
        instance = FMODUnity.RuntimeManager.CreateInstance(fmodEvent);

        FMOD.Studio.EventDescription day_nightEventDescription;
        instance.getDescription(out day_nightEventDescription);
        FMOD.Studio.PARAMETER_DESCRIPTION day_nightParameterDescription;
        day_nightEventDescription.getParameterDescriptionByName("day_night", out day_nightParameterDescription);
        day_nightParameterID = day_nightParameterDescription.id;
        
        instance.start();

        FMODUnity.RuntimeManager.AttachInstanceToGameObject(instance, GetComponent<Transform>(), GetComponent<Rigidbody>());
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(DayLight.singleton.normalisedDayTime);
        instance.setParameterByID(day_nightParameterID, DayLight.singleton.normalisedDayTime);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Audio_DayNightDetector : MonoBehaviour
{
    public GameObject DayLightCycle;

    private FMOD.Studio.EventInstance instance;
    
    [FMODUnity.EventRef]
    public string fmodEvent;

    // Start is called before the first frame update
    void Start()
    {
        instance = FMODUnity.RuntimeManager.CreateInstance(fmodEvent);
        instance.start();
    }

    // Update is called once per frame
    void Update()
    {
        var daylight_script = DayLightCycle.GetComponent<DayLight>();
        Debug.Log(daylight_script.normalisedDayTime);
        instance.setParameterByName("day_night", daylight_script.normalisedDayTime);
    }
}

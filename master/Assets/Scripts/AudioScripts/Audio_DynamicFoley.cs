using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class Audio_DynamicFoley : MonoBehaviour
{
    public static float waterDepth;
    public static float altitude, limbSpeedAvg;
    [SerializeField][Range(100f, 500f)] float fineTune = 50f;
    [SerializeField] GameObject worldRoot, head, lArm, rArm, lLeg, rLeg;
    EventInstance waterMove, bushMove, grassMove, treeMove;
    EventInstance headFoley, lArmFoley, rArmFoley, lLegFoley, rLegFoley;
    Vector3 headPos, lArmPos, rArmPos, lLegPos, rLegPos;
    Vector3 headOffset, lArmOffset, rArmOffset, lLegOffset, rLegOffset;
    GameObject treeObject, bushObject, grassObject, waterObject;
    GameObject eventObject;


    void Start()
    {
        headFoley = StartFoley(head);
        lArmFoley = StartFoley(lArm);
        rArmFoley = StartFoley(rArm);
        lLegFoley = StartFoley(lLeg);
        rLegFoley = StartFoley(rLeg);
    }

    void Update()
    {
        (headPos, headOffset) = LimbUpdate(head, headPos, headOffset, headFoley, out float headSpeed);
        (lArmPos, lArmOffset) = LimbUpdate(lArm, lArmPos, lArmOffset, lArmFoley, out float lArmSpeed);
        (rArmPos, rArmOffset) = LimbUpdate(rArm, rArmPos, rArmOffset, rArmFoley, out float rArmSpeed);
        (lLegPos, lLegOffset) = LimbUpdate(lLeg, lLegPos, lLegOffset, lLegFoley, out float lLegSpeed);
        (rLegPos, rLegOffset) = LimbUpdate(rLeg, rLegPos, rLegOffset, rLegFoley, out float rLegSpeed);

        limbSpeedAvg = (headSpeed + lArmSpeed + rArmSpeed + lLegSpeed + rLegSpeed) / 5;

        altitude = Vector3.Distance(worldRoot.transform.localPosition, transform.position);
        waterDepth = (Mathf.InverseLerp(199.9759f, 199.9550f, altitude));
        waterMove.setParameterByName("WaterDepth", waterDepth);

        if (GetPlaybackState(treeMove) == PLAYBACK_STATE.STOPPING)
            NonSolidExit(treeMove);

        if (GetPlaybackState(bushMove) == PLAYBACK_STATE.STOPPING)
            NonSolidExit(bushMove);

        if (GetPlaybackState(grassMove) == PLAYBACK_STATE.STOPPING)
            NonSolidExit(grassMove);

        if (GetPlaybackState(waterMove) == PLAYBACK_STATE.STOPPING)
            NonSolidExit(waterMove);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Tree" && GetPlaybackState(treeMove) != PLAYBACK_STATE.PLAYING)
        {
            treeMove = RuntimeManager.CreateInstance("event:/SFX/dynamic_foley_tree");
            treeMove.start();
        }
        if (other.gameObject.tag == "Bush" && GetPlaybackState(bushMove) != PLAYBACK_STATE.PLAYING)
        {
            bushMove = RuntimeManager.CreateInstance("event:/SFX/dynamic_foley_bush");
            bushMove.start();
        }
        if (other.gameObject.tag == "Grass" && GetPlaybackState(grassMove) != PLAYBACK_STATE.PLAYING)
        {
            grassMove = RuntimeManager.CreateInstance("event:/SFX/dynamic_foley_grass");
            grassMove.start();
        }
        if (other.gameObject.tag == "Water" && GetPlaybackState(waterMove) != PLAYBACK_STATE.PLAYING)
        {
            waterMove = RuntimeManager.CreateInstance("event:/SFX/dynamic_foley_water");
            waterMove.start();
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Tree")
            treeObject = NonSolidTrack(treeMove, other, "TreeMove");

        if (other.gameObject.tag == "Bush")
            bushObject = NonSolidTrack(bushMove, other, "BushMove");

        if (other.gameObject.tag == "Grass")
            grassObject = NonSolidTrack(grassMove, other, "GrassMove");

        if (other.gameObject.tag == "Water")
            waterObject = NonSolidTrack(waterMove, other, "waterMove");

    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Tree" && GetPlaybackState(treeMove) == PLAYBACK_STATE.PLAYING)
        {
            treeMove.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            treeMove.setParameterByName("TreeMove", 0f);
        }
        if (other.gameObject.tag == "Bush" && GetPlaybackState(bushMove) == PLAYBACK_STATE.PLAYING)
        {
            bushMove.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            bushMove.setParameterByName("BushMove", 0f);
        }
        if (other.gameObject.tag == "Grass" && GetPlaybackState(grassMove) == PLAYBACK_STATE.PLAYING)
        {
            grassMove.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            grassMove.setParameterByName("GrassMove", 0f);
        }
        if (other.gameObject.tag == "Water" && GetPlaybackState(waterMove) == PLAYBACK_STATE.PLAYING)
        {
            waterMove.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            waterMove.setParameterByName("WaterMove", 0f);
        }
        if (eventObject)
            Destroy(eventObject);
        eventObject = new GameObject("NonSolidSound");
    }

    EventInstance StartFoley(GameObject limb)
    {
        EventInstance instance = RuntimeManager.CreateInstance("event:/SFX/dynamic_foley");
        instance.start();
        RuntimeManager.AttachInstanceToGameObject(instance, limb.GetComponent<Transform>());
        return instance;
    }

    public static PLAYBACK_STATE GetPlaybackState(EventInstance instance)
    {
        PLAYBACK_STATE pS;
        instance.getPlaybackState(out pS);
        return pS;
    }

    (Vector3, Vector3) LimbUpdate(GameObject limb, Vector3 oldPos, Vector3 oldOffset, EventInstance foleyEvent, out float limbSpeed)
    {
        Vector3 newOffset = GetComponent<OnFootMovement>().model.transform.position;
        Vector3 newPos = transform.InverseTransformPoint(limb.transform.position);
        limbSpeed = fineTune * Mathf.Abs(Vector3.Distance(newPos, oldPos) - Vector3.Distance(newOffset, oldOffset));

        foleyEvent.setParameterByName("ClothesMove", limbSpeed);

        oldOffset = newOffset;
        oldPos = newPos;
        return (oldPos, oldOffset);
    }

    GameObject NonSolidTrack(EventInstance eventInstance, Collider other, string parameter)
    {
        var collisionPoint = other.GetComponent<Collider>().ClosestPoint(transform.position);
        eventInstance.set3DAttributes(RuntimeUtils.To3DAttributes(collisionPoint));
        eventInstance.get3DAttributes(out FMOD.ATTRIBUTES_3D attributes);
        Vector3 foleyPos = new Vector3(attributes.position.x, attributes.position.y, attributes.position.z);
        GameObject trackedObject = other.gameObject;
        eventInstance.setParameterByName(parameter, limbSpeedAvg);
        return trackedObject;
    }

    void NonSolidExit(EventInstance trackedEvent)
    {
        trackedEvent.get3DAttributes(out FMOD.ATTRIBUTES_3D attributes);
        eventObject.transform.position.Set(attributes.position.x, attributes.position.y, attributes.position.z);
        RuntimeManager.AttachInstanceToGameObject(trackedEvent, eventObject.transform);
        eventObject.transform.SetParent(worldRoot.transform);
    }
}

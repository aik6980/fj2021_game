using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using Fungus;
using System;

public class Audio_MusicManager : MonoBehaviour
{
    [SerializeField] List<GameObject> islandObjects;
    [SerializeField] GameObject creditsObject, shipObject, cameraObject, listenerObject, playerObject, planetObject;
    [SerializeField] EventReference islandMusicRef, windRef;
    [SerializeField] LayerMask sphereMask;

    static Dictionary<GameObject, EventInstance> islandMusicDict = new Dictionary<GameObject, EventInstance>();
    Dictionary<GameObject, EventInstance> islandsToPlay, furthestIslands; EventInstance stargazingInst, introStartInst, introEndInst, sailingInst, islandMusicInst, windInst;
    KeyValuePair<GameObject, EventInstance> closestIsland;

    List<EventInstance> islandMusicInsts = new List<EventInstance>();
    EventInstance creditsSnapshot, pausedSnapshot;

    float shipVelocity, windMusicOld;
    float closestIslandDist = 0f;
    int notClosest;
    bool firstSail = true;

    string[] musicKeys =
    {
        "A", "Bb", "B", "Cb", "C", "C#", "Db", "D", "Eb", "E", "F", "F#", "Gb", "G"
    }; 
    
    enum MusicState
    {
        INTRO,
        INTROEND,
        ISLANDS,
        SAILING,
        DEFAULT
    }
    MusicState musicState = MusicState.INTRO;

    


    void Start()
    {
        CreateMusicDict();
        windInst = RuntimeManager.CreateInstance(windRef);
        windInst.start();
    }

    void Update()
    {
        shipVelocity = shipObject.GetComponent<ShipMovement>().velocity.z;

        UpdateIslands();
        PauseGame();
        ConstellationMusic();
        SetMusicState();
    }

    PLAYBACK_STATE GetPlaybackState(EventInstance instance)
    {
        PLAYBACK_STATE pS;
        instance.getPlaybackState(out pS);
        return pS;
    }

    void CreateMusicDict()
    {
        for (var i = 0; i < islandObjects.Count; i++)
        {
            islandMusicInsts.Add(islandMusicInst);
            islandMusicInsts[i] = RuntimeManager.CreateInstance(islandMusicRef);
            islandMusicInsts[i].setParameterByName("IslandNumber", i);
            islandMusicDict.Add(islandObjects[i], islandMusicInsts[i]);
        }
    }

    void PauseGame()
    {
        if (MenuControls.paused && GetPlaybackState(pausedSnapshot) != PLAYBACK_STATE.PLAYING)
        {
            pausedSnapshot = RuntimeManager.CreateInstance("snapshot:/PauseMenu");
            pausedSnapshot.start();
        }
        else
        {
            pausedSnapshot.release();
            pausedSnapshot.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
    }

    void ConstellationMusic() //operates via a parameter that overrides other music without stopping it
    {
        if (ConstellationMgr.Instance.is_canvas_mode_enabled())
        {
            RuntimeManager.StudioSystem.setParameterByName("ConstellationView", 1.0f);

            if (GetPlaybackState(stargazingInst) != PLAYBACK_STATE.PLAYING)
            {
                stargazingInst = RuntimeManager.CreateInstance("event:/Music/music_stargazing");
                stargazingInst.start();
            }
        }
        else
            RuntimeManager.StudioSystem.setParameterByName("ConstellationView", 0.0f);
    }

    void SetMusicState()
    {
        switch (musicState)
        {
            case MusicState.INTRO:
                {
                    PlayIntroMusic();
                    break;
                }
            case MusicState.INTROEND:
                {
                    if (!playerObject.GetComponent<OnFootMovement>().stopped)
                        musicState = MusicState.ISLANDS;
                    break;
                }
            case MusicState.ISLANDS:
                {
                    PlayIslandMusic();
                    break;
                }
            //maybe a bug if while sailing is playing, player gets off boat and back on quickly
            case MusicState.SAILING:
                {
                    PlaySailingMusic();
                    break;
                }
            case MusicState.DEFAULT:
                {
                    break;
                }
        }
    }
    void PlayIntroMusic()
    {
        if (GetPlaybackState(introStartInst) != PLAYBACK_STATE.PLAYING)
        {
            introStartInst = RuntimeManager.CreateInstance("event:/Music/music_introstart");
            introStartInst.start();
            introStartInst.release();

            if (GetPlaybackState(creditsSnapshot) != PLAYBACK_STATE.PLAYING)
            {
                creditsSnapshot = RuntimeManager.CreateInstance("snapshot:/CreditsSequence");
                creditsSnapshot.start();
                creditsSnapshot.release();
            }
        }
        if (creditsObject.activeSelf != true)
        {
            if (GetPlaybackState(creditsSnapshot) == PLAYBACK_STATE.PLAYING)
            {
                var result = creditsSnapshot.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                Debug.Log("credit snapshot stopped? " + result);
            }
            if (GetPlaybackState(introStartInst) == PLAYBACK_STATE.PLAYING)
            {
                introStartInst.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
            if (GetPlaybackState(introEndInst) != PLAYBACK_STATE.PLAYING)
            {
                introEndInst = RuntimeManager.CreateInstance("event:/Music/music_introend");
                introEndInst.start();
                introEndInst.release();
            }
            musicState = MusicState.INTROEND;
        }
    }

    

    void UpdateIslands()
    {
        int islandCount;
        if (cameraObject.GetComponent<CameraControl>().mode == CameraControl.Mode.LandWalk)
        {
            islandCount = 1;
            RuntimeManager.StudioSystem.setParameterByNameWithLabel("WalkSail", "Walk");
        }
        else
        {
            islandCount = 4;
            RuntimeManager.StudioSystem.setParameterByNameWithLabel("WalkSail", "Sail");
        }

        notClosest = islandMusicDict.Count - islandCount;
        islandsToPlay = islandMusicDict.OrderBy(pair => Vector3.Distance(pair.Key.transform.position, shipObject.transform.position)).Take(islandCount).ToDictionary(pair => pair.Key, pair => pair.Value);
        furthestIslands = islandMusicDict.OrderByDescending(pair => Vector3.Distance(pair.Key.transform.position, shipObject.transform.position)).Take(notClosest).ToDictionary(pair => pair.Key, pair => pair.Value);
        closestIsland = islandMusicDict.OrderBy(pair => Vector3.Distance(pair.Key.transform.position, shipObject.transform.position)).Take(1).ToList()[0];
    }

    void PlayIslandMusic()
    {
        ChangeMusicKey(closestIsland.Value);

        foreach (var pair in islandsToPlay)
        {
            pair.Value.set3DAttributes(pair.Key.transform.position.To3DAttributes());

            if (GetPlaybackState(pair.Value) != PLAYBACK_STATE.PLAYING)
            {
                pair.Value.getParameterByName("IslandNumber", out float value);
                pair.Value.start();
            }
            pair.Value.setParameterByName("DistanceToIsland", Vector3.Distance(pair.Key.transform.position, listenerObject.transform.position));
        }

        foreach (var pair in furthestIslands)
        {
            if (GetPlaybackState(pair.Value) == PLAYBACK_STATE.PLAYING)
            {
                pair.Value.getParameterByName("IslandNumber", out float value);
                pair.Value.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
        }

        if (closestIslandDist > 30f)
        {
            if (cameraObject.GetComponent<CameraControl>().mode != CameraControl.Mode.LandWalk)
                WindMusic(islandsToPlay);

            if (shipVelocity > 50f)
                musicState = MusicState.SAILING;
        }
    }

    private void WindMusic(Dictionary<GameObject, EventInstance> islandsToPlay)
    {
        RuntimeManager.StudioSystem.getParameterByName("WindMusic", out float whatever, out float windMusic);

        if (windMusic != windMusicOld)
        {
            if (windMusic == 0f)
            {
                foreach (var pair in islandMusicDict)
                {
                    pair.Value.setParameterByName("HeardOnWind", 0f);
                }

                var heardPair = islandsToPlay.OrderBy(pair => UnityEngine.Random.Range(0, islandsToPlay.Count)).ElementAt(0);

                ChangeMusicKey(heardPair.Value);

                heardPair.Value.setParameterByName("HeardOnWind", 1f);
                Vector3 sourcePos = AudioHorizon(listenerObject, heardPair.Key, planetObject);
                heardPair.Value.set3DAttributes(sourcePos.To3DAttributes());
                windInst.set3DAttributes(sourcePos.To3DAttributes());

                Debug.DrawLine(listenerObject.transform.position, sourcePos, Color.green, 5f);
            }
            windMusicOld = windMusic;
        }
    }

    void ChangeMusicKey(string key)
    {
        if (musicKeys.Contains(key.ToUpper()))
        {
            RuntimeManager.StudioSystem.setParameterByNameWithLabel("CurrentSongKey", key);
        }
        else
        {
            Debug.LogAssertion("ChangeMusicKey(string key) input was not an accepted key");
        }
    }

    void ChangeMusicKey(EventInstance eventInstance)
    {
        eventInstance.getParameterByName("SongKey", out float ignore, out float songKeyValue);
        RuntimeManager.StudioSystem.setParameterByName("CurrentSongKey", songKeyValue);
    }

    void PlaySailingMusic()
    {
        ChangeMusicKey("C");

        if (firstSail)
        {
            RuntimeManager.StudioSystem.setParameterByNameWithLabel("SailingState", "FirstTime");
            firstSail = false;
        }
        if (GetPlaybackState(sailingInst) != PLAYBACK_STATE.PLAYING)
        {
            sailingInst = RuntimeManager.CreateInstance("event:/Music/music_sailing");
            sailingInst.start();
            sailingInst.release();
        }
        if (shipVelocity > 100f)
            RuntimeManager.StudioSystem.setParameterByNameWithLabel("SailingState", "SailingFast");
        else if (shipVelocity > 50f)
            RuntimeManager.StudioSystem.setParameterByNameWithLabel("SailingState", "SailingSlow");
        else
        {
            RuntimeManager.StudioSystem.setParameterByNameWithLabel("SailingState", "StopSailing");
            musicState = MusicState.ISLANDS;
        }
        if (closestIslandDist < 30f)
        {
            RuntimeManager.StudioSystem.setParameterByNameWithLabel("SailingState", "StopSailing");
            musicState = MusicState.ISLANDS;
        }
    }

    Vector3 AudioHorizon(GameObject listener, GameObject source, GameObject sphere)
    {
        Vector3 listenerPos = listener.transform.position;
        Vector3 sourcePos = source.transform.position;
        Vector3 spherePos = sphere.transform.position;

        if (!Physics.Linecast(listenerPos, sourcePos, layerMask: sphereMask))
            return sourcePos;

        Plane plane = new Plane(listenerPos, sourcePos, spherePos);
        Vector3 normal = plane.normal;
        Vector3 centre = spherePos;
        float radius = sphere.transform.localScale.x * sphere.GetComponent<SphereCollider>().radius;

        Vector3 tanA = Vector3.zero;
        Vector3 tanB = Vector3.zero;
        CircleTangent3D(centre, radius, listenerPos, plane, ref tanA, ref tanB);

        Vector3 tangentPos;
        if (Vector3.Distance(tanA, sourcePos) < Vector3.Distance(tanB, sourcePos))
            tangentPos = tanA;
        else tangentPos = tanB;

        Vector3 heading = tangentPos - listenerPos;
        heading = heading.normalized * (Vector3.Distance(listenerPos, sourcePos) - Vector3.Distance(listenerPos, tangentPos));
        Vector3 pSourcePos = heading + tangentPos;

        return pSourcePos;
    }

    void CircleTangent3D(Vector3 c, float r, Vector3 p, Plane plane, ref Vector3 tanPosA, ref Vector3 tanPosB)
    {
        Vector3 n = plane.normal;
        p -= c;

        float P = p.magnitude;

        float a = r * r / P;
        float q = r * Mathf.Sqrt((P * P) - (r * r)) / P;

        if (Mathf.Sign(n.z) == -1f)
            plane.Flip();

        Vector3 pN = p / P;
        Vector3 pNP = Vector3.Cross(n, pN);
        Vector3 va = pN * a;

        tanPosA = va + pNP * q;
        tanPosB = va - pNP * q;

        tanPosA += c;
        tanPosB += c;
    }
}


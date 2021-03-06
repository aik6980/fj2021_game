using UnityEngine.Audio;
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AudioManager : MonoSingleton<AudioManager>
{
    public AudioMixer           m_masterMixer;
    public AudioMixerGroup      m_sfxMixerGroup;
    public MusicGroup[]         m_musicMixerGroup; 

    public MusicData[] music_data;
    public SFXData[] sfx_data;

    public float music_interval;
    public float music_interval_variation;
    public float crossfade_duration;

    Queue<int> current_playlist;

    AudioSource curr_music;

    public CameraControl camera_control;

    enum MusicState
    {
        Sailing,
        Stargazing,
        Walking,
    }
    MusicState curr_state, prev_state;

    MusicState get_game_music_state()
    {
        if(ConstellationMgr.Instance.is_canvas_mode_enabled())
        {
            return MusicState.Stargazing;
        }
        else
        {
            if (camera_control.mode == CameraControl.Mode.ShipNav)
            {
                return MusicState.Sailing;
            }
            else
            {
                return MusicState.Walking;
            }    
        }
    }

    void Awake()
    {
		if (!camera_control) { this.enabled = false; return; }
	}

	void Start()
    {
        current_playlist = GeneratePlaylist();
        // play first track
        //SwitchTrack();
        // then switch track every interval
        //StartCoroutine(SwitchTrackInterval());

        curr_music = null;
        curr_state = prev_state = get_game_music_state();
    }

    public void PlayMusic(string name)
    {
        var music = Array.Find(music_data, item => item.name == name);
        if (music == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        if(curr_music == music.audio_source)
        {
            return;
        }

        if(curr_music)
        {
            StartCoroutine(StartFade_simple(curr_music, crossfade_duration, 0.0f));
        }

        music.audio_source.Play();
        curr_music = music.audio_source;
        StartCoroutine(StartFade_simple(curr_music, crossfade_duration, 1.0f));

        //var result = m_masterMixer.SetFloat(name+"_vol", 0.0f);
    }
    public void StopMusic(string name)
    {
        MusicGroup music = Array.Find(m_musicMixerGroup, item => item.mixerGroup.name == name);
        if (music == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        //s.source.volume = s.volume * (1f + UnityEngine.Random.Range(-s.volumeVariance / 2f, s.volumeVariance / 2f));
        //s.source.pitch = s.pitch * (1f + UnityEngine.Random.Range(-s.pitchVariance / 2f, s.pitchVariance / 2f));

        //foreach (AudioSource source in music.audio_sources)
        //{
        //    source.Pause();
        //}

        m_masterMixer.SetFloat(name + "_vol", -80.0f);
    }

    public void PlaySFX(string sound)
    {
		if (sfx_data == null) return;

        SFXData s = Array.Find(sfx_data, item => item.name == sound);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        //s.source.volume = s.volume * (1f + UnityEngine.Random.Range(-s.volumeVariance / 2f, s.volumeVariance / 2f));
        //s.source.pitch = s.pitch * (1f + UnityEngine.Random.Range(-s.pitchVariance / 2f, s.pitchVariance / 2f));
        s.audio_source.Play();
    }


    private void Update()
    {
        var next_state = get_game_music_state();

        if(curr_state != next_state)
        {
            prev_state = curr_state;
            curr_state = next_state;
        }

        switch(curr_state)
        {
            case MusicState.Sailing:
                PlayMusic("sail_1");
                break;
            case MusicState.Walking:
                PlayMusic("island_1");
                break;
            case MusicState.Stargazing:
                PlayMusic("stargaze_1");
                break;
        }
    }

    Queue<int> GeneratePlaylist()
    {
        int[] indexArray = new int[m_musicMixerGroup.Length];
        // Fill with sequeential indexes then randomize
        for (int i = 0; i < indexArray.Length; ++i)
        {
            indexArray[i] = i;
        }
        RandomizeArray<int>(indexArray);
        return new Queue<int>(indexArray);
    }

    void SwitchTrack()
    {
        var current_track = current_playlist.Dequeue();
        if (current_playlist.Count == 0)
        {
            current_playlist = GeneratePlaylist();
        }

        for (int i = 0; i < m_musicMixerGroup.Length; ++i)
        {
            var mixerGroup_name = m_musicMixerGroup[i].mixerGroup.name;
            if (i == current_track)
            {
                StartCoroutine(StartFade(m_masterMixer, mixerGroup_name + "_vol", crossfade_duration, 1.0f));
            }
            else
            {
                StartCoroutine(StartFade(m_masterMixer, mixerGroup_name + "_vol", crossfade_duration, 0.0f));
            }
        }
    }

    IEnumerator SwitchTrackInterval()
    {
        //Print the time of when the function is first called.
        Debug.Log("Started Coroutine at timestamp : " + Time.time);

        
        while(true)
        {

            var time_variation = UnityEngine.Random.Range(-1.0f, 1.0f) * music_interval_variation;
            var random_interval = music_interval + time_variation;

            yield return new WaitForSeconds(random_interval);

            SwitchTrack();
        }
    }

    public static IEnumerator StartFade_simple(AudioSource audio, float duration, float targetVolume)
    {
        float currentTime = 0;
        float currentVol = audio.volume;
        //currentVol = Mathf.Pow(10, currentVol / 20);
        float targetValue = Mathf.Clamp(targetVolume, 0.0001f, 1);

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float newVol = Mathf.Lerp(currentVol, targetValue, currentTime / duration);
            //audioMixer.SetFloat(exposedParam, Mathf.Log10(newVol) * 20);
            //audioMixer.SetFloat(exposedParam, newVol);
            //audio.volume = Mathf.Log10(newVol) * 20;
            audio.volume = newVol;

            yield return null;
        }

        if(targetVolume < 0.01)
        {
            audio.Stop();
        }

        yield break;
    }


    public static IEnumerator StartFade(AudioMixer audioMixer, string exposedParam, float duration, float targetVolume)
    {
        float currentTime = 0;
        float currentVol;
        audioMixer.GetFloat(exposedParam, out currentVol);
        currentVol = Mathf.Pow(10, currentVol / 20);
        float targetValue = Mathf.Clamp(targetVolume, 0.0001f, 1);

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float newVol = Mathf.Lerp(currentVol, targetValue, currentTime / duration);
            audioMixer.SetFloat(exposedParam, Mathf.Log10(newVol) * 20);
            //audioMixer.SetFloat(exposedParam, newVol);

            yield return null;
        }
        yield break;
    }

    public static void RandomizeArray<T>(T[] array)
    {
        int size = array.Length;
        for (int i = 0; i < size; i++)
        {
            int indexToSwap = UnityEngine.Random.Range(i, size);
            T swapValue = array[i];
            array[i] = array[indexToSwap];
            array[indexToSwap] = swapValue;
        }
    }

    //IEnumerator FadeSoundIn()
    //{
    //    Sound s = Array.Find(sounds, item => item.name == "Mus_Layer_B");
    //    Sound s1 = Array.Find(sounds, item => item.name == "Env_BG_Desolate");
    //    Sound s2 = Array.Find(sounds, item => item.name == "Env_BG_Coniferous");

    //    while (s.source.volume < 1.0f)
    //    {
    //        s.source.volume += Time.deltaTime / musLerpSmooth;
    //        s1.source.volume -= Time.deltaTime / musLerpSmooth;
    //        s2.source.volume += ((Time.deltaTime / musLerpSmooth) * envVolumeMultiplier);
    //        yield return null;
    //    }
    //}

    //IEnumerator FadeSoundOut()
    //{
    //    Sound s = Array.Find(sounds, item => item.name == "Mus_Layer_B");
    //    Sound s1 = Array.Find(sounds, item => item.name == "Env_BG_Desolate");
    //    Sound s2 = Array.Find(sounds, item => item.name == "Env_BG_Coniferous");

    //    while (s.source.volume > 0.01f)
    //    {
    //        s.source.volume -= Time.deltaTime / musLerpSmooth;
    //        s1.source.volume += ((Time.deltaTime / musLerpSmooth) * envVolumeMultiplier);
    //        s2.source.volume -= Time.deltaTime / musLerpSmooth;
    //        yield return null;
    //    }
    //}
}

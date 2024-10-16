using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Scripting;

public class SoundMixerManager : MonoBehaviour
{
    public static SoundMixerManager instance;

    [SerializeField] private AudioMixer audioMixer;

    private void Awake() 
    {
        if (instance == null) 
        {
            instance = this;
        }
    }

    public void SetMasterVolume(float level) 
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(level) * 20f);
    }

    public void SetSFXVolume(float level) 
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(level) * 20f);
    }

    public void SetEngineVolume(float level) 
    {
        audioMixer.SetFloat("EngineVolume", Mathf.Log10(level) * 20f);
    }

    public void SetEngineSFXVolume(float level) 
    {
        audioMixer.SetFloat("EngineSFXVolume", Mathf.Log10(level) * 20f);
    }

    public void SetMusicVolume(float level) 
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(level) * 20f);
    }
}

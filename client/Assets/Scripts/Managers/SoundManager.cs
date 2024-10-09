using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private float fadeTime = 2;
    [SerializeField] private AudioClip hoverClip;
    [SerializeField] private AudioClip clickClip;

    private void Awake() 
    {
        if (instance == null) 
        {
            instance = this;
        }
    }

    public IEnumerator FadeIn(AudioSource audioSource, float fadeTime) 
    {
        float timeElapsed = 0;

        while (audioSource.volume < 1) 
        {
            audioSource.volume = Mathf.Lerp(0, 1, timeElapsed / fadeTime);
            timeElapsed += Time.deltaTime;
            yield return false;
        }
    }

    public IEnumerator FadeOut(AudioSource audioSource, float fadeTime) 
    {
        float timeElapsed = 0;

        while (audioSource.volume > 0) 
        {
            audioSource.volume = Mathf.Lerp(1, 0, timeElapsed / fadeTime);
            timeElapsed += Time.deltaTime;
            yield return false;
        }
    }

    public IEnumerator MusicFadeIn() 
    {
        float timeElapsed = 0;

        while (musicAudioSource.volume < 1) 
        {
            musicAudioSource.volume = Mathf.Lerp(0, 1, timeElapsed / fadeTime);
            timeElapsed += Time.deltaTime;
            yield return false;
        }
    }

    public IEnumerator MusicFadeOut() 
    {
        float timeElapsed = 0;

        while (musicAudioSource.volume > 0) 
        {
            musicAudioSource.volume = Mathf.Lerp(1, 0, timeElapsed / fadeTime);
            timeElapsed += Time.deltaTime;
            yield return false;
        }
    }

    public void PlayClip(AudioClip audioClip, Transform parent, float volumeLevel) 
    {
        AudioSource audioSource = Instantiate(sfxAudioSource, parent.position, Quaternion.identity);
        audioSource.clip = audioClip;
        audioSource.volume = volumeLevel;
        audioSource.Play();

        // Destroy instance after clip ends
        Destroy(audioSource.gameObject, audioSource.clip.length);
    }

    public void PlayDefaultHover() 
    {
        PlayClip(hoverClip, transform, 1);
    }

    public void PlayDefaultClick() 
    {
        PlayClip(clickClip, transform, 1);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [SerializeField] private AudioSource audioSourceObject;

    private void Awake() 
    {
        if (instance == null) 
        {
            instance = this;
        }
    }

    public void PlayClip(AudioClip audioClip, Transform parent, float volumeLevel) 
    {
        AudioSource audioSource = Instantiate(audioSourceObject, parent.position, Quaternion.identity);
        audioSource.clip = audioClip;
        audioSource.volume = volumeLevel;
        audioSource.Play();

        // Destroy instance after clip ends
        Destroy(audioSource.gameObject, audioSource.clip.length);
    }
}

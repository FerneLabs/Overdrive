using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenManager : MonoBehaviour
{
    public static ScreenManager instance;

    [SerializeField] GameObject[] screens;

    private void Awake() 
    {
        if (instance == null) 
        {
            instance = this;
        }
    }

    public void SetActiveScreen(string screenTag) 
    {
        if (screenTag.Length == 0) {
            Debug.LogError("[ScreenManager] [SetActiveScreen] Received empty tag, not setting active screen.");
            return;
        }
        foreach (var screen in screens)
        {
            screen.SetActive(screen.CompareTag(screenTag)); // Enable only screen matching received tag
        }
    }
}

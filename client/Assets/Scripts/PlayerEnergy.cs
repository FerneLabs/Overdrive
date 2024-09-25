using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEnergy : MonoBehaviour
{
    public int maxEnergy = 100;
    public int currentEnergy;
    public float regenerationRate = 5f; // Energía por segundo

    private void Start()
    {
        currentEnergy = maxEnergy;
        InvokeRepeating("RegenerateEnergy", 1f, 1f);
    }

    private void RegenerateEnergy()
    {
        currentEnergy += Mathf.FloorToInt(regenerationRate);
        if (currentEnergy > maxEnergy)
        {
            currentEnergy = maxEnergy;
        }
        Debug.Log($"Energía actual: {currentEnergy}");
    }

    public bool ConsumeEnergy(int amount)
    {
        if (currentEnergy >= amount)
        {
            currentEnergy -= amount;
            return true;
        }
        else
        {
            Debug.Log("No hay suficiente energía.");
            return false;
        }
    }

    public void IncreaseEnergy(int amount)
    {
        currentEnergy += amount;
        if (currentEnergy > maxEnergy)
        {
            currentEnergy = maxEnergy;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerEnergyUI : MonoBehaviour
{
    public PlayerEnergy playerEnergy;
    public Image energyBar;
    public TMPro.TMP_Text energyText;

    private void Update()
    {
        UpdateEnergyUI();
    }

    private void UpdateEnergyUI()
    {
        float fillAmount = (float)playerEnergy.currentEnergy / playerEnergy.maxEnergy;
        energyBar.fillAmount = fillAmount;
        energyText.text = playerEnergy.currentEnergy + " / " + playerEnergy.maxEnergy;
    }
}

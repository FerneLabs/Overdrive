using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotMachineUI : MonoBehaviour
{
    public SlotMachine slotMachine;
    public PlayerHand playerHand;
    public PlayerEnergy playerEnergy;
    public int energyCostPerRoll = 15;
    
    public List<Image> symbolImages;
    public Sprite avanceSprite;
    public Sprite ataqueSprite;
    public Sprite defensaSprite;
    public Sprite energiaSprite;
    
    private List<Symbol> currentRollSymbols;
    
    public void RollAndDisplaySymbols()
    {
        if (playerEnergy.ConsumeEnergy(energyCostPerRoll))
        {
            currentRollSymbols = slotMachine.Roll();

            for (int i = 0; i < currentRollSymbols.Count; i++)
            {
                // Asignar el sprite correspondiente al símbolo
                symbolImages[i].sprite = GetSpriteForSymbol(currentRollSymbols[i].Type);
                symbolImages[i].gameObject.SetActive(true);
            }
        }
        else
        {
            Debug.Log("No hay suficiente energía para una tirada.");
        }
    }

    private Sprite GetSpriteForSymbol(SymbolType type)
    {
        switch (type)
        {
            case SymbolType.Avance: return avanceSprite;
            case SymbolType.Ataque: return ataqueSprite;
            case SymbolType.Defensa: return defensaSprite;
            case SymbolType.Energia: return energiaSprite;
            default: return null;
        }
    }
    
    public void SaveSymbolToHand(int symbolIndex)
    {
        if (symbolIndex >= 0 && symbolIndex < currentRollSymbols.Count)
        {
            Symbol symbolToSave = currentRollSymbols[symbolIndex];

            if (playerHand.AddSymbolToHand(symbolToSave))
            {
                Debug.Log($"Símbolo {symbolToSave.Type} guardado en la mano.");
                symbolImages[symbolIndex].gameObject.SetActive(false);
                symbolImages[symbolIndex].sprite = null;
                currentRollSymbols[symbolIndex] = null;
            }
        }
    }
}

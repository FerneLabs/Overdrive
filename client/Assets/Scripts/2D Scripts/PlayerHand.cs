using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHand : MonoBehaviour
{
    public event Action OnHandUpdated;
    private List<Symbol> hand = new List<Symbol>();
    private const int maxHandSize = 5; // Puedes ajustar el tamaño máximo de la mano

    public bool AddSymbolToHand(Symbol symbol)
    {
        if (!hand.Contains(symbol))
        {
            if (hand.Count < maxHandSize)
            {
                hand.Add(symbol);
                OnHandUpdated?.Invoke(); // Disparar el evento
                return true;
            }
            else
            {
                Debug.Log("La mano está llena.");
                return false;
            }
        }
        else
        {
            Debug.Log("El símbolo ya está en la mano.");
            return false;
        }
    }

    public void RemoveSymbolFromHand(Symbol symbol)
    {
        if (hand.Contains(symbol))
        {
            hand.Remove(symbol);
            OnHandUpdated?.Invoke(); // Disparar el evento para actualizar la UI
        }
    }

    public List<Symbol> GetHand()
    {
        return hand;
    }

    public void UseSymbol(Symbol symbol)
    {
        if (hand.Contains(symbol))
        {
            ApplySymbolEffect(symbol);
            hand.Remove(symbol);
            OnHandUpdated?.Invoke();
        }
    }

    public void CombineSymbols(SymbolType type)
    {
        List<Symbol> symbolsToCombine = hand.FindAll(s => s.Type == type);

        if (symbolsToCombine.Count >= 2)
        {
            int combinedPower = 0;
            foreach (var sym in symbolsToCombine)
            {
                combinedPower += sym.Power;
            }

            // Si hay 3 o más símbolos, multiplicar el poder total por 3
            if (symbolsToCombine.Count >= 3)
            {
                combinedPower *= 3;
            }

            // Aplicar el efecto combinado
            ApplyCombinedEffect(type, combinedPower);

            // Eliminar los símbolos combinados de la mano
            foreach (var sym in symbolsToCombine)
            {
                hand.Remove(sym);
            }
        }
        else
        {
            Debug.Log("No hay suficientes símbolos para combinar.");
        }
    }

    private void ApplySymbolEffect(Symbol symbol)
    {
        // Aquí puedes aplicar el efecto del símbolo individual
        Debug.Log($"Aplicando efecto de símbolo: {symbol.Type} con poder {symbol.Power}");
    }

    private void ApplyCombinedEffect(SymbolType type, int totalPower)
    {
        // Aquí aplicas el efecto del símbolo combinado
        Debug.Log($"Aplicando efecto combinado de {type} con poder total {totalPower}");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotMachine : MonoBehaviour
{
    public List<Symbol> Roll()
    {
        List<Symbol> symbolsRolled = new List<Symbol>();

        // Generar 3 símbolos aleatorios
        for (int i = 0; i < 3; i++)
        {
            symbolsRolled.Add(GenerateRandomSymbol());
        }

        return symbolsRolled;
    }

    private Symbol GenerateRandomSymbol()
    {
        // Generar un tipo de símbolo aleatorio
        SymbolType randomType = (SymbolType)Random.Range(0, System.Enum.GetValues(typeof(SymbolType)).Length);
        // Generar un valor de potencia aleatorio (ajusta los valores mínimo y máximo según el diseño)
        int randomPower = Random.Range(1, 10);

        return new Symbol(randomType, randomPower);
    }
}

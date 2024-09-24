using UnityEngine;

public enum SymbolType
{
    Avance,
    Ataque,
    Defensa,
    Energia
}

public class Symbol
{
    public SymbolType Type { get; private set; }
    public int Power { get; private set; }

    public Symbol(SymbolType type, int power)
    {
        Type = type;
        Power = power;
    }
}

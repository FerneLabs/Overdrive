using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHandUI : MonoBehaviour
{
    public PlayerHand playerHand;
    public GameObject handSymbolPrefab;
    public Transform handContainer;
    public Sprite avanceSprite;
    public Sprite ataqueSprite;
    public Sprite defensaSprite;
    public Sprite energiaSprite;
    
    private List<GameObject> symbolObjects = new List<GameObject>();
    private void Start()
    {
        // Suscribirse al evento de actualización de la mano
        playerHand.OnHandUpdated += UpdateHandUI;
    }
    private void OnDestroy()
    {
        // Desuscribirse del evento al destruirse el objeto
        playerHand.OnHandUpdated -= UpdateHandUI;
    }

    private void UpdateHandUI()
    {
        foreach (var obj in symbolObjects)
        {
            Destroy(obj);
        }
        symbolObjects.Clear();
        
        // Crear los iconos de los símbolos en la mano
        foreach (var symbol in playerHand.GetHand())
        {
            GameObject symbolObject = Instantiate(handSymbolPrefab, handContainer);
            Image symbolImage = symbolObject.GetComponent<Image>();
            symbolImage.sprite = GetSpriteForSymbol(symbol.Type);
            
            Button button = symbolObject.GetComponentInChildren<Button>();
            button.onClick.AddListener(() => playerHand.UseSymbol(symbol));

            symbolObjects.Add(symbolObject);
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
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Attach to Screens Canvas to detect the pointer events in all UI elements
public class CursorManager : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [SerializeField] private Texture2D pointerCursor;
    [SerializeField] private Texture2D hoverCursor;
    [SerializeField] private Texture2D dragCursor;
    [SerializeField] private Texture2D textCursor;
    [SerializeField] private AudioClip hoverClip;
    [SerializeField] private AudioClip clickClip;

    private bool IsElementValid(GameObject element, string selector) 
    {
        bool isValid = false;
        switch (selector)
        {
            case "button":
                isValid = element.GetComponent<Button>() != null || element.GetComponentInParent<Button>() != null;
                break;
            case "dropdown":
                isValid = element.GetComponent<TMP_Dropdown>();
                break;
        }

        return isValid;
    }

    public void OnDrag()
    {
        Cursor.SetCursor(dragCursor, new Vector2(16, 16), CursorMode.Auto);
    }

    public void OnEndDrag()
    {
        Cursor.SetCursor(pointerCursor, new Vector2(12, 5), CursorMode.Auto);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        GameObject element = eventData.pointerCurrentRaycast.gameObject;
        if (!eventData.dragging && (IsElementValid(element, "button") || IsElementValid(element, "dropdown") )) 
        {
            SoundManager.instance.PlayClip(hoverClip, transform, .6f);
        }
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        GameObject element = eventData.pointerCurrentRaycast.gameObject;
        if (IsElementValid(element, "button")) 
        {
            SoundManager.instance.PlayClip(clickClip, transform, 1f);
        }
    }
}

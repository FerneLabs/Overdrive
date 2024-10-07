using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CursorManager : MonoBehaviour
{
    [SerializeField] private Texture2D pointerCursor;
    [SerializeField] private Texture2D hoverCursor;
    [SerializeField] private Texture2D dragCursor;
    [SerializeField] private Texture2D textCursor;

    private bool IsElementValid(GameObject element) 
    {
        return 
        element.GetComponent<TMP_InputField>() || element.GetComponentInParent<TMP_InputField>() || element.transform.parent.GetComponentInParent<TMP_InputField>() 
        || element.GetComponent<TMP_Dropdown>() 
        || element.GetComponent<Button>() || element.GetComponentInParent<Button>() 
        || element.GetComponent<CipherScript>() || element.GetComponentInParent<CipherScript>();
    }

    public void OnDrag()
    {
        Cursor.SetCursor(dragCursor, new Vector2(16, 16), CursorMode.Auto);
    }

    public void OnEndDrag()
    {
        Cursor.SetCursor(pointerCursor, new Vector2(12, 5), CursorMode.Auto);
    }

    // Leave it disabled for now, it seems a bit overwhelming with so many cursors, so just using the default pointer and drag ones

    // public void OnPointerEnter(PointerEventData eventData)
    // {
    //     if (!eventData.dragging && IsElementValid(eventData.pointerCurrentRaycast.gameObject)) 
    //     {
    //         // Cursor.SetCursor(hoverCursor, new Vector2(8, 8), CursorMode.Auto);
    //         // if (eventData.pointerCurrentRaycast.gameObject.GetComponent<TMP_InputField>()) { Cursor.SetCursor(textCursor, new Vector2(11, 16), CursorMode.Auto); }
    //         // else { Cursor.SetCursor(hoverCursor, new Vector2(8, 8), CursorMode.Auto); }   
    //     }
    // }

    // public void OnPointerExit(PointerEventData eventData)
    // {
    //     Cursor.SetCursor(pointerCursor, new Vector2(12, 5), CursorMode.Auto);
    // }
}

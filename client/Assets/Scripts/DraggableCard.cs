using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 startPosition;
    private Transform originalParent;
    private CanvasGroup canvasGroup;
    public Symbol symbol;
    
    private void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPosition = transform.position;
        originalParent = transform.parent;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }
    
    private bool IsPointerOverSlot(PointerEventData eventData)
    {
        return eventData.pointerEnter != null && eventData.pointerEnter.GetComponent<DropSlot>() != null;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (transform.parent == originalParent || !IsPointerOverSlot(eventData))
        {
            transform.position = startPosition;
        }
        else
        {
            // Notificar al PlayerHand que se ha movido el símbolo
            PlayerHand playerHand = originalParent.GetComponentInParent<PlayerHand>();
            if (playerHand != null)
            {
                playerHand.RemoveSymbolFromHand(symbol);
            }

            // Notificar al slot que ya no tiene la carta
            DropSlot previousSlot = originalParent.GetComponent<DropSlot>();
            if (previousSlot != null)
            {
                previousSlot.RemoveCardFromSlot();
            }
        }
        canvasGroup.blocksRaycasts = true;
    }
}

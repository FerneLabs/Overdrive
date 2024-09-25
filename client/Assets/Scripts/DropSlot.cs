using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropSlot : MonoBehaviour, IDropHandler
{
    private bool isSlotOccupied = false;
    private DraggableCard currentCardInSlot = null;
    
    public void OnDrop(PointerEventData eventData)
    {
        
        if (!isSlotOccupied)
        {
            DraggableCard draggedCard = eventData.pointerDrag.GetComponent<DraggableCard>();
            if (draggedCard != null && currentCardInSlot == null)
            {
                draggedCard.transform.SetParent(transform);
                draggedCard.transform.position = transform.position;
                currentCardInSlot = draggedCard;
            }
        }
        else
        {
            Debug.Log("El slot ya está ocupado.");
        }
    }
    
    public void RemoveCardFromSlot()
    {
        if (currentCardInSlot != null)
        {
            currentCardInSlot = null; // Limpiar la referencia al símbolo actual en el slot
        }
    }
}

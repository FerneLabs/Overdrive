using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public string cardType;
    public int cardValue;
    [SerializeField] private TMP_Text cardTypeTextTop;
    [SerializeField] private TMP_Text cardTypeTextBottom;
    [SerializeField] private RawImage cardTypeIconTop;
    [SerializeField] private RawImage cardTypeIconBottom;
    [SerializeField] private TMP_Text cardValueText;
    [SerializeField] private Texture2D[] typeIcons;
    // [SerializeField] private GameObject tintPanel;

    [SerializeField] private float dragRotationAmount = 10f; // Amount of rotation to apply based on drag
    [SerializeField] private float dragScaleFactor = 0.1f;   // Scaling factor to simulate stretch
    private Vector2 _lastMousePosition;
    private Vector3 _initialScale;
    private Quaternion _initialRotation;

    // Start is called before the first frame update
    private void Start()
    {
        cardTypeTextTop.text = cardType;
        cardTypeTextBottom.text = cardType;

        int typeIndex = 0;
        switch (cardType)
        {
            case "advance":
                typeIndex = 0;
                break;
            case "attack":
                typeIndex = 1;
                break;
            case "shield":
                typeIndex = 2;
                break;
            case "energize":
                typeIndex = 3;
                break;
            default:
                Debug.LogError("Unknown card type received!");
                break;
        }

        cardTypeIconTop.texture = typeIcons[typeIndex];
        cardTypeIconBottom.texture = typeIcons[typeIndex];

        cardValueText.text = $"{cardValue}";

        _initialScale = transform.localScale;
        _initialRotation = transform.rotation;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _lastMousePosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 currentMousePosition = eventData.position;
        Vector2 direction = (currentMousePosition - _lastMousePosition).normalized;

        // Rotate card while dragging
        // Apply Y-axis rotation for horizontal movement and X-axis for vertical
        float horizontalRotation = direction.x * dragRotationAmount;
        float verticalRotation = -direction.y * dragRotationAmount;
        transform.rotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0);
        Debug.Log($"Direction (x,y): {direction.x},{direction.y}, Rotation: {transform.rotation}");

        // Stretch card while dragging
        transform.localScale = new Vector3(
            _initialScale.x - direction.magnitude * dragScaleFactor,
            _initialScale.y + direction.magnitude * dragScaleFactor,
            _initialScale.z
        );

        // Update position
        transform.position = currentMousePosition;
        _lastMousePosition = currentMousePosition;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Reset rotation and scale to original values
        StartCoroutine(ResetTransform());
    }

    private IEnumerator ResetTransform()
    {
        float duration = 0.2f;
        float timeElapsed = 0f;

        Vector3 startScale = transform.localScale;
        Quaternion startRotation = transform.rotation;

        while (timeElapsed < duration)
        {
            transform.localScale = Vector3.Lerp(startScale, _initialScale, timeElapsed / duration);
            // Use initialRotation instead of Quaternion.identity to reset to original rotation
            transform.rotation = Quaternion.Lerp(startRotation, _initialRotation, timeElapsed / duration);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = _initialScale;
        transform.rotation = _initialRotation;
    }

    //public void OnBeginDrag(PointerEventData eventData)
    //{

    //}

    //public void OnEndDrag(PointerEventData eventData)
    //{

    //}
    
}

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler
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
    private Vector3 _initialPosition;
    private Vector3 _initialScale;
    private Quaternion _initialRotation;
    private bool _isHovered = false;
    private Card _hoveredCard = null;

    private GameObject _cipherContainer;
    private GameObject _deckContainer;

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
            case "defend":
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

        _cipherContainer = GameObject.FindGameObjectWithTag("CipherContainer");
        _deckContainer = GameObject.FindGameObjectWithTag("DeckContainer");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _lastMousePosition = eventData.position;
        _initialPosition = transform.position;
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
        // Debug.Log($"Direction (x,y): {direction.x},{direction.y}, Rotation: {transform.rotation}");

        // Stretch card while dragging
        transform.localScale = new Vector3(
            _initialScale.x - direction.magnitude * dragScaleFactor,
            _initialScale.y + direction.magnitude * dragScaleFactor,
            _initialScale.z
        );

        // Update position
        transform.position = currentMousePosition;
        _lastMousePosition = currentMousePosition;

        // Check for overlap with other cards for potential swapping
        CheckForCardOverlap();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log($"Drop is valid? {IsDropValid()}");
        if (IsDropValid())
        {
            if (_hoveredCard != null)
            {
                // Swap cards
                Transform tempParent = this.transform.parent;
                this.transform.SetParent(_hoveredCard.transform.parent);
                _hoveredCard.transform.SetParent(tempParent);

                _hoveredCard.transform.localScale = _initialScale;
                _hoveredCard = null;
            }
            else
            {
                // Check for the closest container
                float dist1 = Vector2.Distance(transform.position, _cipherContainer.transform.position);
                float dist2 = Vector2.Distance(transform.position, _deckContainer.transform.position);
                Transform closestContainer = dist1 < dist2 ? _cipherContainer.transform : _deckContainer.transform;

                // Drop card in container without swapping cards
                transform.SetParent(closestContainer);
            }
        } 
        else
        {
            // Return to original position
            transform.position = _initialPosition;
        }

        StartCoroutine(ResetTransform()); // Reset rotation and scale to original values
    }

    private bool IsDropValid()
    {
        RectTransform ciphers = _cipherContainer.GetComponent<RectTransform>();
        RectTransform deck = _deckContainer.GetComponent<RectTransform>();

        // Check if the container is being touched and if there is enough space for the current item to be dropped
        bool isCipherValid = RectTransformUtility.RectangleContainsScreenPoint(ciphers, Input.mousePosition) 
                            && _cipherContainer.transform.childCount < 3;

        bool isDeckValid = RectTransformUtility.RectangleContainsScreenPoint(deck, Input.mousePosition)
                            && _deckContainer.transform.childCount < 5;
        
        return isCipherValid || isDeckValid;
    }

    // Hover card before drag
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_isHovered)
        {
            _isHovered = true;
            transform.localScale *= 1.1f;  // Increase size slightly 
        }
    }
    // End hover
    public void OnPointerExit(PointerEventData eventData)
    {
        if (_isHovered)
        {
            _isHovered = false;
            transform.localScale = _initialScale;  // Reset scale
        }
    }



    private void CheckForCardOverlap()
    {
        bool overlapped = false;
        foreach (Card otherCard in FindObjectsOfType<Card>())
        {
            if (otherCard != this
                && otherCard != _hoveredCard
                && RectTransformUtility.RectangleContainsScreenPoint(otherCard.GetComponent<RectTransform>(), Input.mousePosition))
            {
                _hoveredCard = otherCard;
                _hoveredCard.transform.localScale *= 1.1f;  // Enlarge the card slightly to indicate a possible swap
                overlapped = true;
                return;
            }
        }

        if (_hoveredCard != null && !overlapped)
        {
            _hoveredCard.transform.localScale = _initialScale;
            _hoveredCard = null;
        }
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
            transform.rotation = Quaternion.Lerp(startRotation, _initialRotation, timeElapsed / duration);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = _initialScale;
        transform.rotation = _initialRotation;
    }
    
}

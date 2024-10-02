using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
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
    private bool _isDragActive = false;
    private string _collidedContainer = "";
    private GameObject _collidedCard = null;

    private GameObject _cipherContainer;
    private GameObject _deckContainer;
    private List<GameObject> _activeCollisions = new List<GameObject>(); 

    // Start is called before the first frame update
    private void Start()
    {
        PopulateCipher();

        _initialScale = transform.localScale;
        _initialRotation = transform.rotation;

        _cipherContainer = GameObject.FindGameObjectWithTag("CipherContainer");
        _deckContainer = GameObject.FindGameObjectWithTag("DeckContainer");
    }

    private void PopulateCipher()
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
    }

    // Hover card
    public void OnPointerEnter(PointerEventData eventData)
    {
        _isDragActive = eventData.dragging;

        if (_isHovered == false && _isDragActive == false)
        {
            _isHovered = true;
            transform.localScale *= 1.1f;
        }
    }

    // End hover
    public void OnPointerExit(PointerEventData eventData)
    {
        _isDragActive = eventData.dragging;

        if (_isHovered && !_isDragActive)
        {
            _isHovered = false;
            transform.localScale = _initialScale;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _isDragActive = true;

        // Workaround to render element on top of all other UI elements
        gameObject.AddComponent<Canvas>();
        gameObject.GetComponent<Canvas>().overrideSorting = true;
        gameObject.GetComponent<Canvas>().sortingOrder = 1;
        gameObject.AddComponent<GraphicRaycaster>();

        _lastMousePosition = eventData.position;
        _initialPosition = transform.position;
        Debug.Log($"[OnBeginDrag] Currently dragging: {gameObject} | _isDragActive {_isDragActive}");
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 currentMousePosition = eventData.position;
        Vector2 direction = currentMousePosition - _lastMousePosition;

        // Calculate the velocity of the mouse movement (distance over time)
        float velocity = direction.magnitude / Time.deltaTime;

        // Normalize the direction to prevent excessive rotation
        direction = direction.normalized;

        // Apply rotation based on the velocity and direction
        float horizontalRotation = Mathf.Lerp(0, direction.x * dragRotationAmount, velocity * 0.01f);
        float verticalRotation = Mathf.Lerp(0, -direction.y * dragRotationAmount, velocity * 0.01f);

        // Get the target rotation
        Quaternion targetRotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0);

        // Interpolate the current rotation to the target rotation based on time since last frame
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * velocity * 0.05f);

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

    public void OnEndDrag(PointerEventData eventData)
    {
        _isDragActive = false;

        Debug.Log($"[OnEndDrag] Drop is valid? {IsDropValid()} | Swap is valid? {IsSwapValid()} | Dragging? {_isDragActive}");
        if (transform.parent.tag == _collidedContainer) // If dropped in original container, return to original pos
        {
            transform.position = _initialPosition;
        }

        if (IsSwapValid()) // Swap cards
        {
            Transform tempParent = transform.parent;
            int tempIndex = transform.GetSiblingIndex();

            transform.SetParent(_collidedCard.transform.parent);
            transform.SetSiblingIndex(_collidedCard.transform.GetSiblingIndex());
            transform.position = _collidedCard.transform.position;

            _collidedCard.transform.SetParent(tempParent);
            _collidedCard.transform.SetSiblingIndex(tempIndex);
            _collidedCard.transform.position = _initialPosition;

            _collidedCard.transform.localScale = _initialScale;

            _collidedCard = null;
        }
        else if (IsDropValid()) // Drop card in container without swapping cards
        {
            Transform collidedContainer = GameObject.FindGameObjectWithTag(_collidedContainer).GetComponent<Transform>();
            transform.SetParent(collidedContainer);
        }
        else // Return to original position without dropping nor swapping
        {
            transform.position = _initialPosition;
        }

        StartCoroutine(ResetTransform()); // Reset rotation and scale to original values

        // Remove components to restore to default sorting layer
        Canvas canvas = gameObject.GetComponent<Canvas>();
        GraphicRaycaster gRaycaster = gameObject.GetComponent<GraphicRaycaster>();
        DestroyImmediate(gRaycaster);
        DestroyImmediate(canvas);
        

        _activeCollisions.Clear();
    }

    private bool IsDropValid()
    {
        // Check if the container is being touched and if there is enough space for the current item to be dropped
        bool isCipherValid = _collidedContainer == "CipherContainer" && _cipherContainer.transform.childCount < 3;

        bool isDeckValid = _collidedContainer == "DeckContainer" && _deckContainer.transform.childCount < 5;
        
        return isCipherValid || isDeckValid;
    }

    private bool IsSwapValid()
    {
        Debug.Log($"[IsSwapValid] {_collidedCard}, {_collidedContainer}");
        // return _collidedCard != null && _collidedContainer != "";
        return _collidedCard != null;
    }

    #nullable enable
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!_isDragActive) { return; }
        GameObject? collisionCard = CheckCollision(collision, true);

        Debug.Log($"[OnCollisionEnter2D] Collision with: {collision.gameObject} | Position: {collision.gameObject.transform.position}");
        Debug.Log($"[OnCollisionEnter2D] Current _collidedCard: {_collidedCard}");

        // Si ya hay una carta en colisión, restaura su tamaño a la escala original
        if (_collidedCard != null && collisionCard != _collidedCard) 
        {
            _collidedCard.transform.localScale = _initialScale;
            _collidedCard = null;
        }

        // Si se detecta una nueva carta en colisión, escalarla en un 10%
        if (collisionCard != null && collisionCard != _collidedCard) 
        {
            _collidedCard = collisionCard;
            _collidedCard.transform.localScale = _initialScale * 1.1f; // Aumentar el tamaño en un 10%
            _activeCollisions.Add(collisionCard);
            Debug.Log($"[OnCollisionEnter2D] Added to _activeCollisions: {collisionCard}");
        }
        Debug.Log($"[OnCollisionEnter2D] New _collidedCard: {_collidedCard}");
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!_isDragActive) { return; }

        if (collision.gameObject.CompareTag("Cipher"))
        {
            GameObject collisionCard = collision.gameObject;
            // GameObject? collisionCard = CheckCollision(collision, false);

            Debug.Log($"[OnCollisionExit2D] Current _collidedCard: {_collidedCard}, exiting collision with: {collisionCard}");
            _activeCollisions.Remove(collisionCard);
            
            if (_collidedCard == collisionCard)
            {
                collisionCard.transform.localScale = _initialScale;
                _collidedCard = null;
            }

            // Si hay más colisiones activas, selecciona la siguiente carta y escalarla un 15%
            Debug.Log($"Active collisions: {_activeCollisions.Count}");
            if (_activeCollisions.Count > 0) 
            {
                _collidedCard = _activeCollisions[0];
                _collidedCard.transform.localScale = _initialScale * 1.15f; // Aumentar el tamaño en un 15%
            }
            Debug.Log($"[OnCollisionExit2D] New _collidedCard: {_collidedCard}");
        } 
        else // Not Cipher on exit
        {
            _collidedContainer = "";
        }
    }
    
    private GameObject? CheckCollision(Collision2D collision, bool isEntry)
    {
        GameObject? collisionCard = null;

        if (collision.gameObject.CompareTag("Cipher"))
        {
            collisionCard = collision.gameObject;
        } 
        else if (isEntry) // Not Cipher on enter
        {
            _collidedContainer = collision.gameObject.tag;
        } 
        else // Not Cipher on exit
        {
            _collidedContainer = "";
        }

        return collisionCard;
    }
    #nullable enable

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

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    [SerializeField] private InputAction PlayerControls;
    private RaycastHit hit;
    private Vector2 mousePos;
    private Collider currentCollider;
    private Collider lastClicked;
    void Start()
    {
        lastClicked = null;
    }

    void Update()
    {
    }

    public void onMouseMove(InputAction.CallbackContext callback) {
        var value = callback.ReadValue<Vector2>();
        Physics.Raycast(Camera.main.ScreenPointToRay(value), out hit);
        mousePos = value;
        currentCollider = hit.collider;
    }

    public void onClick(InputAction.CallbackContext callback) {
        var wasPressed = callback.action.WasReleasedThisFrame();
        if (wasPressed) 
        {
            toggleHighlightRoom();
        }
    }

    public void toggleHighlightRoom()
    {
        lastClicked?.gameObject.GetComponent<RoomHandler>().toggleOutline(false);

        if (currentCollider && !currentCollider.tag.Contains("Core") && currentCollider != lastClicked)
        {
            currentCollider?.gameObject.GetComponent<RoomHandler>().toggleOutline(true);
            lastClicked = currentCollider;
        }
        else
        {
            lastClicked = null;
        }
    }
}

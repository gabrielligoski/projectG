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
        if (wasPressed) {
            if(lastClicked != null && !lastClicked.tag.Contains("Core"))
            {
                lastClicked.gameObject.GetComponent<RoomHandler>().toggleOutline(false);
            }
            if(currentCollider != null)
            {
                if (currentCollider == lastClicked)
                {
                    lastClicked = null;
                }
                else if (!currentCollider.tag.Contains("Core"))
                {
                    currentCollider.gameObject.GetComponent<RoomHandler>().toggleOutline(true);
                    lastClicked = currentCollider;
                }
            }
        
        }
    }
}

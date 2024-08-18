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
        if(hit.collider != null)
        {
            mousePos = value;
            currentCollider = hit.collider;
        }
    }

    public void onClick(InputAction.CallbackContext callback) {
        var wasPressed = callback.action.WasReleasedThisFrame();
        if (wasPressed && currentCollider) {
            if(lastClicked != null)
            {
                lastClicked.gameObject.GetComponent<RoomHandler>().toggleOutline(false);
            }
            lastClicked = currentCollider;
            currentCollider.gameObject.GetComponent<RoomHandler>().toggleOutline(true);
        
        }
    }
}

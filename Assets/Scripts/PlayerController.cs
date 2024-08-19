using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{

    [SerializeField] private InputAction PlayerControls;
    private RaycastHit hit;
    private Vector2 mousePos;
    private Collider currentCollider;
    private Collider lastClicked;

    public static Room.RoomType roomToInstance;

    void Start()
    {
        lastClicked = null;
        roomToInstance = Room.RoomType.none;
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
        if (EventSystem.current.IsPointerOverGameObject())
            return;
        var wasPressed = callback.action.WasReleasedThisFrame();
        if (wasPressed && roomToInstance == Room.RoomType.none) 
        {
            toggleHighlightRoom();
        } else if (wasPressed && currentCollider)
        {
            Debug.Log(roomToInstance.ToString());
            GameMaster.Instance.swapRoom(currentCollider.gameObject, roomToInstance);
            lastClicked?.gameObject.GetComponent<RoomHandler>().toggleOutline(false);
            lastClicked = null;
        }
    }

    public void setEmptyRoom(InputAction.CallbackContext callback) {
        var wasPressed = callback.action.WasReleasedThisFrame();
        if (wasPressed)
        {
            Debug.Log("set empty");
            roomToInstance = roomToInstance == Room.RoomType.empty? Room.RoomType.none: Room.RoomType.empty;
        }
    }

    public void toggleHighlightRoom()
    {

        lastClicked?.gameObject.GetComponent<RoomHandler>().toggleOutline(false);
        
        if (currentCollider && currentCollider.TryGetComponent(out RoomHandler rh) && !currentCollider.tag.Contains("Core") && currentCollider != lastClicked)
        {
            rh.toggleOutline(true);
            lastClicked = currentCollider;
        }
        else
        {
            lastClicked = null;
        }
    }
}

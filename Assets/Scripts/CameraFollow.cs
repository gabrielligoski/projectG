using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    private float minY = 2;
    private float maxY = 8;
    public float spd = 25f;
    public float smooth = 0.25f;
    private Vector2 movementInput;
    private Vector3 dir;
    private bool fastMove;

    private void Start()
    {
        gameObject.transform.position = GameMaster.Instance.map.getCorePosition() + new Vector3(0, 4, -2);
    }

    public void getMovementInput(InputAction.CallbackContext value)
    {
        movementInput = value.ReadValue<Vector2>();

        dir = new Vector3(movementInput.x, 0, movementInput.y);
    }

    public void getZoomInput(InputAction.CallbackContext value)
    {
        var y = -value.ReadValue<float>();
        var newY = gameObject.transform.position + new Vector3(0, y * Time.deltaTime * spd * (fastMove ? 4 : 1) / 4, 0);

        if (newY.y > minY && newY.y < maxY)
            gameObject.transform.position = newY;
    }

    public void setFastMove(InputAction.CallbackContext callback)
    {
        var isPressing = callback.action.IsPressed();
        fastMove = isPressing;
    }

    void LateUpdate()
    {
        var newPos = Vector3.Lerp(gameObject.transform.position, gameObject.transform.position + dir * Time.deltaTime * spd * (fastMove ? 4 : 1), smooth);
        if (GameMaster.Instance.map.getCorePosition().x - GameMaster.Instance.map.getMapSize() / 2 <= newPos.x &&
            GameMaster.Instance.map.getCorePosition().x + GameMaster.Instance.map.getMapSize() / 2 >= newPos.x &&
            GameMaster.Instance.map.getCorePosition().z - GameMaster.Instance.map.getMapSize() / 2 <= newPos.z &&
            GameMaster.Instance.map.getCorePosition().z + GameMaster.Instance.map.getMapSize() / 2 >= newPos.z)
        {
            gameObject.transform.position = newPos;
        }
    }
}
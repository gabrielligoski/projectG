using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    public float minY = 4;
    public float maxY = 20;
    public float spd = 25f;
    public float smooth = 0.25f;
    private Vector2 movementInput;
    private Vector3 dir;
    private bool fastMove;

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
        gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, gameObject.transform.position + dir * Time.deltaTime * spd * (fastMove ? 4 : 1), smooth);
    }
}
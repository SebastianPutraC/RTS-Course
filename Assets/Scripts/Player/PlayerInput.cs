using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private float keyboardPanSpeed = 5f;

    private void Update()
    {
        Vector2 moveAmount = Vector2.zero;

        if (Keyboard.current.upArrowKey.isPressed)
        {
            moveAmount.y += keyboardPanSpeed;
        }   
        if (Keyboard.current.downArrowKey.isPressed)
        {
            moveAmount.y -= keyboardPanSpeed;
        }
        if (Keyboard.current.leftArrowKey.isPressed)
        {
            moveAmount.x -= keyboardPanSpeed;
        }
        if (Keyboard.current.rightArrowKey.isPressed)
        {
            moveAmount.x += keyboardPanSpeed;
        }

        moveAmount *= Time.deltaTime;
        cameraTarget.position +=
            new Vector3(moveAmount.x, 0f, moveAmount.y);
    }
}

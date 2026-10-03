using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -20f;

    private CharacterController controller;
    private float verticalSpeed;
    private int jumpsUsed;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Reset jumps after landing. Keep a small downward push.
        if (controller.isGrounded && verticalSpeed <= 0f)
        {
            verticalSpeed = -2f;
            jumpsUsed = 0;
        }

        Vector3 movement = Vector3.zero;

        if (keyboard.aKey.isPressed) movement.x -= 1f;
        if (keyboard.dKey.isPressed) movement.x += 1f;
        if (keyboard.sKey.isPressed) movement.z -= 1f;
        if (keyboard.wKey.isPressed) movement.z += 1f;

        // Normalizing prevents faster diagonal movement.
        float speed = keyboard.leftShiftKey.isPressed ? runSpeed : walkSpeed;
        //movement = movement.normalized * speed;
        movement = transform.TransformDirection(movement.normalized) * speed;

        // Each press jumps once, with two jumps allowed before landing.
        if (keyboard.spaceKey.wasPressedThisFrame && jumpsUsed < 2)
        {
            verticalSpeed = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpsUsed++;
        }

        verticalSpeed += gravity * Time.deltaTime;
        movement.y = verticalSpeed;

        controller.Move(movement * Time.deltaTime);

        // Stop upward speed when hitting the underside of a platform.
        if ((controller.collisionFlags & CollisionFlags.Above) != 0
            && verticalSpeed > 0f)
        {
            verticalSpeed = 0f;
        }
    }
}
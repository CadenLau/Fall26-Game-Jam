using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerInput))]
public class PlayerScript : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private UIControllerScript controller;

    private PlayerInput playerInput;
    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Camera gameCamera;
    private Vector2 moveDirection;
    private readonly List<Collider2D> nearby = new();

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        gameCamera = Camera.main;

        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        playerInput.actions["Jump"].performed += Pop;
        playerInput.actions["Interact"].performed += PeekRecipe;
    }

    private void Update()
    {
        moveDirection = playerInput.actions["Move"].ReadValue<Vector2>();
        animator.SetBool("Moving", controller.IsPlaying && rb.linearVelocity.sqrMagnitude > 0.01f);

        if (controller.IsPlaying && moveDirection.x != 0f)
            spriteRenderer.flipX = moveDirection.x < 0f;
    }

    private void FixedUpdate()
    {
        if (!controller.IsPlaying)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 position = rb.position + Vector2.ClampMagnitude(moveDirection, 1f)
            * (moveSpeed * Time.fixedDeltaTime);

        Vector2 minimum = gameCamera.ViewportToWorldPoint(Vector3.zero);
        Vector2 maximum = gameCamera.ViewportToWorldPoint(Vector3.one);

        Bounds bounds = playerCollider.bounds;
        Vector2 lowerOffset = rb.position - (Vector2)bounds.min;
        Vector2 upperOffset = (Vector2)bounds.max - rb.position;
        position.x = Mathf.Clamp(position.x, minimum.x + lowerOffset.x, maximum.x - upperOffset.x);
        position.y = Mathf.Clamp(position.y, minimum.y + lowerOffset.y, maximum.y - upperOffset.y);
        rb.linearVelocity = (position - rb.position) / Time.fixedDeltaTime;
    }

    private void OnDisable()
    {
        playerInput.actions["Jump"].performed -= Pop;
        playerInput.actions["Interact"].performed -= PeekRecipe;
    }

    private void PeekRecipe(InputAction.CallbackContext context)
    {
        controller.PeekRecipe();
    }

    private void Pop(InputAction.CallbackContext context)
    {
        if (!controller.IsPlaying)
            return;

        playerCollider.Overlap(new ContactFilter2D { useTriggers = true }, nearby);
        int closestIndex = -1;
        float closestDistance = float.PositiveInfinity;

        for (int i = 0; i < nearby.Count; i++)
        {
            if (!nearby[i].CompareTag("PopObject"))
                continue;

            float distance = ((Vector2)nearby[i].transform.position - rb.position).sqrMagnitude;
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        if (closestIndex >= 0)
            nearby[closestIndex].GetComponent<BubbleScript>().Pop();
    }
}

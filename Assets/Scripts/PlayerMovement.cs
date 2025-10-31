using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 6f;

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 movement;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // 1️⃣ Read input (WASD + Arrows)
        Vector2 input = Vector2.zero;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) input.y += 1;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) input.y -= 1;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) input.x -= 1;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) input.x += 1;
            if (Keyboard.current.spaceKey.isPressed)
            {
                // RaycastHit2D ray = Physics2D.CircleCast(transform.position, 10f, Vector2.zero, 0);
                // Customer customer1 = ray.collider.gameObject.GetComponent<Customer>();
                // customer1.TakeOrder();
            }
        }
        RaycastHit2D ray = Physics2D.CircleCast(transform.position, 2.5f, Vector2.zero, 0);
        if (ray.collider != null && ray.collider.gameObject.GetComponent<Customer>() != null)
        {
            Customer customer1 = ray.collider.gameObject.GetComponent<Customer>();
            customer1.TakeOrder();

        }
        



        movement = input.normalized;

        bool isMoving = movement.sqrMagnitude > 0.01f;
        animator.SetBool("isMoving", isMoving);

        if (isMoving)
        {
            if (Mathf.Abs(movement.y) >= Mathf.Abs(movement.x))
            {
                animator.SetFloat("moveX", 0);
                animator.SetFloat("moveY", movement.y > 0 ? 1 : -1);
            }
            else
            {
                animator.SetFloat("moveX", movement.x > 0 ? 1 : -1);
                animator.SetFloat("moveY", 0);
                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = movement.x < 0;
                }
            }
        }
        else
        {
            // Idle
            animator.SetFloat("moveX", 0);
            animator.SetFloat("moveY", 0);
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 2.5f);
    }
}

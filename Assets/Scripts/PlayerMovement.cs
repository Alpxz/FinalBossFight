using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public int maxHealth = 100;
    public float rotationSpeed = 250f;
    private int currentHealth;
    private float x, y;
    private Animator animator;
    private bool isAnimationPlaying;





    private Rigidbody rb;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        currentHealth = maxHealth;
    }

    void Update()
    {
        isAnimationPlaying = animator.GetCurrentAnimatorStateInfo(0).IsName("Attack");
        Move();
        if (Input.GetMouseButtonDown(1) && !isAnimationPlaying)
        {
            Attack();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Dodge();
        }
    }

    void Move()
    {
        if (!isAnimationPlaying)
        {
            x = Input.GetAxis("Horizontal");
            y = Input.GetAxis("Vertical");

            transform.Rotate(0, x * Time.deltaTime * rotationSpeed, 0);
            transform.Translate(0, 0, y * Time.deltaTime * moveSpeed);

    

            animator.SetFloat("VelX", x);
            animator.SetFloat("VelY", y);
        }
    }

    void Attack()
    {
        Debug.Log("enta ataque");
        animator.Play("Attack");
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 2f))
        {
            if (hit.collider.CompareTag("Boss"))
            {
                hit.collider.GetComponent<Health>().TakeDamage(20);
            }
        }
    }

    void Dodge()
    {
        animator.Play("Dodge");
        transform.Translate(0, 0, Time.deltaTime * moveSpeed *10);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Player died!");
        // Aqu� podr�as recargar la escena o mostrar un mensaje de game over
    }
}

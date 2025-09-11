using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Enemy : MonoBehaviour
{
    public GameObject PointA;
    public GameObject PointB;

    private Rigidbody2D rb;
    private Transform currentTargetPoint;

    public float health;
    public float speed = 2f;
    public float stoppingDistance = 0.5f;
    public int contactDamage = 1;

    [Header("Knockback Settings")]
    public float knockbackForce = 8f;
    public float knockbackDuration = 0.5f;
    public float knockbackDecay = 0.7f;

    private bool isKnockedBack = false;
    private float knockbackTimer = 0f;
    private Vector2 knockbackDirection;
    private Vector2 movementDirection;

    public PlayerHealth playerHealth;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentTargetPoint = PointB.transform;
    }

    void Update()
    {
        if(health <= 0)
        {
            SceneManager.LoadScene("Death");
        }

        HandleKnockback();
        HandleMovement();
        HandleFlip();
        CheckTargetReached();
    }

    void HandleKnockback()
    {
        if (isKnockedBack)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0f)
            {
                isKnockedBack = false;
                rb.linearVelocity = movementDirection * speed;
            }
            else
            {
                float decayFactor = Mathf.Pow(knockbackDecay, Time.deltaTime * 10f);
                rb.linearVelocity = knockbackDirection * knockbackForce * (knockbackTimer / knockbackDuration);
            }
        }
    }

    void HandleMovement()
    {
        if (!isKnockedBack)
        {
            movementDirection = (currentTargetPoint.position - transform.position).normalized;
            rb.linearVelocity = new Vector2(movementDirection.x * speed, rb.linearVelocity.y);
        }
    }

    void HandleFlip()
    {
        if (!isKnockedBack)
        {
            if (movementDirection.x > 0.01f) 
            {
                transform.localScale = new Vector3(1, 1, 1); 
            }
            else if (movementDirection.x < -0.01f) 
            {
                transform.localScale = new Vector3(-1, 1, 1); 
            }
        }
    }

    void CheckTargetReached()
    {
        if (!isKnockedBack && Vector2.Distance(transform.position, currentTargetPoint.position) < stoppingDistance)
        {
            if (currentTargetPoint == PointB.transform)
            {
                currentTargetPoint = PointA.transform;
            }
            else if (currentTargetPoint == PointA.transform)
            {
                currentTargetPoint = PointB.transform;
            }
        }
    }

    void OnDrawGizmos()
    {
        if (PointA != null && PointB != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(PointA.transform.position, stoppingDistance);
            Gizmos.DrawWireSphere(PointB.transform.position, stoppingDistance);
            Gizmos.DrawLine(PointA.transform.position, PointB.transform.position);
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            playerHealth.TakeDamage(contactDamage);
        }
    }

    public void ApplyKnockback(Vector2 direction)
    {
        isKnockedBack = true;
        knockbackTimer = knockbackDuration;
        knockbackDirection = direction.normalized;
    }
}
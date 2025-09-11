using UnityEngine;

public class WallCollisionHandler : MonoBehaviour
{
    [Header("Collision Settings")]
    public LayerMask wallLayer = 1 << 7; // Layer 6 é normalmente "Wall"
    public float collisionCheckDistance = 1.38f;

    private Rigidbody2D rb;
    private bool isAgainstWall = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        CheckWallCollisions();
    }

    void CheckWallCollisions()
    {
        // Verifica colisão nas direções horizontal
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, Vector2.left, collisionCheckDistance, wallLayer);
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, collisionCheckDistance, wallLayer);

        isAgainstWall = hitLeft.collider != null || hitRight.collider != null;

        // Se estiver contra uma parede, para o movimento
        if (isAgainstWall && rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    public bool IsAgainstWall()
    {
        return isAgainstWall;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.left * collisionCheckDistance);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * collisionCheckDistance);
    }
}
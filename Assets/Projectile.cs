using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public Vector2 Direction { get; set; }
    public float speed = 10f;
    public int damage = 1;
    public float lifetime = 3f;

    [Header("Visual Effects")]
    [SerializeField] private GameObject impactEffect;
    [SerializeField] private LayerMask collisionLayers;

    private float lifetimeTimer;

    void Start()
    {
        lifetimeTimer = lifetime;

        // Destroi automaticamente ap�s o tempo de vida
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Move o proj�til
        transform.Translate(Direction * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verifica se colidiu com algo nas layers especificadas
        if (((1 << collision.gameObject.layer) & collisionLayers) != 0)
        {
            HandleCollision(collision);
        }
    }

    private void HandleCollision(Collider2D collision)
    {
        // Verifica se � o jogador
        if (collision.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }

        // Efeito de impacto
        if (impactEffect != null)
        {
            Instantiate(impactEffect, transform.position, Quaternion.identity);
        }

        // Destroi o proj�til
        Destroy(gameObject);
    }

    // Se quiser que o proj�til possa ser aparado
    public bool CanBeParried()
    {
        return true;
    }
}
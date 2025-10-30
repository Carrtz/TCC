using UnityEngine;

public class Shockwave : MonoBehaviour
{
    public float speed = 8f;
    public float lifetime = 3f;
    public int damage = 20;

    private Bounds arenaBounds;
    private bool movingRight = true;

    public void Initialize(Bounds arenaBounds)
    {
        this.arenaBounds = arenaBounds;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Move horizontalmente
        float direction = movingRight ? 1f : -1f;
        transform.Translate(Vector3.right * direction * speed * Time.deltaTime);

        // Verifica se atingiu as paredes da arena
        if (transform.position.x > arenaBounds.max.x || transform.position.x < arenaBounds.min.x)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Causa dano no player
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }
    }
}
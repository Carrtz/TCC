using UnityEngine;

public class Flip : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (rb != null)
        {
            // Flip baseado exclusivamente na direção do movimento
            // Isso funciona tanto para perseguição quanto para retorno
            if (rb.linearVelocity.x > 0.1f) // Direita
            {
                spriteRenderer.flipX = false;
            }
            else if (rb.linearVelocity.x < -0.1f) // Esquerda
            {
                spriteRenderer.flipX = true;
            }
            // Se velocidade x for muito baixa, mantém a direção atual
        }
    }
}
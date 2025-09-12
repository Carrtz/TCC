using UnityEngine;

public class StationaryShootingEnemy : MonoBehaviour
{
    [Header("Shooting Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float shootingRate = 2f;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private int projectileDamage = 1;
    [SerializeField] private float detectionRange = 10f;

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color shootingColor = Color.red;
    [SerializeField] private float colorChangeDuration = 0.2f;

    private Transform player;
    private float shootingTimer = 0f;
    private Color originalColor;
    private float colorChangeTimer = 0f;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        if (firePoint == null)
        {
            firePoint = transform;
            Debug.LogWarning("FirePoint not assigned! Using enemy transform.");
        }
    }

    void Update()
    {
        if (player == null)
        {
            // Tenta encontrar o jogador novamente
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            return;
        }

        // Verifica se o jogador está no alcance de detecção
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            // Atualiza o timer de disparo
            shootingTimer -= Time.deltaTime;

            if (shootingTimer <= 0f)
            {
                Shoot();
                shootingTimer = shootingRate;
            }

            // Mira na direção do jogador
            AimAtPlayer();
        }

        // Handle color feedback
        HandleColorFeedback();
    }

    private void AimAtPlayer()
    {
        // Calcula a direção para o jogador
        Vector2 direction = (player.position - transform.position).normalized;

        // Rotaciona o inimigo para mirar no jogador
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Ajusta a rotação baseado no flip do sprite
        if (spriteRenderer != null && spriteRenderer.flipX)
        {
            angle += 180f;
        }

        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void Shoot()
    {
        if (projectilePrefab == null || firePoint == null || player == null) return;

        // Instancia o projétil
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        // Configura o projétil
        Projectile projectileScript = projectile.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            Vector2 direction = (player.position - firePoint.position).normalized;
            projectileScript.Direction = direction;
            projectileScript.speed = projectileSpeed;
            projectileScript.damage = projectileDamage;
        }

        // Feedback visual
        if (spriteRenderer != null)
        {
            spriteRenderer.color = shootingColor;
            colorChangeTimer = colorChangeDuration;
        }

        Debug.Log("Enemy shot!");
    }

    private void HandleColorFeedback()
    {
        if (spriteRenderer != null && colorChangeTimer > 0f)
        {
            colorChangeTimer -= Time.deltaTime;
            if (colorChangeTimer <= 0f)
            {
                spriteRenderer.color = originalColor;
            }
        }
    }

    // Método para quando o inimigo é destruído
    private void OnDestroy()
    {
        // Pode adicionar efeitos de morte aqui
    }

    // Gizmos para visualização no editor
    private void OnDrawGizmosSelected()
    {
        // Alcance de detecção
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Direção de disparo (apenas se tiver firePoint)
        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(firePoint.position, firePoint.position + firePoint.right * 2f);
        }
    }
}
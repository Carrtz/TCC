using UnityEngine;
using System.Collections;

public class BossController : MonoBehaviour
{
    [Header("Referências")]
    public Transform player;
    public BoxCollider2D arenaBounds;
    public Transform leftShootPosition;
    public Transform rightShootPosition;
    
    [Header("Sprites por Estado")]
    public Sprite introSprite;
    public Sprite idleSprite;
    public Sprite dashSprite;
    public Sprite diveSprite;
    public Sprite shootSprite;
    public Sprite restSprite;
    
    [Header("Prefabs")]
    public GameObject shockwavePrefab;
    public GameObject projectilePrefab;
    
    [Header("Configurações")]
    public float dashSpeed = 15f;
    public float diveSpeed = 20f;
    public float projectileSpeed = 10f;
    public float diveDistance = 3f;
    public float introDuration = 2f;
    public float shortRestDuration = 1f;
    public float longRestDuration = 3f;
    public float attackDelay = 1f;
    public float betweenShotsDelay = 0.5f;
    
    private enum BossState { Intro, Idle, Attacking, Resting }
    private BossState currentState;
    private int consecutiveAttacks = 0;
    private bool fightStarted = false;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Vector3 introPosition;
    private Collider2D bossCollider;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        bossCollider = GetComponent<Collider2D>();
    }

    void Start()
    {
        currentState = BossState.Idle;
        introPosition = transform.position;
        UpdateSprite(idleSprite);
    }

    public void StartBossFight()
    {
        if (!fightStarted)
        {
            fightStarted = true;
            StartCoroutine(BossIntro());
        }
    }

    IEnumerator BossIntro()
    {
        currentState = BossState.Intro;
        UpdateSprite(introSprite);
        
        float timer = 0f;
        
        while (timer < introDuration)
        {
            transform.position = introPosition;
            timer += Time.deltaTime;
            yield return null;
        }
        
        StartCoroutine(BossBehaviorCycle());
    }

    IEnumerator BossBehaviorCycle()
    {
        while (fightStarted)
        {
            currentState = BossState.Idle;
            UpdateSprite(idleSprite);
            rb.linearVelocity = Vector2.zero;
            
            yield return new WaitForSeconds(1f);
            
            yield return StartCoroutine(ExecuteRandomAttack());
            consecutiveAttacks++;
            
            if (consecutiveAttacks == 1 && Random.Range(0, 2) == 0)
            {
                yield return StartCoroutine(ExecuteRandomAttack());
                consecutiveAttacks++;
                
                currentState = BossState.Resting;
                UpdateSprite(restSprite);
                yield return new WaitForSeconds(longRestDuration);
            }
            else
            {
                currentState = BossState.Resting;
                UpdateSprite(restSprite);
                yield return new WaitForSeconds(shortRestDuration);
            }
            
            consecutiveAttacks = 0;
        }
    }

    IEnumerator ExecuteRandomAttack()
    {
        currentState = BossState.Attacking;
        
        int attackType = Random.Range(0, 3);
        
        switch (attackType)
        {
            case 0:
                yield return StartCoroutine(DashAttack());
                break;
            case 1:
                yield return StartCoroutine(DiveAttack());
                break;
            case 2:
                yield return StartCoroutine(ShootAttack());
                break;
        }
        
        yield return new WaitForSeconds(attackDelay);
    }

IEnumerator DashAttack()
{
    UpdateSprite(dashSprite);
    
    // PASSO 1: Aparece em cima, um pouco para o lado do player
    float distanceToLeftWall = Mathf.Abs(player.position.x - arenaBounds.bounds.min.x);
    float distanceToRightWall = Mathf.Abs(player.position.x - arenaBounds.bounds.max.x);
    
    Vector3 spawnPosition;
    
    if (distanceToLeftWall < distanceToRightWall)
    {
        spawnPosition = player.position + Vector3.right * 2f;
    }
    else
    {
        spawnPosition = player.position + Vector3.left * 2f;
    }
    
    spawnPosition.y = player.position.y + diveDistance;
    transform.position = spawnPosition;
    
    yield return new WaitForSeconds(1f);
    
    // PASSO 2: Dash na direção do player
    Vector3 directionToPlayer = (player.position - transform.position).normalized;
    
    // GUARDA a direção horizontal que o boss está seguindo
    Vector3 horizontalDirection = new Vector3(directionToPlayer.x, 0, 0).normalized;
    
    Debug.Log($"DIREÇÃO HORIZONTAL DO DASH: {horizontalDirection}");
    
    float dashTimer = 0f;
    bool reachedGround = false;
    bool hitWallOrCeiling = false;
    Vector3 collisionPosition = Vector3.zero;
    
    while (dashTimer < 1f && !reachedGround && !hitWallOrCeiling)
    {
        Vector3 newPosition = transform.position + directionToPlayer * dashSpeed * Time.deltaTime;
        
        // Verifica se encostou no chão
        if (IsTouchingGround(newPosition))
        {
            reachedGround = true;
            newPosition.y = GetGroundHeight();
            Debug.Log("ENCostOU NO CHÃO!");
        }
        // Verifica se encostou nas paredes ou teto
        else if (IsTouchingWallOrCeiling(newPosition))
        {
            hitWallOrCeiling = true;
            collisionPosition = newPosition;
            Debug.Log("ENCostOU NA PAREDE OU TETO!");
        }
        
        transform.position = newPosition;
        dashTimer += Time.deltaTime;
        yield return null;
    }
    
    // AGORA: Todos os caminhos levam ao movimento para a parede DURANTE ESTE ATAQUE
    
    // CASO 1: Encostou nas paredes ou teto - cai até o chão E DEPOIS vai para parede
    if (hitWallOrCeiling)
    {
        Debug.Log("CAINDO ATÉ O CHÃO - Encostou na parede/teto");
        
        Vector3 groundPosition = new Vector3(transform.position.x, GetGroundHeight(), transform.position.z);
        
        while (transform.position.y > groundPosition.y + 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, groundPosition, diveSpeed * Time.deltaTime);
            yield return null;
        }
        
        transform.position = groundPosition;
        Debug.Log("CHEGOU NO CHÃO APÓS COLISÃO");
        
        // PEQUENA PAUSA antes de ir para a parede
        yield return new WaitForSeconds(0.3f);
        
        // Agora segue para a parede na mesma direção
        yield return StartCoroutine(MoveToWallInDirection(horizontalDirection));
    }
    // CASO 2: Encostou no chão durante o dash - vai direto para parede
    else if (reachedGround)
    {
        Debug.Log("CONTINUANDO ATAQUE - Encostou no chão");
        
        // PEQUENA PAUSA antes de ir para a parede
        yield return new WaitForSeconds(0.3f);
        
        // Segue para a parede na mesma direção
        yield return StartCoroutine(MoveToWallInDirection(horizontalDirection));
    }
    // CASO 3: Dash completo sem colisões - também vai para parede
    else
    {
        Debug.Log("DASH COMPLETO - Seguindo para parede");
        
        // PEQUENA PAUSA antes de ir para a parede
        yield return new WaitForSeconds(0.3f);
        
        // Segue para a parede na mesma direção
        yield return StartCoroutine(MoveToWallInDirection(horizontalDirection));
    }
    
    // Só termina o ataque de dash depois de fazer TUDO isso
    Debug.Log("ATAQUE DE DASH COMPLETO!");
}

    // MÉTODO ATUALIZADO: Move até a parede e depois vai para a parede contrária
    IEnumerator MoveToWallInDirection(Vector3 direction)
    {
        Debug.Log($"INICIANDO MOVIMENTO PARA PAREDE - Direção: {direction}");

        float targetX;
        if (direction.x > 0) // Movendo para direita
        {
            targetX = arenaBounds.bounds.max.x - GetBossHalfWidth();
            Debug.Log(">>> ALVO: Parede DIREITA");
        }
        else if (direction.x < 0) // Movendo para esquerda
        {
            targetX = arenaBounds.bounds.min.x + GetBossHalfWidth();
            Debug.Log(">>> ALVO: Parede ESQUERDA");
        }
        else // Direção neutra
        {
            targetX = (Random.Range(0, 2) == 0) ?
                arenaBounds.bounds.min.x + GetBossHalfWidth() :
                arenaBounds.bounds.max.x - GetBossHalfWidth();
            Debug.Log(">>> ALVO: Parede ALEATÓRIA");
        }

        Vector3 targetPosition = new Vector3(targetX, transform.position.y, transform.position.z);

        // Move até a primeira parede
        while (Mathf.Abs(transform.position.x - targetX) > 0.1f)
        {
            Vector3 newPosition = Vector3.MoveTowards(transform.position, targetPosition, dashSpeed * Time.deltaTime);

            if (IsTouchingWallOrCeiling(newPosition))
            {
                Debug.Log("Encostou na parede durante movimento!");
                break;
            }

            transform.position = newPosition;
            yield return null;
        }

        Debug.Log("CHEGOU NA PRIMEIRA PAREDE!");

        // AGORA: Espera 0.1s e vai para a parede contrária
        yield return new WaitForSeconds(0.1f);

        Debug.Log("INICIANDO MOVIMENTO PARA PAREDE CONTRÁRIA");

        // Calcula a direção contrária
        Vector3 oppositeDirection = -direction;

        // Determina qual é a parede contrária
        float oppositeTargetX;
        if (oppositeDirection.x > 0) // Agora movendo para direita (contrário da esquerda)
        {
            oppositeTargetX = arenaBounds.bounds.max.x - GetBossHalfWidth();
            Debug.Log(">>> ALVO CONTRÁRIO: Parede DIREITA");
        }
        else if (oppositeDirection.x < 0) // Agora movendo para esquerda (contrário da direita)
        {
            oppositeTargetX = arenaBounds.bounds.min.x + GetBossHalfWidth();
            Debug.Log(">>> ALVO CONTRÁRIO: Parede ESQUERDA");
        }
        else // Direção neutra (raro)
        {
            // Se a direção original era neutra, escolhe a parede oposta à atual
            if (Mathf.Abs(transform.position.x - arenaBounds.bounds.min.x) < Mathf.Abs(transform.position.x - arenaBounds.bounds.max.x))
            {
                oppositeTargetX = arenaBounds.bounds.max.x - GetBossHalfWidth(); // Atualmente na esquerda, vai para direita
            }
            else
            {
                oppositeTargetX = arenaBounds.bounds.min.x + GetBossHalfWidth(); // Atualmente na direita, vai para esquerda
            }
            Debug.Log(">>> ALVO CONTRÁRIO: Parede OPOSTA À ATUAL");
        }

        Vector3 oppositeTargetPosition = new Vector3(oppositeTargetX, transform.position.y, transform.position.z);

        // Move até a parede contrária
        while (Mathf.Abs(transform.position.x - oppositeTargetX) > 0.1f)
        {
            Vector3 newPosition = Vector3.MoveTowards(transform.position, oppositeTargetPosition, dashSpeed * Time.deltaTime);

            if (IsTouchingWallOrCeiling(newPosition))
            {
                Debug.Log("Encostou na parede contrária durante movimento!");
                break;
            }

            transform.position = newPosition;
            yield return null;
        }

        Debug.Log("CHEGOU NA PAREDE CONTRÁRIA! ATAQUE FINALIZADO.");
    }

    // NOVO MÉTODO: Verifica se está tocando no chão
    bool IsTouchingGround(Vector3 position)
    {
        if (bossCollider == null || arenaBounds == null) return false;
        
        // Calcula a posição predita do collider
        Bounds bossBounds = bossCollider.bounds;
        Vector3 offset = position - transform.position;
        Bounds predictedBounds = new Bounds(bossBounds.center + offset, bossBounds.size);
        
        // Verifica se a parte de BAIXO do boss está tocando ou passou do chão
        bool touchingGround = predictedBounds.min.y <= arenaBounds.bounds.min.y + 0.05f;
        
        return touchingGround;
    }

    // MÉTODO SIMPLIFICADO: Verifica paredes e teto
    bool IsTouchingWallOrCeiling(Vector3 position)
    {
        if (bossCollider == null || arenaBounds == null) return false;
        
        // Calcula a posição predita do collider
        Bounds bossBounds = bossCollider.bounds;
        Vector3 offset = position - transform.position;
        Bounds predictedBounds = new Bounds(bossBounds.center + offset, bossBounds.size);
        
        // Verifica PAREDES (lados)
        bool touchingLeftWall = predictedBounds.min.x <= arenaBounds.bounds.min.x + 0.05f;
        bool touchingRightWall = predictedBounds.max.x >= arenaBounds.bounds.max.x - 0.05f;
        
        // Verifica TETO
        bool touchingCeiling = predictedBounds.max.y >= arenaBounds.bounds.max.y - 0.05f;
        
        // DEBUG detalhado
        if (touchingLeftWall) Debug.Log(">>> ENCostOU PAREDE ESQUERDA");
        if (touchingRightWall) Debug.Log(">>> ENCostOU PAREDE DIREITA");
        if (touchingCeiling) Debug.Log(">>> ENCostOU TETO");
        
        return touchingLeftWall || touchingRightWall || touchingCeiling;
    }

    IEnumerator DiveAttack()
    {
        UpdateSprite(diveSprite);
        
        // Aparece acima do player
        Vector3 spawnPosition = new Vector3(player.position.x, player.position.y + diveDistance, player.position.z);
        transform.position = spawnPosition;
        
        // Segue o player por 1 segundo (apenas horizontalmente)
        float followTimer = 0f;
        while (followTimer < 1f)
        {
            Vector3 newPosition = new Vector3(player.position.x, transform.position.y, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, newPosition, dashSpeed * Time.deltaTime);
            followTimer += Time.deltaTime;
            yield return null;
        }
        
        // Desce rapidamente para o chão
        float groundY = GetGroundHeight();
        Vector3 groundPosition = new Vector3(transform.position.x, groundY, transform.position.z);
        
        while (transform.position.y > groundY + 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, groundPosition, diveSpeed * Time.deltaTime);
            yield return null;
        }
        
        transform.position = groundPosition;
        
        // Cria shockwaves
        CreateShockwave();
        
        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator ShootAttack()
    {
        UpdateSprite(shootSprite);
        
        // Escolhe lado para atirar
        bool shootFromRight = Random.Range(0, 2) == 0;
        Transform shootPosition = shootFromRight ? rightShootPosition : leftShootPosition;
        
        // Posiciona no ponto de tiro
        transform.position = shootPosition.position;
        
        // Vira para a direção do player
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (player.position.x > transform.position.x ? 1f : -1f);
        transform.localScale = scale;
        
        // Atira 3 vezes
        for (int i = 0; i < 3; i++)
        {
            CreateProjectile();
            yield return new WaitForSeconds(betweenShotsDelay);
        }
    }

    // NOVO MÉTODO: Verifica se o boss está tocando nos limites da arena
    bool IsTouchingArenaBoundary(Vector3 position)
    {
        if (bossCollider == null || arenaBounds == null) return false;
        
        // Calcula os limites do boss na posição especificada
        Bounds bossBounds = bossCollider.bounds;
        Vector3 offset = position - transform.position;
        Bounds predictedBounds = new Bounds(bossBounds.center + offset, bossBounds.size);
        
        // Verifica se o boss está tocando em qualquer limite da arena
        return predictedBounds.min.x <= arenaBounds.bounds.min.x ||
               predictedBounds.max.x >= arenaBounds.bounds.max.x ||
               predictedBounds.min.y <= arenaBounds.bounds.min.y ||
               predictedBounds.max.y >= arenaBounds.bounds.max.y;
    }

    // NOVO MÉTODO: Move para posição verificando limites
    IEnumerator MoveToPositionWithBoundaryCheck(Vector3 target, float speed)
    {
        while (Vector3.Distance(transform.position, target) > 0.1f)
        {
            Vector3 newPosition = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            
            // Se tocar no limite, para o movimento
            if (IsTouchingArenaBoundary(newPosition))
            {
                break;
            }
            
            transform.position = newPosition;
            yield return null;
        }
    }

    // MÉTODO AUXILIAR: Obtém a metade da largura do boss
    float GetBossHalfWidth()
    {
        if (bossCollider != null)
        {
            return bossCollider.bounds.extents.x;
        }
        return 0.5f; // Valor padrão
    }

    void CreateProjectile()
    {
        if (projectilePrefab == null) return;
        
        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        
        // Direção fixa para onde o player estava quando o tiro foi criado
        Vector2 direction = (player.position - transform.position).normalized;
        
        Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
        if (projectileRb != null)
        {
            projectileRb.linearVelocity = direction * projectileSpeed;
        }
        else
        {
            // Fallback se não tiver Rigidbody2D
            StartCoroutine(MoveProjectile(projectile, direction));
        }
    }

    void CreateShockwave()
    {
        if (shockwavePrefab == null) return;
        
        Vector3 spawnPos = transform.position;
        
        // Shockwave para esquerda
        GameObject shockwaveLeft = Instantiate(shockwavePrefab, spawnPos, Quaternion.identity);
        StartCoroutine(MoveShockwave(shockwaveLeft, Vector2.left));
        
        // Shockwave para direita
        GameObject shockwaveRight = Instantiate(shockwavePrefab, spawnPos, Quaternion.identity);
        StartCoroutine(MoveShockwave(shockwaveRight, Vector2.right));
    }

    IEnumerator MoveProjectile(GameObject projectile, Vector2 direction)
    {
        while (projectile != null && IsInArena(projectile.transform.position))
        {
            projectile.transform.Translate(direction * projectileSpeed * Time.deltaTime);
            yield return null;
        }
        
        if (projectile != null)
            Destroy(projectile);
    }

    IEnumerator MoveShockwave(GameObject shockwave, Vector2 direction)
    {
        while (shockwave != null && IsInArena(shockwave.transform.position))
        {
            shockwave.transform.Translate(direction * projectileSpeed * Time.deltaTime);
            yield return null;
        }
        
        if (shockwave != null)
            Destroy(shockwave);
    }

    IEnumerator MoveToPosition(Vector3 target, float speed)
    {
        while (Vector3.Distance(transform.position, target) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            yield return null;
        }
        transform.position = target;
    }

    bool IsInArena(Vector3 position)
    {
        if (arenaBounds == null) return true;
        return arenaBounds.bounds.Contains(position);
    }

    void UpdateSprite(Sprite newSprite)
    {
        if (spriteRenderer != null && newSprite != null)
        {
            spriteRenderer.sprite = newSprite;
        }
    }

    float GetGroundHeight()
    {
        if (arenaBounds == null || bossCollider == null) 
            return transform.position.y;
        
        return arenaBounds.bounds.min.y + bossCollider.bounds.extents.y;
    }
}
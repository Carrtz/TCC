using UnityEngine;
using System.Collections;

public class BossController : MonoBehaviour
{
    [Header("Configurações do Boss")]
    public float moveSpeed = 5f;
    public float dashSpeed = 15f;
    public float diveSpeed = 20f;
    public int health = 500;

    [Header("Referências")]
    public Transform player;
    public BoxCollider2D arenaBounds;
    public Transform leftExtreme;
    public Transform rightExtreme;
    public GameObject shockwavePrefab;
    public GameObject bulletPrefab;

    [Header("Sprites para cada estado")]
    public Sprite introSprite;
    public Sprite idleSprite;
    public Sprite dashSprite;
    public Sprite diveSprite;
    public Sprite shootSprite;
    public Sprite restSprite;

    [Header("Timings")]
    public float introDuration = 2f;
    public float idleDuration = 1f;
    public float attackWindup = 1f;
    public float shortRestDuration = 1f;
    public float longRestDuration = 3f;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private int currentHealth;
    private BossState currentState = BossState.Waiting;
    private Coroutine stateMachineCoroutine;

    // Controle de sequência de ataques
    private int consecutiveAttacks = 0;
    private bool canCombo = false;

    private enum BossState
    {
        Waiting,    // Novo estado: esperando player entrar
        Intro,
        Idle,
        DashAttack,
        DiveAttack,
        ShootAttack,
        ShortRest,
        LongRest
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        currentHealth = health;
        
        // Começa no estado de espera
        currentState = BossState.Waiting;
        Debug.Log("BOSS: Aguardando player entrar na arena");
    }

    // Chamado quando o player entra na arena
    public void StartBossFight()
    {
        if (currentState == BossState.Waiting)
        {
            Debug.Log("BOSS: Iniciando luta!");
            currentState = BossState.Intro;
            stateMachineCoroutine = StartCoroutine(BossStateMachine());
        }
    }

    private IEnumerator BossStateMachine()
    {
        while (currentHealth > 0)
        {
            switch (currentState)
            {
                case BossState.Intro:
                    yield return StartCoroutine(IntroState());
                    break;
                case BossState.Idle:
                    yield return StartCoroutine(IdleState());
                    break;
                case BossState.DashAttack:
                    yield return StartCoroutine(DashAttackState());
                    break;
                case BossState.DiveAttack:
                    yield return StartCoroutine(DiveAttackState());
                    break;
                case BossState.ShootAttack:
                    yield return StartCoroutine(ShootAttackState());
                    break;
                case BossState.ShortRest:
                    yield return StartCoroutine(ShortRestState());
                    break;
                case BossState.LongRest:
                    yield return StartCoroutine(LongRestState());
                    break;
            }
            yield return null;
        }
    }

    private IEnumerator IntroState()
    {
        Debug.Log("BOSS: Introdução");
        ChangeSprite(introSprite);
        
        // Animação de entrada
        Vector3 startPos = transform.position;
        Vector3 targetPos = new Vector3(0, 3, 0); // Posição inicial no topo
        
        float timer = 0f;
        while (timer < introDuration)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, targetPos, timer / introDuration);
            yield return null;
        }

        currentState = BossState.Idle;
    }

    private IEnumerator IdleState()
    {
        Debug.Log("BOSS: Idle");
        ChangeSprite(idleSprite);
        
        yield return new WaitForSeconds(idleDuration);

        // Decide próximo ataque
        int attackChoice = Random.Range(0, 3);
        switch (attackChoice)
        {
            case 0:
                currentState = BossState.DashAttack;
                break;
            case 1:
                currentState = BossState.DiveAttack;
                break;
            case 2:
                currentState = BossState.ShootAttack;
                break;
        }
    }

    private IEnumerator DashAttackState()
    {
        Debug.Log("BOSS: Dash Attack");
        ChangeSprite(dashSprite);
        consecutiveAttacks++;

        // 1. Aparece acima do jogador no lado mais distante
        bool spawnOnRight = ShouldSpawnOnRight();
        Vector3 spawnPosition = GetDashSpawnPosition(spawnOnRight);
        transform.position = spawnPosition;

        // Windup
        yield return new WaitForSeconds(attackWindup);

        // 2. Vai na direção do jogador
        Vector3 playerDirection = (player.position - transform.position).normalized;
        float dashTimer = 0f;
        float dashDuration = 0.5f;

        while (dashTimer < dashDuration)
        {
            dashTimer += Time.deltaTime;
            rb.linearVelocity = playerDirection * dashSpeed;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;

        // 3. Dash entre os extremos
        bool startedFromLeft = spawnOnRight; // Se spawnou na direita, vai primeiro pra esquerda
        yield return StartCoroutine(DashBetweenExtremes(startedFromLeft));

        // Decide próximo estado baseado no combo
        if (consecutiveAttacks == 1 && Random.Range(0, 2) == 0) // 50% chance de combo
        {
            canCombo = true;
            // Escolhe próximo ataque (não repete dash seguido)
            currentState = GetNextAttack(BossState.DashAttack);
        }
        else
        {
            canCombo = false;
            currentState = consecutiveAttacks == 1 ? BossState.ShortRest : BossState.LongRest;
        }
    }

    private IEnumerator DiveAttackState()
    {
        Debug.Log("BOSS: Dive Attack");
        ChangeSprite(diveSprite);
        consecutiveAttacks++;

        // 1. Aparece acima do jogador
        Vector3 spawnPosition = new Vector3(player.position.x, player.position.y + 4f, 0f);
        transform.position = spawnPosition;

        // 2. Segue o jogador por 1 segundo
        float followTimer = 0f;
        while (followTimer < 1f)
        {
            followTimer += Time.deltaTime;
            Vector3 targetPos = new Vector3(player.position.x, transform.position.y, 0f);
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // 3. Mergulha no chão
        float groundY = arenaBounds.bounds.min.y + 0.5f; // Pequeno offset do chão
        Vector3 diveTarget = new Vector3(transform.position.x, groundY, 0f);

        while (Vector3.Distance(transform.position, diveTarget) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, diveTarget, diveSpeed * Time.deltaTime);
            yield return null;
        }

        // 4. Cria onda de choque
        CreateShockwave();

        // Decide próximo estado baseado no combo
        if (consecutiveAttacks == 1 && Random.Range(0, 2) == 0) // 50% chance de combo
        {
            canCombo = true;
            currentState = GetNextAttack(BossState.DiveAttack);
        }
        else
        {
            canCombo = false;
            currentState = consecutiveAttacks == 1 ? BossState.ShortRest : BossState.LongRest;
        }
    }

    private IEnumerator ShootAttackState()
    {
        Debug.Log("BOSS: Shoot Attack");
        ChangeSprite(shootSprite);
        consecutiveAttacks++;

        // Para e mira no jogador
        rb.linearVelocity = Vector2.zero;

        // Atira 3 vezes com intervalo
        for (int i = 0; i < 3; i++)
        {
            // Mira no jogador
            Vector2 direction = (player.position - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Cria o projétil
            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.AngleAxis(angle, Vector3.forward));
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            bulletRb.linearVelocity = direction * 8f;

            yield return new WaitForSeconds(0.5f);
        }

        // Decide próximo estado baseado no combo
        if (consecutiveAttacks == 1 && Random.Range(0, 2) == 0) // 50% chance de combo
        {
            canCombo = true;
            currentState = GetNextAttack(BossState.ShootAttack);
        }
        else
        {
            canCombo = false;
            currentState = consecutiveAttacks == 1 ? BossState.ShortRest : BossState.LongRest;
        }
    }

    private IEnumerator ShortRestState()
    {
        Debug.Log("BOSS: Descanso Curto");
        ChangeSprite(restSprite);
        
        yield return new WaitForSeconds(shortRestDuration);
        
        consecutiveAttacks = 0;
        currentState = BossState.Idle;
    }

    private IEnumerator LongRestState()
    {
        Debug.Log("BOSS: Descanso Longo");
        ChangeSprite(restSprite);
        
        yield return new WaitForSeconds(longRestDuration);
        
        consecutiveAttacks = 0;
        currentState = BossState.Idle;
    }

    // Métodos auxiliares
    private bool ShouldSpawnOnRight()
    {
        float distanceToLeftWall = Mathf.Abs(player.position.x - arenaBounds.bounds.min.x);
        float distanceToRightWall = Mathf.Abs(player.position.x - arenaBounds.bounds.max.x);
        
        return distanceToRightWall > distanceToLeftWall;
    }

    private Vector3 GetDashSpawnPosition(bool spawnOnRight)
    {
        float xPos = spawnOnRight ? arenaBounds.bounds.max.x - 1f : arenaBounds.bounds.min.x + 1f;
        return new Vector3(xPos, player.position.y + 3f, 0f);
    }

    private IEnumerator DashBetweenExtremes(bool startFromLeft)
    {
        Vector3 firstTarget = startFromLeft ? leftExtreme.position : rightExtreme.position;
        Vector3 secondTarget = startFromLeft ? rightExtreme.position : leftExtreme.position;

        // Primeiro dash
        while (Vector3.Distance(transform.position, firstTarget) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, firstTarget, dashSpeed * Time.deltaTime);
            yield return null;
        }

        // Segundo dash
        while (Vector3.Distance(transform.position, secondTarget) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, secondTarget, dashSpeed * Time.deltaTime);
            yield return null;
        }
    }

    private void CreateShockwave()
    {
        GameObject shockwave = Instantiate(shockwavePrefab, transform.position, Quaternion.identity);
        Shockwave waveScript = shockwave.GetComponent<Shockwave>();
        if (waveScript != null)
        {
            waveScript.Initialize(arenaBounds.bounds);
        }
    }

    private BossState GetNextAttack(BossState currentAttack)
    {
        // Escolhe um ataque diferente do atual
        BossState nextAttack;
        do
        {
            nextAttack = (BossState)Random.Range(2, 5); // 2-4 são os ataques
        } while (nextAttack == currentAttack);

        return nextAttack;
    }

    private void ChangeSprite(Sprite newSprite)
    {
        if (spriteRenderer != null && newSprite != null)
        {
            spriteRenderer.sprite = newSprite;
        }
    }

    public void TakeDamage(int damage)
    {
        if (currentState == BossState.Waiting || currentState == BossState.Intro) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        // Efeito de dano
        StartCoroutine(DamageFlash());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator DamageFlash()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }

    private void Die()
    {
        Debug.Log("BOSS DERROTADO!");
        if (stateMachineCoroutine != null)
            StopCoroutine(stateMachineCoroutine);
        
        // Aqui você pode adicionar animação de morte, etc.
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerBullet"))
        {
            TakeDamage(10);
            Destroy(other.gameObject);
        }
    }
}
using UnityEngine;
using System.Collections;

public class BossController : MonoBehaviour
{
    [Header("Referências")]
    public Transform player;
    public BoxCollider2D arenaBounds;
    public Transform leftShootPosition;
    public Transform rightShootPosition;
    public PlayerHealth playerHealth;
    public GameObject[] objectsToDestroy;
    
    [Header("Componentes")]
    public Animator bossAnimator;
    
    [Header("Prefabs")]
    public GameObject shockwavePrefab;
    public GameObject projectilePrefab;
    
    [Header("Configurações de Movimento")]
    public float projectileSpeed = 10f;
    
    [Header("Configurações de Tempo")]
    public float introDuration = 2f;
    public float shortRestDuration = 1f;
    public float longRestDuration = 3f;
    public float attackDelay = 1f;
    
    [Header("Configurações de Colisão")]
    public float groundCheckTolerance = 0.05f;
    public float wallCheckTolerance = 0.05f;
    public float movementStopThreshold = 0.1f;

    [Header("Configurações do Dash Attack")]
    public float dashSpeed = 15f;
    public float dashAttackSpawnDistance = 2f;
    public float dashAttackAimTime = 1f;
    public float dashAttackDuration = 1f;
    public float dashAttackWallPause = 0.1f;
    
    [Header("Configurações do Dive Attack")]
    public float diveSpeed = 20f;
    public float diveDistance = 3f;
    public float diveAttackFollowTime = 1f;
    public float diveAttackPostShockwavePause = 0.5f;
    
    [Header("Configurações do Shoot Attack")]
    public float shootAttackWarningTime = 0.5f;
    public int shootAttackProjectileCount = 3;
    public float shootAttackBetweenShotsDelay = 0.5f;

    [Header("Configurações de Dano")]
    public int damageAmount = 1;
    public float damageCooldown = 1f;

    private readonly int animIntro = Animator.StringToHash("Intro");
    private readonly int animIdle = Animator.StringToHash("Idle");
    private readonly int animDashFall = Animator.StringToHash("DashFall");
    private readonly int animDashImpact = Animator.StringToHash("DashImpact");
    private readonly int animDashLeft = Animator.StringToHash("DashLeft");
    private readonly int animDashRight = Animator.StringToHash("DashRight");
    private readonly int animDashWallHit = Animator.StringToHash("DashWallHit");
    private readonly int animRest = Animator.StringToHash("Rest");
    private readonly int animLongRest = Animator.StringToHash("LongRest");
    private readonly int animDiveStart = Animator.StringToHash("DiveStart");
    private readonly int animDiveFall = Animator.StringToHash("DiveFall");
    private readonly int animDiveImpact = Animator.StringToHash("DiveImpact");
    private readonly int animShootStart = Animator.StringToHash("ShootStart");
    private readonly int animShooting = Animator.StringToHash("Shooting");
    private readonly int animShootEnd = Animator.StringToHash("ShootEnd");
    private readonly int animDeath = Animator.StringToHash("Death");
    
    private enum BossState { Intro, Idle, Attacking, Resting, Dead }
    private BossState currentState;
    private int consecutiveAttacks = 0;
    private bool fightStarted = false;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Vector3 introPosition;
    private Collider2D bossCollider;
    private bool canDamage = true;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        bossCollider = GetComponent<Collider2D>();
        
        if (bossAnimator == null)
            bossAnimator = GetComponent<Animator>();
    }

    void Start()
    {
        currentState = BossState.Idle;
        introPosition = transform.position;
        SetAnimation(animIdle);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        HandlePlayerDamage(collision.gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        HandlePlayerDamage(other.gameObject);
    }

    void HandlePlayerDamage(GameObject otherObject)
    {
        if (canDamage && playerHealth != null && otherObject.CompareTag("Player"))
        {
            playerHealth.TakeDamage(damageAmount);
            StartCoroutine(DamageCooldownRoutine());
        }
    }

    IEnumerator DamageCooldownRoutine()
    {
        canDamage = false;
        yield return new WaitForSeconds(damageCooldown);
        canDamage = true;
    }

    public void StartBossFight()
    {
        if (!fightStarted && currentState != BossState.Dead)
        {
            fightStarted = true;
            StartCoroutine(BossIntro());
        }
    }

    public void StartDeath()
    {
        if (currentState == BossState.Dead) return;
        
        currentState = BossState.Dead;
        fightStarted = false;
        StopAllCoroutines();
        StartCoroutine(DeathRoutine());
    }

    IEnumerator DeathRoutine()
    {
        rb.linearVelocity = Vector2.zero;
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        SetAnimation(animDeath);

        yield return new WaitForSeconds(5f);

        Debug.Log("Boss morreu!");

        // Destruir todos os objetos no array
        if (objectsToDestroy != null && objectsToDestroy.Length > 0)
        {
            foreach (GameObject obj in objectsToDestroy)
            {
                if (obj != null)
                {
                    Destroy(obj);
                    Debug.Log("Objeto destruído: " + obj.name);
                }
            }
        }
    }

    IEnumerator BossIntro()
    {
        currentState = BossState.Intro;
        SetAnimation(animIntro);
        
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
        while (fightStarted && currentState != BossState.Dead)
        {
            currentState = BossState.Idle;
            SetAnimation(animIdle);
            rb.linearVelocity = Vector2.zero;
            
            yield return new WaitForSeconds(0.5f);
            
            yield return StartCoroutine(ExecuteRandomAttack());
            consecutiveAttacks++;
            
            if (consecutiveAttacks == 1 && Random.Range(0, 2) == 0)
            {
                yield return StartCoroutine(ExecuteRandomAttack());
                consecutiveAttacks++;
                
                currentState = BossState.Resting;
                SetAnimation(animLongRest);
                yield return new WaitForSeconds(longRestDuration);
            }
            else
            {
                currentState = BossState.Resting;
                SetAnimation(animRest);
                yield return new WaitForSeconds(shortRestDuration);
            }
            
            consecutiveAttacks = 0;
        }
    }

    IEnumerator ExecuteRandomAttack()
    {
        if (currentState == BossState.Dead) yield break;
        
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
        if (currentState == BossState.Dead) yield break;
        
        float distanceToLeftWall = Mathf.Abs(player.position.x - arenaBounds.bounds.min.x);
        float distanceToRightWall = Mathf.Abs(player.position.x - arenaBounds.bounds.max.x);
        
        Vector3 spawnPosition;
        bool isRightSide;
        
        if (distanceToLeftWall < distanceToRightWall)
        {
            spawnPosition = player.position + Vector3.right * dashAttackSpawnDistance;
            isRightSide = true;
        }
        else
        {
            spawnPosition = player.position + Vector3.left * dashAttackSpawnDistance;
            isRightSide = false;
        }
        
        spawnPosition.y = player.position.y + diveDistance;
        transform.position = spawnPosition;
        
        SetAnimation(animDashFall);
        yield return new WaitForSeconds(dashAttackAimTime);
        
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        Vector3 horizontalDirection = new Vector3(directionToPlayer.x, 0, 0).normalized;
        
        SetAnimation(animDashImpact);
        yield return new WaitForSeconds(0.2f);
        
        if (horizontalDirection.x > 0)
        {
            SetAnimation(animDashRight);
        }
        else
        {
            SetAnimation(animDashLeft);
        }
        
        float dashTimer = 0f;
        bool reachedGround = false;
        bool hitWallOrCeiling = false;
        
        while (dashTimer < dashAttackDuration && !reachedGround && !hitWallOrCeiling && currentState != BossState.Dead)
        {
            Vector3 newPosition = transform.position + directionToPlayer * dashSpeed * Time.deltaTime;
            
            if (IsTouchingGround(newPosition))
            {
                reachedGround = true;
                newPosition.y = GetGroundHeight();
            }
            else if (IsTouchingWallOrCeiling(newPosition))
            {
                hitWallOrCeiling = true;
            }
            
            transform.position = newPosition;
            dashTimer += Time.deltaTime;
            yield return null;
        }

        if (currentState == BossState.Dead) yield break;

        if (hitWallOrCeiling)
        {
            SetAnimation(animDashWallHit);
            
            Vector3 groundPosition = new Vector3(transform.position.x, GetGroundHeight(), transform.position.z);
            
            while (transform.position.y > groundPosition.y + movementStopThreshold && currentState != BossState.Dead)
            {
                transform.position = Vector3.MoveTowards(transform.position, groundPosition, diveSpeed * Time.deltaTime);
                yield return null;
            }
            
            transform.position = groundPosition;
            
            SetAnimation(animIdle);
            yield return new WaitForSeconds(dashAttackWallPause);
            
            yield return StartCoroutine(MoveToWallInDirection(horizontalDirection));
        }
        else if (reachedGround)
        {
            SetAnimation(animIdle);
            yield return new WaitForSeconds(dashAttackWallPause);
            
            yield return StartCoroutine(MoveToWallInDirection(horizontalDirection));
        }
        else
        {
            SetAnimation(animIdle);
            yield return new WaitForSeconds(dashAttackWallPause);
            
            yield return StartCoroutine(MoveToWallInDirection(horizontalDirection));
        }
    }

    IEnumerator MoveToWallInDirection(Vector3 direction)
    {
        if (currentState == BossState.Dead) yield break;
        
        float targetX;
        if (direction.x > 0)
        {
            targetX = arenaBounds.bounds.max.x - GetBossHalfWidth();
            SetAnimation(animDashRight);
        }
        else
        {
            targetX = arenaBounds.bounds.min.x + GetBossHalfWidth();
            SetAnimation(animDashLeft);
        }

        Vector3 targetPosition = new Vector3(targetX, transform.position.y, transform.position.z);

        while (Mathf.Abs(transform.position.x - targetX) > movementStopThreshold && currentState != BossState.Dead)
        {
            Vector3 newPosition = Vector3.MoveTowards(transform.position, targetPosition, dashSpeed * Time.deltaTime);
            if (IsTouchingWallOrCeiling(newPosition)) break;
            transform.position = newPosition;
            yield return null;
        }

        SetAnimation(animIdle);
        yield return new WaitForSeconds(dashAttackWallPause);

        Vector3 oppositeDirection = -direction;
        float oppositeTargetX;
        
        if (oppositeDirection.x > 0)
        {
            oppositeTargetX = arenaBounds.bounds.max.x - GetBossHalfWidth();
            SetAnimation(animDashRight);
        }
        else
        {
            oppositeTargetX = arenaBounds.bounds.min.x + GetBossHalfWidth();
            SetAnimation(animDashLeft);
        }

        Vector3 oppositeTargetPosition = new Vector3(oppositeTargetX, transform.position.y, transform.position.z);

        while (Mathf.Abs(transform.position.x - oppositeTargetX) > movementStopThreshold && currentState != BossState.Dead)
        {
            Vector3 newPosition = Vector3.MoveTowards(transform.position, oppositeTargetPosition, dashSpeed * Time.deltaTime);
            if (IsTouchingWallOrCeiling(newPosition)) break;
            transform.position = newPosition;
            yield return null;
        }
        
        SetAnimation(animIdle);
    }

    IEnumerator DiveAttack()
    {
        if (currentState == BossState.Dead) yield break;
        
        SetAnimation(animDiveStart);
        
        Vector3 spawnPosition = new Vector3(player.position.x, player.position.y + diveDistance, player.position.z);
        transform.position = spawnPosition;
        
        SetAnimation(animDiveFall);
        
        float followTimer = 0f;
        while (followTimer < diveAttackFollowTime && currentState != BossState.Dead)
        {
            Vector3 newPosition = new Vector3(player.position.x, transform.position.y, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, newPosition, dashSpeed * Time.deltaTime);
            followTimer += Time.deltaTime;
            yield return null;
        }
        
        if (currentState == BossState.Dead) yield break;
        
        SetAnimation(animDiveImpact);
        
        float groundY = GetGroundHeight();
        Vector3 groundPosition = new Vector3(transform.position.x, groundY, transform.position.z);
        
        while (transform.position.y > groundY + movementStopThreshold && currentState != BossState.Dead)
        {
            transform.position = Vector3.MoveTowards(transform.position, groundPosition, diveSpeed * Time.deltaTime);
            yield return null;
        }
        
        transform.position = groundPosition;
        CreateShockwave();
        
        yield return new WaitForSeconds(diveAttackPostShockwavePause);
        SetAnimation(animIdle);
    }

    IEnumerator ShootAttack()
    {
        if (currentState == BossState.Dead) yield break;
        
        float distanceToLeftWall = Mathf.Abs(player.position.x - leftShootPosition.position.x);
        float distanceToRightWall = Mathf.Abs(player.position.x - rightShootPosition.position.x);
        
        Transform shootPosition;
        
        if (distanceToLeftWall < distanceToRightWall)
        {
            shootPosition = rightShootPosition;
        }
        else
        {
            shootPosition = leftShootPosition;
        }
        
        transform.position = shootPosition.position;
        
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (player.position.x > transform.position.x ? 1f : -1f);
        transform.localScale = scale;
        
        SetAnimation(animShootStart);
        yield return new WaitForSeconds(shootAttackWarningTime);
        
        SetAnimation(animShooting);
        
        for (int i = 0; i < shootAttackProjectileCount; i++)
        {
            if (currentState == BossState.Dead) yield break;
            CreateProjectile();
            yield return new WaitForSeconds(shootAttackBetweenShotsDelay);
        }
        
        SetAnimation(animShootEnd);
        yield return new WaitForSeconds(0.3f);
        SetAnimation(animIdle);
    }

    private void SetAnimation(int animationHash)
    {
        if (bossAnimator != null && currentState != BossState.Dead)
        {
            bossAnimator.SetBool(animIntro, false);
            bossAnimator.SetBool(animIdle, false);
            bossAnimator.SetBool(animDashFall, false);
            bossAnimator.SetBool(animDashImpact, false);
            bossAnimator.SetBool(animDashLeft, false);
            bossAnimator.SetBool(animDashRight, false);
            bossAnimator.SetBool(animDashWallHit, false);
            bossAnimator.SetBool(animRest, false);
            bossAnimator.SetBool(animLongRest, false);
            bossAnimator.SetBool(animDiveStart, false);
            bossAnimator.SetBool(animDiveFall, false);
            bossAnimator.SetBool(animDiveImpact, false);
            bossAnimator.SetBool(animShootStart, false);
            bossAnimator.SetBool(animShooting, false);
            bossAnimator.SetBool(animShootEnd, false);
            bossAnimator.SetBool(animDeath, false);

            bossAnimator.SetBool(animationHash, true);
        }
    }

    bool IsTouchingGround(Vector3 position)
    {
        if (bossCollider == null || arenaBounds == null) return false;
        
        Bounds bossBounds = bossCollider.bounds;
        Vector3 offset = position - transform.position;
        Bounds predictedBounds = new Bounds(bossBounds.center + offset, bossBounds.size);
        
        return predictedBounds.min.y <= arenaBounds.bounds.min.y + groundCheckTolerance;
    }

    bool IsTouchingWallOrCeiling(Vector3 position)
    {
        if (bossCollider == null || arenaBounds == null) return false;
        
        Bounds bossBounds = bossCollider.bounds;
        Vector3 offset = position - transform.position;
        Bounds predictedBounds = new Bounds(bossBounds.center + offset, bossBounds.size);
        
        bool touchingLeftWall = predictedBounds.min.x <= arenaBounds.bounds.min.x + wallCheckTolerance;
        bool touchingRightWall = predictedBounds.max.x >= arenaBounds.bounds.max.x - wallCheckTolerance;
        bool touchingCeiling = predictedBounds.max.y >= arenaBounds.bounds.max.y - wallCheckTolerance;
        
        return touchingLeftWall || touchingRightWall || touchingCeiling;
    }

    float GetBossHalfWidth()
    {
        if (bossCollider != null)
            return bossCollider.bounds.extents.x;
        return 0.5f;
    }

    void CreateProjectile()
    {
        if (projectilePrefab == null || currentState == BossState.Dead) return;
        
        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        Vector2 direction = (player.position - transform.position).normalized;
        
        Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
        if (projectileRb != null)
        {
            projectileRb.linearVelocity = direction * projectileSpeed;
        }
        else
        {
            StartCoroutine(MoveProjectile(projectile, direction));
        }
    }

    void CreateShockwave()
    {
        if (shockwavePrefab == null || currentState == BossState.Dead) return;
        
        Vector3 spawnPos = transform.position;
        GameObject shockwaveLeft = Instantiate(shockwavePrefab, spawnPos, Quaternion.identity);
        GameObject shockwaveRight = Instantiate(shockwavePrefab, spawnPos, Quaternion.identity);
        
        StartCoroutine(MoveShockwave(shockwaveLeft, Vector2.left));
        StartCoroutine(MoveShockwave(shockwaveRight, Vector2.right));
    }

    IEnumerator MoveProjectile(GameObject projectile, Vector2 direction)
    {
        while (projectile != null && IsInArena(projectile.transform.position) && currentState != BossState.Dead)
        {
            projectile.transform.Translate(direction * projectileSpeed * Time.deltaTime);
            yield return null;
        }
        
        if (projectile != null)
            Destroy(projectile);
    }

    IEnumerator MoveShockwave(GameObject shockwave, Vector2 direction)
    {
        while (shockwave != null && IsInArena(shockwave.transform.position) && currentState != BossState.Dead)
        {
            shockwave.transform.Translate(direction * projectileSpeed * Time.deltaTime);
            yield return null;
        }
        
        if (shockwave != null)
            Destroy(shockwave);
    }

    bool IsInArena(Vector3 position)
    {
        if (arenaBounds == null) return true;
        return arenaBounds.bounds.Contains(position);
    }

    float GetGroundHeight()
    {
        if (arenaBounds == null || bossCollider == null) 
            return transform.position.y;
        return arenaBounds.bounds.min.y + bossCollider.bounds.extents.y;
    }
}
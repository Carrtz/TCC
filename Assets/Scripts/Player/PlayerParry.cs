using UnityEngine;
using TarodevController;

public class PlayerParry : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform parryPoint;

    private PlayerController _playerController;
    private PlayerHealth _playerHealth;
    private ScriptableStats _stats;
    private bool _isParrying;
    private float _parryCooldownTimer;
    private float _parryDurationTimer;

    public event System.Action OnParryStarted;
    public event System.Action OnParryEnded;

    void Start()
    {
        _playerController = GetComponent<PlayerController>();
        _playerHealth = GetComponent<PlayerHealth>();

        if (_playerController == null)
        {
            Debug.LogError("PlayerParry: PlayerController not found!");
            enabled = false;
            return;
        }

        if (_playerHealth == null)
        {
            Debug.LogError("PlayerParry: PlayerHealth not found!");
            enabled = false;
            return;
        }

        // Get stats from PlayerController
        _stats = _playerController.GetStats();

        if (_stats == null)
        {
            Debug.LogError("PlayerParry: ScriptableStats not found!");
            enabled = false;
            return;
        }

        if (parryPoint == null)
        {
            parryPoint = transform;
            Debug.LogWarning("PlayerParry: Parry Point not assigned! Using player transform.");
        }
    }

    void Update()
    {
        HandleParryCooldown();
        HandleParryDuration();

        if (Input.GetKeyDown(KeyCode.P) && CanParry() && _stats.CanParry)
        {
            StartParry();
        }
    }

    private bool CanParry()
    {
        return _parryCooldownTimer <= 0 && !_isParrying;
    }

    private void StartParry()
    {
        _isParrying = true;
        _parryDurationTimer = _stats.ParryDuration;
        _parryCooldownTimer = _stats.ParryCooldown;

        OnParryStarted?.Invoke();
        Debug.Log("Parry Started!");

        // Torna o jogador invencível durante o parry
        if (_stats.InvincibleDuringParry && _playerHealth != null)
        {
            _playerHealth.SetInvincible(true);
        }
    }

    // Método público para verificar se um ataque pode ser bloqueado
    public bool CanBlockAttack(Vector2 attackPosition)
    {
        if (!_isParrying) return false;

        // Verifica se o ataque está dentro do alcance do parry
        float distanceToAttack = Vector2.Distance(parryPoint.position, attackPosition);
        return distanceToAttack <= _stats.ParryRange;
    }

    private void EndParry()
    {
        _isParrying = false;

        // Remove a invencibilidade se estiver ativa
        if (_stats.InvincibleDuringParry && _playerHealth != null)
        {
            _playerHealth.SetInvincible(false);
        }

        OnParryEnded?.Invoke();
        Debug.Log("Parry Ended");
    }

    private void HandleParryCooldown()
    {
        if (_parryCooldownTimer > 0)
        {
            _parryCooldownTimer -= Time.deltaTime;
        }
    }

    private void HandleParryDuration()
    {
        if (_isParrying)
        {
            _parryDurationTimer -= Time.deltaTime;
            if (_parryDurationTimer <= 0)
            {
                EndParry();
            }
        }
    }

    public bool IsParrying()
    {
        return _isParrying;
    }

    private void OnDrawGizmosSelected()
    {
        if (parryPoint == null) return;

        Gizmos.color = _isParrying ? Color.green : Color.blue;
        Gizmos.DrawWireSphere(parryPoint.position, _stats != null ? _stats.ParryRange : 1.5f);
    }
}
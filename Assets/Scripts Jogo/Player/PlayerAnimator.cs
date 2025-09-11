using UnityEngine;

namespace TarodevController
{
    /// <summary>
    /// Animator do Player com correção de escala (evita que Dash/Animações alterem o tamanho).
    /// </summary>
    public class PlayerAnimator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator _anim;
        [SerializeField] private SpriteRenderer _sprite;

        [Header("Settings")]
        [SerializeField, Range(1f, 3f)] private float _maxIdleSpeed = 2;
        [SerializeField] private float _maxTilt = 5;
        [SerializeField] private float _tiltSpeed = 20;

        [Header("Particles")]
        [SerializeField] private ParticleSystem _jumpParticles;
        [SerializeField] private ParticleSystem _launchParticles;
        [SerializeField] private ParticleSystem _moveParticles;
        [SerializeField] private ParticleSystem _landParticles;
        [SerializeField] private ParticleSystem _dashParticles;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip[] _footsteps;
        [SerializeField] private AudioClip _dashSound;

        private AudioSource _source;
        private IPlayerController _player;
        private PlayerAttack _playerAttack;
        private Rigidbody2D _rb;
        private bool _grounded;
        private bool _wasRising;
        private float _previousYVelocity;
        private ParticleSystem.MinMaxGradient _currentGradient;

        // 🔥 Novo: salva a escala padrão do Animator para nunca ser alterada
        private Vector3 _defaultScale;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            _player = GetComponentInParent<IPlayerController>();
            _rb = GetComponentInParent<Rigidbody2D>();
            _playerAttack = GetComponentInParent<PlayerAttack>();

            if (_anim != null)
            {
                _defaultScale = _anim.transform.localScale; // escala base
            }
        }

        private void OnEnable()
        {
            _player.Jumped += OnJumped;
            _player.GroundedChanged += OnGroundedChanged;
            _player.Dashed += OnDashed;

            if (_playerAttack != null)
            {
                PlayerAttack.OnAttack += OnAttack;
            }

            _moveParticles.Play();
        }

        private void OnDisable()
        {
            _player.Jumped -= OnJumped;
            _player.GroundedChanged -= OnGroundedChanged;
            _player.Dashed -= OnDashed;

            if (_playerAttack != null)
            {
                PlayerAttack.OnAttack -= OnAttack;
            }

            _moveParticles.Stop();
        }

        private void Update()
        {
            if (_player == null || _rb == null) return;

            DetectGroundColor();
            HandleSpriteFlip();
            HandleIdleSpeed();
            HandleCharacterTilt();
            CheckLandingTransition();
            HandleWallSlide();

            _anim.SetFloat(SpeedKey, Mathf.Abs(_rb.linearVelocity.x));
        }

        // 🔥 Novo: corrige a escala no final do Update para evitar que o Animator mude
        private void LateUpdate()
        {
            if (_anim == null) return;

            // Mantém sempre a escala original, só espelha no eixo X
            _anim.transform.localScale = new Vector3(
                Mathf.Abs(_defaultScale.x) * (_sprite.flipX ? -1 : 1),
                _defaultScale.y,
                _defaultScale.z
            );
        }

        private void HandleWallSlide()
        {
            if (_player is PlayerController controller)
            {
                _anim.SetBool(WallSlideKey, controller.IsWallSliding);
            }
        }

        private void CheckLandingTransition()
        {
            if (_grounded) return;
            float currentYVelocity = _rb.linearVelocity.y;

            if (_wasRising && currentYVelocity <= 0)
            {
                OnStartFalling();
            }

            _wasRising = currentYVelocity > 0;
            _previousYVelocity = currentYVelocity;
        }

        private void OnStartFalling()
        {
            _anim.SetTrigger(FallingKey);
        }

        private void HandleSpriteFlip()
        {
            if (_player.FrameInput.x != 0) _sprite.flipX = _player.FrameInput.x < 0;
        }

        private void HandleIdleSpeed()
        {
            var inputStrength = Mathf.Abs(_player.FrameInput.x);
            _anim.SetFloat(IdleSpeedKey, Mathf.Lerp(1, _maxIdleSpeed, inputStrength));
            _moveParticles.transform.localScale = Vector3.MoveTowards(
                _moveParticles.transform.localScale,
                Vector3.one * inputStrength,
                2 * Time.deltaTime
            );
        }

        private void HandleCharacterTilt()
        {
            if (_anim.GetCurrentAnimatorStateInfo(0).IsName("Dash") ||
                _anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
            {
                _anim.transform.up = Vector2.up;
                return;
            }

            var runningTilt = _grounded
                ? Quaternion.Euler(0, 0, _maxTilt * _player.FrameInput.x)
                : Quaternion.identity;

            _anim.transform.up = Vector3.RotateTowards(
                _anim.transform.up,
                runningTilt * Vector2.up,
                _tiltSpeed * Time.deltaTime,
                0f
            );
        }

        private void OnDashed()
        {
            _anim.SetTrigger(DashKey);

            if (_dashParticles != null)
            {
                SetColor(_dashParticles);
                _dashParticles.Play();
            }

            if (_dashSound != null)
            {
                _source.PlayOneShot(_dashSound);
            }
        }

        private void OnAttack()
        {
            _anim.SetTrigger(AttackKey);
        }

        private void OnJumped()
        {
            _anim.SetTrigger(JumpKey);
            _anim.ResetTrigger(GroundedKey);
            _anim.ResetTrigger(FallingKey);

            if (_grounded)
            {
                SetColor(_jumpParticles);
                SetColor(_launchParticles);
                _jumpParticles.Play();
            }

            _wasRising = true;
        }

        private void OnGroundedChanged(bool grounded, float impact)
        {
            _grounded = grounded;

            if (grounded)
            {
                DetectGroundColor();
                SetColor(_landParticles);

                _anim.SetTrigger(GroundedKey);
                _anim.ResetTrigger(FallingKey);

                _source.PlayOneShot(_footsteps[Random.Range(0, _footsteps.Length)]);
                _moveParticles.Play();

                _landParticles.transform.localScale =
                    Vector3.one * Mathf.InverseLerp(0, 40, impact);
                _landParticles.Play();

                _wasRising = false;
            }
            else
            {
                _moveParticles.Stop();
            }
        }

        private void DetectGroundColor()
        {
            var hit = Physics2D.Raycast(transform.position, Vector3.down, 2);

            if (!hit || hit.collider.isTrigger || !hit.transform.TryGetComponent(out SpriteRenderer r)) return;

            var color = r.color;
            _currentGradient = new ParticleSystem.MinMaxGradient(color * 0.9f, color * 1.2f);
            SetColor(_moveParticles);
        }

        private void SetColor(ParticleSystem ps)
        {
            var main = ps.main;
            main.startColor = _currentGradient;
        }

        // Animator Keys
        private static readonly int GroundedKey = Animator.StringToHash("Grounded");
        private static readonly int IdleSpeedKey = Animator.StringToHash("IdleSpeed");
        private static readonly int JumpKey = Animator.StringToHash("Jump");
        private static readonly int FallingKey = Animator.StringToHash("Falling");
        private static readonly int WallSlideKey = Animator.StringToHash("WallSlide");
        private static readonly int AttackKey = Animator.StringToHash("Attack");
        private static readonly int DashKey = Animator.StringToHash("Dash");
        private static readonly int SpeedKey = Animator.StringToHash("Speed");
    }
}

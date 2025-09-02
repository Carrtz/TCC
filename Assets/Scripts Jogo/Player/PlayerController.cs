using System;
using UnityEngine;
using System.Collections;
namespace TarodevController
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PlayerController : MonoBehaviour, IPlayerController
    {
        [SerializeField] private ScriptableStats _stats;
        [Header("Attack Settings")]
        [SerializeField] private float _attackSlowdownFactor = 0.001f;
        private Rigidbody2D _rb;
        private CapsuleCollider2D _col;
        public FrameInput _frameInput;
        private Vector2 _frameVelocity;
        private bool _cachedQueryStartInColliders;
        private bool _isTouchingWallLeft;
        private bool _isTouchingWallRight;
        private float _wallJumpCooldown;
        private bool _canWallJump = true;
        private bool _isWallSliding;
        private float _wallSlideSpeed = 2f;
        private Item _currentItem;
        private int _currentDirection = 1;
        [SerializeField] private Transform _visual;
        public bool IsWallSliding => _isWallSliding;
        public bool AttackingIsTrue = false;



        private bool _isHoldingItem => _currentItem != null;

        #region Interface

        public Vector2 FrameInput => _frameInput.Move;
        public event Action<bool, float> GroundedChanged;
        public event Action Jumped;

        #endregion

        private float _time;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<CapsuleCollider2D>();

            _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
        }

        #region AttackMotionless
        void Start()
        {
            PlayerAttack.OnAttack += Attacking;
        }
        
            void OnDestroy()
        {
            PlayerAttack.OnAttack -= Attacking;
        }

        private void Attacking()
        {
            AttackingIsTrue = true;

            _frameVelocity *= _attackSlowdownFactor;
            _rb.linearVelocity *= _attackSlowdownFactor;

            StartCoroutine(AttackDelay());
        }

    private IEnumerator AttackDelay()
    {
        // aqui que você muda o tempo que ele fica sem poder se mexer quando ataca
        yield return new WaitForSeconds(0.2f);
        AttackingIsTrue = false;
    }

    #endregion

        public ScriptableStats GetStats()
        {
            return _stats;
        }

        private void Update()
        {
            _time += Time.deltaTime;
            GatherInput();
            CheckForItems();
            Flip();
        }

        private void PickUpItem(Item item)
        {
            if (_isHoldingItem) return;

            _currentItem = item;
            _currentItem.PickUp(transform);
        }

        private void CheckForItems()
        {
            if (!_isHoldingItem && Input.GetKeyDown(KeyCode.E))
            {
                Collider2D[] nearbyItems = Physics2D.OverlapCircleAll(transform.position, 1.5f);
                foreach (var collider in nearbyItems)
                {
                    Item item = collider.GetComponent<Item>();
                    if (item != null && !item.IsPickedUp)
                    {
                        PickUpItem(item);
                        break;
                    }
                }
            }
            else if (_isHoldingItem && Input.GetKeyDown(KeyCode.Q))
            {
                DropItem();
            }
        }

        private void DropItem()
        {
            if (!_isHoldingItem) return;

            _currentItem.Drop();
            _currentItem = null;
        }

        private void GatherInput()
        {
            _frameInput = new FrameInput
            {
                JumpDown = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.C),
                JumpHeld = Input.GetButton("Jump") || Input.GetKey(KeyCode.C),
                Move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"))
            };

            if (_stats.SnapInput)
            {
                _frameInput.Move.x = Mathf.Abs(_frameInput.Move.x) < _stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.x);
                _frameInput.Move.y = Mathf.Abs(_frameInput.Move.y) < _stats.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.y);
            }

            if (_frameInput.JumpDown)
            {
                _jumpToConsume = true;
                _timeJumpWasPressed = _time;
            }
        }

        private void FixedUpdate()
        {
            CheckCollisions();

            HandleJump();
            HandleDirection();
            HandleGravity();

      
            if (_wallJumpCooldown > 0)
            {
                _wallJumpCooldown -= Time.fixedDeltaTime;
            }
            else
            {
                _canWallJump = true;
            }

            ApplyMovement();
        }

        #region Collisions

        private float _frameLeftGrounded = float.MinValue;
        private bool _grounded;

        private void CheckCollisions()
        {
            Physics2D.queriesStartInColliders = false;

        
            bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down, _stats.GrounderDistance, ~_stats.PlayerLayer);
            bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.up, _stats.GrounderDistance, ~_stats.PlayerLayer);

            // Wall Detection
            _isTouchingWallLeft = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.left, _stats.GrounderDistance, ~_stats.PlayerLayer);
            _isTouchingWallRight = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.right, _stats.GrounderDistance, ~_stats.PlayerLayer);

            // Hit a Ceiling
            if (ceilingHit) _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);

            // Landed on the Ground
            if (!_grounded && groundHit)
            {
                _grounded = true;
                _coyoteUsable = true;
                _bufferedJumpUsable = true;
                _endedJumpEarly = false;
                GroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y));
            }
            // Left the Ground
            else if (_grounded && !groundHit)
            {
                _grounded = false;
                _frameLeftGrounded = _time;
                GroundedChanged?.Invoke(false, 0);
            }

            // Wall Slide Logic
            _isWallSliding = !_grounded && (_isTouchingWallLeft || _isTouchingWallRight) && _frameVelocity.y < 0;

            Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
        }

        #endregion

        #region Jumping

        private bool _jumpToConsume;
        private bool _bufferedJumpUsable;
        private bool _endedJumpEarly;
        private bool _coyoteUsable;
        private float _timeJumpWasPressed;

        private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + _stats.JumpBuffer;
        private bool CanUseCoyote => _coyoteUsable && !_grounded && _time < _frameLeftGrounded + _stats.CoyoteTime;

        private void ExecuteJump()
        {
            _endedJumpEarly = false;
            _timeJumpWasPressed = 0;
            _bufferedJumpUsable = false;
            _coyoteUsable = false;
            _frameVelocity.y = _stats.JumpPower;
            Jumped?.Invoke();
        }

        private void HandleGravity()
        {

            if (AttackingIsTrue == true)
            {
                return;
            }

            if (_grounded && _frameVelocity.y <= 0f)
            {
                _frameVelocity.y = _stats.GroundingForce;
            }
            else
            {
                var inAirGravity = _stats.FallAcceleration;

                // Wall Slide Gravity
                if (_isWallSliding)
                {
                    inAirGravity *= 0.5f;
                    _frameVelocity.y = Mathf.Max(_frameVelocity.y, -_wallSlideSpeed);
                }

                if (_endedJumpEarly && _frameVelocity.y > 0) inAirGravity *= _stats.JumpEndEarlyGravityModifier;
                _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -_stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
            }
        }

        private void HandleJump()
        {
            
            if (AttackingIsTrue == true)
            {
                return;
            }

            if (!_endedJumpEarly && !_grounded && !_frameInput.JumpHeld && _rb.linearVelocity.y > 0) _endedJumpEarly = true;

            if (!_jumpToConsume && !HasBufferedJump) return;

            if (_grounded || CanUseCoyote)
            {
                ExecuteJump();
            }
            else if (_isWallSliding && _canWallJump)
            {
                ExecuteWallJump();
            }

            _jumpToConsume = false;
        }

        private void ExecuteWallJump()
        {
            _endedJumpEarly = false;
            _timeJumpWasPressed = 0;
            _bufferedJumpUsable = false;
            _coyoteUsable = false;
            _canWallJump = false;

            float wallDirection = _isTouchingWallLeft ? 1 : -1;
            _frameVelocity.x = _stats.WallJumpHorizontalForce * wallDirection;
            _frameVelocity.y = _stats.WallJumpVerticalForce;
            _wallJumpCooldown = _stats.WallJumpCooldown;

            Jumped?.Invoke();
        }

        #endregion

        #region Horizontal

        private void HandleDirection()
        {
            if (AttackingIsTrue == true)
            {
                return;
            }

            if (_frameInput.Move.x == 0)
            {
                var deceleration = _grounded ? _stats.GroundDeceleration : _stats.AirDeceleration;
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
            }
            else
            {
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _frameInput.Move.x * _stats.MaxSpeed, _stats.Acceleration * Time.fixedDeltaTime);
            }
        }

        private void Flip()
        {
            
            if (_isWallSliding)
            {
                if (_isTouchingWallLeft)
                {
                    _visual.localScale = new Vector3(Mathf.Abs(_visual.localScale.x), _visual.localScale.y, _visual.localScale.z);
                }
                else if (_isTouchingWallRight)
                {
                    _visual.localScale = new Vector3(-Mathf.Abs(_visual.localScale.x), _visual.localScale.y, _visual.localScale.z);
                }
            }
            else
            {
              
                if (_frameInput.Move.x != 0)
                {
                    float scaleX = Mathf.Abs(_visual.localScale.x) * Mathf.Sign(_frameInput.Move.x);
                    _visual.localScale = new Vector3(scaleX, _visual.localScale.y, _visual.localScale.z);
                    transform.localScale = new Vector3(scaleX, transform.localScale.y, transform.localScale.z);
                }
            }
        }
     
          
  
        #endregion

        private void ApplyMovement() => _rb.linearVelocity = _frameVelocity;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_stats == null) Debug.LogWarning("Please assign a ScriptableStats asset to the Player Controller's Stats slot", this);
        }
#endif
    }

    public struct FrameInput
    {
        public bool JumpDown;
        public bool JumpHeld;
        public Vector2 Move;
    }

    public interface IPlayerController
    {
        public event Action<bool, float> GroundedChanged;
        public event Action Jumped;
        public Vector2 FrameInput { get; }
    }
}
using UnityEngine;

namespace StarboundSprint.Player
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PlayerController2_5D : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private Transform wallCheck;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform projectileSpawn;

        [Header("Run")]
        [SerializeField] private float maxRunSpeed = 9f;
        [SerializeField] private float groundAcceleration = 80f;
        [SerializeField] private float groundDeceleration = 70f;
        [SerializeField] private float airAcceleration = 45f;

        [Header("Jump")]
        [SerializeField] private float jumpForce = 14f;
        [SerializeField] private float jumpHoldDuration = 0.2f;
        [SerializeField] private float jumpHoldGravityScale = 0.55f;
        [SerializeField] private float fallGravityScale = 2.3f;
        [SerializeField] private float coyoteTime = 0.1f;
        [SerializeField] private float jumpBufferTime = 0.12f;

        [Header("Wall")]
        [SerializeField] private float wallSlideSpeed = 2f;
        [SerializeField] private Vector2 wallJumpForce = new(10f, 14f);

        [Header("Special Moves")]
        [SerializeField] private float groundPoundSpeed = 28f;
        [SerializeField] private float spinRadius = 1.2f;
        [SerializeField] private int spinDamage = 1;
        [SerializeField] private float stompBounceForce = 12f;

        private Rigidbody2D _rb;
        private Collider2D _col;
        private PowerUpState _powerUpState;

        private float _moveInput;
        private bool _jumpPressed;
        private bool _jumpHeld;
        private bool _attackPressed;
        private bool _spinPressed;
        private bool _poundPressed;

        private float _coyoteCounter;
        private float _jumpBufferCounter;
        private float _jumpHoldCounter;

        private bool _isGrounded;
        private bool _isWallSliding;
        private bool _isGroundPounding;
        private bool _doubleJumpUsed;
        private int _facingDirection = 1;

        private const float GroundCheckRadius = 0.16f;
        private const float WallCheckRadius = 0.13f;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<Collider2D>();
            _powerUpState = GetComponent<PowerUpState>();
            _rb.gravityScale = fallGravityScale;
        }

        private void Update()
        {
            ReadInput();
            ProbeEnvironment();
            UpdateTimers();
            HandleJumpLogic();
            HandleAttacks();
            HandleFacing();
        }

        private void FixedUpdate()
        {
            HandleRun();
            HandleGravity();
            HandleWallSlide();
            HandleGroundPound();
        }

        private void ReadInput()
        {
            _moveInput = Input.GetAxisRaw("Horizontal");
            _jumpPressed = Input.GetButtonDown("Jump");
            _jumpHeld = Input.GetButton("Jump");
            _attackPressed = Input.GetButtonDown("Fire1");
            _spinPressed = Input.GetButtonDown("Fire2");
            _poundPressed = Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow);
        }

        private void ProbeEnvironment()
        {
            _isGrounded = Physics2D.OverlapCircle(groundCheck.position, GroundCheckRadius, groundLayer);

            if (_isGrounded)
            {
                _isGroundPounding = false;
                _doubleJumpUsed = false;
            }

            bool touchingWall = Physics2D.OverlapCircle(wallCheck.position, WallCheckRadius, groundLayer);
            _isWallSliding = touchingWall && !_isGrounded && _rb.linearVelocity.y < 0f;
        }

        private void UpdateTimers()
        {
            _coyoteCounter = _isGrounded ? coyoteTime : _coyoteCounter - Time.deltaTime;
            _jumpBufferCounter = _jumpPressed ? jumpBufferTime : _jumpBufferCounter - Time.deltaTime;

            if (_jumpHoldCounter > 0f)
            {
                _jumpHoldCounter -= Time.deltaTime;
            }
        }

        private void HandleJumpLogic()
        {
            if (_jumpBufferCounter > 0f && _coyoteCounter > 0f)
            {
                Jump(jumpForce);
                _jumpBufferCounter = 0f;
                _coyoteCounter = 0f;
                return;
            }

            if (_jumpPressed && _isWallSliding)
            {
                WallJump();
                return;
            }

            if (_jumpPressed && CanDoubleJump())
            {
                _doubleJumpUsed = true;
                Jump(jumpForce * 0.95f);
            }
        }

        private void Jump(float force)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0f);
            _rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
            _jumpHoldCounter = jumpHoldDuration;
        }

        private void WallJump()
        {
            int dir = -_facingDirection;
            _rb.linearVelocity = Vector2.zero;
            _rb.AddForce(new Vector2(wallJumpForce.x * dir, wallJumpForce.y), ForceMode2D.Impulse);
            _jumpHoldCounter = jumpHoldDuration * 0.75f;
            _isWallSliding = false;
        }

        private bool CanDoubleJump()
        {
            return !_isGrounded && !_doubleJumpUsed && _powerUpState != null && _powerUpState.HasDoubleJump;
        }

        private void HandleRun()
        {
            if (_isGroundPounding)
            {
                _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
                return;
            }

            float targetSpeed = _moveInput * maxRunSpeed;
            float accel = Mathf.Abs(targetSpeed) > 0.01f
                ? (_isGrounded ? groundAcceleration : airAcceleration)
                : groundDeceleration;

            float speedDifference = targetSpeed - _rb.linearVelocity.x;
            float movement = speedDifference * accel * Time.fixedDeltaTime;

            _rb.AddForce(Vector2.right * movement, ForceMode2D.Force);

            float clampedX = Mathf.Clamp(_rb.linearVelocity.x, -maxRunSpeed, maxRunSpeed);
            _rb.linearVelocity = new Vector2(clampedX, _rb.linearVelocity.y);
        }

        private void HandleGravity()
        {
            if (_isGroundPounding)
            {
                _rb.gravityScale = fallGravityScale * 1.8f;
                return;
            }

            if (_rb.linearVelocity.y > 0f && _jumpHeld && _jumpHoldCounter > 0f)
            {
                _rb.gravityScale = jumpHoldGravityScale;
            }
            else if (_rb.linearVelocity.y < 0f)
            {
                _rb.gravityScale = fallGravityScale;
            }
            else
            {
                _rb.gravityScale = 1.3f;
            }
        }

        private void HandleWallSlide()
        {
            if (!_isWallSliding || _isGroundPounding)
            {
                return;
            }

            float limitedY = Mathf.Max(_rb.linearVelocity.y, -wallSlideSpeed);
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, limitedY);
        }

        private void HandleGroundPound()
        {
            if (_poundPressed && !_isGrounded && !_isGroundPounding)
            {
                _isGroundPounding = true;
                _rb.linearVelocity = new Vector2(0f, -groundPoundSpeed);
            }
        }

        private void HandleAttacks()
        {
            if (_attackPressed)
            {
                TryShootProjectile();
            }

            if (_spinPressed)
            {
                SpinAttack();
            }
        }

        private void TryShootProjectile()
        {
            if (_powerUpState == null || !_powerUpState.CanShootProjectile || projectilePrefab == null || projectileSpawn == null)
            {
                return;
            }

            GameObject bullet = Instantiate(projectilePrefab, projectileSpawn.position, Quaternion.identity);
            var projectile = bullet.GetComponent<Combat.Projectile>();

            if (projectile != null)
            {
                projectile.Initialize(_facingDirection, _powerUpState.ProjectileType);
            }
        }

        private void SpinAttack()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, spinRadius, enemyLayer);
            foreach (Collider2D hit in hits)
            {
                if (hit.TryGetComponent(out Enemies.EnemyBase enemy))
                {
                    enemy.TakeDamage(spinDamage, Combat.DamageType.Physical);
                }
            }
        }

        public void RegisterStompBounce()
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, stompBounceForce);
        }

        private void HandleFacing()
        {
            if (Mathf.Abs(_moveInput) < 0.01f)
            {
                return;
            }

            _facingDirection = _moveInput > 0f ? 1 : -1;
            transform.localScale = new Vector3(_facingDirection, 1f, 1f);
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(groundCheck.position, GroundCheckRadius);
            }

            if (wallCheck != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(wallCheck.position, WallCheckRadius);
            }

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, spinRadius);
        }
    }
}

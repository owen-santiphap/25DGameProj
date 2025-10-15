using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(HealthSystem))]
public class EnemyBase : MonoBehaviour, IHittable
{
    [Header("Enemy Settings")]
    [SerializeField] protected float detectionRange = 10f;
    [SerializeField] protected float attackRange = 2f;
    [SerializeField] protected float attackCooldown = 2f;
    [SerializeField] protected int attackDamage = 1;
    
    [Header("Movement")]
    [SerializeField] protected float moveSpeed = 3f;
    [SerializeField] protected float rotationSpeed = 5f;
    
    [Header("Gravity")]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    private Vector3 _velocity;
    private bool _isGrounded;
    
    [Header("Knockback")]
    [SerializeField] protected float knockbackForce = 5f;
    [SerializeField] protected float knockbackDuration = 0.3f;
    
    [Header("References")]
    [SerializeField] protected Animator animator;
    [SerializeField] protected Transform spriteTransform;
    [SerializeField] protected GameObject deathVFX;
    
    protected HealthSystem HealthSystem;
    protected Transform PlayerTransform;
    private CharacterController _controller;
    
    private float _attackTimer;
    private bool _isKnockedBack;
    private Vector3 _knockbackVelocity;
    private float _knockbackTimer;
    
    // State
    protected EnemyState CurrentState = EnemyState.Idle;
    
    protected enum EnemyState
    {
        Idle,
        Chasing,
        Attacking,
        Hurt,
        Dead
    }
    
    protected virtual void Awake()
    {
        HealthSystem = GetComponent<HealthSystem>();
        _controller = GetComponent<CharacterController>();
        
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }
    
    protected virtual void Start()
    {
        PlayerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        HealthSystem.OnDeath.AddListener(OnDeath);
        HealthSystem.OnDamageTaken.AddListener(OnDamageTaken);
    }
    
    protected virtual void Update()
    {
        if (HealthSystem.IsDead) return;
        
        CheckGround();
        ApplyGravity();
        
        HandleKnockback();
        
        if (!_isKnockedBack)
        {
            UpdateState();
            UpdateAnimations();
        }
        
        _attackTimer -= Time.deltaTime;
    }
    
    private void CheckGround()
    {
        var spherePosition = new Vector3(transform.position.x, _controller.bounds.min.y, transform.position.z);
        _isGrounded = Physics.CheckSphere(spherePosition, 0.1f, groundLayer, QueryTriggerInteraction.Ignore);
    }
    
    private void ApplyGravity()
    {
        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f; // A small force to keep them stuck to the ground
        }

        _velocity.y += gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }
    
    protected virtual void UpdateState()
    {
        if (PlayerTransform == null) return;
        
        var distanceToPlayer = Vector3.Distance(transform.position, PlayerTransform.position);
        
        switch (CurrentState)
        {
            case EnemyState.Idle:
                if (distanceToPlayer <= detectionRange)
                {
                    CurrentState = EnemyState.Chasing;
                }
                break;
                
            case EnemyState.Chasing:
                if (distanceToPlayer <= attackRange)
                {
                    CurrentState = EnemyState.Attacking;
                }
                else if (distanceToPlayer > detectionRange)
                {
                    CurrentState = EnemyState.Idle;
                }
                else
                {
                    ChasePlayer();
                }
                break;
                
            case EnemyState.Attacking:
                if (distanceToPlayer > attackRange)
                {
                    CurrentState = EnemyState.Chasing;
                }
                else
                {
                    AttackPlayer();
                }
                break;
        }
    }
    
    protected virtual void ChasePlayer()
    {
        var direction = (PlayerTransform.position - transform.position).normalized;
        direction.y = 0; // Keep movement on XZ plane
        
        if (_controller != null)
        {
            _controller.Move(direction * moveSpeed * Time.deltaTime);
        }
        else
        {
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
        
        // Flip sprite
        FlipSprite(direction);
    }
    
    protected virtual void AttackPlayer()
    {
        // Face player
        var direction = (PlayerTransform.position - transform.position).normalized;
        FlipSprite(direction);
        
        if (_attackTimer <= 0)
        {
            PerformAttack();
            _attackTimer = attackCooldown;
        }
    }
    
    protected virtual void PerformAttack()
    {
        // Play attack animation
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        var tempSpeed = moveSpeed;
        {
            moveSpeed = 0;
        }
        moveSpeed = tempSpeed;
        
        // Deal damage (can be called from animation event instead)
        //DealDamageToPlayer();
    }
    
    public virtual void DealDamageToPlayer()
    {
        if (PlayerTransform == null) return;

        var distanceToPlayer = Vector3.Distance(transform.position, PlayerTransform.position);
        if (distanceToPlayer <= attackRange)
        {
            var playerHealth = PlayerTransform.GetComponent<HealthSystem>();
            var playerSkills = PlayerTransform.GetComponent<PlayerSkills>();
            //Debug.Log("HIT PLAYER1");
            if (playerHealth != null)
            {
                //Debug.Log("HIT PLAYER2");
                if (playerSkills != null && playerSkills.IsDeflecting)
                {
                    Debug.Log("Player Deflected! Enemy takes damage.");
                    
                    var deflectData = playerSkills.DeflectSkill; 
                    var damageToTake = (deflectData != null) ? deflectData.deflectDamage : 1;
                    
                    HealthSystem.TakeDamage(damageToTake);
                    
                    var knockbackDirection = (transform.position - PlayerTransform.position).normalized;
                    OnHit(damageToTake, knockbackDirection);
                }
                else
                {
                    playerHealth.TakeDamage(attackDamage);
                }
            }
        }
    }

    protected virtual void FlipSprite(Vector3 direction)
    {
        if (spriteTransform != null && direction.x != 0)
        {
            var scale = spriteTransform.localScale;
            scale.x = direction.x > 0 ? 1 : -1;
            spriteTransform.localScale = scale;
        }
    }
    
    public virtual void OnHit(float damage, Vector3 hitDirection)
    {
        // Apply knockback
        _isKnockedBack = true;
        _knockbackTimer = knockbackDuration;
        _knockbackVelocity = hitDirection * knockbackForce;
        _knockbackVelocity.y = 0;
        animator.Play("Idle");
    }
    
    //Event
    protected virtual void HandleKnockback()
    {
        if (_isKnockedBack)
        {
            if (_controller != null)
            {
                _controller.Move(_knockbackVelocity * Time.deltaTime);
            }
            else
            {
                transform.position += _knockbackVelocity * Time.deltaTime;
            }
            
            _knockbackVelocity = Vector3.Lerp(_knockbackVelocity, Vector3.zero, Time.deltaTime * 5f);
            _knockbackTimer -= Time.deltaTime;
            
            if (_knockbackTimer <= 0)
            {
                _isKnockedBack = false;
            }
        }
    }
    
    protected virtual void OnDamageTaken()
    {
        if (animator != null)
        {
            //animator.SetTrigger("Hurt");
        }
    }
    
    protected virtual void OnDeath()
    {
        CurrentState = EnemyState.Dead;
        if (deathVFX != null)
        {
            var rotation = Quaternion.Euler(45, 0, 0);
            Instantiate(deathVFX, transform.position, rotation);
        }
        
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }
        
        // Disable components
        if (_controller != null)
            _controller.enabled = false;
            
        enabled = false;
        
        // Destroy after animation (or immediately)
        Destroy(gameObject, 0f);
    }
    
    protected virtual void UpdateAnimations()
    {
        if (animator == null) return;
        
        var isMoving = CurrentState == EnemyState.Chasing;
        animator.SetBool("IsMoving", isMoving);
        animator.SetBool("IsAttacking", CurrentState == EnemyState.Attacking);
    }
    
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
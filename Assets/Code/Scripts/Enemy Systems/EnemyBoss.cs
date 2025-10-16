using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class EnemyBoss : EnemyBase
{
    [Header("Boss Settings")]
    [SerializeField] private BossPhaseData[] phases;
    [SerializeField] private int currentPhaseIndex = 0;
    
    [Header("Special Attacks")]
    [SerializeField] private float specialAttackCooldown = 5f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform[] projectileSpawnPoints;
    
    [Header("Minion Spawning")]
    [SerializeField] private GameObject minionPrefab;
    [SerializeField] private Transform[] minionSpawnPoints;
    [SerializeField] private int maxMinions = 3;
    [SerializeField] private float minionSpawnCooldown = 10f;
    
    [Header("Events")]
    public UnityEvent<int> OnPhaseChanged;
    public UnityEvent OnBossDefeated;
    
    private float _specialAttackTimer;
    private float _minionSpawnTimer;
    private int _currentMinionCount = 0;
    private BossPhaseData CurrentPhase => phases[currentPhaseIndex];
    
    protected override void Start()
    {
        base.Start();
        
        // Subscribe to health changes to trigger phase transitions
        HealthSystem.OnHealthChanged.AddListener(CheckPhaseTransition);
        
        // Initialize first phase
        if (phases.Length > 0)
        {
            ApplyPhaseSettings(phases[0]);
        }
    }
    
    protected override void Update()
    {
        base.Update();
        
        if (HealthSystem.IsDead) return;
        
        // Handle special attack timing
        _specialAttackTimer -= Time.deltaTime;
        _minionSpawnTimer -= Time.deltaTime;
        
        // Spawn minions if in phase that allows it
        if (CurrentPhase.canSpawnMinions && _minionSpawnTimer <= 0 && _currentMinionCount < maxMinions)
        {
            SpawnMinion();
            _minionSpawnTimer = minionSpawnCooldown;
        }
    }
    
    protected override void UpdateState()
    {
        if (PlayerTransform == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, PlayerTransform.position);
        
        // Boss-specific state machine
        switch (CurrentState)
        {
            case EnemyState.Idle:
                if (distanceToPlayer <= CurrentPhase.detectionRange)
                {
                    CurrentState = EnemyState.Chasing;
                }
                break;
                
            case EnemyState.Chasing:
                if (_specialAttackTimer <= 0 && CurrentPhase.canUseSpecialAttack)
                {
                    PerformSpecialAttack();
                    _specialAttackTimer = specialAttackCooldown;
                }
                else if (distanceToPlayer <= CurrentPhase.attackRange)
                {
                    CurrentState = EnemyState.Attacking;
                }
                else
                {
                    ChasePlayer();
                }
                break;
                
            case EnemyState.Attacking:
                if (distanceToPlayer > CurrentPhase.attackRange)
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
    
    private void CheckPhaseTransition(int currentHealth)
    {
        // Check if we should transition to next phase
        if (currentPhaseIndex < phases.Length - 1)
        {
            float healthPercentage = (float)currentHealth / HealthSystem.MaxHearts;
            
            if (healthPercentage <= phases[currentPhaseIndex + 1].healthThreshold)
            {
                TransitionToNextPhase();
            }
        }
    }
    
    private void TransitionToNextPhase()
    {
        currentPhaseIndex++;
        
        if (currentPhaseIndex < phases.Length)
        {
            ApplyPhaseSettings(CurrentPhase);
            OnPhaseChanged?.Invoke(currentPhaseIndex);
            
            // Play phase transition animation/effect
            if (animator != null)
            {
                animator.SetTrigger("PhaseTransition");
            }
            
            // Optional: Become invincible briefly during transition
            StartCoroutine(PhaseTransitionInvincibility());
        }
    }
    
    private IEnumerator PhaseTransitionInvincibility()
    {
        // Make boss invincible during phase transition
        float transitionDuration = 2f;
        yield return new WaitForSeconds(transitionDuration);
    }
    
    private void ApplyPhaseSettings(BossPhaseData phase)
    {
        moveSpeed = phase.moveSpeed;
        attackRange = phase.attackRange;
        attackCooldown = phase.attackCooldown;
        attackDamage = phase.attackDamage;
        detectionRange = phase.detectionRange;
    }
    
    private void PerformSpecialAttack()
    {
        if (animator != null)
        {
            animator.SetTrigger("SpecialAttack");
        }
        
        // Example: Shoot projectiles in a pattern
        if (CurrentPhase.specialAttackPattern == SpecialAttackPattern.Projectiles)
        {
            ShootProjectilePattern();
        }
        else if (CurrentPhase.specialAttackPattern == SpecialAttackPattern.AOE)
        {
            PerformAOEAttack();
        }
    }
    
    private void ShootProjectilePattern()
    {
        if (projectilePrefab == null || projectileSpawnPoints.Length == 0) return;
        
        foreach (Transform spawnPoint in projectileSpawnPoints)
        {
            GameObject projectile = Instantiate(projectilePrefab, spawnPoint.position, spawnPoint.rotation);
            
            // Add velocity toward player
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 direction = (PlayerTransform.position - spawnPoint.position).normalized;
                rb.linearVelocity = direction * 10f;
            }
        }
    }
    
    private void PerformAOEAttack()
    {
        // Example: Damage all players in radius
        Collider[] hits = Physics.OverlapSphere(transform.position, CurrentPhase.attackRange * 2f);
        
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                HealthSystem playerHealth = hit.GetComponent<HealthSystem>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(attackDamage * 2);
                }
            }
        }
    }
    
    private void SpawnMinion()
    {
        if (minionPrefab == null || minionSpawnPoints.Length == 0) return;
        
        Transform spawnPoint = minionSpawnPoints[Random.Range(0, minionSpawnPoints.Length)];
        GameObject minion = Instantiate(minionPrefab, spawnPoint.position, spawnPoint.rotation);
        
        // Track minion count
        _currentMinionCount++;
        
        // Subscribe to minion death to update count
        HealthSystem minionHealth = minion.GetComponent<HealthSystem>();
        if (minionHealth != null)
        {
            minionHealth.OnDeath.AddListener(() => _currentMinionCount--);
        }
    }
    
    protected override void OnDeath()
    {
        OnBossDefeated?.Invoke();
        base.OnDeath();
    }
    
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Draw phase-specific ranges
        if (phases != null && currentPhaseIndex < phases.Length)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, CurrentPhase.detectionRange);
        }
    }
}

[System.Serializable]
public class BossPhaseData
{
    [Header("Phase Trigger")]
    [Range(0f, 1f)]
    public float healthThreshold = 0.5f; // Trigger at 50% health
    
    [Header("Phase Stats")]
    public float moveSpeed = 4f;
    public float attackRange = 3f;
    public float attackCooldown = 1.5f;
    public int attackDamage = 2;
    public float detectionRange = 15f;
    
    [Header("Phase Abilities")]
    public bool canUseSpecialAttack = true;
    public bool canSpawnMinions = false;
    public SpecialAttackPattern specialAttackPattern = SpecialAttackPattern.Projectiles;
}

public enum SpecialAttackPattern
{
    Projectiles,
    AOE,
    Charge,
    Summon
}
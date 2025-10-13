using UnityEngine;
using UnityEngine.InputSystem;

public class CombatSystem : MonoBehaviour
{
    [Header("Attack Data")]
    [SerializeField] private AttackData[] attackCombo;
    
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Transform spriteTransform;
    [SerializeField] private PlayerSkills playerSkills;
    
    private int _currentAttackIndex = 0;
    private float _attackTimer = 0f;
    private bool _isAttacking = false;
    private bool _canCombo = false;
    private bool _inputBuffered = false;
    
    public bool IsAttacking => _isAttacking;
    
    private void Update()
    {
        if (_isAttacking)
        {
            _attackTimer += Time.deltaTime;
            
            var currentAttack = attackCombo[_currentAttackIndex];
            
            // Check the combo window
            if (_attackTimer >= currentAttack.comboWindowStart && 
                _attackTimer <= currentAttack.comboWindowEnd)
            {
                _canCombo = true;
                
                // If input was buffered, execute the next attack
                if (_inputBuffered)
                {
                    _inputBuffered = false;
                    PerformNextAttack();
                }
            }
            else if (_attackTimer > currentAttack.comboWindowEnd)
            {
                _canCombo = false;
            }
            
            // Check if attack animation is finished
            if (_attackTimer >= currentAttack.animationDuration)
            {
                EndAttack();
            }
        }
    }
    
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (playerSkills != null && (playerSkills.IsAiming || playerSkills.IsDeflecting))
        {
            return;
        }
        
        if (context.performed)
        {
            if (!_isAttacking)
            {
                StartAttack();
            }
            else if (_canCombo)
            {
                PerformNextAttack();
            }
            else
            {
                // Buffer the input if pressed during attack but outside the combo window
                _inputBuffered = true;
            }
        }
    }
    
    private void StartAttack()
    {
        _isAttacking = true;
        _currentAttackIndex = 0;
        _attackTimer = 0f;
        _canCombo = false;
        _inputBuffered = false;
        
        ExecuteAttack(attackCombo[_currentAttackIndex]);
    }
    
    private void PerformNextAttack()
    {
        _currentAttackIndex++;
        
        // Reset combo if at the end
        if (_currentAttackIndex >= attackCombo.Length)
        {
            _currentAttackIndex = 0;
        }
        
        _attackTimer = 0f;
        _canCombo = false;
        
        ExecuteAttack(attackCombo[_currentAttackIndex]);
    }
    
    private void ExecuteAttack(AttackData attack)
    {
        // Play animation
        animator.Play(attack.animationName, 0, 0f);
        
        // Deal damage - can call this from animation event for better timing
        DealDamage(attack);
        
        // Spawn VFX
        if (attack.attackVFX != null)   
        {
            
            var attackPosition = attackPoint.position + transform.TransformDirection(attack.attackOffset);
        
            // Start with the default rotation
            var vfxRotation = attackPoint.rotation; 

            // If the character is flipped, flip the VFX rotation
            if (spriteTransform.localScale.x < 0)
            {
                vfxRotation *= Quaternion.Euler(0, 180f, 0);
            }

            // Instantiate with the corrected rotation
            Instantiate(attack.attackVFX, attackPosition, vfxRotation);
        }
    }
    
    // Call this from Animation Event for precise hit timing
    public void DealDamage()
    {
        if (_currentAttackIndex < attackCombo.Length)
        {
            DealDamage(attackCombo[_currentAttackIndex]);
        }
    }
    
    private void DealDamage(AttackData attack)
    {
        var attackPosition = attackPoint.position + transform.TransformDirection(attack.attackOffset);
        
        var hitEnemies = Physics.OverlapSphere(attackPosition, attack.attackRange, enemyLayer);
        
        foreach (var enemy in hitEnemies)
        {
            // Try to get health system from enemy
            var enemyHealth = enemy.GetComponent<HealthSystem>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(attack.damage); // Or use attack.damage
            }
            
            // can notify the enemy of hit for knockback, etc.
            var hittable = enemy.GetComponent<IHittable>();
            if (hittable != null)
            {
                var hitDirection = (enemy.transform.position - transform.position).normalized;
                hittable.OnHit(attack.damage, hitDirection);
            }
        }
    }
    
    private void EndAttack()
    {
        _isAttacking = false;
        _currentAttackIndex = 0;
        _canCombo = false;
        _inputBuffered = false;
        
        // Return to idle
        animator.Play("Idle");
    }
    
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null || attackCombo == null || attackCombo.Length == 0) return;
        
        Gizmos.color = Color.red;
        foreach (AttackData attack in attackCombo)
        {
            var attackPosition = attackPoint.position + transform.TransformDirection(attack.attackOffset);
            Gizmos.DrawWireSphere(attackPosition, attack.attackRange);
        }
    }

    public void SpawnVFX()
    {
        if (_currentAttackIndex < attackCombo.Length)
        {
            var currentAttack = attackCombo[_currentAttackIndex];
            if (currentAttack.attackVFX != null)
            {
                var attackPosition = attackPoint.position + transform.TransformDirection(currentAttack.attackOffset);
                Instantiate(currentAttack.attackVFX, attackPosition, attackPoint.rotation);
            }
        }
    }
}

// Interface for enemies to implement hit reactions (maybe)
public interface IHittable
{
    void OnHit(float damage, Vector3 hitDirection);
}
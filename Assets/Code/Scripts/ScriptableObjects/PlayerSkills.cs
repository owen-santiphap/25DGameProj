using UnityEngine.InputSystem;
using UnityEngine;

public class PlayerSkills : MonoBehaviour
{
    [Header("Skill Setup")]
    [SerializeField] private SkillData[] skills;
    [SerializeField] private int maxSkillSlots = 4;
    
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform skillCastPoint;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private PlayerController movement;
    
    [Header("Resources")]
    [SerializeField] private int maxMana = 100;
    [SerializeField] private int currentMana = 100;
    [SerializeField] private float manaRegenRate = 5f; // Per second
    
    private float[] skillCooldowns;
    private bool isCastingSkill = false;
    private float castTimer = 0f;
    private SkillData currentSkill;
    
    public bool IsCastingSkill => isCastingSkill;
    public int CurrentMana => currentMana;
    public int MaxMana => maxMana;
    
    private void Awake()
    {
        skillCooldowns = new float[maxSkillSlots];
    }
    
    private void Update()
    {
        UpdateCooldowns();
        RegenerateMana();
        
        if (isCastingSkill)
        {
            castTimer += Time.deltaTime;
            
            if (castTimer >= currentSkill.animationDuration)
            {
                EndSkillCast();
            }
        }
    }
    
    // Input System callbacks - wire these up in PlayerInput component
    public void OnSkill1(InputAction.CallbackContext context)
    {
        if (context.performed) TryCastSkill(0);
    }
    
    public void OnSkill2(InputAction.CallbackContext context)
    {
        if (context.performed) TryCastSkill(1);
    }
    
    public void OnSkill3(InputAction.CallbackContext context)
    {
        if (context.performed) TryCastSkill(2);
    }
    
    public void OnSkill4(InputAction.CallbackContext context)
    {
        if (context.performed) TryCastSkill(3);
    }
    
    private void TryCastSkill(int skillIndex)
    {
        // Validate skill index
        if (skillIndex >= skills.Length || skills[skillIndex] == null)
            return;
        
        // Check if already casting
        if (isCastingSkill)
            return;
        
        SkillData skill = skills[skillIndex];
        
        // Check cooldown
        if (skillCooldowns[skillIndex] > 0)
        {
            Debug.Log($"{skill.skillName} is on cooldown!");
            return;
        }
        
        // Check resource cost
        if (currentMana < skill.manaCost)
        {
            Debug.Log("Not enough mana!");
            return;
        }
        
        // Cast the skill
        CastSkill(skill, skillIndex);
    }
    
    private void CastSkill(SkillData skill, int skillIndex)
    {
        // Consume resources
        currentMana -= skill.manaCost;
        
        // Set cooldown
        skillCooldowns[skillIndex] = skill.cooldownTime;
        
        // Set casting state
        isCastingSkill = true;
        currentSkill = skill;
        castTimer = 0f;
        
        // Play animation
        if (animator != null)
        {
            animator.Play(skill.animationName, 0, 0f);
        }
        
        // Execute skill effect (can be delayed via animation event)
        ExecuteSkillEffect(skill);
    }
    
    // Call this from Animation Event for precise timing
    public void ExecuteCurrentSkillEffect()
    {
        if (currentSkill != null)
        {
            ExecuteSkillEffect(currentSkill);
        }
    }
    
    private void ExecuteSkillEffect(SkillData skill)
    {
        switch (skill.skillType)
        {
            case SkillType.Damage:
                ExecuteDamageSkill(skill);
                break;
            case SkillType.Heal:
                ExecuteHealSkill(skill);
                break;
            case SkillType.Buff:
                ExecuteBuffSkill(skill);
                break;
            case SkillType.Teleport:
                ExecuteTeleportSkill(skill);
                break;
        }
        
        // Spawn effect
        if (skill.effectPrefab != null)
        {
            Vector3 effectPosition = skillCastPoint.position + transform.TransformDirection(skill.offset);
            
            if (skill.projectile)
            {
                SpawnProjectile(skill, effectPosition);
            }
            else
            {
                Instantiate(skill.effectPrefab, effectPosition, Quaternion.identity);
            }
        }
    }
    
    private void ExecuteDamageSkill(SkillData skill)
    {
        Vector3 skillPosition = skillCastPoint.position + transform.TransformDirection(skill.offset);
        
        if (skill.isAOE)
        {
            // AOE damage
            Collider[] hitEnemies = Physics.OverlapSphere(skillPosition, skill.aoeRadius, enemyLayer);
            
            foreach (Collider enemy in hitEnemies)
            {
                DamageEnemy(enemy, skill);
            }
        }
        else
        {
            // Single target or line damage
            Collider[] hitEnemies = Physics.OverlapSphere(skillPosition, skill.range, enemyLayer);
            
            if (hitEnemies.Length > 0)
            {
                DamageEnemy(hitEnemies[0], skill);
            }
        }
    }
    
    private void DamageEnemy(Collider enemy, SkillData skill)
    {
        HealthSystem enemyHealth = enemy.GetComponent<HealthSystem>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(Mathf.RoundToInt(skill.damage));
        }
        
        IHittable hittable = enemy.GetComponent<IHittable>();
        if (hittable != null)
        {
            Vector3 hitDirection = (enemy.transform.position - transform.position).normalized;
            hittable.OnHit(skill.damage, hitDirection);
        }
    }
    
    private void ExecuteHealSkill(SkillData skill)
    {
        HealthSystem playerHealth = GetComponent<HealthSystem>();
        if (playerHealth != null)
        {
            playerHealth.Heal(Mathf.RoundToInt(skill.specialValue));
        }
    }
    
    private void ExecuteBuffSkill(SkillData skill)
    {
        // Implement buff system (speed boost, damage boost, etc.)
        Debug.Log($"Applied buff: {skill.skillName} for {skill.specialValue} seconds");
    }
    
    private void ExecuteTeleportSkill(SkillData skill)
    {
        // Teleport in facing direction
        Vector3 teleportDirection = transform.forward * skill.range;
        CharacterController controller = GetComponent<CharacterController>();
        
        if (controller != null)
        {
            controller.Move(teleportDirection);
        }
        else
        {
            transform.position += teleportDirection;
        }
    }
    
    private void SpawnProjectile(SkillData skill, Vector3 spawnPosition)
    {
        GameObject projectile = Instantiate(skill.effectPrefab, spawnPosition, Quaternion.identity);
        
        // Add velocity
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 direction = transform.forward;
            rb.linearVelocity = direction * skill.projectileSpeed;
        }
        
        // Add projectile script if needed
        SkillProjectile projScript = projectile.GetComponent<SkillProjectile>();
        if (projScript != null)
        {
            projScript.Initialize(skill.damage, enemyLayer);
        }
    }
    
    private void EndSkillCast()
    {
        isCastingSkill = false;
        currentSkill = null;
        
        // Return to idle if not moving
        if (movement != null && animator != null)
        {
            // Movement script will handle animation
        }
    }
    
    private void UpdateCooldowns()
    {
        for (int i = 0; i < skillCooldowns.Length; i++)
        {
            if (skillCooldowns[i] > 0)
            {
                skillCooldowns[i] -= Time.deltaTime;
            }
        }
    }
    
    private void RegenerateMana()
    {
        if (currentMana < maxMana)
        {
            currentMana = Mathf.Min(maxMana, currentMana + Mathf.RoundToInt(manaRegenRate * Time.deltaTime));
        }
    }
    
    public float GetSkillCooldown(int skillIndex)
    {
        if (skillIndex >= 0 && skillIndex < skillCooldowns.Length)
        {
            return skillCooldowns[skillIndex];
        }
        return 0f;
    }
    
    public bool IsSkillReady(int skillIndex)
    {
        return skillCooldowns[skillIndex] <= 0;
    }
}

public class SkillProjectile : MonoBehaviour
{
    private float damage;
    private LayerMask enemyLayer;
    private bool hasHit = false;
    
    [SerializeField] private float lifetime = 5f;
    
    public void Initialize(float dmg, LayerMask layer)
    {
        damage = dmg;
        enemyLayer = layer;
        Destroy(gameObject, lifetime);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;
        
        // Check if hit enemy
        if (((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            HealthSystem enemyHealth = other.GetComponent<HealthSystem>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(Mathf.RoundToInt(damage));
            }
            
            IHittable hittable = other.GetComponent<IHittable>();
            if (hittable != null)
            {
                Vector3 hitDirection = transform.forward;
                hittable.OnHit(damage, hitDirection);
            }
            
            hasHit = true;
            Destroy(gameObject);
        }
    }
}
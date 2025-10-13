using UnityEngine;

// ===== SKILL DATA SCRIPTABLE OBJECT =====
[CreateAssetMenu(fileName = "New Skill", menuName = "Combat/Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("Basic Info")]
    public string skillName;
    public string description;
    public Sprite icon;
    
    [Header("Animation")]
    public string animationName;
    public float animationDuration;
    
    [Header("Cooldown")]
    public float cooldownTime = 5f;
    
    [Header("Combat")]
    public float damage;
    public float range = 5f;
    public Vector3 offset = Vector3.forward;
    public bool isAOE = false;
    public float aoeRadius = 3f;
    
    [Header("Effects")]
    public GameObject effectPrefab;
    public bool projectile = false;
    public float projectileSpeed = 10f;
    
    [Header("Movement")]
    public bool canMoveWhileCasting = false;
    public float movementSpeedMultiplier = 0f;
    
    [Header("Deflect Settings")]
    [Tooltip("How long the deflect state remains active.")]
    public float deflectDuration = 1.5f;
    [Tooltip("Damage dealt back to the attacker on a successful deflect.")]
    public int deflectDamage = 1;
}

public enum SkillType
{
    Damage,
    Heal,
    Buff,
    Debuff,
    Summon,
    Teleport,
    Deflect,
    AimedShot
}
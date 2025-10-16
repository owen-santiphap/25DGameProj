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
    
    [Header("Effects")]
    public GameObject effectPrefab;
    
    [Header("Movement")]
    public bool canMoveWhileCasting = false;
    public float movementSpeedMultiplier = 0f;
    
    [Header("Deflect Settings")]
    [Tooltip("How long the deflect state remains active.")]
    public float deflectDuration = 1.5f;
    [Tooltip("Damage dealt back to the attacker on a successful deflect.")]
    public int deflectDamage = 1;
    
    [Header("Dash Settings")]
    [Tooltip("How far the player will dash.")]
    public float dashDistance = 5f;
    [Tooltip("How long the dash movement takes to complete.")]
    public float dashDuration = 0.25f;
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
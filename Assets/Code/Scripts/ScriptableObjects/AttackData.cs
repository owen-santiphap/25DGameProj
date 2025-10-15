using UnityEngine;

[CreateAssetMenu(fileName = "New Attack", menuName = "Combat/Attack Data")]
public class AttackData : ScriptableObject
{
    [Header("Animation")]
    public string animationName;
    public float animationDuration;
    
    [Header("Timing")]
    public float comboWindowStart; // When can we start accepting the next input
    public float comboWindowEnd;   // When does the combo the window close
    
    [Header("Combat")]
    public float damage;
    public float attackRange = 2f;
    public Vector3 attackOffset = Vector3.forward;
    
    [Header("Movement")]
    public bool canMove = false;
    public float movementSpeedMultiplier = 0f;
    public float forwardMovement = 0f;
    
    [Header("Effects")]
    public GameObject attackVFX;
}
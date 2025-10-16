using UnityEngine;
using UnityEngine.Events;

public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHearts = 3;
    [SerializeField] private int currentHearts;
    
    [Header("Invincibility")]
    [SerializeField] private float invincibilityDuration = 1f;
    private float _invincibilityTimer;
    
    [Header("Events")]
    public UnityEvent<int> OnHealthChanged;
    public UnityEvent OnDeath;
    public UnityEvent OnDamageTaken;
    public UnityEvent OnHealed;
    
    public int CurrentHearts => currentHearts;
    public int MaxHearts => maxHearts;
    public bool IsInvincible => _invincibilityTimer > 0;
    public bool IsDead { get; private set; }
    public bool IsDeflecting { get; set; }
    
    private void Awake()
    {
        currentHearts = maxHearts;
    }
    
    private void Update()
    {
        if (_invincibilityTimer > 0)
        {
            _invincibilityTimer -= Time.deltaTime;
        }
    }
    
    public void TakeDamage(float hearts)
    {
        if (IsDeflecting)
        {
            Debug.Log("Attack Deflected!");
            // add a deflect VFX/SFX trigger here later
            return; 
        }
    
        if (IsDead || IsInvincible) return;
        
        currentHearts = (int)Mathf.Max(0, currentHearts - hearts);
        _invincibilityTimer = invincibilityDuration;
        
        OnHealthChanged?.Invoke(currentHearts);
        OnDamageTaken?.Invoke();
        
        if (currentHearts <= 0)
        {
            Die();
        }
    }
    
    public void Heal(int hearts)
    {
        if (IsDead) return;
        
        currentHearts = Mathf.Min(maxHearts, currentHearts + hearts);
        OnHealthChanged?.Invoke(currentHearts);
        OnHealed?.Invoke();
    }
    
    public void SetMaxHearts(int newMax)
    {
        maxHearts = newMax;
        currentHearts = Mathf.Min(currentHearts, maxHearts);
        OnHealthChanged?.Invoke(currentHearts);
    }
    
    private void Die()
    {
        IsDead = true;
        OnDeath?.Invoke();
    }
    
    public void Revive()
    {
        IsDead = false;
        currentHearts = maxHearts;
        OnHealthChanged?.Invoke(currentHearts);
    }
}
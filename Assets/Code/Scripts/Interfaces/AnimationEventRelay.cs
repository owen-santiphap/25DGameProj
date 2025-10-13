using UnityEngine;

public class AnimationEventRelay : MonoBehaviour
{
    private CombatSystem _combatSystem;
    private PlayerController _playerController;

    private void Awake()
    {
        _combatSystem = GetComponentInParent<CombatSystem>();
        _playerController = GetComponentInParent<PlayerController>();

        if (_combatSystem == null)
        {
            Debug.LogWarning("CombatSystem not found on " + gameObject.name);
        }

        if (_playerController == null)
        {
            Debug.LogWarning("PlayerController not found on " + gameObject.name);
        }
    }

    public void DealDamage()
    {
        if (_combatSystem != null)
        {
            _combatSystem.DealDamage();
        }
    }

    public void SpawnAttackEffect()
    {
        if (_combatSystem != null)
        {
            _combatSystem.SpawnVFX();
        }
    }

    public void PlayFootStepSound()
    {
        
    }

    public void OnAnimationComplete()
    {
        
    }
}

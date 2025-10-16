using Unity.VisualScripting;
using UnityEngine;

public class AnimationEventRelay : MonoBehaviour
{
    [SerializeField] private GameObject warningSprite;
    
    private CombatSystem _combatSystem;
    private PlayerController _playerController;
    private EnemyBase _enemyBase;

    private void Awake()
    {
        _combatSystem = GetComponentInParent<CombatSystem>();
        _playerController = GetComponentInParent<PlayerController>();
        _enemyBase = GetComponentInParent<EnemyBase>();
        

        if (_combatSystem == null)
        {
            Debug.LogWarning($"CombatSystem not found on {gameObject.name}");
        }

        if (_playerController == null)
        {
            Debug.LogWarning($"PlayerController not found on {gameObject.name}");
        }

        if (_enemyBase == null)
        {
            Debug.LogWarning($"EnemyBase not found on {gameObject.name}");
        }
    }

    public void DealDamage()
    {
        if (_combatSystem != null)
        {
            _combatSystem.DealDamage();
        }
    }
    
    public void EnemyWarning()
    {
        if (_enemyBase != null)
        {
            _enemyBase.EnemyWarning();
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
        if (warningSprite != null)
        {
            warningSprite.SetActive(false);
        }
    }

    public void DealDamageToPlayer()
    {
        if (_enemyBase != null)
        {
            _enemyBase.DealDamageToPlayer();
        }
    }
}

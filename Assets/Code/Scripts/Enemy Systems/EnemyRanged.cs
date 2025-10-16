using UnityEngine;

public class RangeEnemy : EnemyBase
{
    [Header("Ranged Attack Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;

    // Override the base PerformAttack method
    protected override void PerformAttack()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        
        FireProjectile();
    }
    
    public void FireProjectile()
    {
        if (projectilePrefab == null || projectileSpawnPoint == null || PlayerTransform == null)
        {
            return;
        }
        
        var directionToPlayer = (PlayerTransform.position - projectileSpawnPoint.position).normalized;
        
        var projectileGO = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
        
        var projectile = projectileGO.GetComponent<EnemyProjectile>();
        if (projectile != null)
        {
            projectile.Initialize(directionToPlayer);
        }
    }
}

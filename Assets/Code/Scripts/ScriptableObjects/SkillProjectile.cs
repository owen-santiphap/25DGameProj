using System;
using UnityEngine;

public class SkillProjectile : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float speed = 20f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private GameObject projectileVfxPrefab;

    private bool _hasHit = false;

    public void Initialize()
    {
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = transform.forward * speed;
        }
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasHit) return;

        // Check if the object hit is on the enemy layer
        if ((enemyLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            _hasHit = true;
            var status = other.GetComponent<EnemyStatusEffect>();
            if (status != null)
            {
                status.AddStack();
                Instantiate(projectileVfxPrefab, other.transform.position, Quaternion.identity);
            }
            Destroy(gameObject);
        }
    }
}
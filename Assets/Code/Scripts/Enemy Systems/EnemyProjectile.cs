using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyProjectile : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private GameObject hitVFX;

    [Header("Collision")]
    [SerializeField] private LayerMask playerLayer;

    private bool _hasHit = false;

    public void Initialize(Vector3 direction)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = direction.normalized * speed;
        }

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasHit) return;

        // Only collide with objects on the player layer (optional)
        if ((playerLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            _hasHit = true;

            var playerHealth = other.GetComponent<HealthSystem>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }

            if (hitVFX != null)
            {
                Instantiate(hitVFX, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
    }
}

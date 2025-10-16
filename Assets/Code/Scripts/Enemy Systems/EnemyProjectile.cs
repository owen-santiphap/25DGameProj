using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 15f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private GameObject hitVFX;

    private Vector3 _direction;

    public void Initialize(Vector3 direction)
    {
        _direction = direction.normalized;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += _direction * (speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
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
using UnityEngine;
using UnityEngine.UI;

public class EnemyStatusEffect : MonoBehaviour
{
    [Header("Explosion Settings")]
    [SerializeField] private int stacksToExplode = 3;
    [SerializeField] private GameObject explosionVFX;
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private int explosionDamage = 10;
    [SerializeField] private LayerMask damageLayer;
    [Header("UI References")]
    [SerializeField] private GameObject stackUIPrefab; 
    private GameObject _stackUIInstance;
    private Text _stackText;

    private int _currentStacks = 0;
    private HealthSystem _healthSystem;

    private void Awake()
    {
        _healthSystem = GetComponent<HealthSystem>();
        damageLayer = LayerMask.GetMask("Enemy");
    }

    public void AddStack()
    {
        if (_healthSystem.IsDead) return;

        _currentStacks++;
        UpdateStackUI();

        if (_currentStacks >= stacksToExplode)
        {
            Explode();
        }
    }

    private void UpdateStackUI()
    {
        if (_stackUIInstance == null && stackUIPrefab != null)
        {
            // Instantiate the UI above the enemy's head
            _stackUIInstance = Instantiate(stackUIPrefab, transform.position + Vector3.up * 2f, Quaternion.identity, transform);
            _stackText = _stackUIInstance.GetComponentInChildren<Text>();
        }

        if (_stackText != null)
        {
            _stackUIInstance.SetActive(true);
            _stackText.text = _currentStacks.ToString();
        }
    }

    private void Explode()
    {
        // Spawn explosion VFX
        if (explosionVFX != null)
        {
            Instantiate(explosionVFX, transform.position, Quaternion.identity);
        }

        // Deal AOE damage to other enemies
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius, damageLayer);
        foreach (var hitCollider in hitColliders)
        {
            // Don't damage self
            if (hitCollider.gameObject == gameObject) continue;

            var enemyHealth = hitCollider.GetComponent<HealthSystem>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(explosionDamage);
            }
        }

        // Instantly kill this enemy
        if (_healthSystem != null)
        {
            _healthSystem.TakeDamage(_healthSystem.CurrentHearts + 999); // Overkill to ensure death
        }

        if (_stackUIInstance != null)
        {
            Destroy(_stackUIInstance);
        }
    }
}

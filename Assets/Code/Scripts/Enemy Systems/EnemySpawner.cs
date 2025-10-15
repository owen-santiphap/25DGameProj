using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Wave Data")]
    [SerializeField] private List<EnemyWaveData> enemyWaves;

    [Header("Spawn Locations")]
    [SerializeField] private List<Transform> spawnPoints;

    private List<float> _spawnTimers;
    private Dictionary<EnemyWaveData, int> _activeEnemies;

    private void Start()
    {
        _spawnTimers = new List<float>();
        _activeEnemies = new Dictionary<EnemyWaveData, int>();

        foreach (var wave in enemyWaves)
        {
            _spawnTimers.Add(wave.spawnInterval);
            _activeEnemies.Add(wave, 0);
        }
    }

    private void Update()
    {
        for (int i = 0; i < enemyWaves.Count; i++)
        {
            _spawnTimers[i] -= Time.deltaTime;
            if (_spawnTimers[i] <= 0)
            {
                TrySpawnEnemy(enemyWaves[i], i);
                _spawnTimers[i] = enemyWaves[i].spawnInterval;
            }
        }
    }

    private void TrySpawnEnemy(EnemyWaveData wave, int waveIndex)
    {
        if (_activeEnemies[wave] >= wave.maxConcurrentSpawns) return;

        if (spawnPoints.Count == 0)
        {
            Debug.LogError("No spawn points assigned to the EnemySpawner.");
            return;
        }

        var spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
        var enemyInstance = Instantiate(wave.enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        
        _activeEnemies[wave]++;

        // Subscribe to the enemy's death event to update our count
        var enemyHealth = enemyInstance.GetComponent<HealthSystem>();
        if (enemyHealth != null)
        {
            enemyHealth.OnDeath.AddListener(() => OnEnemyDied(wave));
        }
    }

    private void OnEnemyDied(EnemyWaveData wave)
    {
        _activeEnemies[wave]--;
        GameManager.Instance.AddScore(wave.scoreValue);
    }
}
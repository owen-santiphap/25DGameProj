using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Wave", menuName = "Spawning/Enemy Wave Data")]
public class EnemyWaveData : ScriptableObject
{
    [Header("Enemy Info")]
    public GameObject enemyPrefab;
    public int scoreValue = 10;

    [Header("Spawning Settings")]
    [Tooltip("The maximum number of this enemy type allowed to be active at once.")]
    public int maxConcurrentSpawns = 5;
    [Tooltip("The time in seconds between each spawn attempt for this enemy.")]
    public float spawnInterval = 3f;
}
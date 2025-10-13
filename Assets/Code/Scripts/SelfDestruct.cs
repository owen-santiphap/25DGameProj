using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    [Tooltip("The lifetime of the object in seconds.")]
    [SerializeField] private float lifetime = 2f;

    private void Start()
    {
        // Destroy the game object this script is attached to after 'lifetime' seconds
        Destroy(gameObject, lifetime);
    }
}
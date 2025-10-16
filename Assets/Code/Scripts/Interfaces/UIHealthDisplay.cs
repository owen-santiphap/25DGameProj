using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIHealthDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HealthSystem playerHealth;
    [SerializeField] private GameObject heartPrefab;
    [SerializeField] private Transform heartsContainer;
    
    [Header("Heart Sprites")]
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite emptyHeart;
    
    [Header("Settings")]
    [SerializeField] private float heartSpacing = 50f;
    
    private readonly List<Image> _heartImages = new List<Image>();
    
    private void Start()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged.AddListener(UpdateHearts);
            InitializeHearts();
        }
    }
    
    private void InitializeHearts()
    {
        // Clear existing hearts
        foreach (Image heart in _heartImages)
        {
            if (heart != null)
                Destroy(heart.gameObject);
        }
        _heartImages.Clear();
        
        // Create heart UI for max health
        for (int i = 0; i < playerHealth.MaxHearts; i++)
        {
            var heartObj = Instantiate(heartPrefab, heartsContainer);
            var heartImage = heartObj.GetComponent<Image>();
            
            if (heartImage != null)
            {
                _heartImages.Add(heartImage);
                
                // Position hearts with spacing
                var rectTransform = heartObj.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = new Vector2(i * heartSpacing, 0);
                }
            }
        }
        
        UpdateHearts(playerHealth.CurrentHearts);
    }
    
    private void UpdateHearts(int currentHearts)
    {
        for (int i = 0; i < _heartImages.Count; i++)
        {
            if (_heartImages[i] != null)
            {
                _heartImages[i].sprite = i < currentHearts ? fullHeart : emptyHeart;
            }
        }
    }
    
    // Call this if max hearts change during gameplay
    public void RefreshHeartDisplay()
    {
        InitializeHearts();
    }
}
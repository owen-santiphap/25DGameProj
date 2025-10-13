using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISkillDisplay : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image cooldownOverlay; // A dark, radial-filled image
    [SerializeField] private TMP_Text cooldownText;

    private void Start()
    {
        // Hide text and overlay initially
        cooldownText.gameObject.SetActive(false);
        cooldownOverlay.gameObject.SetActive(false);
    }

    // Assign the skill's icon from the SkillData
    public void SetIcon(Sprite icon)
    {
        if (icon != null)
        {
            iconImage.sprite = icon;
        }
    }

    public void UpdateCooldown(float currentTime, float totalTime)
    {
        if (currentTime > 0)
        {
            cooldownOverlay.gameObject.SetActive(true);
            cooldownText.gameObject.SetActive(true);

            cooldownOverlay.fillAmount = currentTime / totalTime;
            cooldownText.text = currentTime.ToString("F1"); // Format to one decimal place
        }
        else
        {
            cooldownOverlay.gameObject.SetActive(false);
            cooldownText.gameObject.SetActive(false);
        }
    }
}
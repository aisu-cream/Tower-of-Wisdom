using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarScript : MonoBehaviour

{
    private float currentHealth;
    private float maxHealth;
    [SerializeField] private Slider healthBarSlider;
    [SerializeField] private TextMeshProUGUI healthBarValueText;
    [SerializeField] private IEntity health;

    // Update is called once per frame
    void Update()
    {
        if (health == null) return;
        currentHealth = health.GetHealth();
        maxHealth = health.GetInitialHealth();
        healthBarSlider.value = currentHealth;
        healthBarValueText.text = currentHealth.ToString() + " / " + maxHealth.ToString();
    }
}

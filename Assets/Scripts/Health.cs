using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Health : MonoBehaviour
{

    public TextMeshProUGUI healthText;
    public PlayerController player;
    public string prefix = "HP: ";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null && healthText != null)
        {
            healthText.text = prefix + "" + player.health.ToString("F0");

            if (player.health < 30)
            {
                healthText.color = Color.red;
            }
            else
            {
                healthText.color = Color.white;
            }
        }
    }
}

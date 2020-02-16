using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridStatsUI : MonoBehaviour
{
    public Image healthBar;

    public void SetHealth(float health)
    {
        healthBar.fillAmount = health;
    }

}

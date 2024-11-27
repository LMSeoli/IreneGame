using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fall : MonoBehaviour
{
    GameManager GM;
    // Start is called before the first frame update
    void Start()
    {
        GM = FindObjectOfType<GameManager>();
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            GM.health -= 20;
            GM.UIHealth.fillAmount = (float)GM.health/GM.maxHealth;
            //다시 돌려놓기
            if (GM.health > 0)
                GM.PlayerReposition();
            else
            {
                GM.UIRestartBtn.SetActive(true);
                GM.player.OnDie();
            }
        }
    }
}

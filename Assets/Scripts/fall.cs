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
            GM.UIHealth[GM.health - 1].color = new Color(1, 0, 0, 0.4f);
            GM.health--;
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

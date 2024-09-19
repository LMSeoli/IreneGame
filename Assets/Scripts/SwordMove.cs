using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D;
using UnityEngine;

public class SwordMove : MonoBehaviour
{
    public GameObject Player;
    public GameObject Center;
    public TrailRenderer TrailEffect;

    Animator playerAnim;
    PlayerMove playerMove;
    SpriteRenderer spriteRenderer;

    private void Awake()
    {
        playerAnim = Player.GetComponent<Animator>();
        playerMove = Player.GetComponent<PlayerMove>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }


    //인스펙터 상에서 포지션을 잡을 때, 해당 포지션의 0,0,0기준은 상위 오브젝트지만, 프로그램 상에서 포지션을 잡을 때 0,0,0기준은 프로젝트의 0,0,0이기 때문에, 항상 플레이어의 위치도 입력해야 한다.
    public IEnumerator NormalSlash(bool dir)
    {
        spriteRenderer.enabled = true;
        spriteRenderer.flipY = !dir;
        Center.transform.rotation = Quaternion.Euler(0, 0, !dir ? 50 : 130);
        if(dir) Center.transform.position = new Vector3(Player.transform.position.x+0.08f, Player.transform.position.y+0.04f, 0);
        else Center.transform.position = new Vector3(Player.transform.position.x-0.08f, Player.transform.position.y-0.04f, 0);

        TrailEffect.enabled = true;
        //위에서 잠시 기다리고
        yield return new WaitForSeconds(0.05f);     //위에 있는 시간

        //바로 밑으로 이동
        float Downtime = Time.time;
        while (Time.time - Downtime < 0.02f)        //시전시간 있다면 이거 사용
        {
            Center.transform.Rotate(0, 0, !dir ? 50 - (Time.time - Downtime)*5000 : 130 + (Time.time - Downtime)*5000);
            yield return new WaitForSeconds(0.001f);
        }
        Center.transform.rotation = Quaternion.Euler(0, 0, !dir ? -50 : 230);

        yield return new WaitForSeconds(0.2f);      //밑에서 활성화되어있는 시간. 애니메이션이 끝날 때 같이 사라지는 게 나을 것 같으니
        playerAnim.SetBool("NormalSlash", false);
        TrailEffect.enabled = false;
        spriteRenderer.enabled = false;
        playerMove.isSkill = false;
    }

    public IEnumerator NormalSting()
    {
        yield return null;
    }

    public IEnumerator DownZ()
    {
        yield return null;
        //땅에 꽂힐 때 자신과 플레이어의 움직임을 멈춰야댐

        //화면 지진
    }

    public IEnumerator S1Move()
    {
        yield return null;
    }

    public IEnumerator S2Move()
    {
        //
        yield return null;
    }
}

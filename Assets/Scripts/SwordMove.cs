using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D;
using UnityEngine;

public class SwordMove : MonoBehaviour
{
    public GameObject Player;
    public GameObject Center;
    public TrailRenderer TrailEffect;
    public PolygonCollider2D Collider;

    PlayerMove playerMove;
    SpriteRenderer spriteRenderer;

    private void Awake()
    {
        playerMove = Player.GetComponent<PlayerMove>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public IEnumerator NormalSlash(bool dir)
    {
        spriteRenderer.enabled = true;
        spriteRenderer.flipY = !dir;
        Center.transform.rotation = Quaternion.Euler(0, 0, !dir ? 50 : 130);

        Collider.enabled = true;
        TrailEffect.enabled = true;
        //위에서 잠시 기다리고
        yield return new WaitForSeconds(0.05f);

        //바로 밑으로 이동
        float Downtime = Time.time;
        while (Time.time - Downtime < 0.01f)
        {
            Center.transform.Rotate(0, 0, !dir ? 50 - (Time.time - Downtime)*10000 : 130 + (Time.time - Downtime)*10000);
            yield return new WaitForSeconds(0.001f);
        }
        Center.transform.rotation = Quaternion.Euler(0, 0, !dir ? -50 : 230);
        Collider.enabled = false;

        yield return new WaitForSeconds(0.2f);
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

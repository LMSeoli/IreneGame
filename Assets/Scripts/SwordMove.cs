using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordMove : MonoBehaviour
{
    public GameObject Player;
    public TrailRenderer TrailEffect;
    public PolygonCollider2D Collider;

    PlayerMove playerMove;

    private void Awake()
    {
        playerMove = GetComponent<PlayerMove>();
    }

    public IEnumerator NormalSlash()
    {
        //불러낼 때 위치를 위로 바꾼다 가정
        yield return new WaitForSeconds(0.1f);
        Collider.enabled = true;
        TrailEffect.enabled = true;

        yield return new WaitForSeconds(0.2f);
        Collider.enabled = true;

        yield return new WaitForSeconds(0.3f);
        TrailEffect.enabled = true;
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

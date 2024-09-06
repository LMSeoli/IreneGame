using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordMove : MonoBehaviour
{
    public GameObject Player;

    PlayerMove playerMove;

    private void Awake()
    {
        playerMove = GetComponent<PlayerMove>();
    }

    public void NormalSlash(float angle)
    {
        //불러낼 때 위치를 위로 바꾼다 가정
    }

    public void NormalSting()
    {

    }

    public void DownZ()
    {

        //땅에 꽂힐 때 자신과 플레이어의 움직임을 멈춰야댐

        //화면 지진
    }

    public void S1Move()
    {

    }

    public void S2Move()
    {
        //
    }
}

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
        //�ҷ��� �� ��ġ�� ���� �ٲ۴� ����
    }

    public void NormalSting()
    {

    }

    public void DownZ()
    {

        //���� ���� �� �ڽŰ� �÷��̾��� �������� ����ߴ�

        //ȭ�� ����
    }

    public void S1Move()
    {

    }

    public void S2Move()
    {
        //
    }
}

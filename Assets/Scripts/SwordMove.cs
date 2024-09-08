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
        //�ҷ��� �� ��ġ�� ���� �ٲ۴� ����
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
        //���� ���� �� �ڽŰ� �÷��̾��� �������� ����ߴ�

        //ȭ�� ����
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

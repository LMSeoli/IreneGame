using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D;
using UnityEngine;

public class SwordMove : MonoBehaviour
{
    public GameObject Player;
    public GameObject Center;
    public TrailRenderer TrailEffect;

    Animator anim;
    Animator playerAnim;
    SpriteRenderer spriteRenderer;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        playerAnim = Player.GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    //�ν����� �󿡼� �������� ���� ��, �ش� �������� 0,0,0������ ���� ������Ʈ����, ���α׷� �󿡼� �������� ���� �� 0,0,0������ ������Ʈ�� 0,0,0�̱� ������, �׻� �÷��̾��� ��ġ�� �Է��ؾ� �Ѵ�.

    public IEnumerator S1SwordMove(bool dir)
    {
        spriteRenderer.enabled = true;
        spriteRenderer.flipY = !dir;
        Center.transform.rotation = Quaternion.Euler(0, 0, !dir ? 50 : 130);
        if (dir) Center.transform.position = new Vector3(Player.transform.position.x + 0.08f, Player.transform.position.y + 0.04f, 0);
        else Center.transform.position = new Vector3(Player.transform.position.x - 0.08f, Player.transform.position.y - 0.04f, 0);
        TrailEffect.enabled = true;
        //������ ��� ��ٸ���
        yield return new WaitForSeconds(0.05f);     //���� �ִ� �ð�
        //�ٷ� ������ �̵�
        float Downtime = Time.time;
        while (Time.time - Downtime < 0.04f)        //�����ð� �ִٸ� �̰� ���
        {
            Center.transform.Rotate(0, 0, !dir ? 50 - (Time.time - Downtime) * 2500 : 130 + (Time.time - Downtime) * 2500); //100��/0.04 = 2500
            yield return new WaitForSeconds(0.001f);
        }
        Center.transform.rotation = Quaternion.Euler(0, 0, !dir ? -50 : 230);
        yield return new WaitForSeconds(0.05f);     //�Ʒ��� �ִ� �ð�
        Downtime = Time.time;
        while (Time.time - Downtime < 0.04f)        //�����ð� �ִٸ� �̰� ���
        {
            Center.transform.Rotate(0, 0, !dir ? -50 + (Time.time - Downtime) * 2500 : 230 - (Time.time - Downtime) * 2500);
            yield return new WaitForSeconds(0.001f);
        }
        Center.transform.rotation = Quaternion.Euler(0, 0, !dir ? 50 : 130);
        yield return new WaitForSeconds(0.05f);     //�Ʒ��� �ִ� �ð�
        Downtime = Time.time;
        while (Time.time - Downtime < 0.04f)        //�����ð� �ִٸ� �̰� ���
        {
            Center.transform.Rotate(0, 0, !dir ? 50 - (Time.time - Downtime) * 2500 : 130 + (Time.time - Downtime) * 2500);
            yield return new WaitForSeconds(0.001f);
        }
        Center.transform.rotation = Quaternion.Euler(0, 0, !dir ? -50 : 230);
        yield return new WaitForSeconds(0.05f);     //�Ʒ��� �ִ� �ð�
        //���� ������ �ڷ� ����, �̱���. ��������Ʈ�� ����� ��. ���������� �ڷ� ���� ���ٴ� ��������Ʈ�� �׷��� 3�ܰ�� ������ �ڷ� ���� �ϴ� �� ���� �� ����
        //Center.transform.rotation = Quaternion.Euler(0, 0, !dir ? 0 : -180);
        //��� ���ߴ� �ð�, �̰� ���� ��������Ʈ�� ���߱�
        spriteRenderer.enabled = false;
        TrailEffect.enabled = false;
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

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


    //�ν����� �󿡼� �������� ���� ��, �ش� �������� 0,0,0������ ���� ������Ʈ����, ���α׷� �󿡼� �������� ���� �� 0,0,0������ ������Ʈ�� 0,0,0�̱� ������, �׻� �÷��̾��� ��ġ�� �Է��ؾ� �Ѵ�.
    public IEnumerator NormalSlash(bool dir)
    {
        spriteRenderer.enabled = true;
        spriteRenderer.flipY = !dir;
        Center.transform.rotation = Quaternion.Euler(0, 0, !dir ? 50 : 130);
        if(dir) Center.transform.position = new Vector3(Player.transform.position.x+0.08f, Player.transform.position.y+0.04f, 0);
        else Center.transform.position = new Vector3(Player.transform.position.x-0.08f, Player.transform.position.y-0.04f, 0);

        TrailEffect.enabled = true;
        //������ ��� ��ٸ���
        yield return new WaitForSeconds(0.05f);     //���� �ִ� �ð�

        //�ٷ� ������ �̵�
        float Downtime = Time.time;
        while (Time.time - Downtime < 0.02f)        //�����ð� �ִٸ� �̰� ���
        {
            Center.transform.Rotate(0, 0, !dir ? 50 - (Time.time - Downtime)*5000 : 130 + (Time.time - Downtime)*5000);
            yield return new WaitForSeconds(0.001f);
        }
        Center.transform.rotation = Quaternion.Euler(0, 0, !dir ? -50 : 230);

        yield return new WaitForSeconds(0.2f);      //�ؿ��� Ȱ��ȭ�Ǿ��ִ� �ð�. �ִϸ��̼��� ���� �� ���� ������� �� ���� �� ������
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

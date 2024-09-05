using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Color = UnityEngine.Color;

public class E1Move : MonoBehaviour
{
    Rigidbody2D rigid;
    public int nextmove;
    public bool noMove;

    Animator anim;
    SpriteRenderer spriteRenderer;
    GameManager gameManager;
    new CapsuleCollider2D collider;
    EnemyBasicMove enemyBasicMove;
    
    
    //ó��1ȸ����
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        collider = GetComponent<CapsuleCollider2D>();
        gameManager = FindObjectOfType<GameManager>();
        enemyBasicMove = gameObject.GetComponent<EnemyBasicMove>();
        Think();
        Invoke("Think", 5); //�־��� �ð��� ���� ��, ������ �Լ��� �����ϴ� �Լ�
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (enemyBasicMove.isHit == false && noMove == false)
        {
            //�⺻ ������
            rigid.velocity = new Vector2(nextmove, rigid.velocity.y);

            //�÷��� üũ
            Vector2 frontVec = new Vector2(rigid.position.x + nextmove * 0.3f, rigid.position.y);

            // �������� ���ϴ� ª�� ����ĳ��Ʈ (�������� ����)
            Vector2 rayDirectionFront = new Vector2(nextmove, -1).normalized;
            Debug.DrawRay(frontVec, rayDirectionFront, new Color(0, 1, 0));
            RaycastHit2D frontRayHit = Physics2D.Raycast(frontVec, rayDirectionFront, 1f, LayerMask.GetMask("Platform"));

            // �ٷ� �Ʒ��� �߻�Ǵ� ����ĳ��Ʈ (�� �Ʒ� ���� ����)
            Vector2 rayDirectionDown = Vector2.down;
            Debug.DrawRay(frontVec, rayDirectionDown, new Color(1, 0, 0));
            RaycastHit2D downRayHit = Physics2D.Raycast(frontVec, rayDirectionDown, 1f, LayerMask.GetMask("Platform"));

            // ���������� ������ ���� ������ �ٲ��� �ʵ��� ���� �߰�
            if (frontRayHit.collider == null && downRayHit.collider == null)
            {
                Turn(); // �� �� �������� ���� ���� ��
            }
        }
    }

    //����Լ��� �� �ؿ� �δ� �� ����!
    void Think()
    {
        if (noMove == false)
        {
            if (enemyBasicMove.isHit == false)
            {
                //Set Next Active
                nextmove = Random.Range(-1, 2); //�ּ�~(�ִ�-1) �� ���� �� ����
                Invoke("Think", 5);     //�� �ð��� �������� �ٲ㵵 �Ǵµ�, ������ ����?

                //Sprite Anim
                //anim.SetInteger("WalkSpeed", nextmove); 

                //Flip Sprite
                if (nextmove != 0)
                    spriteRenderer.flipX = nextmove == 1;
            }
            else
            {
                Invoke("Think", 5);     //�� �ð��� �������� �ٲ㵵 �Ǵµ�, ������ ����?
            }
        }
    }

    void Turn()
    {
        nextmove *= -1;
        spriteRenderer.flipX = nextmove == 1;
        //CancelInvoke();
        Invoke("Think", 5);
    }
}
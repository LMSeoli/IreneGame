using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GliderMove : MonoBehaviour
{
    Rigidbody2D rigid;
    public int nextmove;
    public bool noMove;
    public bool followPlayer;
    public float notfollowPlayer;
    public GameObject player;

    Animator anim;
    SpriteRenderer spriteRenderer;
    GameManager gameManager;
    new CapsuleCollider2D collider;
    EnemyBasicMove enemyBasicMove;
    RelicManager relicManager;

    //ó��1ȸ����
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        collider = GetComponent<CapsuleCollider2D>();
        gameManager = FindObjectOfType<GameManager>();
        enemyBasicMove = gameObject.GetComponent<EnemyBasicMove>();
        relicManager = FindObjectOfType<RelicManager>();
        Think();
        Invoke("Think", 5); //�־��� �ð��� ���� ��, ������ �Լ��� �����ϴ� �Լ�
    }

    private void Update()
    {
        DetectPlayer();
        if (notfollowPlayer > 0) notfollowPlayer += Time.deltaTime;
        //spriteRenderer.flipX = rigid.velocity.x>=0;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (enemyBasicMove.onAir == false && noMove == false && rigid.bodyType != RigidbodyType2D.Kinematic)
        {
            //�⺻ ������
            if (notfollowPlayer > 0)
            {
                rigid.velocity = new Vector2(0, rigid.velocity.y);
            }
            else if (followPlayer)
            {
                nextmove = player.transform.position.x > gameObject.transform.position.x ? 1 : -1;
                rigid.velocity = new Vector2(nextmove*enemyBasicMove.moveSpeed, rigid.velocity.y);
            }
            else        //notfollowPlayer�� 0�̰� followPlayer�� 0�̶�� �⺻ �̵�
            {
                rigid.velocity = new Vector2(nextmove*enemyBasicMove.moveSpeed, rigid.velocity.y);
            }
            //(relicManager.RelicItems.Exists(item => item.isOwned && item.number == 2) ? 0.8f : 1)

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
                //����ٸ� �״��
                if (notfollowPlayer > 0) { }
                //�÷��̾ ���󰡴� ���������� �����ߴٸ� ����
                else if (followPlayer)
                {
                    notfollowPlayer = 0.001f;
                    nextmove = player.transform.position.x > gameObject.transform.position.x ? 1 : -1;
                    CancelInvoke("Think");
                    Invoke("Think", 5);
                }
                else Turn(); // �� �� �������� ���� ���� ��
            }
        }
    }

    //����Լ��� �� �ؿ� �δ� �� ����!
    void Think()
    {
        CancelInvoke("Think");
        if (noMove == false)
        {
            if (enemyBasicMove.onAir == false)
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

    private void DetectPlayer()
    {
        //�÷��̾ ���� �ȿ� ���Դٸ� ����
        if (Mathf.Abs(player.transform.position.x - gameObject.transform.position.x) < 6 && Mathf.Abs(player.transform.position.y - gameObject.transform.position.y) < 6)
        {
            //�������� ���̶��
            if (notfollowPlayer > 0)
            {
                //�÷��̾ ���������� �ݴ������� �̵��ߴٸ�
                if (Mathf.Sign(player.transform.position.x - gameObject.transform.position.x) == -nextmove)
                {
                    //�ٽ� ����
                    notfollowPlayer = 0;
                }
                //���� �� 3�ʰ� �����ٸ�
                else if (notfollowPlayer >= 3)
                {
                    //������ �ݴ�� �ٲٰ�
                    nextmove = -nextmove;
                    //�ٽ� �ɾ
                    followPlayer = false;
                    notfollowPlayer = 0;
                    Invoke("Think", 5);
                }
            }
            //�ƴϸ� �׳� ����
            else
            {
                spriteRenderer.flipX = nextmove == 1;
                CancelInvoke("Think");
                followPlayer = true;
            }
        }
        //�÷��̾ ���� �ȿ� ���ٸ� ������ ����
        else
        {
            if (notfollowPlayer == 0) Invoke("Think", 5);
        }
    }

    void Turn()
    {
        nextmove *= -1;
        spriteRenderer.flipX = nextmove == 1;
        CancelInvoke("Think");
        Invoke("Think", 5);
    }

    private void OnDrawGizmos()
    {
        //�÷��̾� ���󰡴� ����
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(gameObject.transform.position, new Vector3(12, 12, 0));     //�� ���� ����, ������ ������Ʈ �߽��� �����̱⿡ ������ 6����?
    }
}
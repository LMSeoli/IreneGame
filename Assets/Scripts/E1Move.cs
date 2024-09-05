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
    
    
    //처음1회실행
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        collider = GetComponent<CapsuleCollider2D>();
        gameManager = FindObjectOfType<GameManager>();
        enemyBasicMove = gameObject.GetComponent<EnemyBasicMove>();
        Think();
        Invoke("Think", 5); //주어진 시간이 지난 뒤, 지정된 함수를 실행하는 함수
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (enemyBasicMove.isHit == false && noMove == false)
        {
            //기본 움직임
            rigid.velocity = new Vector2(nextmove, rigid.velocity.y);

            //플랫폼 체크
            Vector2 frontVec = new Vector2(rigid.position.x + nextmove * 0.3f, rigid.position.y);

            // 앞쪽으로 향하는 짧은 레이캐스트 (내리막길 감지)
            Vector2 rayDirectionFront = new Vector2(nextmove, -1).normalized;
            Debug.DrawRay(frontVec, rayDirectionFront, new Color(0, 1, 0));
            RaycastHit2D frontRayHit = Physics2D.Raycast(frontVec, rayDirectionFront, 1f, LayerMask.GetMask("Platform"));

            // 바로 아래로 발사되는 레이캐스트 (발 아래 지형 감지)
            Vector2 rayDirectionDown = Vector2.down;
            Debug.DrawRay(frontVec, rayDirectionDown, new Color(1, 0, 0));
            RaycastHit2D downRayHit = Physics2D.Raycast(frontVec, rayDirectionDown, 1f, LayerMask.GetMask("Platform"));

            // 내리막길을 감지할 때만 방향을 바꾸지 않도록 조건 추가
            if (frontRayHit.collider == null && downRayHit.collider == null)
            {
                Turn(); // 둘 다 감지되지 않을 때만 턴
            }
        }
    }

    //재귀함수는 맨 밑에 두는 게 좋음!
    void Think()
    {
        if (noMove == false)
        {
            if (enemyBasicMove.isHit == false)
            {
                //Set Next Active
                nextmove = Random.Range(-1, 2); //최소~(최대-1) 의 랜덤 수 생성
                Invoke("Think", 5);     //이 시간도 랜덤으로 바꿔도 되는데, 지금은 굳이?

                //Sprite Anim
                //anim.SetInteger("WalkSpeed", nextmove); 

                //Flip Sprite
                if (nextmove != 0)
                    spriteRenderer.flipX = nextmove == 1;
            }
            else
            {
                Invoke("Think", 5);     //이 시간도 랜덤으로 바꿔도 되는데, 지금은 굳이?
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
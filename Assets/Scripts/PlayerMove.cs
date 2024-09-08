using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public GameManager gameManager;
    public AudioClip audioJump;
    public AudioClip audioAttack;
    public AudioClip audioDamaged;
    public AudioClip audioItem;
    public AudioClip audioDie;
    public AudioClip audioFinish;
    public AudioClip audioBulletIn;
    public AudioClip audioSlideIn;
    public AudioClip audioS3Start;
    public AudioClip audioS3Bound;
    public AudioClip audioS3Shooting;
    public AudioClip audioS3End;
    public AudioClip[] audioS3Voice;

    public float maxSpeed;  //얘네는 유니티쪽에서 설정 가능!
    public float jumpPower;
    public float S1CoolTime;
    public float S1CoolTime_max;
    public float S2CoolTime;
    public float S2CoolTime_max;
    public float S3CoolTime;
    public float S3CoolTime_max;
    public float AttackType;                //0이라면 총, 1이라면 칼
    public bool isReload = false;
    //public bool isS1 = false;
    //public bool isS2 = false;
    //public bool isS3 = false;
    public bool isSkill = false;
    public float S2Speed;
    public float S2STCount;
    public float S3time;

    public Vector3 S2Start;
    public Vector3 S2End;
    public GameObject HandCannon;
    public GameObject Sword;
    public GameObject S3Airbone;
    public GameObject S3AttackArea;

    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    CapsuleCollider2D capsuleCollider;
    Animator anim;
    AudioSource audioSource;
    HandCannonMove handCannonMove;
    SwordMove swordMove;

    // Start is called before the first frame update
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        anim = GetComponent<Animator>();  // Animator 컴포넌트 초기화
        audioSource = GetComponent<AudioSource>();
        handCannonMove = HandCannon.GetComponent<HandCannonMove>();
        swordMove = Sword.GetComponent<SwordMove>();
    }

    //단발적인 입력은 여기에!!
    void Update()
    {
        //Jump
        if (Input.GetButtonDown("Jump") && !anim.GetBool("isJumping"))
        {
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);    //힘만큼 가속!
            anim.SetBool("isJumping", true);
            PlaySound("JUMP");
        }

        //Stop speed
        if (Input.GetButtonUp("Horizontal"))
        {
            rigid.velocity = new Vector2(rigid.velocity.normalized.x * 0.01f, rigid.velocity.y); //float끼리 곱할 때는 마지막에 f 붙여주기!!
        }

        //Reload
        if (Input.GetKeyDown(KeyCode.R) && isSkill == false && isReload == false && gameManager.bullet < 6)
        {
            Debug.Log("Reload?");
            isReload = true;
            gameManager.StartCoroutine(gameManager.Reload());
        }

        //Dash
        if ((Input.GetKeyDown(KeyCode.LeftShift)||Input.GetKeyDown(KeyCode.RightShift)) && isSkill == false && isReload == false)
        {
            //잔상 이펙트를 가진
            //대시 구현해야댐!!
        }

        //Z_Press
        if (Input.GetKeyDown(KeyCode.Z) && isSkill == false && isReload == false)
        {
            if (anim.GetBool("isJumping") == true)
            {
                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    //아래로

                    //휘두르면서 90도 돌리고

                    //내려찍기
                }
                else if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    //위로 베기
                }
                else if (Input.GetAxisRaw("Horizontal") != 0)
                {
                    //일반 베기
                    isSkill = true;
                    swordMove.StartCoroutine(swordMove.NormalSlash(spriteRenderer.flipX));
                }
                //아무것도 안 누르고 z만 누른 상태라면
                else if (!HandCannon.activeSelf && gameManager.bullet > 0)
                {
                    //가까운 적에게 발사
                }
            }
            else
            {
                if (Input.GetAxisRaw("Horizontal") != 0 && !HandCannon.activeSelf && gameManager.bullet > 0)
                {
                    HandCannon.SetActive(true);
                    gameManager.BulletDown();
                    handCannonMove.Shot(spriteRenderer.flipX);
                }
                else if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    //위로 베기
                }
                else
                {
                    //전방으로 살짝 돌진하며 찌르기
                }
            }
        }

        //X_Press
        if (Input.GetKeyDown(KeyCode.X) && !anim.GetBool("isJumping") && isSkill == false && isReload == false && S2CoolTime <= 0 && gameManager.bullet > 0)
        {
            //Skill1
            if (Input.GetKey(KeyCode.UpArrow) && S1CoolTime <= 0)
            {
                HandCannon.SetActive(true);
                gameManager.BulletDown();
                isSkill = true;
                StartCoroutine(Skill_1());
            }
            //Skill 2
            else
            {
                isSkill = true;
                gameManager.BulletDown();
                StartCoroutine(Skill_2());
            }
        }

        //Skill_3
        if (Input.GetKeyDown(KeyCode.C) && !anim.GetBool("isJumping") && isSkill == false && isReload == false && S3CoolTime <= 0 && gameManager.bullet > 5)
        {
            isSkill = true;
            StartCoroutine(Skill_3());
        }

        //Direction Sprite
        if (Input.GetButton("Horizontal"))
            spriteRenderer.flipX = Input.GetAxisRaw("Horizontal") == -1;

        //Animation
        if (Mathf.Abs(rigid.velocity.x) < 0.1)
            anim.SetBool("isWalking", false);
        else
            anim.SetBool("isWalking", true);
    }

    void FixedUpdate()
    {
        //쿨타임
        if (S1CoolTime > 0)
        {
            S1CoolTime -= Time.deltaTime;
        }

        if (S2CoolTime > 0)
        {
            S2CoolTime -= Time.deltaTime;
        }

        if (S3CoolTime > 0)
        {
            S3CoolTime -= Time.deltaTime;
        }

        //움직임 속도
        if (isSkill == false)
        {
            //Move Speed
            float h = Input.GetAxisRaw("Horizontal");       //단위를 구한다? normalized가 비슷하대
            rigid.AddForce(Vector2.right * h, ForceMode2D.Impulse);

            //Max Speed
            if (rigid.velocity.x > maxSpeed) //Right Max Speed
                rigid.velocity = new Vector2(maxSpeed, rigid.velocity.y);
            else if (rigid.velocity.x < maxSpeed * (-1)) //Left Max Speed
                rigid.velocity = new Vector2(maxSpeed * (-1), rigid.velocity.y);
        }

        //Landing Platform
        if (rigid.velocity.y < 0)
        {
            Debug.DrawRay(rigid.position, Vector3.down, new UnityEngine.Color(0, 1, 0));         //에디터상에서 Ray를 그려주는 함수, color은 rgb 이용
            RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, Vector3.down, 1, LayerMask.GetMask("Platform")); //빔에 맞은 애에 대한 정보, 이 변수의 콜라이더로 검색 확인 가능
            if (rayHit.collider != null)
            {
                //Debug.Log("??");
                if (rayHit.distance < 0.6f)
                {
                    //Debug.Log(rayHit.collider.name);      //이거로 테스트 가능!
                    anim.SetBool("isJumping", false);
                }
            }
        }
    }

    //피격 이벤트
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "enemy")
        {
            if (isSkill == true)
            {
                /*if (rigid.velocity.y <= 0 && transform.position.y > collision.transform.position.y)
                {
                    PlaySound("ATTACK");
                    OnAttack(collision.transform);
                }
                else*/
                collision.gameObject.layer = 12;
                collision.rigidbody.velocity = Vector2.zero;
                S2EnemyStrike(collision.transform);
                //Physics2D.IgnoreCollision(collision.collider, capsuleCollider, false);
            }
            else
            {
                if (isSkill == false) OnDamaged(collision.transform.position);
            }
        }
        else if (collision.gameObject.tag == "trap") OnDamaged(collision.transform.position);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "item")
        {
            // Point
            PlaySound("ITEM");
            bool isBronze = collision.gameObject.name.Contains("bronze");   //bronze라는 이름을 포함한다면 true
            bool isSilver = collision.gameObject.name.Contains("silver");
            bool isGold = collision.gameObject.name.Contains("gold");
            if (isBronze)
                gameManager.stagePoint += 50;
            else if (isSilver)
                gameManager.stagePoint += 100;
            else if (isGold)
                gameManager.stagePoint += 200;
            //삭제
            collision.gameObject.SetActive(false);
        }
        else if (collision.gameObject.tag == "Finish")
        {
            //Next Stage
            gameManager.NextStage();
        }
    }

    private IEnumerator Skill_1()
    {
        yield return null;
        isSkill = false;
    }

    /* private IEnumerator Skill_2()
     {
         //float originalMaxSpeed = maxSpeed;
         S2Start = this.gameObject.transform.position;
         S2STCount = 0;
         //maxSpeed = S2Speed;

         capsuleCollider.direction = CapsuleDirection2D.Horizontal;
         capsuleCollider.size = new Vector2(1, 0.5f);

         int direction = spriteRenderer.flipX ? -1 : 1;
         S2End = new Vector3(transform.position.x + direction * 6.14f, transform.position.y);    //기본 이동거리가 5.75라 가정할 때, 스프라이트의 크기를 고려하여 0.39f는 추가해야 함
         //direction 방향으로 거리 5의 레이를 발사하고 충돌하는 플랫폼이 존재한다면 그 사이의 거리만큼 이동해야함. 
         // 위아래에서 전방으로 레이를 발사함.
          //맞는 게 없다면 그대로.
         //맞았는데 위와 아래의 값이 거의 동일하다면(벽이라면) 그 직전까지만 이동함
          //맞았는데 위와 아래의 값이 1만큼 차이난다면(오르막길이라면) 어떻게 해야? 멈추는 건 또 맛이 없으니까 이건 안맞더라도 그대로?
          //이건 모두 구현한 다음에 만드는거로
         //float targetPosition = S2Start.x + 5 * direction;

         S2STCount = 0;  // 이동 시간 초기화

         while (Mathf.Abs(transform.position.x - S2Start.x) < 5f)   // 5만큼 이동
         {
             rigid.velocity = new Vector2(maxSpeed * direction, 0);  // 계속해서 속도를 유지하며 이동
             S2STCount += Time.deltaTime;

             yield return null;
         }
         Debug.Log("걸린시간:" + S2STCount);
         Debug.Log("이동거리:" + (transform.position.x - S2Start.x));

         capsuleCollider.size = new Vector2(0.77f, 1);
         maxSpeed = originalMaxSpeed;
         rigid.velocity = Vector2.zero;
         HandCannon.SetActive(true);     //보험
         handCannonMove.S2Shoot(S2End, spriteRenderer.flipX);
         //isS2는 HandCannon에서 끌거임
     }*/

    private IEnumerator Skill_2()
    {
        float distanceToMove = 5.75f;  // 이동할 총 거리
        float moveDuration = 0.1f;     // 이동하는데 걸릴 시간 (일단 0.1초)
        float elapsedTime = 0f;

        S2Start = transform.position;
        S2End = S2Start + new Vector3(spriteRenderer.flipX ? -distanceToMove : distanceToMove, 0, 0);  // 방향에 따른 이동 목표

        capsuleCollider.direction = CapsuleDirection2D.Horizontal;
        capsuleCollider.size = new Vector2(1, 0.5f);  // 캡슐 충돌체 사이즈 변경

        // 시간 경과에 따라 거리를 일정 비율로 이동
        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / moveDuration;  // 현재 시간에 따른 비율 (0~1)
            transform.position = Vector3.Lerp(S2Start, S2End, t);  // 시작 지점에서 목표 지점까지 점진적으로 이동
            yield return null;
        }
        Debug.Log("이동거리 : "+ Mathf.Abs(S2Start.x - transform.position.x));

        // 이동이 끝난 후 캡슐 충돌체 원래 크기로 복구
        capsuleCollider.size = new Vector2(0.77f, 1);
        rigid.velocity = Vector2.zero;

        HandCannon.SetActive(true);  // HandCannon 활성화
        handCannonMove.S2Shoot(S2End, spriteRenderer.flipX);
    }

    private IEnumerator Skill_3()
    {
        Debug.Log("Skill_3 started");
        S3time = Time.time;
        audioSource.PlayOneShot(audioS3Start);
        Time.timeScale = 0.2f;

        while (Time.time - S3time < 0.2f)
        {
            yield return null;
        }

        Debug.Log("Cutscene complete");

        //에어본
        audioSource.PlayOneShot(audioS3Bound);

        S3Airbone.SetActive(true);
        Time.timeScale = 1;
        S3time = Time.time;
        while (Time.time - S3time < 0.4f)
        {
            yield return null;
            if (Time.time - S3time > 0.1f) S3Airbone.SetActive(false);
        }

        Debug.Log("Airborne phase complete");
        
        //회전 회오리
        HandCannon.SetActive(true);     //꼭 핸드캐논 활성화부터!
        handCannonMove.S3Shoot(spriteRenderer.flipX);
        audioSource.PlayOneShot(audioS3Shooting);

        //시간 초기화
        S3time = Time.time;
        S3AttackArea.SetActive(true);

        //S3 보이스
        //audioSource.PlayOneShot(audioS3Voice[Random.Range(0, 4)]);

        while (Time.time - S3time < 1.7f)
        {
            //0.1333초마다 잠시 대상을 바라보는 코드를 넣거나, 중간에 여러 방향을 바라보게 하는 코드를 넣는것도 좋을 거 같음.
            transform.rotation = Quaternion.Euler(0, (Time.time - S3time) * 360 * 12, 0);
            yield return null;
        }

        S3AttackArea.SetActive(false);
        transform.rotation = Quaternion.Euler(0, 0, 0);
        S3time = Time.time;
        audioSource.PlayOneShot(audioS3End);
        Invoke("S3False", 0.4f);
        Debug.Log("Skill_3 complete");
    }
    void S3False()
    {
        isSkill = false;
    }

    void S2EnemyStrike(Transform enemy)
    {
        int direction = spriteRenderer.flipX ? -1 : 1;
        EnemyBasicMove enemyMove = enemy.GetComponent<EnemyBasicMove>();
        enemyMove.S2Hit(S2End, direction, handCannonMove.S2ShootDelay);
    }


    void OnDamaged(Vector2 targetPos)
    {
        gameManager.HealthDown();

        PlaySound("DAMAGED");

        //레이어 변경
        gameObject.layer = 9;

        //색 변경
        spriteRenderer.color = new UnityEngine.Color(1, 1, 1, 0.4f); //rgb다음은 투명도!

        //튕겨나가기
        int dirc = transform.position.x - targetPos.x > 0 ? 1 : -1;
        rigid.AddForce(new Vector2(dirc, 1) * 7, ForceMode2D.Impulse);

        //Animation
        anim.SetTrigger("doDamaged");
        Invoke("OffDamaged", 1);
    }

    void OffDamaged()
    {
        gameObject.layer = 8;
        spriteRenderer.color = new UnityEngine.Color(1, 1, 1, 1);
    }

    public void OnDie()
    {
        PlaySound("DIE");

        spriteRenderer.color = new UnityEngine.Color(1, 1, 1, 0.4f);

        spriteRenderer.flipY = true;

        capsuleCollider.enabled = false;

        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
    }

    public void VelocityZero()
    {
        rigid.velocity = Vector2.zero;
    }

    public void BD()
    {
        gameManager.BulletDown();
    }

    public void PlaySound(string action)
    {
        switch (action)
        {
            case "JUMP":
                audioSource.clip = audioJump;
                break;
            case "ATTACK":
                audioSource.clip = audioAttack;
                break;
            case "DAMAGED":
                audioSource.clip = audioDamaged;
                break;
            case "ITEM":
                audioSource.clip = audioItem;
                break;
            case "DIE":
                audioSource.clip = audioDie;
                break;
            case "FINISH":
                audioSource.clip = audioFinish;
                break;
            case "S3Bound":
                audioSource.clip = audioS3Bound;
                break;
            case "S3Shooting":
                audioSource.clip = audioS3Shooting;
                break;
            case "S3End":
                audioSource.clip = audioS3End;
                break;
        }
        audioSource.Play();
    }
}
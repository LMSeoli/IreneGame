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

    public float maxSpeed;  //��״� ����Ƽ�ʿ��� ���� ����!
    public float jumpPower;
    public float S1CoolTime;
    public float S1CoolTime_max;
    public float S2CoolTime;
    public float S2CoolTime_max;
    public float S3CoolTime;
    public float S3CoolTime_max;
    public float AttackType;                //0�̶�� ��, 1�̶�� Į
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
        anim = GetComponent<Animator>();  // Animator ������Ʈ �ʱ�ȭ
        audioSource = GetComponent<AudioSource>();
        handCannonMove = HandCannon.GetComponent<HandCannonMove>();
        swordMove = Sword.GetComponent<SwordMove>();
    }

    //�ܹ����� �Է��� ���⿡!!
    void Update()
    {
        //Jump
        if (Input.GetButtonDown("Jump") && !anim.GetBool("isJumping"))
        {
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);    //����ŭ ����!
            anim.SetBool("isJumping", true);
            PlaySound("JUMP");
        }

        //Stop speed
        if (Input.GetButtonUp("Horizontal"))
        {
            rigid.velocity = new Vector2(rigid.velocity.normalized.x * 0.01f, rigid.velocity.y); //float���� ���� ���� �������� f �ٿ��ֱ�!!
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
            //�ܻ� ����Ʈ�� ����
            //��� �����ؾߴ�!!
        }

        //Z_Press
        if (Input.GetKeyDown(KeyCode.Z) && isSkill == false && isReload == false)
        {
            if (anim.GetBool("isJumping") == true)
            {
                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    //�Ʒ���

                    //�ֵθ��鼭 90�� ������

                    //�������
                }
                else if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    //���� ����
                }
                else if (Input.GetAxisRaw("Horizontal") != 0)
                {
                    //�Ϲ� ����
                    isSkill = true;
                    swordMove.StartCoroutine(swordMove.NormalSlash(spriteRenderer.flipX));
                }
                //�ƹ��͵� �� ������ z�� ���� ���¶��
                else if (!HandCannon.activeSelf && gameManager.bullet > 0)
                {
                    //����� ������ �߻�
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
                    //���� ����
                }
                else
                {
                    //�������� ��¦ �����ϸ� ���
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
        //��Ÿ��
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

        //������ �ӵ�
        if (isSkill == false)
        {
            //Move Speed
            float h = Input.GetAxisRaw("Horizontal");       //������ ���Ѵ�? normalized�� ����ϴ�
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
            Debug.DrawRay(rigid.position, Vector3.down, new UnityEngine.Color(0, 1, 0));         //�����ͻ󿡼� Ray�� �׷��ִ� �Լ�, color�� rgb �̿�
            RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, Vector3.down, 1, LayerMask.GetMask("Platform")); //���� ���� �ֿ� ���� ����, �� ������ �ݶ��̴��� �˻� Ȯ�� ����
            if (rayHit.collider != null)
            {
                //Debug.Log("??");
                if (rayHit.distance < 0.6f)
                {
                    //Debug.Log(rayHit.collider.name);      //�̰ŷ� �׽�Ʈ ����!
                    anim.SetBool("isJumping", false);
                }
            }
        }
    }

    //�ǰ� �̺�Ʈ
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
            bool isBronze = collision.gameObject.name.Contains("bronze");   //bronze��� �̸��� �����Ѵٸ� true
            bool isSilver = collision.gameObject.name.Contains("silver");
            bool isGold = collision.gameObject.name.Contains("gold");
            if (isBronze)
                gameManager.stagePoint += 50;
            else if (isSilver)
                gameManager.stagePoint += 100;
            else if (isGold)
                gameManager.stagePoint += 200;
            //����
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
         S2End = new Vector3(transform.position.x + direction * 6.14f, transform.position.y);    //�⺻ �̵��Ÿ��� 5.75�� ������ ��, ��������Ʈ�� ũ�⸦ ����Ͽ� 0.39f�� �߰��ؾ� ��
         //direction �������� �Ÿ� 5�� ���̸� �߻��ϰ� �浹�ϴ� �÷����� �����Ѵٸ� �� ������ �Ÿ���ŭ �̵��ؾ���. 
         // ���Ʒ����� �������� ���̸� �߻���.
          //�´� �� ���ٸ� �״��.
         //�¾Ҵµ� ���� �Ʒ��� ���� ���� �����ϴٸ�(���̶��) �� ���������� �̵���
          //�¾Ҵµ� ���� �Ʒ��� ���� 1��ŭ ���̳��ٸ�(���������̶��) ��� �ؾ�? ���ߴ� �� �� ���� �����ϱ� �̰� �ȸ´��� �״��?
          //�̰� ��� ������ ������ ����°ŷ�
         //float targetPosition = S2Start.x + 5 * direction;

         S2STCount = 0;  // �̵� �ð� �ʱ�ȭ

         while (Mathf.Abs(transform.position.x - S2Start.x) < 5f)   // 5��ŭ �̵�
         {
             rigid.velocity = new Vector2(maxSpeed * direction, 0);  // ����ؼ� �ӵ��� �����ϸ� �̵�
             S2STCount += Time.deltaTime;

             yield return null;
         }
         Debug.Log("�ɸ��ð�:" + S2STCount);
         Debug.Log("�̵��Ÿ�:" + (transform.position.x - S2Start.x));

         capsuleCollider.size = new Vector2(0.77f, 1);
         maxSpeed = originalMaxSpeed;
         rigid.velocity = Vector2.zero;
         HandCannon.SetActive(true);     //����
         handCannonMove.S2Shoot(S2End, spriteRenderer.flipX);
         //isS2�� HandCannon���� ������
     }*/

    private IEnumerator Skill_2()
    {
        float distanceToMove = 5.75f;  // �̵��� �� �Ÿ�
        float moveDuration = 0.1f;     // �̵��ϴµ� �ɸ� �ð� (�ϴ� 0.1��)
        float elapsedTime = 0f;

        S2Start = transform.position;
        S2End = S2Start + new Vector3(spriteRenderer.flipX ? -distanceToMove : distanceToMove, 0, 0);  // ���⿡ ���� �̵� ��ǥ

        capsuleCollider.direction = CapsuleDirection2D.Horizontal;
        capsuleCollider.size = new Vector2(1, 0.5f);  // ĸ�� �浹ü ������ ����

        // �ð� ����� ���� �Ÿ��� ���� ������ �̵�
        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / moveDuration;  // ���� �ð��� ���� ���� (0~1)
            transform.position = Vector3.Lerp(S2Start, S2End, t);  // ���� �������� ��ǥ �������� ���������� �̵�
            yield return null;
        }
        Debug.Log("�̵��Ÿ� : "+ Mathf.Abs(S2Start.x - transform.position.x));

        // �̵��� ���� �� ĸ�� �浹ü ���� ũ��� ����
        capsuleCollider.size = new Vector2(0.77f, 1);
        rigid.velocity = Vector2.zero;

        HandCannon.SetActive(true);  // HandCannon Ȱ��ȭ
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

        //���
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
        
        //ȸ�� ȸ����
        HandCannon.SetActive(true);     //�� �ڵ�ĳ�� Ȱ��ȭ����!
        handCannonMove.S3Shoot(spriteRenderer.flipX);
        audioSource.PlayOneShot(audioS3Shooting);

        //�ð� �ʱ�ȭ
        S3time = Time.time;
        S3AttackArea.SetActive(true);

        //S3 ���̽�
        //audioSource.PlayOneShot(audioS3Voice[Random.Range(0, 4)]);

        while (Time.time - S3time < 1.7f)
        {
            //0.1333�ʸ��� ��� ����� �ٶ󺸴� �ڵ带 �ְų�, �߰��� ���� ������ �ٶ󺸰� �ϴ� �ڵ带 �ִ°͵� ���� �� ����.
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

        //���̾� ����
        gameObject.layer = 9;

        //�� ����
        spriteRenderer.color = new UnityEngine.Color(1, 1, 1, 0.4f); //rgb������ ����!

        //ƨ�ܳ�����
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
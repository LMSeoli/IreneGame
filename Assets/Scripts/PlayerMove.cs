using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Burst.CompilerServices;
using UnityEngine;
using Color = UnityEngine.Color;


public class PlayerMove : MonoBehaviour
{
    //�⺻���� ���
    public GameManager gameManager;
    public CutsceneManager cutsceneManager;
    public RelicManager relic;
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

    //��꿡 ���� ���Ǵ� ���
    public float maxSpeed;  //��״� ����Ƽ�ʿ��� ���� ����!
    public float jumpPower;
    public float ghostDelayTime;
    public float ghostDelay;
    public bool isCS;
    public float CSDelay;
    public int CSCount;                 //0~5�� ������, 0�̸� ����, 1��2�� �Ϲ� �ֵθ���, 3�� ���, 4�� ����, 5�� �Ѿ��� �ִٸ� ���߿� ��� �� ���
    public float[] CSCool = new float[6];
    public float NSCoolTime;
    public float NSCoolTime_max;
    public float S1CoolTime;
    public float S1CoolTime_max;
    public float S2CoolTime;
    public float S2CoolTime_max;
    public float S3Count;
    public float S3Count_max;
    public bool isReload = false;
    public int rayDistance;
    //public bool isS1a = false;
    //public bool isS1b = false;
    public bool isS2 = false;
    //public bool isS3 = false;
    public bool isSkill = false;            //��� ��ų, ��� ����߿� true
    public bool isDash = false;             //��� ��� �߿� true
    public float S2Speed;
    public float S2STCount;
    public float S3Time;

    //������ ��� (��꿡�� ���Ǵ� �͵� ���Ƽ� ������ ���� �ǹ̾��� �ѵ�)
    public GameObject ghost;
    public GameObject bullet;
    public GameObject swordRotater;
    public HashSet<GameObject> CSShootTarget = new HashSet<GameObject>();
    public Vector3 s1Direction;
    public Vector3 S2Start;
    public Vector3 S2End;
    public GameObject HandCannon;
    public GameObject Sword;
    public GameObject S3Airbone;
    public GameObject S3AttackArea;
    public GameObject targetMark;
    SpriteRenderer markRenderer;
    public Transform CS_StingPos;
    public Vector2 CS_StingBoxSize;
    public Transform CS_AirbonePos;
    public Vector2 CS_AirboneBoxSize;
    public Coroutine NS;
    public Transform NSPos;
    public Vector2 NSBoxSize;
    public Coroutine HS;
    public float HSRadius;
    public float HSAngle;
    public Transform S1Pos;
    public Vector2 S1BoxSize;


    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    CapsuleCollider2D capsuleCollider;
    Animator anim;
    Animator swordRotationAnim;
    AudioSource audioSource;
    HandCannonMove handCannonMove;
    SwordRotationHandler swordRotation;
    SwordMove swordMove;
    LineRenderer lineRenderer;
    CameraMove cameraMove;

    // Start is called before the first frame update
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        anim = GetComponent<Animator>();  // Animator ������Ʈ �ʱ�ȭ
        audioSource = GetComponent<AudioSource>();
        handCannonMove = HandCannon.GetComponent<HandCannonMove>();
        swordRotation = FindObjectOfType<SwordRotationHandler>();
        swordMove = Sword.GetComponent<SwordMove>();
        swordRotationAnim = swordRotater.GetComponent<Animator>();
        lineRenderer = GetComponent<LineRenderer>();
        markRenderer = targetMark.GetComponent<SpriteRenderer>();
        cameraMove = FindObjectOfType<CameraMove>();
    }

    //�ܹ����� �Է��� ���⿡!!
    void Update()
    {
        //Jump
        if (Input.GetButtonDown("Jump") && !anim.GetBool("isJumping"))
        {
            Debug.Log("������");
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
        if (Input.GetKeyDown(KeyCode.R) && NSCoolTime <= 0 && isSkill == false && isReload == false && gameManager.bullet < 6)
        {
            Debug.Log("Reload?");
            isReload = true;
            gameManager.StartCoroutine(gameManager.Reload());
        }

        //Dash
        if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)) && isSkill == false && isReload == false)
        {
            Debug.Log("��� �ߵ�");
            isDash = true;
            StartCoroutine(Dash());
            Debug.Log("��� �����۵�");
        }

        //Z_Press
        if (Input.GetKeyDown(KeyCode.Z) && isSkill == false && isReload == false)
        {
            //���߿� ���� ��
            if (anim.GetBool("isJumping") == true)
            {
                //�Ʒ��� ���� ���¶��
                if (Input.GetKey(KeyCode.DownArrow) && NSCoolTime <= 0)
                {
                    HS = StartCoroutine(DetectHSOverTime(Vector2.down, 0.2f));
                    anim.SetBool("NormalSlash", true);
                    swordRotation.HorizontalSlashStart(spriteRenderer.flipX, false);
                    NSCoolTime = NSCoolTime_max;


                    //�ǿ뼺�� ���� �� ����, ���� �����ȱ�� ����� �� ���� ��.
                }
                //���� ���� ���¶��
                else if (Input.GetKey(KeyCode.UpArrow) && NSCoolTime <= 0)
                {
                    //���� ����
                    HS = StartCoroutine(DetectHSOverTime(Vector2.up, 0.2f));
                    anim.SetBool("NormalSlash", true);
                    swordRotation.HorizontalSlashStart(spriteRenderer.flipX, true);
                    NSCoolTime = NSCoolTime_max;
                }
                //�翷�� �����ų� �ƿ� ������ ���� ���¶��
                else if (/*Input.GetAxisRaw("Horizontal")!=0 &&*/ NSCoolTime <= 0)
                {
                    //�Ϲ� ����
                    NS = StartCoroutine(DetectNSOverTime(0.2f));
                    anim.SetBool("NormalSlash", true);
                    swordRotation.NormalSlashStart(spriteRenderer.flipX);
                    NSCoolTime = NSCoolTime_max;
                }
            }
            //�� ���� ���� ��
            else
            {
                Debug.Log("Į �ֵθ�!");
                if (Input.GetKey(KeyCode.UpArrow) && NSCoolTime <= 0)
                {
                    Debug.Log("�����ߴ�");
                    //���� ����
                    HS = StartCoroutine(DetectHSOverTime(Vector2.up, 0.2f));
                    anim.SetBool("NormalSlash", true);
                    swordRotation.HorizontalSlashStart(spriteRenderer.flipX, true);
                    NSCoolTime = NSCoolTime_max;
                }
                else
                {
                    if (CSCount == 0 || CSCool[CSCount] < Time.time-CSDelay) {
                        isSkill = true;
                        StartCoroutine(ComboSlash());
                    }
                }
                /*else if (!HandCannon.activeSelf && gameManager.bullet > 0)
                {
                    HandCannon.SetActive(true);
                    gameManager.BulletDown();
                    handCannonMove.Shot(spriteRenderer.flipX);
                }*/
            }
        }

        //X_Press
        if (Input.GetKeyDown(KeyCode.X) && !anim.GetBool("isJumping") && isSkill == false && isReload == false && gameManager.bullet > 0 && S1CoolTime <= 0)
        {
            if (Input.GetKey(KeyCode.UpArrow))
            {
                StartCoroutine(Skill_2b());
            }
            else if (!anim.GetBool("isJumping"))
            {
                isSkill = true;
                lineRenderer.enabled = true;
                StartCoroutine(Skill_1());
            }
        }

        //C_Press (Skill_2)
        if (Input.GetKeyDown(KeyCode.C) && !anim.GetBool("isJumping") && isSkill == false && isReload == false && S2CoolTime <= 0 && gameManager.bullet > 0)
        {
            StopSlash();
            Debug.Log("���� ������");
            if (Input.GetAxis("Horizontal") != 0) {
                //��ų ����
                isSkill = true;
                Debug.Log("S2 �����");
                StartCoroutine(Skill_2());
            }
        }

        //V_Press (Skill_3)
        if (Input.GetKeyDown(KeyCode.V) && !anim.GetBool("isJumping") && isSkill == false && isReload == false && S3Count >= S3Count_max && gameManager.bullet > 5)
        {
            isSkill = true;
            StartCoroutine(Skill_3());
        }

        if(CSCount != 0)
        {
            if (Time.time - CSDelay > 0.5f)
            {
                swordRotation.SwordOff();
                swordRotationAnim.SetInteger("CSCount", 0);
                anim.SetInteger("CSCount", 0);
                CSCount = 0;
            }
        }
        //��������Ʈ ����
        if (Input.GetButton("Horizontal")) spriteRenderer.flipX = Input.GetAxisRaw("Horizontal") == -1;

        //Animation
        if (Mathf.Abs(rigid.velocity.x) < 0.1)
            anim.SetBool("isWalking", false);
        else
            anim.SetBool("isWalking", true);
    }


    void FixedUpdate()
    {
        //��Ÿ��
        if (NSCoolTime > 0)
        {
            NSCoolTime -= Time.deltaTime;
        }

        if (S1CoolTime > 0)
        {
            S1CoolTime -= Time.deltaTime;
        }

        if (S2CoolTime > 0)
        {
            S2CoolTime -= Time.deltaTime;
        }

        //������ �ӵ�
        if (isSkill == false && CSCool[CSCount] <= CSDelay)
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

        //���� �� ���� ����
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
            if (isS2 == true)
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
                OnDamaged(collision.transform.position);
            }
        }
        else if (collision.gameObject.tag == "trap") OnDamaged(collision.transform.position);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        //�����۰� �����ٸ�
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
        else if (collision.gameObject.tag == "Relic")
        {
            PlaySound("ITEM");
            relic.AddRandomRelic();
            collision.gameObject.SetActive(false);
        }

        //���������� �����ٸ�
        else if (collision.gameObject.tag == "Finish")
        {
            //Next Stage
            gameManager.NextStage();
        }
    }

    private IEnumerator Dash()
    {
        isSkill = true;
        StopSlash();

        //�׸��� �÷��̾��� ���̾ ���� �浹���� �ʴ� ���̾�� �ٲ�� ��
        gameObject.layer = 9;
        rigid.gravityScale = 0;

        //���� ��� ����
        float distanceToMove = 3f;      // �̵��� �� �Ÿ�
        float moveDuration = 0.2f;      // �̵��ϴµ� �ɸ� �ð� (�ϴ� 0.2��)
        float elapsedTime = 0f;
        S2Start = transform.position;
        float dir = spriteRenderer.flipX ? -1 : 1;
        S2End = S2Start + new Vector3(dir * distanceToMove, 0, 0);  // ���⿡ ���� �̵� ��ǥ

        Debug.Log("1������");
        // �ð� ����� ���� �Ÿ��� ���� ������ �̵�
        while (elapsedTime <= moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / moveDuration;  // ���� �ð��� ���� ���� (0~1)
            transform.position = Vector3.Lerp(S2Start, S2End, t);  // ���� �������� ��ǥ �������� ���������� �̵�
            if (ghostDelayTime > 0)
            {
                ghostDelayTime -= Time.deltaTime;
            }
            else
            {
                GameObject currentGhost = Instantiate(ghost, this.transform.position, this.transform.rotation);
                Sprite currentSprite = this.GetComponent<SpriteRenderer>().sprite;
                currentGhost.transform.localScale = this.transform.localScale;
                currentGhost.GetComponent<SpriteRenderer>().sprite = currentSprite;
                currentGhost.GetComponent<SpriteRenderer>().flipX = spriteRenderer.flipX;
                this.ghostDelayTime = this.ghostDelay;
                Destroy(currentGhost, 0.5f);
            }
            yield return null;
        }
        Debug.Log("�̵��Ÿ� : " + Mathf.Abs(S2Start.x - transform.position.x));

        //��� ���� �� ���� ���� ����ȭ
        isSkill = false;
        isDash = false;
        gameObject.layer = 8;
        rigid.gravityScale = 2;
        //��� ���� �� ������� �ʰ� �����̰� �ϱ�
        rigid.velocity = new Vector2(maxSpeed * dir, 0);
        /*if (dir == Input.GetAxisRaw("Horizontal"))
        {
            rigid.velocity = new Vector2(maxSpeed*dir, 0);
        }*/
    }

    private IEnumerator ComboSlash()                        //�⺻ ��Ÿ��!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    {
        CSCount += 1;
        anim.SetInteger("CSCount", CSCount);
        //�޺� ����
        if (CSCount <= 2)
        {
            NS = StartCoroutine(DetectNSOverTime(0.2f));
            anim.SetBool("NormalSlash", true);
            swordRotation.ComboSlash(spriteRenderer.flipX, CSCount);
            CSDelay = Time.time;
            Vector3 startPosition = transform.position;
            Vector3 targetPosition = transform.position + (spriteRenderer.flipX?-transform.right:transform.right) * 0.5f;
            while (Time.time - CSDelay < 0.05f)
            {
                transform.position = Vector3.Lerp(startPosition, targetPosition, Time.time - CSDelay / 0.5f);
                yield return null;
            }
            transform.position = targetPosition;
            yield return new WaitForSeconds(0.2f-(Time.time-CSDelay)); 
            isSkill = false;
            //NSCoolTime = NSCoolTime_max;
        }
        else if (CSCount == 3)
        {
            //�� ��� �迭 �ʱ�ȭ
            //��� ����, � ���� ����
            anim.SetBool("NormalSlash", true);
            swordRotation.ComboSlash(spriteRenderer.flipX, CSCount);
            CSDelay = Time.time;
            //0.2�ʵ��� ��� �ڷ� �������ٰ�
            Vector2 initialPosition = transform.position;
            Vector2 targetPosition1 = initialPosition + new Vector2(-0.5f, 0);
            float elapsed = 0f;
            while (elapsed < 0.2f)
            {
                elapsed += Time.deltaTime;
                rigid.MovePosition(Vector2.Lerp(initialPosition, targetPosition1, elapsed / 0.2f));
                yield return new WaitForFixedUpdate();
            }
            transform.position = targetPosition1;
            //�������� ����&�̵�. ���� �Լ� ����� �� �ξ� ������ ��...
            while (elapsed < 0.3f)
            {
                elapsed += Time.deltaTime;
                if (elapsed < 0.22f)
                {
                    rigid.MovePosition(Vector2.Lerp(targetPosition1, targetPosition1 + new Vector2(1f, 0), (elapsed - 0.2f) / 0.02f));
                    Debug.Log("1");
                }
                else if (elapsed<0.27f) transform.position = targetPosition1+new Vector2(1f, 0);

                Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(new Vector3(gameObject.transform.position.x + ((CS_StingPos.position.x - gameObject.transform.position.x) * (spriteRenderer.flipX ? -1 : 1)), CS_StingPos.position.y, CS_StingPos.position.z), CS_StingBoxSize, 0);
                //���� �� ���� C#�� �� ���� �밡�ٽ����� �߱� ������ ����ȭ �ʿ�
                foreach (Collider2D collider in collider2Ds)
                {
                    if (collider.gameObject.tag.Contains("enemy"))
                    {
                        EnemyBasicMove enemy = collider.GetComponent<EnemyBasicMove>();
                        // ���� �̹� ���ݹ��� ���� ���ٸ� ��� ó��
                        if (enemy != null && !CSShootTarget.Contains(collider.gameObject))
                        {
                            CSShootTarget.Add(collider.gameObject); // ���� ���
                            enemy.CS3Hit();
                        }
                    }
                }
                yield return new WaitForFixedUpdate();
                Debug.Log("���� �۵���");
            }
            //0.02�ʵ��� ������ �뽬
            isSkill = false;
        }
        else if (CSCount == 4)
        {
            //� ���� ��¦ ���ø���, ���� ���� ������ ���� �����.
            swordRotation.ComboSlash(spriteRenderer.flipX, CSCount);
            CSDelay = Time.time;
            isSkill = false;
        }
        else if (CSCount == 5 && gameManager.bullet > 0)
        {
            //��� ������ �� ���

            CSCount = 0;
            isSkill = false;
        }
        yield return null;
    }

    private IEnumerator DetectNSOverTime(float duration)
    {
        float timer = 0f;
        HashSet<EnemyBasicMove> hitEnemies = new HashSet<EnemyBasicMove>(); // �ߺ� ������ ���� ����
        while (timer < duration)
        {
            Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(new Vector3(gameObject.transform.position.x + ((NSPos.position.x - gameObject.transform.position.x) * (spriteRenderer.flipX ? -1 : 1)), NSPos.position.y, NSPos.position.z), NSBoxSize, 0);
            //���� �� ���� C#�� �� ���� �밡�ٽ����� �߱� ������ ����ȭ �ʿ�
            foreach (Collider2D collider in collider2Ds)
            {
                if (collider.gameObject.tag.Contains("enemy"))
                {
                    EnemyBasicMove enemy = collider.GetComponent<EnemyBasicMove>();

                    // ���� �̹� ���ݹ��� ���� ���ٸ� ��� ó��
                    if (enemy != null && !hitEnemies.Contains(enemy))
                    {
                        hitEnemies.Add(enemy); // ���� ���
                        enemy.OnDamaged(transform.position);    //���� �ʹ� ���� �з����� 3Ÿ�� ���� �� ����. �̰� �״�� ����Ϸ��� �ƿ� ���� �����̸� �ø��ų� hpDown�� �̿��ϰų� �ؾ� ��.
                    }
                }
            }
            yield return null; // �� �����Ӹ��� ����
            timer += Time.deltaTime;
        }
    }

    private IEnumerator DetectHSOverTime(Vector2 AtkDir, float duration)
    {
        float timer = 0f;
        HashSet<EnemyBasicMove> hitEnemies = new HashSet<EnemyBasicMove>(); // �ߺ� ������ ���� ����

        while (timer < duration)
        {
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, HSRadius); // ������ �� ��� �� ����

            foreach (Collider2D collider in hitColliders)
            {
                if (collider.gameObject.tag.Contains("enemy"))
                {
                    // ���� �÷��̾� ������ ���� ���
                    Vector2 directionToEnemy = (collider.transform.position - transform.position).normalized;

                    // ���� ��� (������ ������ �̿��Ͽ� ���� ���� ���)
                    float angleToEnemy = Vector2.Angle(AtkDir, directionToEnemy);

                    // ������ ��ä�� ���� ���� �ִ��� Ȯ��
                    if (angleToEnemy < HSAngle / 2)
                    {
                        // ���� ��ä�� ���� ���� ������ �ǰ� ó��
                        EnemyBasicMove enemy = collider.GetComponent<EnemyBasicMove>();
                        if (enemy != null && !hitEnemies.Contains(enemy))
                        {
                            hitEnemies.Add(enemy); // ���� ���
                            enemy.OnDamaged(transform.position); // �ǰ� ó��
                        }
                    }
                }
            }
            yield return null; // �� �����Ӹ��� ����
            timer += Time.deltaTime;
        }
    }

    private void StopSlash()
    {
        if (NS != null)
        {
            StopCoroutine(NS);
            NS = null;
        }
        if (HS != null)
        {
            StopCoroutine(HS);
            HS = null;
        }
    }

    private IEnumerator Skill_1()
    {
        /*�� ���� �ڵ�
        HandCannon.SetActive(true);
        handCannonMove.StartCoroutine(handCannonMove.S1Shoot(spriteRenderer.flipX, gameManager.bullet));
        yield return new WaitForSeconds(gameManager.bullet * 0.06f);*/

        //���ʼ�����
        HandCannon.SetActive(true);         //2. 8���� ���� ���
        lineRenderer.positionCount = 2;
        rigid.velocity = Vector2.zero;
        while (isSkill == true)
        {
            //�ϴ� x�� ������ �ִٸ� ��ų1�� �����Ѵ�
            if (!Input.GetKey(KeyCode.X))
            {
                //S1Shoot �ߵ�
                handCannonMove.S1Shoot(spriteRenderer.flipX, s1Direction);
                gameManager.BulletDown();
                //����ȭ
                lineRenderer.enabled = false;
                markRenderer.enabled = false;
                isSkill = false;
                cameraMove.StartJungSangHwa();
            }
            //���������� �ʴٸ� ��� ������ �����Ѵ�
            else
            {
                float horizontal = Input.GetAxisRaw("Horizontal"); // -1 (����), 1 (������)
                float vertical = Input.GetAxisRaw("Vertical"); // -1 (�Ʒ�), 1 (��)
                                                               // �¿찡 ���ÿ� ������ ���� �Է��� 0���� ó��
                if (Input.GetKey(KeyCode.LeftArrow) && Input.GetKey(KeyCode.RightArrow))
                {
                    horizontal = 0;
                }
                // ���ϰ� ���ÿ� ������ ���� �Է��� 0���� ó��
                if (Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.DownArrow))
                {
                    vertical = 0;
                }
                Vector2 inputDirection = new Vector2(horizontal, vertical).normalized;
                // �ƹ� �Էµ� ������ Vector3.zero ��ȯ
                if (inputDirection != Vector2.zero)
                {
                    s1Direction = inputDirection;
                }
                // ����ĳ��Ʈ ����
                int layerMask = ~(1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("Default"));
                RaycastHit2D hit = Physics2D.Raycast(transform.position, s1Direction, rayDistance, layerMask);
                //�ڵ�ĳ�� ���� ����
                float angle = Mathf.Atan2(s1Direction.y, s1Direction.x) * Mathf.Rad2Deg; // ���� ������ ���� ���
                HandCannon.transform.rotation = Quaternion.Euler(0, 0, angle); // Z�� �������� ȸ��
                SpriteRenderer handCannonSprite = HandCannon.GetComponent<SpriteRenderer>();
                handCannonSprite.flipY = (angle > 90 || angle <= -90) ? true : false;

                // ���� ������(��Ÿ�&�����Ÿ� ǥ��) ����
                lineRenderer.SetPosition(0, transform.position);
                if (hit.collider != null)
                {
                    //Debug.Log(hit.point);
                    //Debug.Log(hit.collider.gameObject.name);
                    lineRenderer.SetPosition(1, hit.point);
                    // �浹�� ��ü�� "enemy" �±����� Ȯ��
                    if (hit.collider.CompareTag("enemy"))
                    {
                        targetMark.transform.position = hit.collider.transform.position;
                        markRenderer.enabled = true;
                        cameraMove.cameraTarget.position = Vector3.Lerp(cameraMove.cameraTarget.position, (hit.collider.transform.position + cameraMove.originalCameraTargetPosition.position)/2, cameraMove.cameraSmoothSpeed * Time.deltaTime);
                    }
                    else
                    {
                        cameraMove.cameraTarget.position = Vector3.Lerp(cameraMove.cameraTarget.position, cameraMove.originalCameraTargetPosition.position, cameraMove.cameraSmoothSpeed * Time.deltaTime);
                        markRenderer.enabled = false;
                    }
                }
                else
                {
                    // �浹�� ������ ������ �� ��ġ�� �ִ� �Ÿ��� ����
                    lineRenderer.SetPosition(1, transform.position + (Vector3)s1Direction * rayDistance);
                    markRenderer.enabled = false;
                    cameraMove.cameraTarget.position = Vector3.Lerp(cameraMove.cameraTarget.position, cameraMove.originalCameraTargetPosition.position, cameraMove.cameraSmoothSpeed * Time.deltaTime);
                }
            }
            yield return null;
        }
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
          //�¾Ҵµ� ���� �Ʒ��� ���� 1��ŭ ���̳��ٸ�(���������̶��) ��� �ؾ�? �̰� ���������� ����ؼ� �÷��̾�� 1ĭ ������ �ٽ� ���̸� ���, �� ������ �� 1ĭ ������ ���̸� ��� �ؼ� ��Ȯ�� ���� �ľ� �� �̵��ϵ��� �ϴ� �� ���� ��?
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
        isS2 = true;
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

        // �̵��� ���� �� ĸ�� �ݶ��̴� ���� ũ��� ����
        capsuleCollider.direction = CapsuleDirection2D.Vertical;
        capsuleCollider.size = new Vector2(0.77f, 1);
        rigid.velocity = Vector2.zero;

        HandCannon.SetActive(true);  // HandCannon Ȱ��ȭ
        handCannonMove.S2Shoot(S2End, spriteRenderer.flipX); //flipX ���� �������⿡ ���� �������� �ؾ�, �ݴ�������� ������ �� ������ �� ����
        isS2 = false;
    }
    void S2EnemyStrike(Transform enemy)
    {
        int direction = spriteRenderer.flipX ? -1 : 1;
        EnemyBasicMove enemyMove = enemy.GetComponent<EnemyBasicMove>();
        enemyMove.S2Hit(S2End, direction, handCannonMove.S2ShootDelay);
    }

    private IEnumerator Skill_2b()
    {
        // 1-1. �� ����� ���� ����Ʈ ����
        List<GameObject> enemyList = new List<GameObject>();
        // 1-2. ��ǥ�� �����ϱ� ���� �ε��� ���� (�ʱⰪ 0)
        int targetIndex = 0;

        // 2. Ÿ�ӽ������� 0.3f�� ����
        Time.timeScale = 0.3f;

        // 5-1. ��/�� ȭ��ǥ �νĿ� ���� ����
        bool isHorizontalDown = false;

        while (true) // 3. ���� ����
        {
            // 4. 12f ���� ���� �� ����
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 12f);
            for (int i = enemyList.Count - 1; i >= 0; i--)
            {
                // 4-1. ���� ������ ��� ��� ����Ʈ���� ����
                if (!hitColliders.Any(collider => collider.gameObject == enemyList[i]))
                {
                    if (i <= targetIndex && targetIndex > 0)
                    {
                        targetIndex--; // �ε��� ����
                    }
                    enemyList.RemoveAt(i);
                }
            }

            // 4-2. ���� �ȿ� ���� ���� �� �߰�
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.gameObject.tag.Contains("enemy") && !enemyList.Contains(hitCollider.gameObject) && hitCollider.GetComponent<EnemyBasicMove>() != null)
                {
                    EnemyBasicMove enemyMove = hitCollider.GetComponent<EnemyBasicMove>();
                    if (enemyMove.onAir == true)
                    {
                        enemyList.Add(hitCollider.gameObject);
                    }
                }
            }

            enemyList = enemyList.OrderBy(enemy => enemy.transform.position.x).ToList();
            if (targetIndex >= enemyList.Count)
            {
                targetIndex = enemyList.Count - 1; // ��ȿ�� �ε����� ����
            }

            if (enemyList.Count == 0)
            {
                Debug.Log("�ƹ��� �������� �ʽ��ϴ�");
                markRenderer.enabled = false;
                if (!Input.GetKey(KeyCode.X))
                {
                    Time.timeScale = 1f; // ���� Ÿ�ӽ����Ϸ� ����
                    markRenderer.enabled = false;
                    rigid.bodyType = RigidbodyType2D.Dynamic; // ������ٵ� ���� ����
                    isSkill = false; // ��ų ���� ����
                    yield break; // �ڷ�ƾ ����
                }
                yield return null;
                continue;
            }

            // 5. ��/�� ȭ��ǥ�� ������ �ε��� ����
            if (!isHorizontalDown)
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow) && targetIndex > 0)
                {
                    targetIndex--;
                    isHorizontalDown = true;
                }
                else if (Input.GetKeyDown(KeyCode.RightArrow) && targetIndex < enemyList.Count - 1)
                {
                    targetIndex++;
                    isHorizontalDown = true;
                }
            }
            if (!Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow))
            {
                isHorizontalDown = false;
            }

            if (enemyList.Count > 0)
            {
                Transform hit = enemyList[targetIndex].transform;
                targetMark.transform.position = hit.position;
                markRenderer.enabled = true;
                cameraMove.cameraTarget.position = Vector3.Lerp(cameraMove.cameraTarget.position, (hit.position + cameraMove.originalCameraTargetPosition.position) / 2, cameraMove.cameraSmoothSpeed * 5 * Time.deltaTime);
            }

            // 6. XŰ�� �����ٸ� ��� Ȯ��
            if (!Input.GetKey(KeyCode.X))
            {
                Debug.Log(enemyList);

                if (enemyList.Count > targetIndex && enemyList[targetIndex] != null)
                {
                    markRenderer.enabled = false;
                    break;
                }
            }
            yield return null;
        }

        // ���� ���� ��
        // 7. Ÿ�ӽ������� 1f�� ����
        Time.timeScale = 1f;

        /*// 8. ������ ���� Rigidbody2D ��ġ ����
        GameObject targetEnemy = enemyList[targetIndex];
        Rigidbody2D enemyRb = targetEnemy.GetComponent<Rigidbody2D>();
        Vector2 predictedPosition = (Vector2)targetEnemy.transform.position + enemyRb.velocity * 0.1f - new Vector2(0, 12f * 0.1f);

        // 9. �� ���� ��ġ�� 1f ���� �̵�
        capsuleCollider.isTrigger = true;
        transform.position = predictedPosition + Vector2.up;
        //transform.position = targetEnemy.transform.position + Vector3.up;*/
        capsuleCollider.isTrigger = true;
        GameObject targetEnemy = enemyList[targetIndex];
        if (transform.position.x > targetEnemy.transform.position.x) transform.position = targetEnemy.transform.position + new Vector3(-0.5f, 0.5f, 0);
        else transform.position = targetEnemy.transform.position + new Vector3(0.5f, 0.5f, 0);
        rigid.velocity = Vector3.zero;
        rigid.bodyType = RigidbodyType2D.Kinematic;
        cameraMove.StartJungSangHwa();

        //�����ص״� �������� 0.2f��ŭ ��ٸ�
        //���� 0.2f��ŭ ��ٸ��� �� �ƴ϶�, Lerp�� �̵��ϴ� ����� �����ְ�, �������ڸ��� ������ �� ������ ���� ��?
        yield return new WaitForSeconds(0.2f);

        // 10. ���� ���� �� ���� ���� �Ʒ��� ����ġ�� �ڵ�
        float duration = 0.1f;
        float timer = 0f;
        HashSet<EnemyBasicMove> hitEnemies = new HashSet<EnemyBasicMove>(); // �ߺ� ������ ���� ����
        Vector2 AtkDir = Vector2.down; // ���� ���� ����

        while (timer < duration)
        {
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, HSRadius);
            foreach (Collider2D collider in hitColliders)
            {
                if (collider.gameObject.tag.Contains("enemy"))
                {
                    Vector2 directionToEnemy = (collider.transform.position - transform.position).normalized;
                    float angleToEnemy = Vector2.Angle(AtkDir, directionToEnemy);

                    if (angleToEnemy < HSAngle / 2)
                    {
                        EnemyBasicMove enemy = collider.GetComponent<EnemyBasicMove>();
                        if (enemy != null && !hitEnemies.Contains(enemy))
                        {
                            hitEnemies.Add(enemy);
                            enemy.HpDown(); // HP ����
                            Rigidbody2D enemyRigidBody = enemy.GetComponent<Rigidbody2D>();
                            if (enemyRigidBody != null)
                            {
                                enemyRigidBody.velocity = Vector2.down * 20f;
                            }
                        }
                    }
                }
            }
            yield return null;
            timer += Time.deltaTime;
        }
        //źȯ�� �����ִٸ� �� ��󿡰� �� �߻�
        if (gameManager.bullet > 0)
        {
            HandCannon.SetActive(true);
            Vector3 directionToTarget = (targetEnemy.transform.position - transform.position).normalized;
            yield return new WaitForSeconds(0.2f);
            handCannonMove.NormalShot(spriteRenderer.flipX, directionToTarget);
            gameManager.BulletDown();
            rigid.bodyType = RigidbodyType2D.Dynamic;
            rigid.velocity = -directionToTarget * 5;
        }
        else rigid.bodyType = RigidbodyType2D.Dynamic;
        capsuleCollider.isTrigger = false;
        isSkill = false;
    }


    private IEnumerator Skill_3()
    {
        S3Time = Time.time;
        audioSource.PlayOneShot(audioS3Start);
        Time.timeScale = 0f;                      //�� ����� Ĺ�������
        cutsceneManager.StartCoroutine(cutsceneManager.PlayUlt());
        // �Ͻ����� ���� ���� 1�� ���
        float elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.unscaledDeltaTime; // ���� �ð� ���
            yield return null;
        }
        Debug.Log("Cutscene complete");
        //���
        Time.timeScale = 1;
        audioSource.PlayOneShot(audioS3Bound);
        S3Airbone.SetActive(true);
        S3Time = Time.time;
        while (Time.time - S3Time < 0.4f)
        {
            yield return null;
            if (Time.time - S3Time > 0.1f) S3Airbone.SetActive(false);
        }
        Debug.Log("Airborne phase complete");
        
        //ȸ�� ȸ����
        HandCannon.SetActive(true);     //�� �ڵ�ĳ�� Ȱ��ȭ����!
        handCannonMove.S3Shoot(spriteRenderer.flipX);
        audioSource.PlayOneShot(audioS3Shooting);

        //�ð� �ʱ�ȭ
        S3Time = Time.time;
        S3AttackArea.SetActive(true);
        S3AttackArea.transform.position = transform.position;

        //S3 ���̽�
        //audioSource.PlayOneShot(audioS3Voice[Random.Range(0, 4)]);

        while (Time.time - S3Time < 1.7f)
        {
            //0.1333�ʸ��� ��� ����� �ٶ󺸴� �ڵ带 �ְų�, �߰��� ���� ������ �ٶ󺸰� �ϴ� �ڵ带 �ִ°͵� ���� �� ����.
            transform.rotation = Quaternion.Euler(0, (Time.time - S3Time) * 360 * 12, 0);
            yield return null;
        }

        S3AttackArea.SetActive(false);
        transform.rotation = Quaternion.Euler(0, 0, 0);
        S3Time = Time.time;
        audioSource.PlayOneShot(audioS3End);

        //Ÿ�̹� ��� �ʿ�
        gameManager.S3CountDown();

        Invoke("S3False", 0.4f);
        Debug.Log("Skill_3 complete");
    }
    void S3False()
    {
        gameManager.ReloadAll();
        isSkill = false;
        S3Count = 0;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(NSPos.position, NSBoxSize);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(CS_StingPos.position, CS_StingBoxSize);
    }

    void OnDrawGizmosSelected()
    {
        // �÷��̾��� ���� ��ġ ��������
        Vector3 position = transform.position;

        // ��ä���� ���� ���� ���
        Vector3 startDirection = Quaternion.Euler(0, 0, -HSAngle / 2) * Vector3.up;

        // Gizmos ���� ����
        Gizmos.color = Color.red;

        // ��ä���� �߽ɿ��� �������� �������� ������ �׸���
        Gizmos.DrawLine(position, position + startDirection * HSRadius);

        // ��ä���� ��ȣ�� �׸��� ���� �� ���
        int segments = 20; // ��ȣ�� �� ����
        float angleStep = HSAngle / segments; // ���� ����

        Vector3 previousPoint = position + startDirection * HSRadius; // ù ��° ��

        for (int i = 1; i <= segments; i++)
        {
            // ���� ȸ�� ���
            float currentAngle = -HSAngle / 2 + angleStep * i;
            Vector3 nextDirection = Quaternion.Euler(0, 0, currentAngle) * Vector3.up;

            // �� �� ���
            Vector3 nextPoint = position + nextDirection * HSRadius;

            // ���� ���� �� ���� �����Ͽ� �� �׸���
            Gizmos.DrawLine(previousPoint, nextPoint);
            previousPoint = nextPoint;
        }

        // ��ä���� ������ ������ �ٽ� �߽����� �� �׸���
        Gizmos.DrawLine(previousPoint, position);
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
    
    public void Jumping()
    {
        anim.SetBool("isJumping", true);
    }
}
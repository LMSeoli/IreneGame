using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Burst.CompilerServices;
using UnityEngine;
using Color = UnityEngine.Color;


public class PlayerMove : MonoBehaviour
{
    //기본적인 요소
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

    //계산에 직접 사용되는 요소
    public float maxSpeed;  //얘네는 유니티쪽에서 설정 가능!
    public float jumpPower;
    public float ghostDelayTime;
    public float ghostDelay;
    public bool isCS;
    public float CSDelay;
    public int CSCount;                 //0~5의 값으로, 0이면 보통, 1과2은 일반 휘두르기, 3는 찌르기, 4은 띄우기, 5는 총알이 있다면 공중에 띄운 적 쏘기
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
    public bool isSkill = false;            //모든 스킬, 대시 사용중에 true
    public bool isDash = false;             //대시 사용 중에 true
    public float S2Speed;
    public float S2STCount;
    public float S3Time;

    //외적인 요소 (계산에도 사용되는 것도 많아서 구분이 거의 의미없긴 한디)
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
        anim = GetComponent<Animator>();  // Animator 컴포넌트 초기화
        audioSource = GetComponent<AudioSource>();
        handCannonMove = HandCannon.GetComponent<HandCannonMove>();
        swordRotation = FindObjectOfType<SwordRotationHandler>();
        swordMove = Sword.GetComponent<SwordMove>();
        swordRotationAnim = swordRotater.GetComponent<Animator>();
        lineRenderer = GetComponent<LineRenderer>();
        markRenderer = targetMark.GetComponent<SpriteRenderer>();
        cameraMove = FindObjectOfType<CameraMove>();
    }

    //단발적인 입력은 여기에!!
    void Update()
    {
        //Jump
        if (Input.GetButtonDown("Jump") && !anim.GetBool("isJumping"))
        {
            Debug.Log("점프함");
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
        if (Input.GetKeyDown(KeyCode.R) && NSCoolTime <= 0 && isSkill == false && isReload == false && gameManager.bullet < 6)
        {
            Debug.Log("Reload?");
            isReload = true;
            gameManager.StartCoroutine(gameManager.Reload());
        }

        //Dash
        if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)) && isSkill == false && isReload == false)
        {
            Debug.Log("대시 발동");
            isDash = true;
            StartCoroutine(Dash());
            Debug.Log("대시 정상작동");
        }

        //Z_Press
        if (Input.GetKeyDown(KeyCode.Z) && isSkill == false && isReload == false)
        {
            //공중에 있을 때
            if (anim.GetBool("isJumping") == true)
            {
                //아래를 누른 상태라면
                if (Input.GetKey(KeyCode.DownArrow) && NSCoolTime <= 0)
                {
                    HS = StartCoroutine(DetectHSOverTime(Vector2.down, 0.2f));
                    anim.SetBool("NormalSlash", true);
                    swordRotation.HorizontalSlashStart(spriteRenderer.flipX, false);
                    NSCoolTime = NSCoolTime_max;


                    //실용성이 없는 것 같아, 차라리 내리꽂기로 만드는 게 나을 듯.
                }
                //위를 누른 상태라면
                else if (Input.GetKey(KeyCode.UpArrow) && NSCoolTime <= 0)
                {
                    //위로 베기
                    HS = StartCoroutine(DetectHSOverTime(Vector2.up, 0.2f));
                    anim.SetBool("NormalSlash", true);
                    swordRotation.HorizontalSlashStart(spriteRenderer.flipX, true);
                    NSCoolTime = NSCoolTime_max;
                }
                //양옆을 누르거나 아예 누르지 않은 상태라면
                else if (/*Input.GetAxisRaw("Horizontal")!=0 &&*/ NSCoolTime <= 0)
                {
                    //일반 베기
                    NS = StartCoroutine(DetectNSOverTime(0.2f));
                    anim.SetBool("NormalSlash", true);
                    swordRotation.NormalSlashStart(spriteRenderer.flipX);
                    NSCoolTime = NSCoolTime_max;
                }
            }
            //땅 위에 있을 때
            else
            {
                Debug.Log("칼 휘두름!");
                if (Input.GetKey(KeyCode.UpArrow) && NSCoolTime <= 0)
                {
                    Debug.Log("이제야댐");
                    //위로 베기
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
            Debug.Log("판정 중지함");
            if (Input.GetAxis("Horizontal") != 0) {
                //스킬 실행
                isSkill = true;
                Debug.Log("S2 실행댐");
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
        //스프라이트 방향
        if (Input.GetButton("Horizontal")) spriteRenderer.flipX = Input.GetAxisRaw("Horizontal") == -1;

        //Animation
        if (Mathf.Abs(rigid.velocity.x) < 0.1)
            anim.SetBool("isWalking", false);
        else
            anim.SetBool("isWalking", true);
    }


    void FixedUpdate()
    {
        //쿨타임
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

        //움직임 속도
        if (isSkill == false && CSCool[CSCount] <= CSDelay)
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

        //점프 후 착지 여부
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
        //아이템과 만난다면
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
        else if (collision.gameObject.tag == "Relic")
        {
            PlaySound("ITEM");
            relic.AddRandomRelic();
            collision.gameObject.SetActive(false);
        }

        //종료지점과 만난다면
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

        //그리고 플레이어의 레이어를 적과 충돌하지 않는 레이어로 바꿔야 댐
        gameObject.layer = 9;
        rigid.gravityScale = 0;

        //이제 대시 구현
        float distanceToMove = 3f;      // 이동할 총 거리
        float moveDuration = 0.2f;      // 이동하는데 걸릴 시간 (일단 0.2초)
        float elapsedTime = 0f;
        S2Start = transform.position;
        float dir = spriteRenderer.flipX ? -1 : 1;
        S2End = S2Start + new Vector3(dir * distanceToMove, 0, 0);  // 방향에 따른 이동 목표

        Debug.Log("1차성공");
        // 시간 경과에 따라 거리를 일정 비율로 이동
        while (elapsedTime <= moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / moveDuration;  // 현재 시간에 따른 비율 (0~1)
            transform.position = Vector3.Lerp(S2Start, S2End, t);  // 시작 지점에서 목표 지점까지 점진적으로 이동
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
        Debug.Log("이동거리 : " + Mathf.Abs(S2Start.x - transform.position.x));

        //대시 종료 후 여러 설정 정상화
        isSkill = false;
        isDash = false;
        gameObject.layer = 8;
        rigid.gravityScale = 2;
        //대시 종료 후 어색하지 않게 움직이게 하기
        rigid.velocity = new Vector2(maxSpeed * dir, 0);
        /*if (dir == Input.GetAxisRaw("Horizontal"))
        {
            rigid.velocity = new Vector2(maxSpeed*dir, 0);
        }*/
    }

    private IEnumerator ComboSlash()                        //기본 평타임!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    {
        CSCount += 1;
        anim.SetInteger("CSCount", CSCount);
        //콤보 베기
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
            //적 목록 배열 초기화
            //찌르는 판정, 찌른 적들 경직
            anim.SetBool("NormalSlash", true);
            swordRotation.ComboSlash(spriteRenderer.flipX, CSCount);
            CSDelay = Time.time;
            //0.2초동안 잠시 뒤로 물러섰다가
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
            //공격판정 생성&이동. 따로 함수 만드는 게 훨씬 나았을 듯...
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
                //위의 한 줄은 C#을 잘 몰라서 노가다식으로 했기 때문에 간결화 필요
                foreach (Collider2D collider in collider2Ds)
                {
                    if (collider.gameObject.tag.Contains("enemy"))
                    {
                        EnemyBasicMove enemy = collider.GetComponent<EnemyBasicMove>();
                        // 적이 이미 공격받은 적이 없다면 즉시 처리
                        if (enemy != null && !CSShootTarget.Contains(collider.gameObject))
                        {
                            CSShootTarget.Add(collider.gameObject); // 적을 기록
                            enemy.CS3Hit();
                        }
                    }
                }
                yield return new WaitForFixedUpdate();
                Debug.Log("루프 작동중");
            }
            //0.02초동안 앞으로 대쉬
            isSkill = false;
        }
        else if (CSCount == 4)
        {
            //찌른 적들 살짝 띄어올리기, 범위 내에 있으면 전부 띄워짐.
            swordRotation.ComboSlash(spriteRenderer.flipX, CSCount);
            CSDelay = Time.time;
            isSkill = false;
        }
        else if (CSCount == 5 && gameManager.bullet > 0)
        {
            //띄운 적에게 총 쏘기

            CSCount = 0;
            isSkill = false;
        }
        yield return null;
    }

    private IEnumerator DetectNSOverTime(float duration)
    {
        float timer = 0f;
        HashSet<EnemyBasicMove> hitEnemies = new HashSet<EnemyBasicMove>(); // 중복 방지를 위한 집합
        while (timer < duration)
        {
            Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(new Vector3(gameObject.transform.position.x + ((NSPos.position.x - gameObject.transform.position.x) * (spriteRenderer.flipX ? -1 : 1)), NSPos.position.y, NSPos.position.z), NSBoxSize, 0);
            //위의 한 줄은 C#을 잘 몰라서 노가다식으로 했기 때문에 간결화 필요
            foreach (Collider2D collider in collider2Ds)
            {
                if (collider.gameObject.tag.Contains("enemy"))
                {
                    EnemyBasicMove enemy = collider.GetComponent<EnemyBasicMove>();

                    // 적이 이미 공격받은 적이 없다면 즉시 처리
                    if (enemy != null && !hitEnemies.Contains(enemy))
                    {
                        hitEnemies.Add(enemy); // 적을 기록
                        enemy.OnDamaged(transform.position);    //적이 너무 위로 밀려나서 3타가 거의 안 맞음. 이걸 그대로 사용하려면 아예 공격 딜레이를 늘리거나 hpDown을 이용하거나 해야 됨.
                    }
                }
            }
            yield return null; // 매 프레임마다 감지
            timer += Time.deltaTime;
        }
    }

    private IEnumerator DetectHSOverTime(Vector2 AtkDir, float duration)
    {
        float timer = 0f;
        HashSet<EnemyBasicMove> hitEnemies = new HashSet<EnemyBasicMove>(); // 중복 방지를 위한 집합

        while (timer < duration)
        {
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, HSRadius); // 반지름 내 모든 적 감지

            foreach (Collider2D collider in hitColliders)
            {
                if (collider.gameObject.tag.Contains("enemy"))
                {
                    // 적과 플레이어 사이의 방향 계산
                    Vector2 directionToEnemy = (collider.transform.position - transform.position).normalized;

                    // 각도 계산 (벡터의 내적을 이용하여 방향 각도 계산)
                    float angleToEnemy = Vector2.Angle(AtkDir, directionToEnemy);

                    // 지정된 부채꼴 범위 내에 있는지 확인
                    if (angleToEnemy < HSAngle / 2)
                    {
                        // 적이 부채꼴 범위 내에 있으면 피격 처리
                        EnemyBasicMove enemy = collider.GetComponent<EnemyBasicMove>();
                        if (enemy != null && !hitEnemies.Contains(enemy))
                        {
                            hitEnemies.Add(enemy); // 적을 기록
                            enemy.OnDamaged(transform.position); // 피격 처리
                        }
                    }
                }
            }
            yield return null; // 매 프레임마다 감지
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
        /*총 난사 코드
        HandCannon.SetActive(true);
        handCannonMove.StartCoroutine(handCannonMove.S1Shoot(spriteRenderer.flipX, gameManager.bullet));
        yield return new WaitForSeconds(gameManager.bullet * 0.06f);*/

        //기초설정만
        HandCannon.SetActive(true);         //2. 8방향 조준 사격
        lineRenderer.positionCount = 2;
        rigid.velocity = Vector2.zero;
        while (isSkill == true)
        {
            //일단 x가 떼어져 있다면 스킬1을 종료한다
            if (!Input.GetKey(KeyCode.X))
            {
                //S1Shoot 발동
                handCannonMove.S1Shoot(spriteRenderer.flipX, s1Direction);
                gameManager.BulletDown();
                //정상화
                lineRenderer.enabled = false;
                markRenderer.enabled = false;
                isSkill = false;
                cameraMove.StartJungSangHwa();
            }
            //떼어져있지 않다면 계속 방향을 조정한다
            else
            {
                float horizontal = Input.GetAxisRaw("Horizontal"); // -1 (왼쪽), 1 (오른쪽)
                float vertical = Input.GetAxisRaw("Vertical"); // -1 (아래), 1 (위)
                                                               // 좌우가 동시에 눌리면 수평 입력을 0으로 처리
                if (Input.GetKey(KeyCode.LeftArrow) && Input.GetKey(KeyCode.RightArrow))
                {
                    horizontal = 0;
                }
                // 상하가 동시에 눌리면 수직 입력을 0으로 처리
                if (Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.DownArrow))
                {
                    vertical = 0;
                }
                Vector2 inputDirection = new Vector2(horizontal, vertical).normalized;
                // 아무 입력도 없으면 Vector3.zero 반환
                if (inputDirection != Vector2.zero)
                {
                    s1Direction = inputDirection;
                }
                // 레이캐스트 실행
                int layerMask = ~(1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("Default"));
                RaycastHit2D hit = Physics2D.Raycast(transform.position, s1Direction, rayDistance, layerMask);
                //핸드캐논 방향 조작
                float angle = Mathf.Atan2(s1Direction.y, s1Direction.x) * Mathf.Rad2Deg; // 방향 벡터의 각도 계산
                HandCannon.transform.rotation = Quaternion.Euler(0, 0, angle); // Z축 기준으로 회전
                SpriteRenderer handCannonSprite = HandCannon.GetComponent<SpriteRenderer>();
                handCannonSprite.flipY = (angle > 90 || angle <= -90) ? true : false;

                // 라인 렌더러(사거리&직선거리 표시) 설정
                lineRenderer.SetPosition(0, transform.position);
                if (hit.collider != null)
                {
                    //Debug.Log(hit.point);
                    //Debug.Log(hit.collider.gameObject.name);
                    lineRenderer.SetPosition(1, hit.point);
                    // 충돌한 객체가 "enemy" 태그인지 확인
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
                    // 충돌이 없으면 레이의 끝 위치를 최대 거리로 설정
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
         S2End = new Vector3(transform.position.x + direction * 6.14f, transform.position.y);    //기본 이동거리가 5.75라 가정할 때, 스프라이트의 크기를 고려하여 0.39f는 추가해야 함
         //direction 방향으로 거리 5의 레이를 발사하고 충돌하는 플랫폼이 존재한다면 그 사이의 거리만큼 이동해야함. 
         // 위아래에서 전방으로 레이를 발사함.
          //맞는 게 없다면 그대로.
         //맞았는데 위와 아래의 값이 거의 동일하다면(벽이라면) 그 직전까지만 이동함
          //맞았는데 위와 아래의 값이 1만큼 차이난다면(오르막길이라면) 어떻게 해야? 이건 오르막길을 고려해서 플레이어보다 1칸 위에서 다시 레이를 쏘고, 또 맞으면 또 1칸 위에서 레이를 쏘고 해서 정확한 지형 파악 후 이동하도록 하는 게 좋을 듯?
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
        isS2 = true;
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

        // 이동이 끝난 후 캡슐 콜라이더 원래 크기로 복구
        capsuleCollider.direction = CapsuleDirection2D.Vertical;
        capsuleCollider.size = new Vector2(0.77f, 1);
        rigid.velocity = Vector2.zero;

        HandCannon.SetActive(true);  // HandCannon 활성화
        handCannonMove.S2Shoot(S2End, spriteRenderer.flipX); //flipX 말고 시전방향에 따른 기준으로 해야, 반대방향으로 나가는 걸 방지할 수 있음
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
        // 1-1. 적 목록을 담을 리스트 생성
        List<GameObject> enemyList = new List<GameObject>();
        // 1-2. 목표를 지정하기 위한 인덱스 생성 (초기값 0)
        int targetIndex = 0;

        // 2. 타임스케일을 0.3f로 설정
        Time.timeScale = 0.3f;

        // 5-1. 좌/우 화살표 인식용 변수 생성
        bool isHorizontalDown = false;

        while (true) // 3. 루프 시작
        {
            // 4. 12f 범위 내의 적 감지
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 12f);
            for (int i = enemyList.Count - 1; i >= 0; i--)
            {
                // 4-1. 적이 범위를 벗어난 경우 리스트에서 제거
                if (!hitColliders.Any(collider => collider.gameObject == enemyList[i]))
                {
                    if (i <= targetIndex && targetIndex > 0)
                    {
                        targetIndex--; // 인덱스 조정
                    }
                    enemyList.RemoveAt(i);
                }
            }

            // 4-2. 범위 안에 새로 들어온 적 추가
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
                targetIndex = enemyList.Count - 1; // 유효한 인덱스로 조정
            }

            if (enemyList.Count == 0)
            {
                Debug.Log("아무도 감지되지 않습니다");
                markRenderer.enabled = false;
                if (!Input.GetKey(KeyCode.X))
                {
                    Time.timeScale = 1f; // 원래 타임스케일로 복귀
                    markRenderer.enabled = false;
                    rigid.bodyType = RigidbodyType2D.Dynamic; // 리지드바디 상태 복구
                    isSkill = false; // 스킬 상태 종료
                    yield break; // 코루틴 종료
                }
                yield return null;
                continue;
            }

            // 5. 좌/우 화살표를 눌러서 인덱스 조정
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

            // 6. X키를 떼었다면 대상 확정
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

        // 루프 종료 후
        // 7. 타임스케일을 1f로 복구
        Time.timeScale = 1f;

        /*// 8. 지정된 적의 Rigidbody2D 위치 예측
        GameObject targetEnemy = enemyList[targetIndex];
        Rigidbody2D enemyRb = targetEnemy.GetComponent<Rigidbody2D>();
        Vector2 predictedPosition = (Vector2)targetEnemy.transform.position + enemyRb.velocity * 0.1f - new Vector2(0, 12f * 0.1f);

        // 9. 적 예상 위치의 1f 위로 이동
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

        //지정해뒀던 딜레이인 0.2f만큼 기다림
        //이젠 0.2f만큼 기다리는 게 아니라, Lerp로 이동하는 모습을 보여주고, 도착하자마자 찍어내리는 게 오히려 나을 듯?
        yield return new WaitForSeconds(0.2f);

        // 10. 일정 범위 내 적을 수직 아래로 내려치는 코드
        float duration = 0.1f;
        float timer = 0f;
        HashSet<EnemyBasicMove> hitEnemies = new HashSet<EnemyBasicMove>(); // 중복 방지를 위한 집합
        Vector2 AtkDir = Vector2.down; // 공격 방향 설정

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
                            enemy.HpDown(); // HP 감소
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
        //탄환이 남아있다면 그 대상에게 총 발사
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
        Time.timeScale = 0f;                      //더 월드다 캣새퀴들아
        cutsceneManager.StartCoroutine(cutsceneManager.PlayUlt());
        // 일시정지 상태 동안 1초 대기
        float elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.unscaledDeltaTime; // 실제 시간 경과
            yield return null;
        }
        Debug.Log("Cutscene complete");
        //에어본
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
        
        //회전 회오리
        HandCannon.SetActive(true);     //꼭 핸드캐논 활성화부터!
        handCannonMove.S3Shoot(spriteRenderer.flipX);
        audioSource.PlayOneShot(audioS3Shooting);

        //시간 초기화
        S3Time = Time.time;
        S3AttackArea.SetActive(true);
        S3AttackArea.transform.position = transform.position;

        //S3 보이스
        //audioSource.PlayOneShot(audioS3Voice[Random.Range(0, 4)]);

        while (Time.time - S3Time < 1.7f)
        {
            //0.1333초마다 잠시 대상을 바라보는 코드를 넣거나, 중간에 여러 방향을 바라보게 하는 코드를 넣는것도 좋을 거 같음.
            transform.rotation = Quaternion.Euler(0, (Time.time - S3Time) * 360 * 12, 0);
            yield return null;
        }

        S3AttackArea.SetActive(false);
        transform.rotation = Quaternion.Euler(0, 0, 0);
        S3Time = Time.time;
        audioSource.PlayOneShot(audioS3End);

        //타이밍 고려 필요
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
        // 플레이어의 현재 위치 가져오기
        Vector3 position = transform.position;

        // 부채꼴의 시작 각도 계산
        Vector3 startDirection = Quaternion.Euler(0, 0, -HSAngle / 2) * Vector3.up;

        // Gizmos 색상 설정
        Gizmos.color = Color.red;

        // 부채꼴의 중심에서 반지름을 기준으로 시작점 그리기
        Gizmos.DrawLine(position, position + startDirection * HSRadius);

        // 부채꼴의 원호를 그리기 위한 점 계산
        int segments = 20; // 원호의 점 개수
        float angleStep = HSAngle / segments; // 각도 분할

        Vector3 previousPoint = position + startDirection * HSRadius; // 첫 번째 점

        for (int i = 1; i <= segments; i++)
        {
            // 각도 회전 계산
            float currentAngle = -HSAngle / 2 + angleStep * i;
            Vector3 nextDirection = Quaternion.Euler(0, 0, currentAngle) * Vector3.up;

            // 새 점 계산
            Vector3 nextPoint = position + nextDirection * HSRadius;

            // 이전 점과 새 점을 연결하여 선 그리기
            Gizmos.DrawLine(previousPoint, nextPoint);
            previousPoint = nextPoint;
        }

        // 부채꼴의 마지막 점에서 다시 중심으로 선 그리기
        Gizmos.DrawLine(previousPoint, position);
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
    
    public void Jumping()
    {
        anim.SetBool("isJumping", true);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBasicMove : MonoBehaviour
{
    //체력, 속도같은 보편적이고 기본적인 요소들은 여기에 설정해두어야 범용성, 통일성 덕에 여러가지 조정이 편해짐
    //일단 속도, 공격력같은 
    //외부에 작용받지 않는 것만 개별 적들의 코드에 있으면 좋을 거 같음
    Rigidbody2D rigid;
    public float health;
    public float basicMoveSpeed=2;
    public float moveSpeed=2;
    public float basicAtk=10;
    public float atk=10;
    public bool onAir;
    public bool isBoss;

    public GameObject S3Airbone;
    public GameObject S3AttackArea;
    private List<EnemyBasicMove> detectedEnemies = new List<EnemyBasicMove>();

    //유물효과 관련
    public bool isSlowed;
    public Coroutine slowCoroutine;


    RelicManager relic;
    Animator anim;
    SpriteRenderer spriteRenderer;
    GameManager gameManager;
    public FawlbeastMove fawlbeastMove;
    new CapsuleCollider2D collider;

    //처음1회실행
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        collider = GetComponent<CapsuleCollider2D>();
        gameManager = FindObjectOfType<GameManager>();
        relic = FindObjectOfType<RelicManager>();
    }

    private void FixedUpdate()
    {
        if(onAir == true && rigid.velocity.y <= 0)
        {
            Debug.DrawRay(rigid.position, Vector3.down, new UnityEngine.Color(0, 1, 0));         //에디터상에서 Ray를 그려주는 함수, color은 rgb 이용
            RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, Vector3.down, 1, LayerMask.GetMask("Platform")); //빔에 맞은 애에 대한 정보, 이 변수의 콜라이더로 검색 확인 가능
            if (rayHit.collider != null && rayHit.distance < 0.6f)
            {
                onAir = false;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "out")
        {
            Dead();
        }
        if (collision.gameObject == S3Airbone)
        {
            StartCoroutine(S3Hit(collision.gameObject.transform));
        }
    }

    public void OnDamaged(Vector3 player, float damage)
    {
        //화면 진동 효과와
        //특수 피격 이펙트가 필요함
        if(onAir == true && FindObjectOfType<RelicManager>().RelicItems.Exists(item => item.isOwned && item.number == 3))
        {
            fawlbeastMove.FawlbeastAttack(transform);
        }
        onAir = true;
        gameObject.layer = 14;
        rigid.velocity = Vector3.zero;
        //튕겨나가기
        int dirc = transform.position.x - player.x > 0 ? 1 : -1;
        rigid.AddForce(new Vector2(dirc, 1f) * 2, ForceMode2D.Impulse);
            //플레이어가 검으로 타격하며 전진할 경우, 적이 충분히 밀려나지 않아 피격당할 수 있음. 이를 해결하는 게 좋을까?
        //플레이어 S3게이지 채우기, 게임매니저로 넘기면 좋을 듯
        gameManager.S3CountUp();
        //체력 계산
        health -= 1;
        if (health <= 0)
        {
            rigid.velocity = Vector3.zero;
            Dead();
            return;
        }
        //gameObject.layer = 14;
        //색 변경
        spriteRenderer.color = new UnityEngine.Color(1, 1, 1, 0.4f); //rgb다음은 투명도!

        Invoke("Return", 0.3f);
        //Animation
        //anim.SetTrigger("doDamaged");
        //Invoke("OffDamaged", 0.2f);
    }

    public void HpDown(float damage)
    {
        onAir = true;
        gameObject.layer = 14;
        gameManager.S3CountUp();
        //체력 계산
        health -= damage;
        if (health <= 0)
        {
            rigid.velocity = Vector3.zero;
            Dead();
            return;
        }
        spriteRenderer.color = new UnityEngine.Color(1, 1, 1, 0.4f); //rgb다음은 투명도!
        Invoke("Return", 0.3f);
    }

    public void S3Nominated()
    {
        // 이미 감지된 적들을 제거하여 중복되지 않도록 처리
        detectedEnemies.Clear();
        // 주변 원형 범위 내에 있는 모든 적을 인식
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 3f);

        foreach (Collider2D collider in hitColliders)
        {
            if (collider.CompareTag("enemy"))
            {
                EnemyBasicMove enemyMove = collider.GetComponent<EnemyBasicMove>();
                if (enemyMove != null && !detectedEnemies.Contains(enemyMove))
                {
                    detectedEnemies.Add(enemyMove);
                }
            }
        }
        // 감지된 모든 적들에게 S3Damaged 발동
        foreach (EnemyBasicMove enemy in detectedEnemies)
        {
            enemy.S3Damaged();
        }
    }
    public void S3Damaged()
    {
        Debug.Log("5");
        health -= 1;                                                 //체력 감소량 조정 필요
        fawlbeastMove.FawlbeastAttack(transform);
        spriteRenderer.color = new Color(1, 0, 0, 0.4f);
        Invoke("Return", 0.1f);
    }

    public void CS3Hit()
    {
        onAir = true;
        rigid.velocity = Vector3.zero;
        rigid.bodyType = RigidbodyType2D.Kinematic;
        HpDown(5);
        Invoke("Return", 0.5f);
    }

    public void CS4Hit()
    {
        onAir = true;
        rigid.bodyType = RigidbodyType2D.Dynamic;
        rigid.velocity = new Vector3 (0, 15, 0);
        spriteRenderer.color = new Color(1, 0, 0, 0.4f);
        Invoke("Return", 0.1f);
    }    

    public void S1Hit(Vector3 S2End, int direction, float ShootDelay)
    {
        //이미 S2에서 띄우고 쏘는 걸 했으니까, 이걸 아이린 1스로 해버리면 너무 특색이 없어짐.
        //아예 총 난사해서 공중에 띄우는 거로?
    }

    public void S2Hit(Vector3 S2End, int direction, float ShootDelay)
    {
        //gameObject.layer = 12;        //현재는 playermove에서 처리
        onAir = true;
        rigid.velocity = Vector2.zero;
        rigid.bodyType = RigidbodyType2D.Dynamic;
        Vector3 TS = this.gameObject.transform.position;

        float gravity = 12;     //현재 중력을 12로 설정해둠
                                //끝지점과 처음 지점의y+5.75부분을 이은 직선을 구함.
                                //chatgpt의 힘으로 0.5초후에 내려오며 도착하는 식 작성
        Vector3 S2connect = new Vector3(S2End.x - 5.75f * direction, S2End.y + 5.75f);
        float horizonalDistance = Mathf.Abs(S2End.x - transform.position.x);

        // 현재 위치에서 빗변으로 투영된 위치 계산
        Vector3 targetPosition = Vector3.Lerp(S2connect, S2End, 1 - horizonalDistance / 5.75f);

        // 수직 거리 계산
        float verticalDistance = targetPosition.y - transform.position.y;

        // 초기 수직 속도 계산
        float verticalVelocity = verticalVelocity = (verticalDistance + 0.5f * gravity * ShootDelay * ShootDelay) / ShootDelay;

        rigid.velocity = new Vector2(0, verticalVelocity);

        // 직접 메서드 호출
        Invoke("Return", 2f);
    }

    public void S2Push(Vector3 S2End)
    {
        Vector3 TS = gameObject.transform.position;
        Vector2 dirc = TS - S2End;
        rigid.AddForce(dirc.normalized * 10 / (S2End - TS).magnitude, ForceMode2D.Impulse);
    }

    private IEnumerator S3Hit(Transform player)
    {
        Debug.Log("S3Hit started");
        CancelInvoke("Return");
        onAir = true;
        rigid.velocity = Vector2.zero;

        float jumpDuration = 0.4f;
        float targetHeight = player.position.y + 3f;
        float gravity = 12f;

        float verticalDistance = targetHeight - transform.position.y;
        float initialVelocity = (verticalDistance + 0.5f * gravity * jumpDuration * jumpDuration) / jumpDuration;

        rigid.velocity = new Vector2(0, initialVelocity);
        yield return new WaitForSeconds(jumpDuration);

        rigid.velocity = Vector2.zero;
        rigid.bodyType = RigidbodyType2D.Kinematic;

        yield return new WaitForSeconds(2.1f);
        
        //3스 이후 동작
        rigid.bodyType = RigidbodyType2D.Dynamic;
        rigid.AddForce(new Vector2((transform.position.x - player.position.x) >= 0 ? 1 : -1, 1) * 3, ForceMode2D.Impulse);
        if (health <= 0)
        {
            Dead();
            yield return null;
        }
        Invoke("Return", 0.3f);
        Debug.Log("S3Hit complete");
    }

    void Return()
    {
        //색과 레이어를 원래대로
        spriteRenderer.color = new Color(1, 1, 1);
        gameObject.layer = 6;
        //rigid.bodyType = RigidbodyType2D.Dynamic;
    }
    public void Dynamic()
    {
        rigid.bodyType = RigidbodyType2D.Dynamic;
    }

    public void Dead()
    {
        gameManager.stagePoint += 100;

        gameObject.layer = 15;

        spriteRenderer.color = new Color(1, 1, 1, 0.4f);

        spriteRenderer.flipY = true;

        collider.enabled = false;

        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);

        Invoke("DeActive", 4);
    }

    void DeActive()
    {
        gameObject.SetActive(false);
    }

    void FawlbeastShotCheck()
    {
        if (onAir == true)
        {
            fawlbeastMove.FawlbeastAttack(transform);
        }
        /*if(relic.RelicItems.Exists(item => item.isOwned && item.itemName == "Health Reduction") && onAir==true)
        {
            //fawlBeast.target(gameObject);
            //
        }*/
    }
}

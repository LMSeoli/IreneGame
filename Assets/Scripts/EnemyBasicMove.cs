using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBasicMove : MonoBehaviour
{
    Rigidbody2D rigid;
    public int health;
    public bool isHit;
    public bool isBoss;

    public GameObject S3Airbone;
    public GameObject S3AttackArea;
    private List<EnemyBasicMove> detectedEnemies = new List<EnemyBasicMove>();

    Animator anim;
    SpriteRenderer spriteRenderer;
    GameManager gameManager;
    new CapsuleCollider2D collider;


    //처음1회실행
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        collider = GetComponent<CapsuleCollider2D>();
        gameManager = FindObjectOfType<GameManager>();
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
        if (collision.gameObject.layer == 16)
        {
            OnDamaged(collision.transform.position);
        }
    }

    public void OnDamaged(Vector3 player)
    {
        //화면 진동 효과와
        //특수 피격 이펙트가 필요함

        CancelInvoke("NotHit");
        CancelInvoke("Return");
        isHit = true;
        rigid.velocity = Vector3.zero;
        //튕겨나가기
        int dirc = transform.position.x - player.x > 0 ? 1 : -1;
        rigid.AddForce(new Vector2(dirc, 1) * 2, ForceMode2D.Impulse);
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
        Invoke("NotHit", 1f);
        //Animation
        //anim.SetTrigger("doDamaged");
        //Invoke("OffDamaged", 0.2f);
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
        Debug.Log("3");

        // 감지된 모든 적들에게 S3Damaged 발동
        foreach (EnemyBasicMove enemy in detectedEnemies)
        {
            enemy.S3Damaged();
        }
        Debug.Log("4");
    }

    public void S3Damaged()
    {
        Debug.Log("5");
        health -= 1;
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
        CancelInvoke("NotHit");
        //gameObject.layer = 12;        //현재는 playermove에서 처리
        isHit = true;
        rigid.velocity = Vector2.zero;
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
        Invoke("NotHit", 1f);
        //}
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
        CancelInvoke("NotHit");
        isHit = true;
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
        Invoke("NotHit", 1f);
        Debug.Log("S3Hit complete");
    }


    void Return()
    {
        spriteRenderer.color = new Color(1, 1, 1);
        gameObject.layer = 6;
    }

    void NotHit()
    {
        isHit = false;
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
}

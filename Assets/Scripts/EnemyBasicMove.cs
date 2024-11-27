using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBasicMove : MonoBehaviour
{
    //ü��, �ӵ����� �������̰� �⺻���� ��ҵ��� ���⿡ �����صξ�� ���뼺, ���ϼ� ���� �������� ������ ������
    //�ϴ� �ӵ�, ���ݷ°��� 
    //�ܺο� �ۿ���� �ʴ� �͸� ���� ������ �ڵ忡 ������ ���� �� ����
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

    //����ȿ�� ����
    public bool isSlowed;
    public Coroutine slowCoroutine;


    RelicManager relic;
    Animator anim;
    SpriteRenderer spriteRenderer;
    GameManager gameManager;
    public FawlbeastMove fawlbeastMove;
    new CapsuleCollider2D collider;

    //ó��1ȸ����
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
            Debug.DrawRay(rigid.position, Vector3.down, new UnityEngine.Color(0, 1, 0));         //�����ͻ󿡼� Ray�� �׷��ִ� �Լ�, color�� rgb �̿�
            RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, Vector3.down, 1, LayerMask.GetMask("Platform")); //���� ���� �ֿ� ���� ����, �� ������ �ݶ��̴��� �˻� Ȯ�� ����
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
        //ȭ�� ���� ȿ����
        //Ư�� �ǰ� ����Ʈ�� �ʿ���
        if(onAir == true && FindObjectOfType<RelicManager>().RelicItems.Exists(item => item.isOwned && item.number == 3))
        {
            fawlbeastMove.FawlbeastAttack(transform);
        }
        onAir = true;
        gameObject.layer = 14;
        rigid.velocity = Vector3.zero;
        //ƨ�ܳ�����
        int dirc = transform.position.x - player.x > 0 ? 1 : -1;
        rigid.AddForce(new Vector2(dirc, 1f) * 2, ForceMode2D.Impulse);
            //�÷��̾ ������ Ÿ���ϸ� ������ ���, ���� ����� �з����� �ʾ� �ǰݴ��� �� ����. �̸� �ذ��ϴ� �� ������?
        //�÷��̾� S3������ ä���, ���ӸŴ����� �ѱ�� ���� ��
        gameManager.S3CountUp();
        //ü�� ���
        health -= 1;
        if (health <= 0)
        {
            rigid.velocity = Vector3.zero;
            Dead();
            return;
        }
        //gameObject.layer = 14;
        //�� ����
        spriteRenderer.color = new UnityEngine.Color(1, 1, 1, 0.4f); //rgb������ ����!

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
        //ü�� ���
        health -= damage;
        if (health <= 0)
        {
            rigid.velocity = Vector3.zero;
            Dead();
            return;
        }
        spriteRenderer.color = new UnityEngine.Color(1, 1, 1, 0.4f); //rgb������ ����!
        Invoke("Return", 0.3f);
    }

    public void S3Nominated()
    {
        // �̹� ������ ������ �����Ͽ� �ߺ����� �ʵ��� ó��
        detectedEnemies.Clear();
        // �ֺ� ���� ���� ���� �ִ� ��� ���� �ν�
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
        // ������ ��� ���鿡�� S3Damaged �ߵ�
        foreach (EnemyBasicMove enemy in detectedEnemies)
        {
            enemy.S3Damaged();
        }
    }
    public void S3Damaged()
    {
        Debug.Log("5");
        health -= 1;                                                 //ü�� ���ҷ� ���� �ʿ�
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
        //�̹� S2���� ���� ��� �� �����ϱ�, �̰� ���̸� 1���� �ع����� �ʹ� Ư���� ������.
        //�ƿ� �� �����ؼ� ���߿� ���� �ŷ�?
    }

    public void S2Hit(Vector3 S2End, int direction, float ShootDelay)
    {
        //gameObject.layer = 12;        //����� playermove���� ó��
        onAir = true;
        rigid.velocity = Vector2.zero;
        rigid.bodyType = RigidbodyType2D.Dynamic;
        Vector3 TS = this.gameObject.transform.position;

        float gravity = 12;     //���� �߷��� 12�� �����ص�
                                //�������� ó�� ������y+5.75�κ��� ���� ������ ����.
                                //chatgpt�� ������ 0.5���Ŀ� �������� �����ϴ� �� �ۼ�
        Vector3 S2connect = new Vector3(S2End.x - 5.75f * direction, S2End.y + 5.75f);
        float horizonalDistance = Mathf.Abs(S2End.x - transform.position.x);

        // ���� ��ġ���� �������� ������ ��ġ ���
        Vector3 targetPosition = Vector3.Lerp(S2connect, S2End, 1 - horizonalDistance / 5.75f);

        // ���� �Ÿ� ���
        float verticalDistance = targetPosition.y - transform.position.y;

        // �ʱ� ���� �ӵ� ���
        float verticalVelocity = verticalVelocity = (verticalDistance + 0.5f * gravity * ShootDelay * ShootDelay) / ShootDelay;

        rigid.velocity = new Vector2(0, verticalVelocity);

        // ���� �޼��� ȣ��
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
        
        //3�� ���� ����
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
        //���� ���̾ �������
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

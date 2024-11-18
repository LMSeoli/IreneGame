using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEditor.PackageManager;
using UnityEngine;

public class BulletMove : MonoBehaviour
{
    Rigidbody2D rigid;
    PlayerMove playerMove;
    
    public float ShootType;     //0은 적과 닿으면 사라짐, 2는 적과 닿아도 관통함
    public float NormalShotSpeed;
    Vector3 shot1StartPosition;
    Vector3 explosionPosition;
    public ParticleSystem explodeParticle;
    public float S2ShotSpeed;
    public GameObject player;
    private List<EnemyBasicMove> detectedEnemies = new List<EnemyBasicMove>();


    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        playerMove = player.GetComponent<PlayerMove>();
    }

    void Update()
    {
        //슛타입이 1일 경우, 최대거리 이상으로 도달하면 삭제함.
        if (ShootType == 1)
        {
            if (Vector2.Distance(shot1StartPosition, gameObject.transform.position) >= playerMove.rayDistance)     
            {
                Debug.Log(Vector2.Distance(shot1StartPosition, gameObject.transform.position));
                Destroy(gameObject);
            }
        }
    }

    private void FixedUpdate()
    {
        
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (ShootType == 1)
        {
            explosionPosition = transform.position;
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 2.5f);       //뒤의 숫자가 폭발 반경
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
                else if (collider.CompareTag("Player") && rigid.velocity.y<0)
                {
                    Debug.Log("player detected");
                    Vector3 dirc = (player.transform.position - gameObject.transform.position).normalized;
                    Rigidbody2D playerRigid = player.GetComponent<Rigidbody2D>();
                    playerRigid.AddForce(dirc * 20, ForceMode2D.Impulse);
                    playerMove.Jumping();
                }
            }
            // 감지된 모든 적들에게 OnDamaged 발동
            foreach (EnemyBasicMove enemy in detectedEnemies)
            {
                if (enemy == collision.GetComponent<EnemyBasicMove>()) enemy.OnDamaged(player.transform.position);
                //bullet를 istrigger로 해서 enemy와의 물리적 상호작용을 없애버렸는데, 이 때문에 탄환의 속도가 빨라 적에게 정면충돌해도 때때로 적의 뒤쪽에서 부딪힌 것으로 판정되어, 임시방편으로 이렇게 처음 맞은 적이라도 데미지를 받도록 해두었습니다.
                else enemy.OnDamaged(explosionPosition);
            }
            ParticleSystem effect = Instantiate(explodeParticle, gameObject.transform.position, Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, effect.main.duration);
            Destroy(gameObject);
        }
        else if (collision.gameObject.tag == "enemy")
        {
            if (ShootType == 0)
            {
                EnemyHit(collision.transform);
                Destroy(gameObject);
            }
            else if (ShootType == 2)
            {
                EnemyHit(collision.transform);
            }
        }
        else if (collision.gameObject.tag == "Platform")
        {
            Destroy(gameObject);
        }
    }

    void EnemyHit(Transform enemy)
    {
        EnemyBasicMove enemyMove = enemy.GetComponent<EnemyBasicMove>();
        enemyMove.OnDamaged(player.transform.position);
    }

    public void NormalShot()
    {
        ShootType = 0;
        rigid.bodyType = RigidbodyType2D.Dynamic;
        rigid.AddForce(transform.right * NormalShotSpeed, ForceMode2D.Impulse);
        rigid.bodyType = RigidbodyType2D.Kinematic;
        Debug.Log(gameObject.transform.rotation);
        Invoke("delete", 2);
    }

    public void ExplodeShot()
    {
        ShootType = 1;
        rigid.bodyType = RigidbodyType2D.Dynamic;
        shot1StartPosition = gameObject.transform.position;
        rigid.AddForce(transform.right * NormalShotSpeed, ForceMode2D.Impulse);
        rigid.bodyType = RigidbodyType2D.Kinematic;
    }

    public void S2Shot()            //(관통 샷)
    {
        ShootType = 2;
        rigid.bodyType = RigidbodyType2D.Dynamic;
        rigid.AddForce(transform.right * S2ShotSpeed, ForceMode2D.Impulse);
        rigid.bodyType = RigidbodyType2D.Kinematic;
        Invoke("delete", 2);
    }

    void delete()
    {
        Destroy(gameObject);
    }
}
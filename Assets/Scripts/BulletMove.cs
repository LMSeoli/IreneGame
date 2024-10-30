using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEditor.PackageManager;
using UnityEngine;

public class BulletMove : MonoBehaviour
{
    Rigidbody2D rigid;
    PlayerMove playerMove;
    
    public float ShootType;     //1은 적과 닿으면 사라짐, 2는 적과 닿아도 관통함
    public float NormalShotSpeed;
    Transform shot1StartPosition;
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
            if (Vector2.Distance(shot1StartPosition.position, gameObject.transform.position) >= 30)
            {
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
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 3f);       //뒤의 숫자가 폭발 반경
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
                else if (collider.CompareTag("Player"))
                {
                    Debug.Log("player detected");
                    Vector3 dirc = (player.transform.position - gameObject.transform.position).normalized;
                    Rigidbody2D playerRigid = player.GetComponent<Rigidbody2D>();
                    playerRigid.AddForce(dirc * 25, ForceMode2D.Impulse);
                    playerMove.Jumping();
                }
            }
            // 감지된 모든 적들에게 OnDamaged 발동
            foreach (EnemyBasicMove enemy in detectedEnemies)
            {
                enemy.OnDamaged(explosionPosition);
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
            else if (ShootType == 3)
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
        shot1StartPosition = gameObject.transform;
        rigid.AddForce(transform.right * NormalShotSpeed, ForceMode2D.Impulse);
        rigid.bodyType = RigidbodyType2D.Kinematic;
        Debug.Log(gameObject.transform.rotation);
        Invoke("delete", 2);
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
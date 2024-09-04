using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class BulletMove : MonoBehaviour
{
    Rigidbody2D rigid;
    PlayerMove playerMove;
    
    public float ShootType;     //1은 적과 닿으면 사라짐, 2는 적과 닿아도 관통함
    public float NormalShotSpeed;
    public float S2ShotSpeed;
    public GameObject player;
    

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        playerMove = GetComponent<PlayerMove>();
    }

    void Update()
    {
        
    }

    private void FixedUpdate()
    {

    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "enemy")
        {
            if (ShootType == 0)
            {
                EnemyHit(collision.transform);
                Destroy(gameObject);
            }
            else if (ShootType == 1)
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
            //파티클 효과
            Destroy(gameObject);
        }
    }

    void EnemyHit(Transform enemy)
    {
        E1Move enemyMove = enemy.GetComponent<E1Move>();
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

    public void S2Shot()
    {
        ShootType = 2;
        rigid.bodyType = RigidbodyType2D.Dynamic;
        rigid.AddForce(transform.right * S2ShotSpeed, ForceMode2D.Impulse);
        rigid.bodyType = RigidbodyType2D.Kinematic;
        //rigid.velocity = transform.right * S2ShotSpeed;
        Invoke("delete", 2);
    }

    void delete()
    {
        Destroy(gameObject);
    }
}
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BeastAttack : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "enemy")
        {
            GameObject enemy = collision.gameObject;
            Rigidbody2D enemyRigid = enemy.GetComponent<Rigidbody2D>();
            if(enemyRigid.bodyType != RigidbodyType2D.Kinematic) enemyRigid.velocity = new Vector2 (transform.position.x > enemy.transform.position.x ?-3:3, 3);
            EnemyBasicMove enemyMove = enemy.GetComponent<EnemyBasicMove>();
            enemyMove.HpDown();
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            Destroy(gameObject);
        }
    }


    public IEnumerator Attack(float curveHeight, float accelerationFactor, Transform enemy)
    {
        Vector3 startPosition = transform.position; // 요정의 시작 위치

        // 곡선의 중간 지점을 높여 포물선 효과 만들기
        Vector3 midPosition = (startPosition*2/3 + enemy.position*1/3) + Vector3.up * Random.Range(-curveHeight, curveHeight);

        float timeElapsed = 0f;
        float duration = 0.6f; // 목표 지점에 도달하는 데 걸리는 총 시간

        while (timeElapsed < duration)
        {
            // 시간에 비례해 이동 위치 계산
            float t = Mathf.Pow(timeElapsed/duration, accelerationFactor); // 시간이 지날수록 속도가 빨라짐
            Vector3 currentPosition = Vector3.Lerp(Vector3.Lerp(startPosition, midPosition, t), Vector3.Lerp(midPosition, enemy.position, t), t);

            transform.position = currentPosition;

            timeElapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = enemy.position;
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
}

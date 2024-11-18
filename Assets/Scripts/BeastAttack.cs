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
        Vector3 startPosition = transform.position; // ������ ���� ��ġ

        // ��� �߰� ������ ���� ������ ȿ�� �����
        Vector3 midPosition = (startPosition*2/3 + enemy.position*1/3) + Vector3.up * Random.Range(-curveHeight, curveHeight);

        float timeElapsed = 0f;
        float duration = 0.6f; // ��ǥ ������ �����ϴ� �� �ɸ��� �� �ð�

        while (timeElapsed < duration)
        {
            // �ð��� ����� �̵� ��ġ ���
            float t = Mathf.Pow(timeElapsed/duration, accelerationFactor); // �ð��� �������� �ӵ��� ������
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

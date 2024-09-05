using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class S3Attack : MonoBehaviour
{
    public ParticleSystem sparkEffect;

    private HashSet<GameObject> targets = new HashSet<GameObject>();  // HashSet으로 중복 방지
    public GameObject HandCannon;
    private HandCannonMove handCannonMove;
    Collider2D col;
    PlayerMove playerMove;

    private void Awake()
    {
        handCannonMove = HandCannon.GetComponent<HandCannonMove>();
        col = GetComponent<Collider2D>();
        playerMove = FindObjectOfType<PlayerMove>();
    }

    private void OnEnable()
    {
        targets.Clear();        //깔쌈하게 지워버리자궈~

        List<Collider2D> results = new List<Collider2D>();
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true; // 트리거만 감지

        col.OverlapCollider(filter, results);

        // 감지된 모든 오브젝트 처리
        foreach (var result in results)
        {
            if (result.gameObject.tag.Contains("enemy"))
            {
                if (!targets.Contains(result.gameObject)) // 중복 추가 방지
                {
                    targets.Add(result.gameObject);
                    Debug.Log($"적 추가됨: {result.gameObject.name}");
                }
            }
        }
        StartCoroutine(S3Shot());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("enemy"))
        {
            if (!targets.Contains(collision.gameObject))
            {
                targets.Add(collision.gameObject);  // HashSet은 자동으로 중복 제거
                Debug.Log("Enemy entered: " + collision.gameObject.name);
            }
        }
    }

    //적이 범위에서 벗어난다면 타겟을 해제해야 하지만, 그럴 경우가 적기도 하고 굳이 그럴 필요도 없기 때문에 배제. 사실 버그때매 그럼
    /*private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("enemy"))
        {
            if (targets.Contains(collision.gameObject))
            {
                targets.Remove(collision.gameObject);
                Debug.Log("Enemy exited: " + collision.gameObject.name);
            }
        }
    }*/

    private IEnumerator S3Shot()
    {
        Debug.Log("S3Shot started");
        yield return new WaitForSeconds(0.05f);

        int ShootNumber = 12;
        float ShootTime = 1.6f / ShootNumber;

        Debug.Log("ShootNumber: " + ShootNumber);
        Debug.Log("ShootTime: " + ShootTime);

        while (ShootNumber > 0)
        {
            Debug.Log("Entering while loop. ShootNumber: " + ShootNumber + ", TargetsCount: " + targets.Count);

            if (targets.Count <= 0)
            {
                Debug.Log("No targets available");
                ShootNumber -= 1;
                yield return new WaitForSeconds(ShootTime);
                continue;
            }

            GameObject[] targetArray = new GameObject[targets.Count];
            targets.CopyTo(targetArray);
            Debug.Log("Target array size: " + targetArray.Length);
            GameObject AttackTarget = targetArray[Random.Range(0, targetArray.Length)];

            if (AttackTarget == null)
            {
                Debug.LogWarning("AttackTarget is null. Skipping this target.");
                ShootNumber -= 1;
                yield return new WaitForSeconds(ShootTime);
                continue;
            }
            EnemyBasicMove AttackTargetMove = AttackTarget.GetComponent<EnemyBasicMove>();
            if (AttackTargetMove == null)
            {
                Debug.LogWarning("AttackTargetMove is null. Skipping this target.");
                ShootNumber -= 1;
                yield return new WaitForSeconds(ShootTime);
                continue;
            }

            // StartCoroutine(handCannonMove.S3ShootMove(AttackTarget));
            handCannonMove.PlaySound("S3Shot");

            AttackTargetMove.S3Nominated();
            Debug.Log("S3Nominated called for target: " + AttackTarget.name);
            ParticleSystem effect = Instantiate(sparkEffect, AttackTarget.transform.position, Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, effect.main.duration);

            ShootNumber -= 1;
            if (ShootNumber %2 == 0) playerMove.BD();
            yield return new WaitForSeconds(ShootTime);
        }

        Debug.Log("S3Shot ended");
    }
}
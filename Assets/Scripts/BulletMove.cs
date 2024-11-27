using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UIElements;

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

    public LineRenderer lineRenderer1; // 첫 번째 라인
    public LineRenderer lineRenderer2; // 두 번째 라인
    public float pointSpacing = 0.1f;  // 점 사이의 최소 거리
    public int maxPoints = 100;        // 최대 점 개수

    public float sineAmplitude = 0.5f; // Sine Curve의 진폭
    public float sineFrequency = 5f;   // Sine Curve의 주파수
    private float sinePhaseOffset1;    // 첫 번째 라인의 시작 위상
    private float sinePhaseOffset2;    // 두 번째 라인의 시작 위상

    private Vector3 lastPointPosition;
    private int currentPointIndex;

    ParticleSystem particle;

    void Start()
    {
        // 시작 위상을 랜덤으로 설정
        sinePhaseOffset1 = Random.Range(0f, Mathf.PI * 2f); // 0 ~ 2π
        sinePhaseOffset2 = Random.Range(0f, Mathf.PI * 2f); // 0 ~ 2π

        lineRenderer1.positionCount = 1;
        lineRenderer2.positionCount = 1;
        lastPointPosition = transform.position;
        lineRenderer1.SetPosition(0, lastPointPosition);
        lineRenderer2.SetPosition(0, lastPointPosition);

        // 초기 투명도 설정
        SetLineTransparency(lineRenderer1, Color.white);
        SetLineTransparency(lineRenderer2, Color.white);
    }

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        playerMove = player.GetComponent<PlayerMove>();
        particle = GetComponent<ParticleSystem>();
    }

    void Update()
    {
        //슛타입이 1일 경우, 최대거리 이상으로 도달하면 삭제함.
        if (ShootType == 1)
        {
            if (Vector2.Distance(shot1StartPosition, gameObject.transform.position) >= playerMove.rayDistance)     
            {
                //Debug.Log(Vector2.Distance(shot1StartPosition, gameObject.transform.position));
                StartCoroutine(delete(0));
            }
        }

        float distance = Vector3.Distance(lastPointPosition, transform.position);

        if (distance >= pointSpacing)
        {
            if (currentPointIndex < maxPoints)
            {
                // 점 추가
                lineRenderer1.positionCount = currentPointIndex + 1;
                lineRenderer2.positionCount = currentPointIndex + 1;

                Vector3 newPosition1 = ApplySineCurve(transform.position, currentPointIndex, sinePhaseOffset1);
                Vector3 newPosition2 = ApplySineCurve(transform.position, currentPointIndex, sinePhaseOffset2, true);

                lineRenderer1.SetPosition(currentPointIndex, newPosition1);
                lineRenderer2.SetPosition(currentPointIndex, newPosition2);

                currentPointIndex++;
            }
            else
            {
                // 오래된 점 삭제하고 새 점 추가 (Trail 효과)
                for (int i = 0; i < maxPoints - 1; i++)
                {
                    lineRenderer1.SetPosition(i, lineRenderer1.GetPosition(i + 1));
                    lineRenderer2.SetPosition(i, lineRenderer2.GetPosition(i + 1));
                }

                Vector3 newPosition1 = ApplySineCurve(transform.position, maxPoints - 1, sinePhaseOffset1);
                Vector3 newPosition2 = ApplySineCurve(transform.position, maxPoints - 1, sinePhaseOffset2, true);

                lineRenderer1.SetPosition(maxPoints - 1, newPosition1);
                lineRenderer2.SetPosition(maxPoints - 1, newPosition2);
            }

            lastPointPosition = transform.position;

            // 투명도 업데이트
            SetLineTransparency(lineRenderer1, Color.white);
            SetLineTransparency(lineRenderer2, Color.white);
        }
    }

    // Sine curve 계산 (기존 라인용)
    private Vector3 ApplySineCurve(Vector3 basePosition, int index, float phaseOffset, bool reverse = false)
    {
        float sineOffset = Mathf.Sin(index * pointSpacing * sineFrequency + phaseOffset) * sineAmplitude;
        if (reverse) sineOffset *= -1; // 반대 방향으로 적용
        return new Vector3(basePosition.x, basePosition.y + sineOffset, basePosition.z);
    }

    private void SetLineTransparency(LineRenderer lineRenderer, Color baseColor)
    {
        Gradient gradient = new Gradient();
        GradientColorKey[] colorKeys = new GradientColorKey[2];
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[8];

        // 색상 고정
        colorKeys[0] = new GradientColorKey(baseColor, 0f);
        colorKeys[1] = new GradientColorKey(baseColor, 1f);

        // Alpha 키: 앞뒤가 투명, 가운데가 선명
        for (int i = 0; i < alphaKeys.Length; i++)
        {
            float alphaPosition = (float)i / (alphaKeys.Length - 1);
            float alphaValue = Mathf.Sin(Mathf.PI * alphaPosition); // Sine Curve로 앞뒤 투명
            alphaKeys[i] = new GradientAlphaKey(alphaValue, alphaPosition);
        }

        gradient.SetKeys(colorKeys, alphaKeys);
        lineRenderer.colorGradient = gradient;
    }

    public void FadeOutLine(LineRenderer lineRenderer, float duration)
    {
        StartCoroutine(FadeOutCoroutine(lineRenderer, duration));
    }

    private IEnumerator FadeOutCoroutine(LineRenderer lineRenderer, float duration)
    {
        float elapsed = 0f;
        Gradient gradient = lineRenderer.colorGradient;
        GradientAlphaKey[] alphaKeys = gradient.alphaKeys;

        while (elapsed < duration)
        {
            float alphaMultiplier = Mathf.Lerp(1f, 0f, elapsed / duration); // Alpha 점진적으로 감소

            // Alpha 값 업데이트
            for (int i = 0; i < alphaKeys.Length; i++)
            {
                alphaKeys[i].alpha *= alphaMultiplier;
            }

            gradient.alphaKeys = alphaKeys;
            lineRenderer.colorGradient = gradient;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 마지막에 완전히 투명하게
        for (int i = 0; i < alphaKeys.Length; i++)
        {
            alphaKeys[i].alpha = 0f;
        }

        gradient.alphaKeys = alphaKeys;
        lineRenderer.colorGradient = gradient;
    }

//여기서부터는 완전히 총알의 움직임 관련
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
                if (enemy == collision.GetComponent<EnemyBasicMove>()) enemy.OnDamaged(player.transform.position, playerMove.normalDamage);
                //bullet를 istrigger로 해서 enemy와의 물리적 상호작용을 없애버렸는데, 이 때문에 탄환의 속도가 빨라 적에게 정면충돌해도 때때로 적의 뒤쪽에서 부딪힌 것으로 판정되어, 임시방편으로 이렇게 처음 맞은 적이라도 데미지를 받도록 해두었습니다.
                else enemy.OnDamaged(explosionPosition, playerMove.normalDamage);
            }
            ParticleSystem effect = Instantiate(explodeParticle, gameObject.transform.position, Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, effect.main.duration);
            StartCoroutine(delete(0));
        }
        else if (collision.gameObject.tag == "enemy")
        {
            if (ShootType == 0)
            {
                EnemyHit(collision.transform);
                StartCoroutine(delete(0));
            }
            else if (ShootType == 2)
            {
                EnemyHit(collision.transform);
            }
        }
        else if (collision.gameObject.tag == "Platform")
        {
            StartCoroutine(delete(0));
        }
    }

    void EnemyHit(Transform enemy)
    {
        EnemyBasicMove enemyMove = enemy.GetComponent<EnemyBasicMove>();
        enemyMove.OnDamaged(player.transform.position, playerMove.normalDamage);
    }

    public void NormalShot()
    {
        ShootType = 0;
        rigid.bodyType = RigidbodyType2D.Dynamic;
        rigid.AddForce(transform.right * NormalShotSpeed, ForceMode2D.Impulse);
        rigid.bodyType = RigidbodyType2D.Kinematic;
        Debug.Log(gameObject.transform.rotation);
        StartCoroutine(delete(2));
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
        StartCoroutine(delete(2));
    }


    //여기 밑은  이펙트 부분


    public IEnumerator delete(float delay)
    {
        yield return new WaitForSeconds(delay);
        particle.Stop();
        rigid.velocity = Vector2.zero;
        gameObject.GetComponent<CapsuleCollider2D>().enabled = false;
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        FadeOutLine(lineRenderer1, 3f);
        FadeOutLine(lineRenderer2, 3f);
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
    }
}
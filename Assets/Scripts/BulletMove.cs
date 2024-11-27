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
    
    public float ShootType;     //0�� ���� ������ �����, 2�� ���� ��Ƶ� ������
    public float NormalShotSpeed;
    Vector3 shot1StartPosition;
    Vector3 explosionPosition;
    public ParticleSystem explodeParticle;
    public float S2ShotSpeed;
    public GameObject player;
    private List<EnemyBasicMove> detectedEnemies = new List<EnemyBasicMove>();

    public LineRenderer lineRenderer1; // ù ��° ����
    public LineRenderer lineRenderer2; // �� ��° ����
    public float pointSpacing = 0.1f;  // �� ������ �ּ� �Ÿ�
    public int maxPoints = 100;        // �ִ� �� ����

    public float sineAmplitude = 0.5f; // Sine Curve�� ����
    public float sineFrequency = 5f;   // Sine Curve�� ���ļ�
    private float sinePhaseOffset1;    // ù ��° ������ ���� ����
    private float sinePhaseOffset2;    // �� ��° ������ ���� ����

    private Vector3 lastPointPosition;
    private int currentPointIndex;

    ParticleSystem particle;

    void Start()
    {
        // ���� ������ �������� ����
        sinePhaseOffset1 = Random.Range(0f, Mathf.PI * 2f); // 0 ~ 2��
        sinePhaseOffset2 = Random.Range(0f, Mathf.PI * 2f); // 0 ~ 2��

        lineRenderer1.positionCount = 1;
        lineRenderer2.positionCount = 1;
        lastPointPosition = transform.position;
        lineRenderer1.SetPosition(0, lastPointPosition);
        lineRenderer2.SetPosition(0, lastPointPosition);

        // �ʱ� ���� ����
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
        //��Ÿ���� 1�� ���, �ִ�Ÿ� �̻����� �����ϸ� ������.
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
                // �� �߰�
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
                // ������ �� �����ϰ� �� �� �߰� (Trail ȿ��)
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

            // ���� ������Ʈ
            SetLineTransparency(lineRenderer1, Color.white);
            SetLineTransparency(lineRenderer2, Color.white);
        }
    }

    // Sine curve ��� (���� ���ο�)
    private Vector3 ApplySineCurve(Vector3 basePosition, int index, float phaseOffset, bool reverse = false)
    {
        float sineOffset = Mathf.Sin(index * pointSpacing * sineFrequency + phaseOffset) * sineAmplitude;
        if (reverse) sineOffset *= -1; // �ݴ� �������� ����
        return new Vector3(basePosition.x, basePosition.y + sineOffset, basePosition.z);
    }

    private void SetLineTransparency(LineRenderer lineRenderer, Color baseColor)
    {
        Gradient gradient = new Gradient();
        GradientColorKey[] colorKeys = new GradientColorKey[2];
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[8];

        // ���� ����
        colorKeys[0] = new GradientColorKey(baseColor, 0f);
        colorKeys[1] = new GradientColorKey(baseColor, 1f);

        // Alpha Ű: �յڰ� ����, ����� ����
        for (int i = 0; i < alphaKeys.Length; i++)
        {
            float alphaPosition = (float)i / (alphaKeys.Length - 1);
            float alphaValue = Mathf.Sin(Mathf.PI * alphaPosition); // Sine Curve�� �յ� ����
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
            float alphaMultiplier = Mathf.Lerp(1f, 0f, elapsed / duration); // Alpha ���������� ����

            // Alpha �� ������Ʈ
            for (int i = 0; i < alphaKeys.Length; i++)
            {
                alphaKeys[i].alpha *= alphaMultiplier;
            }

            gradient.alphaKeys = alphaKeys;
            lineRenderer.colorGradient = gradient;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // �������� ������ �����ϰ�
        for (int i = 0; i < alphaKeys.Length; i++)
        {
            alphaKeys[i].alpha = 0f;
        }

        gradient.alphaKeys = alphaKeys;
        lineRenderer.colorGradient = gradient;
    }

//���⼭���ʹ� ������ �Ѿ��� ������ ����
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (ShootType == 1)
        {
            explosionPosition = transform.position;
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 2.5f);       //���� ���ڰ� ���� �ݰ�
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
            // ������ ��� ���鿡�� OnDamaged �ߵ�
            foreach (EnemyBasicMove enemy in detectedEnemies)
            {
                if (enemy == collision.GetComponent<EnemyBasicMove>()) enemy.OnDamaged(player.transform.position, playerMove.normalDamage);
                //bullet�� istrigger�� �ؼ� enemy���� ������ ��ȣ�ۿ��� ���ֹ��ȴµ�, �� ������ źȯ�� �ӵ��� ���� ������ �����浹�ص� ������ ���� ���ʿ��� �ε��� ������ �����Ǿ�, �ӽù������� �̷��� ó�� ���� ���̶� �������� �޵��� �صξ����ϴ�.
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

    public void S2Shot()            //(���� ��)
    {
        ShootType = 2;
        rigid.bodyType = RigidbodyType2D.Dynamic;
        rigid.AddForce(transform.right * S2ShotSpeed, ForceMode2D.Impulse);
        rigid.bodyType = RigidbodyType2D.Kinematic;
        StartCoroutine(delete(2));
    }


    //���� ����  ����Ʈ �κ�


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
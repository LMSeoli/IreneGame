using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class HandCannonMove : MonoBehaviour
{
    public float ShotDelay;
    public float S2ShootDelay;
    public GameObject Player;
    public GameObject bulletObject;
    public AudioClip shotSound;
    public AudioClip S2ShotSound;
    public AudioClip S3HitSound;

    SpriteRenderer spriteRenderer;
    PlayerMove playerMove;
    BulletMove bulletMove;
    AudioSource audioSource;
    GameManager gameManager;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerMove = Player.GetComponent<PlayerMove>();
        audioSource = Player.GetComponent<AudioSource>();
        gameManager = FindObjectOfType<GameManager>();
    }

    public void NormalShot(bool dir, Vector3 targetDirection)
    {
        CancelInvokeLog("disable");
        PlaySound("Shot");
        spriteRenderer.flipY = dir;
        //transform.position = new Vector2(Player.transform.position.x + (dir?-0.4f:0.4f), Player.transform.position.y);
        float angle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg; // 방향 벡터의 각도 계산
        transform.rotation = Quaternion.Euler(0, 0, angle); // Z축 기준으로 회전

        GameObject bullet = Instantiate(bulletObject, transform.position, transform.rotation);  //각도도 맞춰진다?!!
        bullet.SetActive(true);
        BulletMove bulletMove = bullet.GetComponent<BulletMove>();
        bulletMove.NormalShot();
        Invoke("disable", ShotDelay);
    }

    public void S1Shoot(bool dir, Vector3 ShootDirection)
    {
        /*CancelInvokeLog("disable");               //난사 코드
        spriteRenderer.flipY = dir;
        transform.position = new Vector2(Player.transform.position.x + (dir ? -0.4f : 0.4f), Player.transform.position.y);
        transform.rotation = Quaternion.Euler(0, 0, dir ? 180 : 0);
        while (BulLeft > 0)
        {
            PlaySound("Shot");
            GameObject bullet = Instantiate(bulletObject, transform.position, transform.rotation);  //각도도 맞춰진다?!!
            bullet.SetActive(true);
            BulletMove bulletMove = bullet.GetComponent<BulletMove>();
            bulletMove.S2Shot();
            yield return new WaitForSeconds(0.09f);
            BulLeft -= 1;
            gameManager.BulletDown();
        }
        gameManager.StartCoroutine(gameManager.ReloadAtOnce());
        Invoke("disable", ShotDelay);*/

        CancelInvokeLog("disable");
        PlaySound("Shot");
        float angle = Mathf.Atan2(ShootDirection.y, ShootDirection.x) * Mathf.Rad2Deg; // 방향 벡터의 각도 계산
        transform.rotation = Quaternion.Euler(0, 0, angle);
        spriteRenderer.flipY = (angle > 90 || angle <= -90) ? true : false;
        GameObject bullet = Instantiate(bulletObject, transform.position, transform.rotation);
        bullet.SetActive(true);
        BulletMove bulletMove = bullet.GetComponent<BulletMove>();
        bulletMove.ExplodeShot();
        Invoke("disable", ShotDelay);
    }

    public void S2Shoot(Vector3 S2End, bool dir)
    {
        CancelInvokeLog("disable");
        //스프라이트 반전 여부는 처음에!!
        spriteRenderer.flipY = !dir;

        //존나 돌려
        transform.rotation = Quaternion.Euler(0, 0, dir ? 45 : 135);
        StartCoroutine(S2RotateOverTime(S2ShootDelay, dir));
    }

    private IEnumerator S2RotateOverTime(float ShootTime, bool dir)
    {
        float startTime = Time.time;  // 코루틴 시작 시점의 시간을 기록
        float endTime = startTime + ShootTime;  // 종료 시간 계산
        float rotationSpeed = 360f / ShootTime;  // 한 바퀴를 도는 속도

        while (Time.time < endTime)
        {
            float elapsedTime = Time.time - startTime;  // 경과 시간 계산
            float deltaRotation = rotationSpeed * Time.deltaTime * 8;  // 프레임 당 회전량 계산
            transform.Rotate(0, 0, deltaRotation);  // 오브젝트 회전
            yield return null;
        }
        transform.rotation = Quaternion.Euler(0, 0, dir ? 45 : 135);
        //총알 발사
        PlaySound("S2Shot");
        gameManager.BulletDown();
        GameObject bullet = Instantiate(bulletObject, transform.position, transform.rotation);  //각도도 맞춰진다?!!
        bullet.SetActive(true);
        CapsuleCollider2D bulletCollider = bullet.GetComponent<CapsuleCollider2D>();
        bulletCollider.size *= 3;
        BulletMove bulletMove = bullet.GetComponent<BulletMove>();
        bulletMove.S2Shot();
        Invoke("disable", ShotDelay);       //일단은 저거랑 맞추려고
        Invoke("S2Cool", ShotDelay);
    }

    public void S3Shoot(bool dir)
    {
        Debug.Log("S3Shoot started");
        CancelInvokeLog("disable");
        spriteRenderer.flipY = !dir;
        transform.rotation = Quaternion.Euler(0, 0, dir ? 45 : 135);
        Invoke("disable", 2.2f);
        StartCoroutine(S3EndRotation(dir));
    }

    public IEnumerator S3ShootMove(GameObject AttackTarget)
    {
        Debug.Log("총은 잘 바라봄");
        spriteRenderer.flipY = AttackTarget.transform.position.x - transform.position.x < 0 ? true : false;
        float looktime = Time.time;
        while (Time.time - looktime < 0.08f)
        {
            transform.LookAt(AttackTarget.transform);
            yield return null;
        }
    }

    private IEnumerator S3EndRotation(bool dir)
    {
        Debug.Log("S3EndRotation started");
        yield return new WaitForSeconds(1.7f);
        spriteRenderer.flipY = dir;
        float S3time = Time.time;
        while (Time.time - S3time < 0.3f)
        {
            float deltaRotation = (Time.time - S3time)*1800;
            transform.Rotate(0, 0, deltaRotation);
            yield return null;
        }
        transform.rotation = Quaternion.Euler(0, 0, dir ? 225 : -45);
        Debug.Log("S3EndRotation complete");
    }

    void disable()
    {
        gameObject.SetActive(false);
    }

    void S2Cool()
    {
        playerMove.S2CoolTime = playerMove.S2CoolTime_max;
        playerMove.isSkill = false;
    }

    void CancelInvokeLog(string what)
    {
        CancelInvoke(what);
        transform.position = Player.transform.position;
    }

    public void PlaySound(string action)
    {
        switch (action)
        {
            case "Shot":
                audioSource.PlayOneShot(shotSound);
                break;
            case "S2Shot":
                audioSource.PlayOneShot(S2ShotSound);
                break;
            case "S3Shot":
                audioSource.PlayOneShot(S3HitSound);
                break;
        }
    }
}

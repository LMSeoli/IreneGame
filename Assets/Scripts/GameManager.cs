using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public AudioClip audioBulletIn;
    public AudioClip audioSlideIn;
    public AudioClip audioBackSong1;

    public int totalPoint;                                  //점수 관리는 여기에서
    public int stagePoint;
    public int stageIndex;
    public int maxHealth;                                   //체력 관리는 여기에서
    public int health;
    public int bullet;                                      //탄환 관리는 여기에서
    public int lampLight = 100;                             //등불 관리!
    public PlayerMove player;
    public GameObject[] Stages;

    public class rejectionReaction
    {
        public string rejectionName;        // 거부반응 이름
        public string description;          // 아이템 설명
        public bool isActive;               // 활성화 여부

        public rejectionReaction(string name, string desc)
        {
            rejectionName = name;
            description = desc;
            isActive = false;
        }
    }
    public List<rejectionReaction> rejections = new List<rejectionReaction>();

    //UI관련
    public Image UIHealth;
    public Image[] UIBullet;
    public TextMeshProUGUI UIPoint;
    public TextMeshProUGUI UIStage;
    public TextMeshProUGUI ScriptTxt;
    public TextMeshProUGUI S1CoolTxt;           //
    public TextMeshProUGUI S2CoolTxt;           //
    public TextMeshProUGUI S3CoolTxt;           //테스트 시엔 일단 텍스트로 표시?
    public GameObject GameCamera;
    public GameObject UIRestartBtn;

    public float reloadSpeed = 0.6f;

    CameraMove cameraMove;
    AudioSource audioSource;
    RelicManager relicManager;

    private void Awake()
    {
        cameraMove = GameCamera.GetComponent<CameraMove>();
        audioSource = gameObject.GetComponent<AudioSource>();
        player = FindObjectOfType<PlayerMove>();
        relicManager = FindObjectOfType<RelicManager>();
        PlaySound("BackSong1");

        rejections.Add(new rejectionReaction("신경쇠퇴", "피격 시 0.5초간 경직"));
        rejections.Add(new rejectionReaction("조혈장애", "피격 시 추가 출혈 데미지 받음"));//
        rejections.Add(new rejectionReaction("주의력 결핍", "궁극기 발동 가능 시 즉시 사용"));//
        rejections.Add(new rejectionReaction("생체 변이", "받는 데미지 2배, 주는 데미지 2/3배, 이동속도 1.5배, 쿨타임 0.5배"));//2스킬 거리같은 걸 조정하기보단, 그냥 쿨타임을 줄여버리자. 탄환 개수랑 피격 데미지 증가 때문에 밸런스는 맞을지도?
        rejections.Add(new rejectionReaction("잠식", "감시자같은 특수 기믹, 지속적으로 데미지를 받음, 등불로 삭제 가능, 사망한다면..."));//패배씬이 있으면 안된다.. 대신 영구 버프? 뭔가 큰 디버프를 걸면 좋을 듯. 
    }

    private void Update()
    {
        UIPoint.text = (totalPoint + stagePoint).ToString();
    }


    public void NextStage()
    {
        //Change stage
        if(stageIndex < Stages.Length-1) {
            //일단 스테이지 변경. 사이에 로딩창이 있으면 좋겠다?
            Stages[stageIndex].SetActive(false);
            stageIndex++;
            Stages[stageIndex].SetActive(true);
            //이제 기본적 요소들 적용. 일단 초기 등불 수치를 적들에게 전달해야 하는데, 이건 EnemyBasicMove에 넣는 게 좋을 듯. awake에서 조져
            PlayerReposition();
            cameraMove.FrameChange(stageIndex);
            //전부 바꾼 후 스테이지 이름 잠시 띄우고 사라지게 하기. 미구현
            UIStage.text = "STAGE " + (stageIndex + 1); 
        }
        else
        {
            player.PlaySound("FINISH");
            Time.timeScale = 0;
            Debug.Log("게임 클리어!");
            TextMeshProUGUI btnText = UIRestartBtn.GetComponentInChildren<TextMeshProUGUI>();
            btnText.text = "Game Clear!";
            UIRestartBtn.SetActive(true);
        }

        //Calculate Point
        totalPoint += stagePoint;
        stagePoint = 0;
    }

    public void HealthDown(int damage)
    {
        health -= damage;
        UIHealth.fillAmount = (float)health/maxHealth;
        if (health < 1)
        {
            UIRestartBtn.SetActive(true);
            player.OnDie();
        }
        else if ((float)health/maxHealth <= 0.2f)
        {
            UIHealth.color = Color.red;
        }
    }

    public void BulletDown()
    {
        bullet--;
        UIBullet[bullet].color = new Color(1, 1, 1, 0);
    }

    public void S3CountUp()
    {
        //S3Count가 최대가 아니라면 높임
        if (player.S3Count < 16)
        {
            player.S3Count++;

            //ui에서도 채워지게
        }
        
        //3스 게이지 인터페이스 제작해야댐
    }
    public void S3CountDown()
    {
        player.S3Count=0;
        //ui에서 게이지 1개씩 소모
    }

    public IEnumerator Reload()
    {
        Debug.Log("Reload!");
        while (bullet < 6)
        {
            //이쪽 오디오는 원샷보다 오디오 클립으로 하는 게 나을 거 같음
            audioSource.PlayOneShot(audioBulletIn);
            float Reloadtime = Time.time;
            while (Time.time - Reloadtime < reloadSpeed)       //재장전 유물 여부에 따라 달라짐
            {
                if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
                {
                    //audioSource.PlayOneShot(audioSlideIn);
                    PlaySound("Reload");
                    player.isReload = false;
                    yield break;
                }
                yield return null;
            }
            UIBullet[bullet].color = new Color(1, 1, 1, 1);
            bullet++;
        }
        audioSource.PlayOneShot(audioSlideIn);
        player.isReload = false;
    }

    public IEnumerator ReloadAtOnce()           //bullet을 전부 사용하고 한 번에 리로딩되게 하는 함수, skill로 취급하면 충돌할 일도 없음
    {
        Debug.Log("Reload!");
        while (bullet < 6)
        {
            //이쪽 오디오는 원샷보다 오디오 클립으로 하는 게 나을 거 같음
            audioSource.PlayOneShot(audioBulletIn);
            float Reloadtime = Time.time;
            yield return new WaitForSeconds(0.1f);        //여기의 숫자가 탄환의 재장전 시간을 나타냄. 사운드에 맞는 초기값은 0.4f
            UIBullet[bullet].color = new Color(1, 1, 1, 1);
            bullet++;
        }
        audioSource.PlayOneShot(audioSlideIn);
        //player.isReload = false;
        player.isSkill = false;
    }

    public void ReloadAll()                         //궁극기 사용 후 모든 탄환을 소모 후 전부 재장전되도록.
    {
        int bulLeft = bullet;
        for (int i = 0; bullet<6; i++) {
            UIBullet[bulLeft+i].color = new Color(1, 1, 1, 1);
            Debug.Log(bullet);
            bullet++;
        }
    }

    public void PlayerReposition()
    {
        player.transform.position = new Vector3(0, 0, -1);
        player.VelocityZero();
    }

    public void Restart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }

    public void PlaySound(string action)
    {
        switch (action)
        {
            case "BackSong1":
                audioSource.clip = audioBackSong1;
                break;
            case "Reload":
                audioSource.clip = audioSlideIn;
                break;
        }
        audioSource.Play();
    }
}
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

    public int totalPoint;
    public int stagePoint;
    public int stageIndex;
    public int health;
    public int bullet;
    public PlayerMove player;
    public GameObject[] Stages;

    public Image[] UIHealth;
    public Image[] UIBullet;
    public TextMeshProUGUI UIPoint;
    public TextMeshProUGUI UIStage;
    public TextMeshProUGUI ScriptTxt;
    public TextMeshProUGUI S1CoolTxt;           //
    public TextMeshProUGUI S2CoolTxt;           //
    public TextMeshProUGUI S3CoolTxt;           //테스트 시엔 일단 텍스트로 표시
    public GameObject GameCamera;
    public GameObject UIRestartBtn;

    CameraMove cameraMove;
    AudioSource audioSource;

    private void Awake()
    {
        cameraMove = GameCamera.GetComponent<CameraMove>();
        audioSource = gameObject.GetComponent<AudioSource>();
        player = FindObjectOfType<PlayerMove>();
        //PlaySound("BackSong1");
    }

    private void Update()
    {
        UIPoint.text = (totalPoint + stagePoint).ToString();
    }


    public void NextStage()
    {
        //Change stage
        if(stageIndex < Stages.Length-1) {
            Stages[stageIndex].SetActive(false);
            stageIndex++;
            Stages[stageIndex].SetActive(true);
            PlayerReposition();
            cameraMove.FrameChange(stageIndex);
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

    public void HealthDown()
    {
        UIHealth[health - 1].color = new Color(1, 0, 0, 0.4f);
        health--;
        if (health < 1)
        {
            UIRestartBtn.SetActive(true);
            player.OnDie();
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
            while (Time.time - Reloadtime < 0.6f)       //여기의 숫자가 탄환의 재장전 시간을 나타냄. 사운드에 맞는 초기값은 0.4f
            {
                if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.C))
                {
                    audioSource.PlayOneShot(audioSlideIn);
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
        }
        audioSource.Play();
    }
}
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

    public int totalPoint;                                  //���� ������ ���⿡��
    public int stagePoint;
    public int stageIndex;
    public int maxHealth;                                   //ü�� ������ ���⿡��
    public int health;
    public int bullet;                                      //źȯ ������ ���⿡��
    public int lampLight = 100;                             //��� ����!
    public PlayerMove player;
    public GameObject[] Stages;

    public class rejectionReaction
    {
        public string rejectionName;        // �źι��� �̸�
        public string description;          // ������ ����
        public bool isActive;               // Ȱ��ȭ ����

        public rejectionReaction(string name, string desc)
        {
            rejectionName = name;
            description = desc;
            isActive = false;
        }
    }
    public List<rejectionReaction> rejections = new List<rejectionReaction>();

    //UI����
    public Image UIHealth;
    public Image[] UIBullet;
    public TextMeshProUGUI UIPoint;
    public TextMeshProUGUI UIStage;
    public TextMeshProUGUI ScriptTxt;
    public TextMeshProUGUI S1CoolTxt;           //
    public TextMeshProUGUI S2CoolTxt;           //
    public TextMeshProUGUI S3CoolTxt;           //�׽�Ʈ �ÿ� �ϴ� �ؽ�Ʈ�� ǥ��?
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

        rejections.Add(new rejectionReaction("�Ű����", "�ǰ� �� 0.5�ʰ� ����"));
        rejections.Add(new rejectionReaction("�������", "�ǰ� �� �߰� ���� ������ ����"));//
        rejections.Add(new rejectionReaction("���Ƿ� ����", "�ñر� �ߵ� ���� �� ��� ���"));//
        rejections.Add(new rejectionReaction("��ü ����", "�޴� ������ 2��, �ִ� ������ 2/3��, �̵��ӵ� 1.5��, ��Ÿ�� 0.5��"));//2��ų �Ÿ����� �� �����ϱ⺸��, �׳� ��Ÿ���� �ٿ�������. źȯ ������ �ǰ� ������ ���� ������ �뷱���� ��������?
        rejections.Add(new rejectionReaction("���", "�����ڰ��� Ư�� ���, ���������� �������� ����, ��ҷ� ���� ����, ����Ѵٸ�..."));//�й���� ������ �ȵȴ�.. ��� ���� ����? ���� ū ������� �ɸ� ���� ��. 
    }

    private void Update()
    {
        UIPoint.text = (totalPoint + stagePoint).ToString();
    }


    public void NextStage()
    {
        //Change stage
        if(stageIndex < Stages.Length-1) {
            //�ϴ� �������� ����. ���̿� �ε�â�� ������ ���ڴ�?
            Stages[stageIndex].SetActive(false);
            stageIndex++;
            Stages[stageIndex].SetActive(true);
            //���� �⺻�� ��ҵ� ����. �ϴ� �ʱ� ��� ��ġ�� ���鿡�� �����ؾ� �ϴµ�, �̰� EnemyBasicMove�� �ִ� �� ���� ��. awake���� ����
            PlayerReposition();
            cameraMove.FrameChange(stageIndex);
            //���� �ٲ� �� �������� �̸� ��� ���� ������� �ϱ�. �̱���
            UIStage.text = "STAGE " + (stageIndex + 1); 
        }
        else
        {
            player.PlaySound("FINISH");
            Time.timeScale = 0;
            Debug.Log("���� Ŭ����!");
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
        //S3Count�� �ִ밡 �ƴ϶�� ����
        if (player.S3Count < 16)
        {
            player.S3Count++;

            //ui������ ä������
        }
        
        //3�� ������ �������̽� �����ؾߴ�
    }
    public void S3CountDown()
    {
        player.S3Count=0;
        //ui���� ������ 1���� �Ҹ�
    }

    public IEnumerator Reload()
    {
        Debug.Log("Reload!");
        while (bullet < 6)
        {
            //���� ������� �������� ����� Ŭ������ �ϴ� �� ���� �� ����
            audioSource.PlayOneShot(audioBulletIn);
            float Reloadtime = Time.time;
            while (Time.time - Reloadtime < reloadSpeed)       //������ ���� ���ο� ���� �޶���
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

    public IEnumerator ReloadAtOnce()           //bullet�� ���� ����ϰ� �� ���� ���ε��ǰ� �ϴ� �Լ�, skill�� ����ϸ� �浹�� �ϵ� ����
    {
        Debug.Log("Reload!");
        while (bullet < 6)
        {
            //���� ������� �������� ����� Ŭ������ �ϴ� �� ���� �� ����
            audioSource.PlayOneShot(audioBulletIn);
            float Reloadtime = Time.time;
            yield return new WaitForSeconds(0.1f);        //������ ���ڰ� źȯ�� ������ �ð��� ��Ÿ��. ���忡 �´� �ʱⰪ�� 0.4f
            UIBullet[bullet].color = new Color(1, 1, 1, 1);
            bullet++;
        }
        audioSource.PlayOneShot(audioSlideIn);
        //player.isReload = false;
        player.isSkill = false;
    }

    public void ReloadAll()                         //�ñر� ��� �� ��� źȯ�� �Ҹ� �� ���� �������ǵ���.
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
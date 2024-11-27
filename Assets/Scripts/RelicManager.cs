using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelicItem
{
    public int number;          // ���� ��ȣ
    public string itemName;     // ������ �̸�
    public string description;  // ������ ����
    public bool isOwned;        // ���� ����
    public bool isActive;       // Ȱ��ȭ ����, ���Ŀ� ��� isOwned üũ �ڵ带 �ٲ� �ʿ� ����?? �ƿ� �� �̸��� �ٲٸ� �ǰ�, random�ڵ常 �����ϸ� �� ��

    public RelicItem(int num, string name, string desc)
    {
        number = num;
        itemName = name;
        description = desc;
        isOwned = false;         //�ƿ� �׽�Ʈ �ÿ� �����ϰ� ������ ��ü�� Ȱ��ȭ ���� �����
    }
}

public class RelicManager : MonoBehaviour
{
    //��ü �ڵ� ���� ��ҵ�
    public List<RelicItem> RelicItems = new List<RelicItem>();
    private bool[] appliedRelicEffects;
    
    //���� ȿ�� ���� ��ҵ�
    PlayerMove playerMove;
    GameManager gameManager;
    public GameObject tictactoe;
    public GameObject fawlBeast;

    void Start()
    {
        playerMove = FindObjectOfType<PlayerMove>();
        gameManager = FindObjectOfType<GameManager>();
        // ������ ��� �߰� - buffItems.Add(new BuffItem(����, "�̸�", "ȿ��"));
        // ������ ���� �ֵ�� �߰����� ����Ʈ�� �����ϱ⿡ ������ �������� �ʾ�����, �ε����� �����ϰ� �� ��� 0���� ���� ������ �ε����� ��ȣ�� ���� �ʰ� �ȴ�. ���� ��������.
        RelicItems.Add(new RelicItem(1, "ƽ����", "�� ���ù����� ���ǰ��� ȭ���� ����ϴ�..! \n����!!"));//
        RelicItems.Add(new RelicItem(2, "����� ����", "�ǰ� �� �ֺ��� �ִ� ������ �̵��ӵ��� ����ϴ�."));//
        RelicItems.Add(new RelicItem(3, "���� �Ŀ��Ʈ�� ����", "���߿� �� �ִ� ������ ���� ������ ���, �Ŀ��Ʈ�� �߰����� ������ �����ϴ�."));//
        RelicItems.Add(new RelicItem(4, "���� źâ ��ü", "������ �ð��� �����."));//
        RelicItems.Add(new RelicItem(5, "����� ȸ��", "ȸ�� ��Ÿ���� �����."));//
        RelicItems.Add(new RelicItem(6, "��� ���ȭ", "���� Ÿ���� ������ ��� ��ġ�� ���ݾ� �����մϴ�."));
        RelicItems.Add(new RelicItem(7, "������ Ÿ��", "�˿� �ǰݵ� ���� �̵��ӵ��� �����մϴ�."));
        RelicItems.Add(new RelicItem(8, "Ȯ���� ������", "���� X ��ų�� ������ ���� �������� �ʰ�, ������ ������ źȯ�� �߻��մϴ�. (��Ÿ�� 0.5��)"));//�̷� ����ũ�� ���� �ִ� �͵� �߰��ϰ�, �̹� ȹ���� ������ Ȱ��ȭ/��Ȱ��ȭ �ϴ� �͵� �ʿ��� ��.
        RelicItems.Add(new RelicItem(9, "���", "�޺� ���� ���� ������ ���ϴ� ���ط��� �����մϴ�."));
        RelicItems.Add(new RelicItem(10, "����", "�⺻ �̵��ӵ��� �����մϴ�"));

        appliedRelicEffects = new bool[RelicItems.Count];
    }

    // ���� 1ȸ���� ȿ�� ���� ����
    void ApplyRelicEffect(int relicIndex)
    {
        switch (relicIndex+1)
        {
            case 1:
                Debug.Log("���� ȿ�� ����: ���ݷ� ����");
                // ���ݷ� ���� ����
                playerMove.normalDamage *= 1.2f; // ����
                tictactoe.GetComponent<SpriteRenderer>().enabled = true;
                break;

            case 3:
                Debug.Log("���� ȿ�� ����: �Ŀ��Ʈ ��ȯ");
                fawlBeast.SetActive(true);
                break;

            case 4:
                Debug.Log("���� ȿ�� ����: ������ �ð� ����");
                gameManager.reloadSpeed *= 1/2f;
                break;


            default:
                Debug.Log($"�������� ���� ȿ���� ���� (index: {relicIndex})");
                break;
        }
    }

    //���� ����ȿ�� ���� ����
    //�Ƹ� ��� �̼Ӱ��Ұ��� �Ϻ� ȿ���� ���⿡�� �����ϴ� �� ��������

    public void AddRandomRelic()
    {
        // �������� ���� ������ ���͸�
        List<RelicItem> unownedItems = RelicItems.FindAll(item => !item.isOwned);

        if (unownedItems.Count > 0)
        {
            // �������� �������� ���� ������ �ϳ� ����
            RelicItem randomItem = unownedItems[Random.Range(0, unownedItems.Count)];
            randomItem.isOwned = true;
            Debug.Log($"{randomItem.itemName} �������� ȹ���߽��ϴ�: {randomItem.description}");
        }
        else
        {
            Debug.Log("��� �������� �̹� �����ϰ� �ֽ��ϴ�!");
        }
        CheckOwnedRelics();
    }

    public void AddCertainRelicByNumber(int number)
    {
        RelicItem item = RelicItems.Find(relic => relic.number == number);
        if (item != null && !item.isOwned)
        {
            item.isOwned = true;
            Debug.Log($"{item.itemName} �������� ȹ���߽��ϴ�: {item.description}");
        }
        else if (item != null && item.isOwned)
        {
            Debug.Log($"{item.itemName} �������� �̹� �����ϰ� �ֽ��ϴ�!");
        }
        else
        {
            Debug.Log($"��ȣ {number}�� �ش��ϴ� �������� �����ϴ�.");
        }
        CheckOwnedRelics();
    }

    void CheckOwnedRelics()
    {
        List<int> ownedRelicIndices = new List<int>(); // ������ relic ��ȣ ����

        for (int i = 0; i < RelicItems.Count; i++)
        {
            if (RelicItems[i].isOwned) // ���� ���� Ȯ��
            {
                ownedRelicIndices.Add(i); // ������ Relic�� ��ȣ �߰�

                // �ش� ���� ȿ���� ���� ������� �ʾҴٸ�
                if (!appliedRelicEffects[i])
                {
                    ApplyRelicEffect(i); // ���� ȿ�� ����
                    appliedRelicEffects[i] = true; // ȿ�� ���� ���
                }
            }
        }

        // ������ Relic ��ȣ���� �ϳ��� ���ڿ��� ���
        /*if (ownedRelicIndices.Count > 0)
        {
            Debug.Log("������ Relic ��ȣ: " + string.Join(", ", ownedRelicIndices));
        }
        else
        {
            Debug.Log("������ Relic�� �����ϴ�.");
        }*/
    }
    //RelicManager.RelicItems.Exists(item => item.isOwned && item.number == 9) �� ���� ���� �˾Ƴ� �� ����
}

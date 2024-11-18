using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelicItem
{
    public string itemName;
    public string description;
    public bool isOwned; // ������ ���� ����

    public RelicItem(string name, string desc)
    {
        itemName = name;
        description = desc;
        isOwned = false;
    }
}

public class RelicManager : MonoBehaviour
{
    public List<RelicItem> RelicItems = new List<RelicItem>();

    void Start()
    {
        // ������ ��� �߰�
        // buffItems.Add(new BuffItem("�̸�", "ȿ��"));
        RelicItems.Add(new RelicItem("�޷��� �νó���!", "�⺻ �̵��ӵ��� �����մϴ�"));
        RelicItems.Add(new RelicItem("ƽ����", "�� ���ù����� ���ǰ��� ȭ���� ����ϴ�..! \n����!!"));
        RelicItems.Add(new RelicItem("����� ����", "�ǰ� �� �ֺ��� �ִ� ������ �̵��ӵ��� ����ϴ�."));
        RelicItems.Add(new RelicItem("���� �Ŀ��Ʈ�� ����", "���߿� �� �ִ� ������ ���� ������ ���, �Ŀ��Ʈ�� �߰����� ������ �����ϴ�."));
        RelicItems.Add(new RelicItem("���� źâ ��ü", "������ �ð��� �����."));
        RelicItems.Add(new RelicItem("����� ȸ��", "ȸ�� ��Ÿ���� �����."));
        RelicItems.Add(new RelicItem("��� ���ȭ", "���� Ÿ���� ������ ��� ��ġ�� ���ݾ� �����մϴ�."));
        RelicItems.Add(new RelicItem("������ Ÿ��", "�˿� �ǰݵ� ���� �̵��ӵ��� �����մϴ�."));
        RelicItems.Add(new RelicItem("Ȯ���� ������", "���� X ��ų�� ������ ���� �������� �ʰ�, ������ ������ źȯ�� �߻��մϴ�. (��Ÿ�� 1��)"));
        RelicItems.Add(new RelicItem("���", "�޺� ���� ���� ������ ���ϴ� ���ط��� �����մϴ�."));
    }

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
    }

    public void AddCertainRelic(int relicNumber)
    {
        RelicItem randomItem = RelicItems[relicNumber];
        randomItem.isOwned = true;
        Debug.Log($"{randomItem.itemName} �������� ȹ���߽��ϴ�: {randomItem.description}");
    }

    //RelicManager.RelicItems.Exists(item => item.isOwned && item.itemName == "Health Reduction") �� ���� ���� �˾Ƴ� �� ����
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelicItem
{
    public string itemName;
    public string description;
    public bool isOwned; // 아이템 소유 여부

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
        // 아이템 목록 추가
        // buffItems.Add(new BuffItem("이름", "효과"));
        RelicItems.Add(new RelicItem("달려라 로시난테!", "기본 이동속도가 증가합니다"));
        RelicItems.Add(new RelicItem("틱택토", "이 무시무시한 재판관을 화나게 만듭니다..! \n사형!!"));
        RelicItems.Add(new RelicItem("등불의 위압", "피격 시 주변에 있는 적들의 이동속도를 낮춥니다."));
        RelicItems.Add(new RelicItem("작은 파울비스트의 도움", "공중에 떠 있는 상태의 적을 공격할 경우, 파울비스트가 추가적인 공격을 날립니다."));
        RelicItems.Add(new RelicItem("빠른 탄창 교체", "재장전 시간을 낮춘다."));
        RelicItems.Add(new RelicItem("재빠른 회피", "회피 쿨타임을 낮춘다."));
        RelicItems.Add(new RelicItem("등불 재발화", "적을 타격할 때마다 등불 수치가 조금씩 증가합니다."));
        RelicItems.Add(new RelicItem("서늘한 타격", "검에 피격된 적의 이동속도가 감소합니다."));
        RelicItems.Add(new RelicItem("확실한 마무리", "위로 X 스킬이 공중의 적을 추적하지 않고, 공중의 적에게 탄환을 발사합니다. (쿨타임 1초)"));
        RelicItems.Add(new RelicItem("고양", "콤보 수에 따라 적에게 가하는 피해량이 증가합니다."));
    }

    public void AddRandomRelic()
    {
        // 소유하지 않은 아이템 필터링
        List<RelicItem> unownedItems = RelicItems.FindAll(item => !item.isOwned);

        if (unownedItems.Count > 0)
        {
            // 랜덤으로 소유하지 않은 아이템 하나 선택
            RelicItem randomItem = unownedItems[Random.Range(0, unownedItems.Count)];
            randomItem.isOwned = true;
            Debug.Log($"{randomItem.itemName} 아이템을 획득했습니다: {randomItem.description}");
        }
        else
        {
            Debug.Log("모든 아이템을 이미 소유하고 있습니다!");
        }
    }

    public void AddCertainRelic(int relicNumber)
    {
        RelicItem randomItem = RelicItems[relicNumber];
        randomItem.isOwned = true;
        Debug.Log($"{randomItem.itemName} 아이템을 획득했습니다: {randomItem.description}");
    }

    //RelicManager.RelicItems.Exists(item => item.isOwned && item.itemName == "Health Reduction") 로 존재 여부 알아낼 수 있음
}

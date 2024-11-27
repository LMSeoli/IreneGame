using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelicItem
{
    public int number;          // 고유 번호
    public string itemName;     // 아이템 이름
    public string description;  // 아이템 설명
    public bool isOwned;        // 소유 여부
    public bool isActive;       // 활성화 여부, 추후에 모든 isOwned 체크 코드를 바꿀 필요 있음?? 아예 둘 이름을 바꾸면 되고, random코드만 주의하면 될 듯

    public RelicItem(int num, string name, string desc)
    {
        number = num;
        itemName = name;
        description = desc;
        isOwned = false;         //아예 테스트 시엔 용이하게 생성자 자체에 활성화 여부 만들까
    }
}

public class RelicManager : MonoBehaviour
{
    //자체 코드 관련 요소들
    public List<RelicItem> RelicItems = new List<RelicItem>();
    private bool[] appliedRelicEffects;
    
    //유물 효과 관련 요소들
    PlayerMove playerMove;
    GameManager gameManager;
    public GameObject tictactoe;
    public GameObject fawlBeast;

    void Start()
    {
        playerMove = FindObjectOfType<PlayerMove>();
        gameManager = FindObjectOfType<GameManager>();
        // 아이템 목록 추가 - buffItems.Add(new BuffItem(숫자, "이름", "효과"));
        // 랜덤도 없는 애들로 추가적일 리스트를 생성하기에 오류가 생기지는 않았지만, 인덱스로 선택하게 될 경우 0번이 없어 각각의 인덱스와 번호가 맞지 않게 된다. 이점 주의하자.
        RelicItems.Add(new RelicItem(1, "틱택토", "이 무시무시한 재판관을 화나게 만듭니다..! \n사형!!"));//
        RelicItems.Add(new RelicItem(2, "등불의 위압", "피격 시 주변에 있는 적들의 이동속도를 낮춥니다."));//
        RelicItems.Add(new RelicItem(3, "작은 파울비스트의 도움", "공중에 떠 있는 상태의 적을 공격할 경우, 파울비스트가 추가적인 공격을 날립니다."));//
        RelicItems.Add(new RelicItem(4, "빠른 탄창 교체", "재장전 시간을 낮춘다."));//
        RelicItems.Add(new RelicItem(5, "재빠른 회피", "회피 쿨타임을 낮춘다."));//
        RelicItems.Add(new RelicItem(6, "등불 재발화", "적을 타격할 때마다 등불 수치가 조금씩 증가합니다."));
        RelicItems.Add(new RelicItem(7, "서늘한 타격", "검에 피격된 적의 이동속도가 감소합니다."));
        RelicItems.Add(new RelicItem(8, "확실한 마무리", "위로 X 스킬이 공중의 적을 추적하지 않고, 공중의 적에게 탄환을 발사합니다. (쿨타임 0.5초)"));//이런 리스크가 조금 있는 것들 추가하고, 이미 획득한 유물을 활성화/비활성화 하는 것도 필요할 듯.
        RelicItems.Add(new RelicItem(9, "고양", "콤보 수에 따라 적에게 가하는 피해량이 증가합니다."));
        RelicItems.Add(new RelicItem(10, "ㄱㄱ", "기본 이동속도가 증가합니다"));

        appliedRelicEffects = new bool[RelicItems.Count];
    }

    // 유물 1회적용 효과 적용 로직
    void ApplyRelicEffect(int relicIndex)
    {
        switch (relicIndex+1)
        {
            case 1:
                Debug.Log("유물 효과 적용: 공격력 증가");
                // 공격력 증가 로직
                playerMove.normalDamage *= 1.2f; // 예시
                tictactoe.GetComponent<SpriteRenderer>().enabled = true;
                break;

            case 3:
                Debug.Log("유물 효과 적용: 파울비스트 소환");
                fawlBeast.SetActive(true);
                break;

            case 4:
                Debug.Log("유물 효과 적용: 재장전 시간 감소");
                gameManager.reloadSpeed *= 1/2f;
                break;


            default:
                Debug.Log($"단편적인 유물 효과는 없음 (index: {relicIndex})");
                break;
        }
    }

    //유물 지속효과 적용 로직
    //아마 등불 이속감소같은 일부 효과는 여기에서 구현하는 게 좋을지도

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
        CheckOwnedRelics();
    }

    public void AddCertainRelicByNumber(int number)
    {
        RelicItem item = RelicItems.Find(relic => relic.number == number);
        if (item != null && !item.isOwned)
        {
            item.isOwned = true;
            Debug.Log($"{item.itemName} 아이템을 획득했습니다: {item.description}");
        }
        else if (item != null && item.isOwned)
        {
            Debug.Log($"{item.itemName} 아이템은 이미 소유하고 있습니다!");
        }
        else
        {
            Debug.Log($"번호 {number}에 해당하는 아이템이 없습니다.");
        }
        CheckOwnedRelics();
    }

    void CheckOwnedRelics()
    {
        List<int> ownedRelicIndices = new List<int>(); // 보유한 relic 번호 저장

        for (int i = 0; i < RelicItems.Count; i++)
        {
            if (RelicItems[i].isOwned) // 보유 여부 확인
            {
                ownedRelicIndices.Add(i); // 보유한 Relic의 번호 추가

                // 해당 유물 효과가 아직 적용되지 않았다면
                if (!appliedRelicEffects[i])
                {
                    ApplyRelicEffect(i); // 유물 효과 적용
                    appliedRelicEffects[i] = true; // 효과 적용 기록
                }
            }
        }

        // 보유한 Relic 번호들을 하나의 문자열로 출력
        /*if (ownedRelicIndices.Count > 0)
        {
            Debug.Log("보유한 Relic 번호: " + string.Join(", ", ownedRelicIndices));
        }
        else
        {
            Debug.Log("보유한 Relic이 없습니다.");
        }*/
    }
    //RelicManager.RelicItems.Exists(item => item.isOwned && item.number == 9) 로 존재 여부 알아낼 수 있음
}

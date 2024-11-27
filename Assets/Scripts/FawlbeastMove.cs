using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class FawlbeastMove : MonoBehaviour
{
    public float curveHeight;
    public float accelerationFactor;
    public GameObject attackObject;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void FawlbeastAttack(Transform enemy)
    {
        Debug.Log("1");
        GameObject newAttackObject = Instantiate(attackObject, transform.position, Quaternion.identity);
        newAttackObject.SetActive(true);
        //SpriteRenderer AttackSprite = newAttackObject.GetComponent<SpriteRenderer>();
        //AttackSprite.enabled = true;
        // 생성된 인스턴스의 공격 스크립트를 가져와서 공격 함수 호출
        Debug.Log("2");
        var beastAttack = newAttackObject.GetComponent<BeastAttack>(); // AttackScript는 attackObject의 공격을 담당하는 스크립트
        beastAttack.StartCoroutine(beastAttack.Attack(curveHeight, accelerationFactor, enemy));
    }
}

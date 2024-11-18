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

    BeastAttack beastAttack;

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
        GameObject newAttackObject = Instantiate(attackObject, transform.position, Quaternion.identity);
        newAttackObject.SetActive(true);
            //SpriteRenderer AttackSprite = newAttackObject.GetComponent<SpriteRenderer>();
            //AttackSprite.enabled = true;
        // ������ �ν��Ͻ��� ���� ��ũ��Ʈ�� �����ͼ� ���� �Լ� ȣ��

        var beastAttack = newAttackObject.GetComponent<BeastAttack>(); // AttackScript�� attackObject�� ������ ����ϴ� ��ũ��Ʈ
        beastAttack.StartCoroutine(beastAttack.Attack(curveHeight, accelerationFactor, enemy));
    }
}

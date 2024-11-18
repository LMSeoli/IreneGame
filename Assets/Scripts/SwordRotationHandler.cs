using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordRotationHandler : MonoBehaviour
{
    public GameObject Player;
    public GameObject sword;
    public TrailRenderer TrailEffect;

    Animator anim;
    Animator playerAnim;
    SpriteRenderer spriteRenderer;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        playerAnim = Player.GetComponent<Animator>();
        spriteRenderer = sword.GetComponent<SpriteRenderer>();
    }

    //인스펙터 상에서 포지션을 잡을 때, 해당 포지션의 0,0,0기준은 상위 오브젝트지만, 프로그램 상에서 포지션을 잡을 때 0,0,0기준은 프로젝트의 0,0,0이기 때문에, 항상 플레이어의 위치도 입력해야 한다.
    public void NormalSlashStart(bool dir)
    {
        //if (playerAnim.GetInteger("CSCount") == 0) anim.SetInteger("CSCount", 0);
        spriteRenderer.enabled = true;
        spriteRenderer.flipY = !dir;
        if (dir) anim.SetBool("isLeftVerticalSlash", true);
        else anim.SetBool("isRightVerticalSlash", true);
        TrailEffect.enabled = true;
    }
    public void NormalSlashEnd()
    {
        playerAnim.SetBool("NormalSlash", false);
        anim.SetBool("isLeftVerticalSlash", false);
        anim.SetBool("isRightVerticalSlash", false);
        TrailEffect.enabled = false;
        spriteRenderer.enabled = false;
    }

    public void HorizontalSlashStart(bool dir, bool up)
    {
        spriteRenderer.enabled = true;
        //spriteRenderer.flipY = !dir;
        if (up) anim.SetBool("isUpSlash", true);
        else anim.SetBool("isDownSlash", true);
        TrailEffect.enabled = true;
    }

    public void HorizontalSlashEnd()
    {
        playerAnim.SetBool("NormalSlash", false);
        anim.SetBool("isUpSlash", false);
        anim.SetBool("isDownSlash", false);
        TrailEffect.enabled = false;
        spriteRenderer.enabled = false;
    }

    public void ComboSlash(bool dir, int count)
    {
        spriteRenderer.enabled = true;
        spriteRenderer.flipY = !dir;
        TrailEffect.enabled = true;
        anim.SetInteger("CSCount", count);
    }

    public void SwordOff()
    {
        TrailEffect.enabled = false;
        spriteRenderer.enabled = false;
    }
}

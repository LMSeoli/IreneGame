using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.SceneView;

//컷씬 전용 코드. 타임스케일을 무시하는 코드가 있으므로, 정지화면을 불러올 때 코드 자체가 정지해야 함.
public class CutsceneManager : MonoBehaviour
{
    public Image ultImage; // 지나가는 이미지
    public float speed = 200f;  // 이동 속도
    public Vector2 startPosition = new Vector2(0, 0); // 초기 위치 (캔버스 중앙)
    public Vector2 targetPosition = new Vector2(0, 200); // 목표 위치 (예: 위쪽으로 이동)
    private RectTransform ultImageRectTransform;
    private CanvasGroup canvasGroup;

    public RectTransform maskRectTransform; // 마스크의 RectTransform
    public float totalDuration = 0.9f;       // 전체 지속 시간
    public float maxWidth = 400f;            // 최대 너비, 테스트용은 400, 그 이미지는 600. inspecter상에서 바꿔야 하는 듯.

    private void Start()
    {
        ultImageRectTransform = ultImage.GetComponent<RectTransform>();
        canvasGroup = ultImage.GetComponent<CanvasGroup>();
        ultImageRectTransform.anchoredPosition = startPosition;
    }

    private void Update()
    {
       
    }


    public IEnumerator PlayUlt()
    {
        //ultImageRectTransform.anchoredPosition += Vector2.up * speed * Time.unscaledDeltaTime;
        float elapsedTime = 0;
        while (elapsedTime < totalDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            // 첫 0.1초 동안 너비 증가
            if (elapsedTime < 0.05f)
            {
                float t = elapsedTime / 0.05f; // 0.1초 동안의 진행 비율
                maskRectTransform.sizeDelta = new Vector2(Mathf.Lerp(0, maxWidth, t), maskRectTransform.sizeDelta.y);
            }
            // 중간 0.7초 동안 최대 너비 유지
            else if (elapsedTime < totalDuration - 0.05f)
            {
                maskRectTransform.sizeDelta = new Vector2(maxWidth, maskRectTransform.sizeDelta.y);
            }
            // 마지막 0.1초 동안 너비 감소
            else
            {
                float t = (elapsedTime - (totalDuration - 0.05f)) / 0.05f; // 마지막 0.1초 동안의 진행 비율
                maskRectTransform.sizeDelta = new Vector2(Mathf.Lerp(maxWidth, 0, t), maskRectTransform.sizeDelta.y);
            }
            yield return null;
        }
    }
}

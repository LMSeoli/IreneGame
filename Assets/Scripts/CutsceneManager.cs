using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.SceneView;

//�ƾ� ���� �ڵ�. Ÿ�ӽ������� �����ϴ� �ڵ尡 �����Ƿ�, ����ȭ���� �ҷ��� �� �ڵ� ��ü�� �����ؾ� ��.
public class CutsceneManager : MonoBehaviour
{
    public Image ultImage; // �������� �̹���
    public float speed = 200f;  // �̵� �ӵ�
    public Vector2 startPosition = new Vector2(0, 0); // �ʱ� ��ġ (ĵ���� �߾�)
    public Vector2 targetPosition = new Vector2(0, 200); // ��ǥ ��ġ (��: �������� �̵�)
    private RectTransform ultImageRectTransform;
    private CanvasGroup canvasGroup;

    public RectTransform maskRectTransform; // ����ũ�� RectTransform
    public float totalDuration = 0.9f;       // ��ü ���� �ð�
    public float maxWidth = 400f;            // �ִ� �ʺ�, �׽�Ʈ���� 400, �� �̹����� 600. inspecter�󿡼� �ٲ�� �ϴ� ��.

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

            // ù 0.1�� ���� �ʺ� ����
            if (elapsedTime < 0.05f)
            {
                float t = elapsedTime / 0.05f; // 0.1�� ������ ���� ����
                maskRectTransform.sizeDelta = new Vector2(Mathf.Lerp(0, maxWidth, t), maskRectTransform.sizeDelta.y);
            }
            // �߰� 0.7�� ���� �ִ� �ʺ� ����
            else if (elapsedTime < totalDuration - 0.05f)
            {
                maskRectTransform.sizeDelta = new Vector2(maxWidth, maskRectTransform.sizeDelta.y);
            }
            // ������ 0.1�� ���� �ʺ� ����
            else
            {
                float t = (elapsedTime - (totalDuration - 0.05f)) / 0.05f; // ������ 0.1�� ������ ���� ����
                maskRectTransform.sizeDelta = new Vector2(Mathf.Lerp(maxWidth, 0, t), maskRectTransform.sizeDelta.y);
            }
            yield return null;
        }
    }
}

using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using static UnityEditor.SceneView;

public class CameraMove : MonoBehaviour
{
    public Collider2D[] StageFrame;
    public Transform cameraTarget; // CameraTarget ������Ʈ ����
    public Transform originalCameraTargetPosition; // ���� CameraTarget ��ġ (�÷��̾�)
    public float cameraSmoothSpeed = 6f; // ī�޶� �̵� �ӵ�
    public Coroutine cameraCoroutine;

    CinemachineConfiner2D cinemachineConfiner2D;

    private void Awake()
    {
        cinemachineConfiner2D = GetComponent<CinemachineConfiner2D>();
    }

    public void FrameChange(int index)
    {
        cinemachineConfiner2D.m_BoundingShape2D = StageFrame[index];
    }
    public void StartJungSangHwa()
    {
        // ���� ���� �ڷ�ƾ�� ������ ����
        if (cameraCoroutine != null)
        {
            StopCoroutine(cameraCoroutine);
        }
        // ���ο� JungSangHwa �ڷ�ƾ ���� �� ���� ����
        cameraCoroutine = StartCoroutine(JungSangHwa());
    }
    public IEnumerator JungSangHwa()
    {
        while (Vector3.Distance(cameraTarget.position, originalCameraTargetPosition.position) > 0.01f)      //���� �Ӱ谪
        {
            cameraTarget.position = Vector3.Lerp(cameraTarget.position, originalCameraTargetPosition.position, cameraSmoothSpeed * Time.deltaTime);
            yield return null;
        }
        cameraTarget.position = originalCameraTargetPosition.position;
    }
    public void StartMiddleCamera(Vector3 targetPos)
    {
        // ���� ���� �ڷ�ƾ�� ������ ����
        if (cameraCoroutine != null)
        {
            StopCoroutine(cameraCoroutine);
        }
        // ���ο� middleCamera �ڷ�ƾ ���� �� ���� ����
        cameraCoroutine = StartCoroutine(middleCamera(targetPos));
    }
    public IEnumerator middleCamera(Vector3 targetPos)
    {
        while (true)
        {
            cameraTarget.position = Vector3.Lerp(cameraTarget.position, (targetPos + cameraTarget.transform.position) / 2, cameraSmoothSpeed * Time.deltaTime);
            yield return null;
        }
    }
}

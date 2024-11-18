using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using static UnityEditor.SceneView;

public class CameraMove : MonoBehaviour
{
    public Collider2D[] StageFrame;
    public Transform cameraTarget; // CameraTarget 오브젝트 참조
    public Transform originalCameraTargetPosition; // 원래 CameraTarget 위치 (플레이어)
    public float cameraSmoothSpeed = 6f; // 카메라 이동 속도
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
        // 실행 중인 코루틴이 있으면 중지
        if (cameraCoroutine != null)
        {
            StopCoroutine(cameraCoroutine);
        }
        // 새로운 JungSangHwa 코루틴 시작 및 참조 저장
        cameraCoroutine = StartCoroutine(JungSangHwa());
    }
    public IEnumerator JungSangHwa()
    {
        while (Vector3.Distance(cameraTarget.position, originalCameraTargetPosition.position) > 0.01f)      //작은 임계값
        {
            cameraTarget.position = Vector3.Lerp(cameraTarget.position, originalCameraTargetPosition.position, cameraSmoothSpeed * Time.deltaTime);
            yield return null;
        }
        cameraTarget.position = originalCameraTargetPosition.position;
    }
    public void StartMiddleCamera(Vector3 targetPos)
    {
        // 실행 중인 코루틴이 있으면 중지
        if (cameraCoroutine != null)
        {
            StopCoroutine(cameraCoroutine);
        }
        // 새로운 middleCamera 코루틴 시작 및 참조 저장
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

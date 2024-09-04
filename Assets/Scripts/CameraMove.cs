using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public Collider2D[] StageFrame;

    CinemachineConfiner2D cinemachineConfiner2D;

    private void Awake()
    {
        cinemachineConfiner2D = GetComponent<CinemachineConfiner2D>();
    }

    public void FrameChange(int index)
    {
        cinemachineConfiner2D.m_BoundingShape2D = StageFrame[index];
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UIElements;

public class CameraRotation : MonoBehaviour
{
    [SerializeField] private CinemachineFreeLook freeLookCam;
    private Camera cam;
    private Vector3 previousPosition;

    private void Start()
    {
        cam = Camera.main;
    }
    // Update is called once per frame
    void Update()
    {
        RotateCamera();
    }

    public void RotateCamera()
    {
        if (Input.GetMouseButtonDown(2))
        {
           previousPosition = cam.ScreenToViewportPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(2))
        {
            Vector3 mousePos = previousPosition - cam.ScreenToViewportPoint(Input.mousePosition);

            freeLookCam.m_XAxis.Value -= mousePos.x * 180;
            freeLookCam.m_YAxis.Value += mousePos.y;

            previousPosition = cam.ScreenToViewportPoint(Input.mousePosition);
        }
    }

}

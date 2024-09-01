using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ClickOnObjects : MonoBehaviour
{
    private Camera mainCamera;

    private Ray ray;
    private RaycastHit hit;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray, out hit, 100f)) 
            {
                Debug.Log("Click");
                Debug.Log(hit.collider.gameObject.transform.position);

                EventManager.TriggerEvent("MoveTo", new int2((int)(hit.collider.gameObject.transform.position.x / 2), (int)(hit.collider.gameObject.transform.position.z / 2)));
            }
        }
    }
}

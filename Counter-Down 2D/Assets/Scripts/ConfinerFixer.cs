using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

/// <summary>
/// a weird bug of cinemachine confiner2d as it expected, this script is a fix for it
/// </summary>
public class ConfinerFixer : MonoBehaviour
{
    private CinemachineConfiner2D cinemachineConfiner2D;
    void Awake()
    {
        var cinemachine = GetComponent<CinemachineVirtualCamera>();
        cinemachineConfiner2D = cinemachine.GetComponent<CinemachineConfiner2D>();
        if (cinemachineConfiner2D != null)
        {
            cinemachineConfiner2D.enabled = false;
            Invoke("enableconfiner", .75f);
        }
    }

    void enableconfiner()
    {
        cinemachineConfiner2D.enabled = true;
        Destroy(this);
    }
}

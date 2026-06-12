using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformManager : MonoBehaviour
{
    [SerializeField] private bool isMobileTest;
    public static PlatformManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public bool IsMobile()
    {
        return isMobileTest || SystemInfo.deviceType == DeviceType.Handheld;
    }
}

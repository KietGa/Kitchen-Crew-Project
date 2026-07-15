using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileUI : MonoBehaviour
{
    private void Start()
    {
        ShowMobileControl();
    }

    private void ShowMobileControl()
    {
        gameObject.SetActive(PlatformManager.Instance.IsMobile());
    }
}

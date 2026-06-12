using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileUI : MonoBehaviour
{
    private void Start()
    {
        gameObject.SetActive(PlatformManager.Instance.IsMobile());
    }
}

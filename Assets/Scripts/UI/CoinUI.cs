using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CoinUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI totalCoinText;

    private void Start()
    {
        DeliveryManager.Instance.OnCoinChanged += DeliveryManager_OnCoinChanged;
    }

    private void DeliveryManager_OnCoinChanged(object sender, DeliveryManager.OnCoinChangedEventArgs e)
    {
        totalCoinText.text = e.totalCoin.ToString();
    }
}

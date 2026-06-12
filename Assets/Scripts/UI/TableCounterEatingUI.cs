using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableCounterEatingUI : MonoBehaviour
{
    [SerializeField] private TableCounter tableCounter;

    private void Start()
    {
        tableCounter.OnRecipeCompleted += TableCounter_OnRecipeCompleted;
        tableCounter.OnCustomerLeaving += TableCounter_OnCustomerLeaving;
        Hide();
    }

    private void TableCounter_OnCustomerLeaving(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void TableCounter_OnRecipeCompleted(object sender, System.EventArgs e)
    {
        Show();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}

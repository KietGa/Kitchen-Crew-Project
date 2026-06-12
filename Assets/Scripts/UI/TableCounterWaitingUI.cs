using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableCounterWaitingUI : MonoBehaviour
{
    [SerializeField] private TableCounter tableCounter;

    private void Start()
    {
        tableCounter.OnCustomerThinking += TableCounter_OnCustomerThinking;
        tableCounter.OnRecipeSpawned += TableCounter_OnRecipeSpawned;

        Hide();
    }

    private void TableCounter_OnRecipeSpawned(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void TableCounter_OnCustomerThinking(object sender, System.EventArgs e)
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

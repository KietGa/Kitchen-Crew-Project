using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TableCounterDeliveryUI : MonoBehaviour
{
    [SerializeField] private TableCounter tableCounter;
    [SerializeField] private Transform recipeTransform;
    [SerializeField] private Image timeImage;

    private void Start()
    {
        tableCounter.OnRecipeSpawned += TableCounter_OnRecipeSpawned;
        tableCounter.OnRecipeCompleted += TableCounter_OnRecipeCompleted;
        tableCounter.OnRecipeFailed += TableCounter_OnRecipeCompleted;
        tableCounter.OnCustomerLeaving += TableCounter_OnCustomerLeaving;
        tableCounter.OnCustomerWaiting += TableCounter_OnWaitingRecipe;
        recipeTransform.gameObject.SetActive(false);
    }

    private void TableCounter_OnCustomerLeaving(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void TableCounter_OnWaitingRecipe(object sender, System.EventArgs e)
    {
        float value = tableCounter.GetDeliveryTimeNormalized();
        timeImage.fillAmount = value;

        if (value > 0.6f)
        {
            timeImage.color = Color.green;
        }
        else if (value > 0.2f)
        {
            timeImage.color = new Color(1, 200f / 255f, 0, 1f);
        }
        else
        {
            timeImage.color = Color.red;
        }
    }

    private void TableCounter_OnRecipeCompleted(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void TableCounter_OnRecipeSpawned(object sender, System.EventArgs e)
    {
        Show();
    }

    private void Show()
    {
        recipeTransform.GetComponent<DeliveryManagerSingleUI>().SetRecipeSO(tableCounter.GetWaitingRecipeSO(), tableCounter.GetTableIndex());
        recipeTransform.gameObject.SetActive(true);
    }

    private void Hide()
    {
        recipeTransform.gameObject.SetActive(false);
    }
}

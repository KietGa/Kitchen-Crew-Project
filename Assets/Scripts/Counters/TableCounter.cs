using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TableCounter : BaseCounter
{
    [SerializeField] private Transform customerTrans;
    public event EventHandler OnCustomerThinking;
    public event EventHandler OnRecipeSpawned;
    public event EventHandler OnRecipeCompleted;
    public event EventHandler OnRecipeFailed;
    public event EventHandler OnCustomerWaiting;
    public event EventHandler OnCustomerLeaving;
    private RecipeSO waitingRecipeSO;
    private int tableIndex;
    private float timeCounter;
    private float waitingTime;
    private Customer customer;

    private void Update()
    {
        if (waitingRecipeSO != null)
        {
            timeCounter -= Time.deltaTime;
            OnCustomerWaiting?.Invoke(this, EventArgs.Empty);
        }
    }

    public override void Interact(Player player)
    {
        if (player.HasKitchenObject())
        {
            if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
            {
                DeliverRecipe(plateKitchenObject);
                player.GetKitchenObject().SetKitchenObjectParent(this);
            }
        }
    }

    public Vector3 GetCustomerPos()
    {
        return customerTrans.position;
    }

    public void SetWaitingRecipeSO(RecipeSO recipeSO, float waitingTime)
    {
        this.waitingTime = waitingTime;
        timeCounter = waitingTime;
        waitingRecipeSO = recipeSO;
        OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
    }

    public void SetCustomer(Customer customer)
    {
        this.customer = customer;
    }

    public RecipeSO GetWaitingRecipeSO()
    {
        return waitingRecipeSO;
    }

    public void SetTableIndex(int tableIndex)
    {
        this.tableIndex = tableIndex;
    }

    public int GetTableIndex()
    {
        return tableIndex;
    }
    public float GetDeliveryTimeNormalized()
    {
        return timeCounter / waitingTime;
    }

    [ClientRpc]
    public void CustomerThinkingClientRpc()
    {
        CustomerThinking();
    }

    private void CustomerThinking()
    {
        OnCustomerThinking?.Invoke(this, EventArgs.Empty);
    }

    public void CustomerLeaving()
    {
        CustomerLeavingClientRpc();
    }

    [ClientRpc]
    private void CustomerLeavingClientRpc()
    {
        OnCustomerLeaving?.Invoke(this, EventArgs.Empty);
        customer = null;
    }

    public void CustomerFailedLeaving()
    {
        DeliveryManager.Instance.DeliverRecipe(DeliveryManager.Instance.GetIndexTableCounter(this), 0);
        CustomerFailedLeavingClientRpc();
    }

    [ClientRpc]
    public void CustomerFailedLeavingClientRpc()
    {
        OnRecipeFailed?.Invoke(this, EventArgs.Empty);
        OnCustomerLeaving?.Invoke(this, EventArgs.Empty);
        customer = null;
        waitingRecipeSO = null;
    }

    private void DeliverRecipe(PlateKitchenObject plateKitchenObject)
    {
        if (waitingRecipeSO.kitchenObjectSOList.Count == plateKitchenObject.GetKitchenObjectSOList().Count)
        {
            // Has the same number of ingredients
            bool plateContentsMatchesRecipe = true;
            foreach (KitchenObjectSO recipeKitchenObjectSO in waitingRecipeSO.kitchenObjectSOList)
            {
                // Cycling through all ingredients in the Recipe
                bool ingredientFound = false;
                foreach (KitchenObjectSO plateKitchenObjectSO in plateKitchenObject.GetKitchenObjectSOList())
                {
                    // Cycling through all ingredients in the Plate
                    if (plateKitchenObjectSO == recipeKitchenObjectSO)
                    {
                        // Ingredient matches!
                        ingredientFound = true;
                        break;
                    }
                }
                if (!ingredientFound)
                {
                    // This Recipe ingredient was not found on the Plate
                    plateContentsMatchesRecipe = false;
                }
            }

            if (plateContentsMatchesRecipe)
            {
                // Player delivered the correct recipe!

                DeliveryManager.Instance.DeliverRecipe(DeliveryManager.Instance.GetIndexTableCounter(this), waitingRecipeSO.price);
                CustomerEatingServerRpc();
                return;
            }

            DeliveryFailedServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CustomerEatingServerRpc()
    {
        customer.Eating();
        DeliveryCompleteClientRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeliveryFailedServerRpc() 
    {
        CustomerFailedLeaving();
    }

    [ClientRpc]
    private void DeliveryCompleteClientRpc()
    {
        //DeliveryManager.Instance.DeliverySuccess(DeliveryManager.Instance.GetIndexTableCounter(this));
        OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
        waitingRecipeSO = null;
    }
}

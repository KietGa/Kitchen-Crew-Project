using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DeliveryManager : NetworkBehaviour {


    public event EventHandler OnRecipeSpawned;
    public event EventHandler OnRecipeCompleted;
    public event EventHandler OnRecipeSuccess;
    public event EventHandler OnRecipeFailed;
    public event EventHandler<OnCoinChangedEventArgs> OnCoinChanged;
    public class OnCoinChangedEventArgs : EventArgs
    {
        public int totalCoin;
    }

    public static DeliveryManager Instance { get; private set; }


    [SerializeField] private RecipeListSO recipeListSO;
    [SerializeField] private List<TableCounter> tableCounterList;
    [SerializeField] private List<TableCounter> emptyTableCounterList;

    private List<RecipeSO> waitingRecipeSOList;
    private float spawnRecipeTimer = 4f;
    private float spawnRecipeTimerMax = 4f;
    private int waitingRecipesMax = 4;
    private int successfulRecipesAmount;
    private int totalCoin;

    private void Awake() {
        Instance = this;

        for (int i = 0; i < tableCounterList.Count; i++)
        {
            tableCounterList[i].SetTableIndex(i);
        }

        waitingRecipeSOList = new List<RecipeSO>();
        emptyTableCounterList = new List<TableCounter>(tableCounterList);
    }

    private void Update() {
        if (!IsServer) {
            return;
        }

        /*
        spawnRecipeTimer -= Time.deltaTime;
        if (spawnRecipeTimer <= 0f) {
            spawnRecipeTimer = spawnRecipeTimerMax;

            if (KitchenGameManager.Instance.IsGamePlaying() && waitingRecipeSOList.Count < waitingRecipesMax) {
                int waitingRecipeSOIndex = UnityEngine.Random.Range(0, recipeListSO.recipeSOList.Count);

                SpawnNewWaitingRecipeClientRpc(waitingRecipeSOIndex);
            }
        }
        */
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddEmptyTableServerRpc(int tableCounterIndex)
    {
        CustomerSpawner.Instance.CustomerOut();
        emptyTableCounterList.Add(GetTableCounterByIndex(tableCounterIndex));
    }

    private void AddCoin(int amount)
    {
        totalCoin += amount;
        OnCoinChanged?.Invoke(this, new OnCoinChangedEventArgs
        {
            totalCoin = totalCoin
        });
    }

    public void DeliverRecipe(int tableCounterIndex, int amount)
    {
        AddEmptyTableServerRpc(tableCounterIndex);
        DeliverRecipeServerRpc(tableCounterIndex, amount);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeliverRecipeServerRpc(int tableCounterIndex, int amount)
    {
        DeliverRecipeClientRpc(tableCounterIndex, amount);
    }

    [ClientRpc]
    private void DeliverRecipeClientRpc(int tableCounterIndex, int amount)
    {
        AddCoin(amount);

        if (amount != 0)
        {
            OnRecipeSuccess?.Invoke(GetTableCounterByIndex(tableCounterIndex), EventArgs.Empty);
        }
        else
        {
            OnRecipeFailed?.Invoke(GetTableCounterByIndex(tableCounterIndex), EventArgs.Empty);
        }
    }

    public int GetRandomTableIndex() => UnityEngine.Random.Range(0, emptyTableCounterList.Count);

    public TableCounter GetEmptyTableByIndex(int index)
    {
        int randomTableIndex = index;
        TableCounter tableCounter = emptyTableCounterList[randomTableIndex];
        emptyTableCounterList.Remove(tableCounter);
        return tableCounter;
    }

    public int GetIndexTableCounter(TableCounter currentTable)
    {
        for (int i = 0; i < tableCounterList.Count; i++) 
        {
            if (tableCounterList[i] == currentTable) return i;
        }

        return -1;
    }

    public TableCounter GetTableCounterByIndex(int index)
    {
        return tableCounterList[index];
    }

    public int GetRandomRecipeIndex => UnityEngine.Random.Range(0, recipeListSO.recipeSOList.Count);

    [ClientRpc]
    public void SpawnRecipeClientRpc(int tableIndex, float waitingTime, int recipeIndex)
    {
        TableCounter tableCounter = GetTableCounterByIndex(tableIndex);
        RecipeSO waitingRecipeSO = recipeListSO.recipeSOList[recipeIndex];
        tableCounter.SetWaitingRecipeSO(waitingRecipeSO, waitingTime);
        OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
    }

    [ClientRpc]
    private void SpawnNewWaitingRecipeClientRpc(int waitingRecipeSOIndex) {
        RecipeSO waitingRecipeSO = recipeListSO.recipeSOList[waitingRecipeSOIndex];

        waitingRecipeSOList.Add(waitingRecipeSO);

        OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
    }

    public void DeliverRecipe(PlateKitchenObject plateKitchenObject) {
        for (int i = 0; i < waitingRecipeSOList.Count; i++) {
            RecipeSO waitingRecipeSO = waitingRecipeSOList[i];

            if (waitingRecipeSO.kitchenObjectSOList.Count == plateKitchenObject.GetKitchenObjectSOList().Count) {
                // Has the same number of ingredients
                bool plateContentsMatchesRecipe = true;
                foreach (KitchenObjectSO recipeKitchenObjectSO in waitingRecipeSO.kitchenObjectSOList) {
                    // Cycling through all ingredients in the Recipe
                    bool ingredientFound = false;
                    foreach (KitchenObjectSO plateKitchenObjectSO in plateKitchenObject.GetKitchenObjectSOList()) {
                        // Cycling through all ingredients in the Plate
                        if (plateKitchenObjectSO == recipeKitchenObjectSO) {
                            // Ingredient matches!
                            ingredientFound = true;
                            break;
                        }
                    }
                    if (!ingredientFound) {
                        // This Recipe ingredient was not found on the Plate
                        plateContentsMatchesRecipe = false;
                    }
                }

                if (plateContentsMatchesRecipe) {
                    // Player delivered the correct recipe!
                    DeliverCorrectRecipeServerRpc(i);
                    return;
                }
            }
        }

        // No matches found!
        // Player did not deliver a correct recipe
        DeliverIncorrectRecipeServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeliverIncorrectRecipeServerRpc() {
        DeliverIncorrectRecipeClientRpc();
    }

    [ClientRpc]
    private void DeliverIncorrectRecipeClientRpc() {
        OnRecipeFailed?.Invoke(this, EventArgs.Empty);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeliverCorrectRecipeServerRpc(int waitingRecipeSOListIndex) {
        DeliverCorrectRecipeClientRpc(waitingRecipeSOListIndex);
    }

    [ClientRpc]
    private void DeliverCorrectRecipeClientRpc(int waitingRecipeSOListIndex) {
        successfulRecipesAmount++;

        waitingRecipeSOList.RemoveAt(waitingRecipeSOListIndex);

        OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
        OnRecipeSuccess?.Invoke(this, EventArgs.Empty);
    }


    public List<RecipeSO> GetWaitingRecipeSOList() {
        return waitingRecipeSOList;
    }

    public int GetSuccessfulRecipesAmount() {
        return successfulRecipesAmount;
    }

    public int GetTotalCoin()
    {
        return totalCoin;
    }
}

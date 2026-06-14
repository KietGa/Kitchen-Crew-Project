using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class Customer : NetworkBehaviour
{
    private enum State
    {
        Going,
        Thinking,
        Waiting,
        Eating,
        Leaving
    }

    private State state;
    private NavMeshAgent agent;
    private TableCounter tableCounter;
    private float orderTimeMax;
    [SerializeField] private float waitingTimeMax = 30f;
    [SerializeField] private float rotationSpeed = 100f;
    private float timeCounter;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void SetUp(int counterIndex, float orderTime)
    {
        state = State.Going;
        tableCounter = DeliveryManager.Instance.GetEmptyTableByIndex(counterIndex);
        tableCounter.SetCustomer(this);
        agent.SetDestination(tableCounter.GetCustomerPos());
        orderTimeMax = orderTime;
    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }


        switch (state)
        {
            case State.Going:
                if (agent.remainingDistance <= 0.1f)
                {
                    state = State.Thinking;
                    timeCounter = orderTimeMax;
                    tableCounter.CustomerThinkingClientRpc();
                }

                break;

            
            case State.Thinking:
                timeCounter -= Time.deltaTime;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 90, 0), rotationSpeed * Time.deltaTime);

                if (timeCounter < 0)
                {
                    state = State.Waiting;
                    timeCounter = waitingTimeMax;

                    if (KitchenGameManager.Instance.IsGamePlaying())
                    {
                        DeliveryManager.Instance.SpawnRecipeClientRpc(
                            DeliveryManager.Instance.GetIndexTableCounter(tableCounter), 
                            waitingTimeMax, DeliveryManager.Instance.GetRandomRecipeIndex);
                    }
                }

                break;

                
            case State.Waiting:
                timeCounter -= Time.deltaTime;

                if (timeCounter < 0)
                {
                    LeavingFailed();
                }

                break;

            case State.Eating:
                timeCounter -= Time.deltaTime;

                if (timeCounter < 0)
                {
                    LeavingSuccess();
                }
                break;

            case State.Leaving:
                if (agent.remainingDistance <= 0.1f)
                {
                    DestroySelf();
                }
                break;
        }
    }

    [ClientRpc]
    private void CustomerLeaveClientRpc()
    {
        agent.SetDestination(CustomerSpawner.Instance.GetPosition());
    }

    public void Eating()
    {
        state = State.Eating;
        timeCounter = Random.Range(3f, 5f);
    }

    private void DestroySelf()
    {
        if (IsServer)
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }

    public void LeavingFailed()
    {
        state = State.Leaving;
        CustomerLeaveClientRpc();
        tableCounter.CustomerFailedLeaving();
    }

    private void LeavingSuccess()
    {
        state = State.Leaving;
        CustomerLeaveClientRpc();
        tableCounter.CustomerSuccessLeaving();
    }
}

using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CustomerSpawner : NetworkBehaviour
{
    public static CustomerSpawner Instance { get; private set; }
    [SerializeField] private Customer customerPrefab;
    [SerializeField] private float customerSpawnTimerMax = 4f;
    [SerializeField] private int customerMax = 4;

    private float customerSpawnTimer;
    private int count;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }

        customerSpawnTimer -= Time.deltaTime;
        if (customerSpawnTimer <= 0f)
        {
            customerSpawnTimer = customerSpawnTimerMax;

            if (KitchenGameManager.Instance.IsGamePlaying() && count < customerMax)
            {
                count++;
                SpawnCustomer(DeliveryManager.Instance.GetRandomTableIndex(), Random.Range(3f,5f));
            }
        }
    }

    private void SpawnCustomer(int index, float orderTime)
    {
        Customer customer = Instantiate(customerPrefab, transform.position, transform.rotation);
        customer.SetUp(index, orderTime);

        NetworkObject customerNetwork = customer.GetComponent<NetworkObject>();
        customerNetwork.Spawn(true);
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public void CustomerOut()
    {
        count--;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerVisual : MonoBehaviour
{
    [SerializeField] private MeshRenderer headMeshRenderer;
    [SerializeField] private MeshRenderer bodyMeshRenderer;

    private Material material;

    private void Awake()
    {
        SetMaterial();
        RandomColor();
    }

    private void SetMaterial()
    {
        material = new Material(headMeshRenderer.material);
        headMeshRenderer.material = material;
        bodyMeshRenderer.material = material;
    }

    private void RandomColor()
    {
        material.color = KitchenGameMultiplayer.Instance.GetRandomColor();
    }
}

using UnityEngine;

public class RotateSkybox : MonoBehaviour
{
    [SerializeField] private float speed = 1f;
    private float rotation;

    void Update()
    {
        rotation += Time.deltaTime * speed;

        RenderSettings.skybox.SetFloat(
            "_Rotation",
            rotation
        );
    }
}
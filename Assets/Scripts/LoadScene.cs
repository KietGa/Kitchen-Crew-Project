using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadScene : MonoBehaviour
{
    [SerializeField] private Image bgImage;
    [SerializeField] private Sprite[] sprites;

    void Start()
    {
        bgImage.sprite = sprites[Random.Range(0, sprites.Length)];
    }
}

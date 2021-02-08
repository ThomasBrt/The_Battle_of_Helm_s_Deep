using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tb_WaveBonus : MonoBehaviour
{
    //On reprend la logique vue dans ArrowPlayer
    //On définit la vitesse et le temps avant destruction
    public float Force = 150, DestroyTime = 2f;
    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        rb.AddForce(Vector2.up * Force);
        Destroy(gameObject, DestroyTime);
    }
}

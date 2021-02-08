using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tb_ArrowPlayer : MonoBehaviour
{
    //On définit la vitesse et le temps avant destruction
    public float Force = 600f, DestroyTime = 1f;
    Rigidbody2D rb;
    //On définit les gameobject à instancier après la mort d'un orc
    public GameObject OrcDeath;
    //public GameObject OrcBonus;

    private tb_PlayerController playerController;
    private GameManager gameManager;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<tb_PlayerController>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    void Start()
    {
        rb.AddForce(Vector2.down * Force);
        Destroy(gameObject, DestroyTime);
    }

    private void OnDestroy()
    {
        playerController.CanShoot = true;
    }

    //A la collision avec un orc, on détruit l'orc pour instancier soit une explosion, soit un bonus
    void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Orc"))
        {
            Destroy(collision.gameObject);
            Instantiate(OrcDeath, collision.transform.position, Quaternion.identity);
            gameManager.Score += 50;
            Destroy(this.gameObject);
        }
    }
}

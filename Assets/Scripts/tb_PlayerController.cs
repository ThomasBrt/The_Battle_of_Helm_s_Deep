using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class tb_PlayerController : MonoBehaviour
{
    //Déclaration des variables
    Vector2 PositionPlayer;
    float speed = 5f;
    float limitx = 2.5f; //Limite en x de la scène, vaut à 2.5f car l'écran est en 16:9 portrait ici
    public GameObject arrowPlayer;
    Transform ejectPosition; 

    public bool CanShoot = true;  //Détermine si le player peut tirer
    public bool CanShootBonus = false;
    tb_Wave WaveScript;
    GameManager gameManagerScript;
    bool detect = true; //Détecter la 1ere collision avec le player

    //Gestion des tirs d'orcs
    public GameObject SpearOrc;
    bool orcCanShoot = true;
    int layerDefault; //Permet de gérer l'animation de mort du player
    public float OrcShootRate = 3f;

    void Start()
    {
        //On récupère la position du player pour la modifier plus tard
        PositionPlayer = transform.position;
        //On récupère notre objet Eject, qui servira à déterminer le point d'apparition de la flêche du player
        ejectPosition = transform.Find("Eject");
        WaveScript = GameObject.Find("Wave").GetComponent<tb_Wave>();
        gameManagerScript = GameObject.Find("GameManager").GetComponent<GameManager>();
        layerDefault = LayerMask.GetMask("Default");
    }

    void Update()
    {
        //On regarde s'il faut déplacer et/ou faire tirer le player
        tb_MovePlayer();
        tb_PlayerShoot();
        //On regarde s'il faut faire tirer les orcs
        tb_OrcShoot();
    }

    /****************************************  Mouvements et tirs du player  ***********************************/

    void tb_MovePlayer()
    {
            PositionPlayer.x += Input.GetAxis("Horizontal") * Time.deltaTime * speed;
            //La position en x du player ne doit pas sortir de l'écran
            PositionPlayer.x = Mathf.Clamp(PositionPlayer.x, -limitx, limitx);
            transform.position = PositionPlayer;
    }

    void tb_PlayerShoot()
    {
        if(Input.GetKeyDown(KeyCode.Space) && CanShoot)
        {
            CanShoot = false;
            Instantiate(arrowPlayer, ejectPosition.position, Quaternion.identity);
            if(CanShootBonus){
                CanShootBonus = false;
                //On fait apparaître 2 flêches supp à gauche et à droite de l'écran au prochain tir
                Instantiate(arrowPlayer, new Vector3(-1.5f, 2.5f, 1), Quaternion.identity);
                Instantiate(arrowPlayer, new Vector3(1.5f, 2.5f, 1), Quaternion.identity);
            }
        }
    }


    /****************************************  Collision, mort et rénitialisation du player  ***********************************/

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //A la collision entre la vague OU un tir d'orc avec le player 
        if(collision.CompareTag("Orc") && detect || collision.CompareTag("SpearOrc"))
        {
            detect = false;
            StartCoroutine(tb_OrcKillPlayer());
        }
        //A la collision entre le bonus et le player
        if(collision.gameObject.CompareTag("Bonus"))
        {
            Destroy(collision.gameObject);
            //On fait gagner 30 pts bonus au joueur
            gameManagerScript.Score += 30;
            //On lui permet d'utiliser un tir bonus au prochain tir
            CanShootBonus = true;
        }
    }

    IEnumerator tb_OrcKillPlayer()
    {
        //On stoppe la vague
        WaveScript.tb_StopWave();
        //Et on tue le player
        GetComponent<Animator>().SetTrigger("normal");

        tb_PlayerDeath();
        //On enlève une vie
        gameManagerScript.Lives -= 1;
        if(gameManagerScript.Lives == 0)
        {
            GameManager.state = GameManager.States.wait;
            gameManagerScript.tb_GameOver();
        }
        else
        {
            //La détection de collision est à nouveau dispo
            yield return new WaitForSeconds(0.2f);
            detect = true;
            //Puis on redémarre la vague
            WaveScript.tb_RestartWave(1f);
        }
    }

    public void tb_PlayerDeath()
    {
        GetComponent<Animator>().SetTrigger("death");
        CanShoot = false;
    }

    //Réinitialisation pour une nouvelle partie
    public void tb_ReinitPlayer()
    {
       CanShoot = true;
       tb_MovePlayer();
    }

    //Initialisation à chaque mort du player
    public void tb_InitPlayer()
    {
        //Suite à l'animation de death, on réaffiche le sprite normal
        GetComponent<Animator>().SetTrigger("normal");
        //On réautorise le tir
        CanShoot = true;
    }

    /****************************************  Tir d'orc vers le player ***********************************/

    void tb_OrcShoot()
    {
        //Rayon détection d'un orc en bas du player
        Debug.DrawRay(transform.position, Vector2.down * 5);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, Mathf.Infinity, layerDefault);
        //Si un collider est touché par le rayon
        if(hit.collider != null)
        {
            if(hit.collider.CompareTag("Orc") && orcCanShoot)
            {
                StartCoroutine(tb_PauseOrcShoot());
                Instantiate(SpearOrc, hit.point, Quaternion.identity);
            }
        }
    }

    IEnumerator tb_PauseOrcShoot()
    {
        orcCanShoot = false;
        yield return new WaitForSeconds(OrcShootRate);
        orcCanShoot = true;
    }
}

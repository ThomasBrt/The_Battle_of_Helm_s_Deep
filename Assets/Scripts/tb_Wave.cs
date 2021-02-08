using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tb_Wave : MonoBehaviour
{
    //Déclaration de nos orcs définis par nos prefabs dans Unity
    public GameObject OrcSoldier;
    public GameObject OrcChief;

    //Propriétés des vagues d'orcs
    public float SpaceColumns = 2f, SpaceRows = 2f;
    public int TotalOrcInline = 2;
    public bool Walkright = true, Walkup = false;
    public float WaveStepRight = 1f, WaveStepUp = 2f; // tb_WaveSpeed = 1.2f;
    public bool WaveMoving = true;

    //Gestion du nombre d'orcs
    public int TotalOrcInWave;
    private int remainingOrc;
    public int RemainingOrc{
        get{return remainingOrc;}
        set{remainingOrc = value;}
    }

    //Redémarrage de la vague SANS CHANGEMENT DE LEVEL mais à la collision avec le player!
    Vector2 InitialPositionWave;
    tb_PlayerController playerController;

    //Redémarrage de la vague après changement de level
    GameManager gameManager;

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public void tb_LoadWave(int TotalWaveRows)
    {
        GameManager.state = GameManager.States.wait;
        //Générateur de vague d'orcs
        for(int i = 0; i < TotalWaveRows; i++)
        {
           float posY = -4 + (SpaceRows * i); 

           for(int n = 0; n < TotalOrcInline; n++)
           {
               Vector2 pos = new Vector2(-2 + (SpaceColumns * n), posY);
               GameObject GoOrc = Instantiate(OrcSoldier, pos, Quaternion.identity);
               GoOrc.transform.SetParent(this.transform);
               GoOrc.name = "Orc_" + (n+1) + "_Row" + (i+1);
           }
        }

        //Assignation du nombre d'orcs
        //On prend le nombre d'enfants de l'objet tb_Wave
        TotalOrcInWave = transform.childCount;
        RemainingOrc = TotalOrcInWave;
        //Position initiale
        InitialPositionWave = transform.position;
        playerController = GameObject.Find("Player").GetComponent<tb_PlayerController>();
        //La vague doit avancer
        WaveMoving = true;
        GameManager.state = GameManager.States.play;
        tb_MoveWave(); 
    }
    
    public void tb_IsWaveEmpty()
    {
        //Si tous les orcs ont été détruits, on stoppe le niveau
        if(RemainingOrc == 0 )
        {
            tb_StopWave();
            if(GameManager.state == GameManager.States.levelup){
                tb_StopWave();
                gameManager.tb_EndOfLevel();
            }
        }
    }

    private void Update()
    {
        tb_MoveWave();
    }

    void tb_MoveWave()
    {
        //On vérifie que la vague doit pouvoir avancer
        if(WaveMoving && GameManager.state == GameManager.States.play)
        {
            //Si oui, soit la vague monte, soit elle se déplace de gauche à droite
            if(Walkup)
            {
                Vector2 directionUp = Vector2.up;
                transform.Translate(directionUp  * WaveStepUp * Time.deltaTime * gameManager.WaveSpeedForLevel);
            }
            else
            {
                Vector2 direction = Walkright ? Vector2.right : Vector2.left;
                transform.Translate(direction  * WaveStepRight * Time.deltaTime * gameManager.WaveSpeedForLevel);
            }
        }
    }

    public void tb_WaveTouchLimit()
    {
        //On inverse la direction gauche/droite
        Walkright = !Walkright;
        //On fait avancer (la vague n'ira pas gauche ou droite pour l'instant)
        Walkup = true;
        //On fait attendre avant de continuer vers la nouvelle direction
        StartCoroutine(tb_wait());
    }

    IEnumerator tb_wait()
    {
        yield return new WaitForSeconds(0.2f);
        Walkup = false;
    }

    //Stopper la vague quand elle touche le player
    public void tb_StopWave()
    {
        WaveMoving = false;
    }

    public void tb_RestartWave(float delay)
    {
        StartCoroutine(tb_Restart(delay));
    }

    IEnumerator tb_Restart(float delay)
    {
        GameManager.state = GameManager.States.wait;
        //On repositionne la vague à sa position initiale
        yield return new WaitForSeconds(delay);
        transform.position = InitialPositionWave;
        playerController.tb_InitPlayer();
        //La vague peut de nouveau monter
        WaveMoving = true;
        GameManager.state = GameManager.States.play;
        tb_MoveWave();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    //Gestion des états du jeu
    public enum States
    {
        wait, play, levelup
    }
    public static States state;

    //Variables pour les informations en jeu (avec les accesseurs, ces variables sont modifiables depuis d'autres scripts)
    public Text levelTxt;
    public Text scoreTxt;
    public Text livesTxt;
    public Text levelupTxt;
    public Text messageTxt;
    public Text pauseTxt;
    public Text pseudoTxt;
    public InputField pseudoField;

    public float WaveSpeedForLevel;
    private int level;
    public int Level{
        get{ return level;}
        set{ level = value; levelTxt.text = "Level: " + level;} //Remplace la méthode UpdateTexts vue en cours
    }

    private int score;
    public int Score{
        get{ return score;}
        set{ score = value; scoreTxt.text = "Score: " + score; }
    }

    private int lives;
    public int Lives{
        get{ return lives;}
        set{ lives = value; livesTxt.text = "Lives: " + lives; }
    }

    GameObject player;
    tb_PlayerController playerControllerScript;
    public GameObject waitToStart; //panel
    public GameObject gameInfo; 
    public GameObject networkPanel;
    tb_NetworkManager networkManager;

    //Gestion des vagues d'orcs
    tb_Wave waveScript;
    int TotalWaveRows;


    void Start()
    {
        networkManager = GetComponent<tb_NetworkManager>();
        networkPanel.gameObject.SetActive(true);

        waveScript = GameObject.Find("Wave").GetComponent<tb_Wave>();
        playerControllerScript = GameObject.Find("Player").GetComponent<tb_PlayerController>();

        player = GameObject.FindWithTag("Player");
		//player.SetActive(false); //L'utilisation du SetActive empêche le déplacement du player après une nouvelle partie ...
        gameInfo.SetActive(false);
        messageTxt.gameObject.SetActive(false);
        pauseTxt.gameObject.SetActive(false);
        levelupTxt.gameObject.SetActive(false);

        pseudoTxt.text = pseudoField.text == "" ? "Anonymous" : pseudoField.text;
        
        int highscore = PlayerPrefs.GetInt("highscore");
        if(highscore > 0){
            levelupTxt.text = "New highscore: " + score;
            levelupTxt.gameObject.SetActive(true);
        }
        state = States.wait;
    }

    public void tb_LaunchGame() //public pour l'attribuer directement au bouton dans Unity
    {
        //interface
        waitToStart.SetActive(false);
        gameInfo.SetActive(true);
        levelupTxt.gameObject.SetActive(false);
        networkPanel.gameObject.SetActive(false);
        pseudoTxt.gameObject.SetActive(false);
        levelupTxt.gameObject.SetActive(false);

		//player.SetActive(true);

        //lancer une partie
        tb_InitGame();
        state = States.play;
        StartCoroutine(tb_waitBeforeNewLevel());
    }

    void tb_InitGame()
    {
        Level = 1;
        Score = 0;
        Lives = 5;       
    }

    public void tb_LoadLevel()
    {
     if(state == States.play){           
            //On commence avec une vague d'une ligne, puis toutes les autres auront 2 lignes
            TotalWaveRows = level == 1 ? 1 : 2;
            //A chaque niveau on augmente la vitesse de la vague
            WaveSpeedForLevel = 1f + (level * 0.2f);
            waveScript.tb_LoadWave(TotalWaveRows);
        }
    }

    public void tb_GameOver()
    {
        //Par précaution
        StopAllCoroutines();
        GameObject[] orcs = GameObject.FindGameObjectsWithTag("Orc");
        foreach(GameObject orc in orcs)
        {
            Destroy(orc);
        }

        state = States.wait;

		int highscore = PlayerPrefs.GetInt("highscore");
		if (score > highscore)
		{
			PlayerPrefs.SetInt("highscore", score);
			messageTxt.text = "new highscore: " + score;
		}
		else
		{
			messageTxt.text = "game over\nhighscore: " + highscore;
		}

        StartCoroutine(tb_GameOverToStart());

    }

    IEnumerator tb_GameOverToStart()
    {
        messageTxt.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        messageTxt.gameObject.SetActive(false);

        //player.SetActive(false);
        gameInfo.SetActive(false);
        
		networkManager.tb_SendScore(score);
		networkPanel.gameObject.SetActive(true);
        pseudoTxt.gameObject.SetActive(true);
		waitToStart.gameObject.SetActive(true);
    }

    IEnumerator tb_waitBeforeNewLevel()
    {
        yield return new WaitForSeconds(2f);
        tb_LoadLevel();
    }

    public void tb_EndOfLevel()
    {
        StartCoroutine(tb_LevelUp());
    }

    IEnumerator tb_LevelUp()
    {
        //afficher le message levelUp
        levelupTxt.text = "Level " + level + " completed !";
        levelupTxt.gameObject.SetActive(true);
        //marquer une pause (coroutine)
        yield return new WaitForSecondsRealtime(3f);
        // cacher le message 
        levelupTxt.gameObject.SetActive(false);
        Level += 1;
        state = States.play;
        tb_LoadLevel();
    }
}

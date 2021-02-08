using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tb_OrcDeath : MonoBehaviour
{
    //permet de jouer avec l'affichage
    SpriteRenderer sr;
    //Délai avant la mort de l'orc, on précise que la variable est sérializable pour la modifier dans Unity
    [SerializeField] float delay = 0.5f;
    AudioSource audioSource;

    tb_Wave waveScript;
    public GameObject waveBonus;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        waveScript = GameObject.Find("Wave").GetComponent<tb_Wave>();
    }

    private void Start()
    {
        StartCoroutine(DestroyExplosion());
    }

    IEnumerator DestroyExplosion()
    {
        //On attend un certain délai avant la destruction du gameobject
        yield return new WaitForSeconds(delay);
        //On décrémente le nombre d'orcs restants
        waveScript.RemainingOrc -= 1;
        //Puis on détruit l'orc touché
        Vector2 OrcPosition = this.transform.position;
        Destroy(this.gameObject, delay);
        //On gère la fin de level s'il n'y a plus d'orcs
        if(waveScript.RemainingOrc == 0){
            //On instancie le bonus
            Instantiate(waveBonus, OrcPosition, Quaternion.identity);
            //On active la montée de level
            GameManager.state = GameManager.States.levelup;
            waveScript.tb_IsWaveEmpty();
        }
    }
}

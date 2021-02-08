using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tb_Limits : MonoBehaviour
{
    bool detect = true;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Orc") && detect)
        {
            detect = false;
            collision.GetComponentInParent<tb_Wave>().tb_WaveTouchLimit();
            StartCoroutine(tb_wait());
        }
    }

    IEnumerator tb_wait()
    {
        yield return new WaitForSeconds(0.2f);
        detect = true;
    }
}

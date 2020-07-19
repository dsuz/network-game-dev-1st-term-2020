using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillzoneController2D : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            PlayerController2D p = collision.gameObject.GetComponent<PlayerController2D>();
            if (p)
            {
                p.Kill();
            }
        }
    }
}

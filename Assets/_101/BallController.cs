using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Photon 用の名前空間を参照する
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(Rigidbody))]
public class BallController : MonoBehaviour
{
    [SerializeField] float m_speed = 1f;
    Rigidbody m_rb;
    Animator m_anim;
    PhotonView m_view;

    void Start()
    {
        m_view = GetComponent<PhotonView>();

        if (m_view)
        {
            if (m_view.IsMine)
            {
                // 同期元（自分で操作して動かす）オブジェクトの場合のみ Rigidbody, Animator を使う
                m_rb = GetComponent<Rigidbody>();
                m_anim = GetComponent<Animator>();
            }
        }
    }

    void Update()
    {
        if (!m_view.IsMine) return;  // 同期先のオブジェクトだった場合は何もしない

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 dir = (Vector3.forward * v + Vector3.right * h).normalized;
        m_rb.AddForce(dir * m_speed);

        if (Input.GetButtonDown("Jump"))
        {
            m_anim.SetBool("Heat", true);
        }

        if (Input.GetButtonUp("Jump"))
        {
            m_anim.SetBool("Heat", false);
        }
    }
}

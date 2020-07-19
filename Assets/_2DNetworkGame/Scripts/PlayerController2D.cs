using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Photon 用の名前空間を参照する
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    [SerializeField] float m_rotatePower = 10f;
    [SerializeField] float m_movePower = 10f;
    [SerializeField] float m_dashPower = 10f;
    [SerializeField] float m_brakeCoefficient = 0.9f;
    [SerializeField] Animator m_anim;
    [SerializeField] SpriteRenderer m_sprite;
    Rigidbody2D m_rb;
    PhotonView m_view;
    bool m_isDead;

    void Start()
    {
        m_rb = GetComponent<Rigidbody2D>();

        // 中央を向く
        Vector3 lookDirection = Vector3.zero - transform.position;
        lookDirection.z = 0;
        transform.up = lookDirection;

        m_view = GetComponent<PhotonView>();

        // 自分だけ色を変える
        if (m_view && m_view.IsMine && m_sprite)
        {
            m_sprite.color = Color.yellow;
        }
    }

    void Update()
    {
        if (!m_view || !m_view.IsMine) return;
        if (m_isDead) return;

        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        if (h != 0)
        {
            m_rb.AddTorque(-1 * h * m_rotatePower, ForceMode2D.Force);
        }

        if (v != 0)
        {
            m_rb.AddForce(v * transform.up, ForceMode2D.Force);
        }

        if (Input.GetButtonDown("Fire1"))
        {
            m_rb.AddForce(m_dashPower * transform.up, ForceMode2D.Impulse);
        }

        if (Input.GetButton("Fire2"))
        {
            m_rb.velocity *= m_brakeCoefficient;
        }
    }

    public void Kill()
    {
        if (!m_isDead)
        {
            m_isDead = true;
            m_rb.velocity = Vector2.zero;
            Invoke("Exit", 10f);    // 10 秒後に退場

            if (m_anim)
            {
                m_anim.SetTrigger("DeadTrigger");
            }
        }
    }

    void Exit()
    {
        if (m_view.IsMine)
        {
            PhotonNetwork.Destroy(this.gameObject);
        }
    }
}

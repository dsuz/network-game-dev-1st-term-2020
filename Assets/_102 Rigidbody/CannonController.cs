using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Photon 用の名前空間を参照する
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(Rigidbody))]
public class CannonController : MonoBehaviourPunCallbacks // Photon Realtime 用のクラスを継承する
{
    [SerializeField] float m_power = 1f;
    [SerializeField] float m_lifeTime = 1f;
    float m_timer;
    Rigidbody m_rb;
    PhotonView m_view;

    void Start()
    {
        m_view = GetComponent<PhotonView>();
        m_rb = GetComponent<Rigidbody>();

        if (m_view.IsMine)
        {
            m_rb.AddForce(transform.forward * m_power, ForceMode.Impulse);
        }
    }

    void Update()
    {
        if (!m_view.IsMine) return;

        m_timer += Time.deltaTime;
        if (m_timer > m_lifeTime)
        {
            PhotonNetwork.Destroy(this.gameObject);
        }
    }
}

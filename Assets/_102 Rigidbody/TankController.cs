using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Photon 用の名前空間を参照する
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(Rigidbody))]
public class TankController : MonoBehaviourPunCallbacks // Photon Realtime 用のクラスを継承する
{
    [SerializeField] float m_speed = 1f;
    [SerializeField] float m_rotateSpeed = 1f;
    [SerializeField] Transform m_muzzle;
    [SerializeField] string m_cannonPrefabName = "CannonPrefab";
    Rigidbody m_rb;
    Animator m_anim;
    GameObject m_cannonObject;
    PhotonView m_view;

    void Start()
    {
        m_view = GetComponent<PhotonView>();
        m_rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!m_view.IsMine) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 dir = this.transform.forward * v * m_speed;
        dir = new Vector3(dir.x, m_rb.velocity.y, dir.z);
        m_rb.velocity = dir;
        this.transform.Rotate(0f, m_rotateSpeed * h * Time.deltaTime, 0f);

        if (Input.GetButtonDown("Fire1"))
        {
            if (m_cannonObject == null)
            {
                m_cannonObject = PhotonNetwork.Instantiate(m_cannonPrefabName, m_muzzle.position, m_muzzle.rotation);
            }
        }
    }
}

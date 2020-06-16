using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Photon 用の名前空間を参照する
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UIElements;

/// <summary>
/// 戦車の動きを与える
/// 上下左右、左クリックで操作する
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class TankController : MonoBehaviourPunCallbacks // Photon Realtime 用のクラスを継承する
{
    /// <summary>前進・後退する速度</summary>
    [SerializeField] float m_speed = 1f;
    /// <summary>旋回速度</summary>
    [SerializeField] float m_rotateSpeed = 1f;
    /// <summary>砲弾が発射される位置</summary>
    [SerializeField] Transform m_muzzle;
    /// <summary>砲弾のプレハブ名 (/Resources の下に置くこと)</summary>
    [SerializeField] string m_cannonPrefabName = "CannonPrefab";
    Rigidbody m_rb;
    GameObject m_cannonObject;  // 砲弾のオブジェクトを参照する（連射させないため）
    PhotonView m_view;

    void Start()
    {
        // 中心を向く
        transform.LookAt(new Vector3(0, this.transform.position.y, 0));

        m_view = GetComponent<PhotonView>();
        m_rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!m_view.IsMine) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // 上下で前後に進む
        Vector3 dir = this.transform.forward * v * m_speed;
        dir = new Vector3(dir.x, m_rb.velocity.y, dir.z);
        m_rb.velocity = dir;
        // 左右で回転する
        this.transform.Rotate(0f, m_rotateSpeed * h * Time.deltaTime, 0f);

        // 左クリックで砲弾を生成する
        if (Input.GetButtonDown("Fire1"))
        {
            if (m_cannonObject == null)
            {
                // ネットワークオブジェクトとして生成する
                m_cannonObject = PhotonNetwork.Instantiate(m_cannonPrefabName, m_muzzle.position, m_muzzle.rotation);
            }
        }
    }
}

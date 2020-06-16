using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Photon 用の名前空間を参照する
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// 生成された砲弾を制御する
/// 生成されると前方に発射され、一定時間で消滅する
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class CannonController : MonoBehaviourPunCallbacks // Photon Realtime 用のクラスを継承する
{
    /// <summary>発射する力</summary>
    [SerializeField] float m_power = 1f;
    /// <summary>消滅するまでの秒数</summary>
    [SerializeField] float m_lifeTime = 1f;
    float m_timer;
    Rigidbody m_rb;
    PhotonView m_view;

    void Start()
    {
        m_view = GetComponent<PhotonView>();
        m_rb = GetComponent<Rigidbody>();

        // 自分が生成したオブジェクトなら、飛んでいく
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
            PhotonNetwork.Destroy(this.gameObject); // ネットワークオブジェクトとして Destroy する（他のクライアントからも消える）
        }
    }
}

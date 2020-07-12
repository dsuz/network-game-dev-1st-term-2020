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
/// もしくは、何かに当たると消滅する
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class CannonController : MonoBehaviourPunCallbacks // Photon Realtime 用のクラスを継承する
{
    /// <summary>発射する力</summary>
    [SerializeField] float m_power = 1f;
    /// <summary>消滅するまでの秒数</summary>
    [SerializeField] float m_lifeTime = 1f;
    /// <summary>弾が与えるダメージ量</summary>
    [SerializeField] int m_attackPower = 1;
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

    private void OnCollisionEnter(Collision collision)
    {
        // 自分が発射した弾が相手に当たった時に処理する
        if (m_view.IsMine)
        {
            // 相手がタンクだったらダメージを与える
            TankController tank = collision.gameObject.GetComponent<TankController>();
            if (tank)
            {
                tank.Damage(PhotonNetwork.LocalPlayer.ActorNumber, m_attackPower);
                // 破棄しないと何度も当たってしまうので、破棄する
                PhotonNetwork.Destroy(this.gameObject);
            }
        }
    }
}

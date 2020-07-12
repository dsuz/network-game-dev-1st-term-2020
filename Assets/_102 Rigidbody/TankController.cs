using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Photon 用の名前空間を参照する
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

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
    /// <summary>ライフの最大値（初期値）</summary>
    [SerializeField] int m_maxLife = 30;
    /// <summary>現在のライフ</summary>
    int m_life;
    /// <summary>ライフ表示</summary>
    [SerializeField] UnityEngine.UI.Text m_lifeText;
    /// <summary>ライフをオーナーから同期する間隔</summary>
    [SerializeField] float m_syncInterval = 1f;


    Rigidbody m_rb;
    GameObject m_cannonObject;  // 砲弾のオブジェクトを参照する（連射させないため）
    PhotonView m_view;
    float m_syncTimer;

    void Start()
    {
        // 中心を向く
        transform.LookAt(new Vector3(0, this.transform.position.y, 0));

        m_view = GetComponent<PhotonView>();
        m_rb = GetComponent<Rigidbody>();

        if (m_view.IsMine)
        {
            // ライフを初期化する
            m_life = m_maxLife;
            RefreshLifeText();
        }
    }

    void Update()
    {
        if (!m_view.IsMine) return;

        // ライフを同期する。後から参加する場合はこれが必要になる。
        m_syncTimer += Time.deltaTime;
        if (m_syncTimer > m_syncInterval)
        {
            m_syncTimer = 0;
            object[] parameters = new object[] { m_life };
            m_view.RPC("SyncLife", RpcTarget.Others, parameters);
        }

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

    /// <summary>
    /// ダメージを与える。ダメージを与えた側が呼び出す。
    /// </summary>
    /// <param name="playerId">ダメージを与えたプレイヤーのID</param>
    /// <param name="damage">ダメージ量</param>
    public void Damage(int playerId, int damage)
    {
        m_life -= damage;
        RefreshLifeText();

        // ライフが減ったら、他のクライアントとライフを同期する
        object[] parameters = new object[] { m_life };
        m_view.RPC("SyncLife", RpcTarget.Others, parameters);

        Debug.LogFormat("Player {0} が Player {1} の {2} に {3} のダメージを与えた", playerId, m_view.Owner.ActorNumber, name, damage);
        Debug.LogFormat("Player {0} の {1} の残りライフは {2}", m_view.Owner.ActorNumber, gameObject.name, m_life);
    }

    /// <summary>
    /// ダメージを与えたことをクライアント間で同期する
    /// </summary>
    /// <param name="currentLife"></param>
    [PunRPC]
    void SyncLife(int currentLife)
    {
        m_life = currentLife;
        RefreshLifeText();
        Debug.LogFormat("Player {0} の {1} の残りライフは {2}", m_view.Owner.ActorNumber, gameObject.name, m_life);
    }

    /// <summary>
    /// ライフ表示を更新する
    /// </summary>
    void RefreshLifeText()
    {
        if (m_lifeText)
        {
            m_lifeText.text = m_life.ToString();
            if (m_life < 5)
            {
                m_lifeText.color = Color.red;
            }
        }
    }
}

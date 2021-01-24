using UnityEngine;
// Photon 用の名前空間を参照する
using Photon.Pun;
using Photon.Pun.UtilityScripts;

/// <summary>
/// Chip を制御するコンポーネント
/// ←→で移動、↑で回転、↓で落とす
/// 出現時にはコライダーが無効になるが、落とした時にコライダーを有効にする（落とした物と出現した物を区別するため）
/// </summary>
public class ChipController : MonoBehaviour
{
    /// <summary>左右に動く速さ</summary>
    [SerializeField] float m_moveSpeed = 1f;
    /// <summary>回転する速さ</summary>
    [SerializeField] float m_rotateSpeed = 1f;

    PunTurnManager m_turnManager;
    PhotonView m_view = null;
    Rigidbody2D m_rb = null;
    Collider2D m_collider = null;
    /// <summary>落としたかどうか判定するフラグ</summary>
    bool m_isDropped = false;

    private void Awake()
    {
        m_collider = GetComponent<Collider2D>();
        m_collider.enabled = false;
    }

    void Start()
    {
        m_rb = GetComponent<Rigidbody2D>();
        m_view = GetComponent<PhotonView>();
        // 落ちないようにする
        m_rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        m_turnManager = GameObject.FindObjectOfType<PunTurnManager>();  // PunTurnManager がシーン内に一つしかないことを前提とする
    }

    void Update()
    {
        // 自分のオブジェクトではない場合は操作させない、何もしない
        if (!m_view.IsMine) return;

        // まだ落としていないオブジェクトは操作できる
        if (!m_isDropped)
        {
            float v = Input.GetAxisRaw("Vertical");
            float h = Input.GetAxisRaw("Horizontal");
            m_rb.velocity = Vector2.right * m_moveSpeed * h;

            if (v > 0)
            {
                transform.Rotate(Vector3.back, m_rotateSpeed * Time.deltaTime);
            }
            else if (v < 0)
            {
                m_isDropped = true;
                m_view.RPC("Drop", RpcTarget.All, null);
                m_turnManager.SendMove(null, true);
            }
        }
    }

    [PunRPC]
    void Drop()
    {
        m_rb.constraints = RigidbodyConstraints2D.None;
        m_rb.WakeUp();  // constraints を変えた後にこれをやらないと重力が働かないことがある
        m_collider.enabled = true;
    }
}

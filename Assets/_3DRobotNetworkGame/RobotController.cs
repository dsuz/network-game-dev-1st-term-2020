using UnityEngine;
using Cinemachine;
using Photon.Pun;

/// <summary>
/// Rigidbody を使ってプレイヤーを動かすコンポーネント
/// 入力を受け取り、それに従ってオブジェクトを動かす
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class RobotController : MonoBehaviour
{
    /// <summary>動く速さ</summary>
    [SerializeField] float m_movePower = 5f;
    /// <summary>ターンの速さ</summary>
    [SerializeField] float m_turnSpeed = 3f;
    /// <summary>ジャンプ力</summary>
    [SerializeField] float m_jumpPower = 5f;
    /// <summary>ホバー力</summary>
    [SerializeField] float m_hoverPower = 5f;
    /// <summary>接地判定の際、中心 (Pivot) からどれくらいの距離を「接地している」と判定するかの長さ</summary>
    [SerializeField] float m_isGroundedLength = 1.1f;
    /// <summary>キャラクターの Animator</summary>
    [SerializeField] Animator m_anim;

    Rigidbody m_rb;
    PhotonView m_view;
    
    void Start()
    {
        m_rb = GetComponent<Rigidbody>();
        m_view = GetComponent<PhotonView>();

        // カメラターゲットに自分を設定する
        if (m_view.IsMine)
        {
            CinemachineVirtualCameraBase vcam = GameObject.FindObjectOfType<CinemachineVirtualCameraBase>();
            vcam.Follow = transform;
            vcam.LookAt = transform;
        }
    }

    void Update()
    {
        if (!m_view.IsMine) return;

        // TODO: 動きの処理・ホバーの処理は FixedUpdate で処理すべき。ジャンプは Update でもよい。

        // 方向の入力を取得し、方向を求める
        float v = Input.GetAxisRaw("Vertical");
        float h = Input.GetAxisRaw("Horizontal");

        // 入力方向のベクトルを組み立てる
        Vector3 dir = Vector3.forward * v + Vector3.right * h;

        if (dir != Vector3.zero)
        {
            // カメラを基準に入力が上下=奥/手前, 左右=左右にキャラクターを向ける
            dir = Camera.main.transform.TransformDirection(dir);    // メインカメラを基準に入力方向のベクトルを変換する
            dir.y = 0;  // y 軸方向はゼロにして水平方向のベクトルにする

            // 入力方向に滑らかに回転させる
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            this.transform.rotation = Quaternion.Slerp(this.transform.rotation, targetRotation, Time.deltaTime * m_turnSpeed);

            Vector3 force = dir.normalized * m_movePower; // 入力した方向に力をかける
            m_rb.AddForce(force, ForceMode.Force);
        }

        // Animator Controller のパラメータをセットする
        if (m_anim)
        {
            // 攻撃ボタンを押された時の処理
            if (Input.GetButtonDown("Fire1") && IsGrounded())
            {
                m_anim.SetTrigger("Attack");
                /* -----------
                 * TODO: アニメーションイベントを使って攻撃モーションが始まる時に動きを止め、
                 * 攻撃モーションが終わったら動けるように処理を追加する
                 * ----------- */
            }

            // 水平方向の速度を Speed にセットする
            Vector3 velocity = m_rb.velocity;
            velocity.y = 0f;
            m_anim.SetFloat("Speed", velocity.magnitude);

            // 地上/空中の状況に応じて IsGrounded をセットする
            if (m_rb.velocity.y <= 0f && IsGrounded())
            {
                m_anim.SetBool("IsGrounded", true);
            }
            else if (!IsGrounded())
            {
                m_anim.SetBool("IsGrounded", false);
            }
        }

        // ジャンプの入力を取得し、接地している時に押されていたらジャンプする
        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            m_rb.AddForce(Vector3.up * m_jumpPower, ForceMode.Impulse);

            // Animator Controller のパラメータをセットする
            if (m_anim)
            {
                m_anim.SetBool("IsGrounded", false);
            }
        }

        // 空中でジャンプボタンを押し続けると若干ホバーする
        if (Input.GetButton("Jump") && !IsGrounded())
        {
            m_rb.AddForce(Vector3.up * m_hoverPower, ForceMode.Force);
        }
    }

    /// <summary>
    /// 地面に接触しているか判定する
    /// </summary>
    /// <returns></returns>
    bool IsGrounded()
    {
        // Physics.Linecast() を使って足元から線を張り、そこに何かが衝突していたら true とする
        CapsuleCollider col = GetComponent<CapsuleCollider>();
        Vector3 start = this.transform.position + col.center;   // start: 体の中心
        Vector3 end = start + Vector3.down * m_isGroundedLength;  // end: start から真下の地点
        Debug.DrawLine(start, end); // 動作確認用に Scene ウィンドウ上で線を表示する
        bool isGrounded = Physics.Linecast(start, end); // 引いたラインに何かがぶつかっていたら true とする
        return isGrounded;
    }
}

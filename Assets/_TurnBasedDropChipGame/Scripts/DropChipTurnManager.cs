using UnityEngine;
// Photon 用の名前空間を参照する
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;

/// <summary>
/// ターン管理用コンポーネント
/// 自分の番が周ってきた時に、自分がオーナーとしてチップをネットワークオブジェクトとして生成する
/// </summary>
public class DropChipTurnManager : MonoBehaviour, IPunTurnManagerCallbacks
{
    /// <summary>チップとして使うオブジェクトの名前リスト</summary>
    [SerializeField] string[] m_chipPrefabNameList = null;
    /// <summary>チップを出現させる場所</summary>
    [SerializeField] Transform m_chipSpawnPoint = null;
    /// <summary>現在順番が周ってきているプレイヤーが何番目のプレイヤーなのか示す index</summary>
    int m_activePlayerIndex = 0;
    PunTurnManager m_turnManager = null;

    /// <summary>
    /// 順番を次のプレイヤーに移動する
    /// </summary>
    void MoveToNextPlayer()
    {
        m_activePlayerIndex = (m_activePlayerIndex + 1) % PhotonNetwork.PlayerList.Length;
        Debug.Log($"Active player changed to {PhotonNetwork.PlayerList[m_activePlayerIndex].ActorNumber}");
    }

    /// <summary>
    /// 現在順番が周ってきているプレイヤーの Player オブジェクト
    /// </summary>
    Player ActivePlayer
    {
        get
        {
            return PhotonNetwork.PlayerList[m_activePlayerIndex];
        }
    }

    /// <summary>
    /// ターン管理コンポーネントが Photon のターン管理イベントを受信できるように初期化する
    /// </summary>
    public void Init()
    {
        // 同じオブジェクトに追加している PunTurnManager を取得し、ターン管理のイベントを Listen するよう設定する
        m_turnManager = GetComponent<Photon.Pun.UtilityScripts.PunTurnManager>();
        m_turnManager.TurnManagerListener = this;
    }

    /// <summary>
    /// チップを生成する
    /// </summary>
    void SpawnChip()
    {
        string chipName = m_chipPrefabNameList[Random.Range(0, m_chipPrefabNameList.Length)];
        Debug.Log($"Spawn chip: {chipName}.");
        PhotonNetwork.Instantiate(chipName, m_chipSpawnPoint.position, Quaternion.identity);
    }

    #region IPunTurnManagerCallbacks の実装
    void IPunTurnManagerCallbacks.OnTurnBegins(int turn)
    {
        Debug.Log("Enter OnTurnBegins.");

        m_activePlayerIndex = 0;

        // 自分の番ならチップを出す
        if (this.ActivePlayer.Equals(PhotonNetwork.LocalPlayer))
        {
            Debug.Log("This is my turn.");
            SpawnChip();
        }
        else
        {
            Debug.Log("Not my turn.");
        }
    }

    void IPunTurnManagerCallbacks.OnPlayerMove(Player player, int turn, object move)
    {

    }

    void IPunTurnManagerCallbacks.OnPlayerFinished(Player player, int turn, object move)
    {
        Debug.Log("Enter OnPlayerFinished.");

        MoveToNextPlayer();

        // 全員が終わっている場合はチップを生成せず、続きの処理は OnTurnCompleted に任せる。まだ順番を終わらせていないプレイヤーが居る場合は、番が周ってきているプレイヤーがチップを生成する
        if (!m_turnManager.IsCompletedByAll)
        {
            // 自分の番ならチップを出す
            if (this.ActivePlayer.Equals(PhotonNetwork.LocalPlayer))
            {
                Debug.Log("This is my turn.");
                SpawnChip();
            }
            else
            {
                Debug.Log("Not my turn.");
            }
        }
    }

    void IPunTurnManagerCallbacks.OnTurnCompleted(int turn)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            m_turnManager.BeginTurn();
        }
    }

    void IPunTurnManagerCallbacks.OnTurnTimeEnds(int turn)
    {
    }
    #endregion
}

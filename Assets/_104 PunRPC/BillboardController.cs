using UnityEngine;

/// <summary>
/// UI オブジェクトが常にカメラの方に向く機能を持つコンポーネント
/// </summary>
public class BillboardController : MonoBehaviour
{
    void LateUpdate()
    {
        transform.rotation = Camera.main.transform.rotation;
    }
}

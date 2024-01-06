using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

public class Door : MonoBehaviour
{
    public EConnectionType ConnectionType;

    public Vector3Int GetPosition()
    {
        int width = ConnectionType == EConnectionType.Horizontal ? 2 : 1;
        int height = ConnectionType == EConnectionType.Horizontal ? 1 : 2;
        return Vector3Int.FloorToInt(transform.position
            - new Vector3Int(width / 2, height / 2, 0));
    }
}
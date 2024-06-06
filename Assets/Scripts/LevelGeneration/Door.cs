using UnityEngine;

public class Door : MonoBehaviour
{
    public EConnectionType ConnectionType;                   // horizontally or vertically connected rooms 
    public EDoorDirection Direction;                         // the direction this door is connected towards (inside -> outside direction)
    public int minSize;                                      // the minimum possible size of this door
    public int maxSize;                                      // the maximum possible size of this door
    // public int doorSize;
    public int DoorSizeReal { get; set; }
    private int _width;                                      // the width of the possible range a door can be placed
    private int _height;                                     // the height of the possible range a door can be placed
    public int CandidateRange { get; private set; }          // depending on the door connection type, return the width/height of the possible range
    public Vector3Int RangeLocalBLPosition { get; private set; } // the bottom-left local position of the possible range (before instantiation)
    public Vector3Int RangeWorldBLPosition { get; set; }         // The bottom-left world position of the possible range (after instantiation)
    public Vector3Int DoorLocalBLPosition { get; set; }
    public Vector3Int DoorWorldBLPosition { get; set; }

    public void Init()
    {
        // doorSize = 3;
        _width = (int)transform.localScale.x;
        _height = (int)transform.localScale.y;
        ConnectionType = _width == 1 ? EConnectionType.Horizontal : EConnectionType.Vertical;
        RangeLocalBLPosition = Vector3Int.FloorToInt(transform.position - new Vector3Int(_width / 2, _height / 2, 0));
        CandidateRange = ConnectionType == EConnectionType.Horizontal ? _height : _width;
    }

    public void SetValues(EConnectionType type, EDoorDirection dir, Vector3Int blPosWorld, int minSize, int maxSize, int range)
    {
        ConnectionType = type;
        Direction = dir;
        RangeWorldBLPosition = blPosWorld;
        this.minSize = minSize;
        this.maxSize = maxSize;
        CandidateRange = range;
    }
    
    // public void SetValues(EConnectionType type, EDoorDirection dir, Vector3Int blPosWorld, int doorSize, int range)
    // {
    //     ConnectionType = type;
    //     Direction = dir;
    //     RangeWorldBLPosition = blPosWorld;
    //     this.doorSize = doorSize;
    //     CandidateRange = range;
    // }
}
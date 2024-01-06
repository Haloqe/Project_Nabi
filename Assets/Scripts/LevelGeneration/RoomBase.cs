using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.Tilemaps;

public class RoomBase : MonoBehaviour
{
    public ERoomType RoomType;
    private List<Door>[] _doors;
    [NamedArray(typeof(ETilemapType))] public Tilemap[] Tilemaps = new Tilemap[6];

    public void Initialise()
    {
        FindDoors();
    }
    
    private void FindDoors()
    {
        // Initialise door list
        _doors = new List<Door>[2];
        _doors[0] = new List<Door>();
        _doors[1] = new List<Door>();

        // Fill door data
        var doors = GetComponentsInChildren<Door>();
        foreach (var door in doors)
        {
            _doors[(int)door.ConnectionType].Add(door);
        }
    }

    public List<Door>[] GetAllDoors()
    {
        return _doors;
    }

    public List<Door> GetDoors(EConnectionType connectionType)
    {
        return _doors[(int)connectionType];
    }

    public bool HasDoors(EConnectionType connectionType)
    {
        return _doors[(int)connectionType].Count > 0;
    }

    // This function assumes that there is at least one door
    public Door GetRandomDoor()
    {
        // Randomly select door type
        EConnectionType connectionType;
        if (!HasDoors(EConnectionType.Horizontal) || !HasDoors(EConnectionType.Vertical))
        {
            connectionType = !HasDoors(EConnectionType.Horizontal) ? EConnectionType.Vertical : EConnectionType.Horizontal;
        }
        else
        {
            connectionType = (EConnectionType)Random.Range(0, 2);
        }

        // Randomly select a door
        return _doors[(int)connectionType][Random.Range(0, _doors[(int)connectionType].Count)];
    }
}
using System.Collections.Generic;
using System.Diagnostics;

public class LevelGraph
{
    // Node to represent a room
    public struct SNode
    {
        public int RoomID;
        public ERoomType RoomType;
    }

    private List<SNode> _nodes = new();
    private List<List<int>> _graph = new();
    private int _nextID = 0;
    private int _startID = 0;

    public ERoomType GetRoomType(int roomID)
    {
        return _nodes[roomID].RoomType;
    }

    public void SetStartRoom(int roomID)
    {
        _startID = roomID;
    }

    public int GetStartRoom()
    {
        return _startID;
    }

    public bool IsStartRoom(int roomID)
    {
        return (_startID == roomID);
    }

    public int GetNumRooms()
    {
        return _nodes.Count;
    }

    public List<int> GetConnectedRooms(int roomID)
    {
        Debug.Assert(_nodes.Count > roomID);
        return _graph[roomID];
    }

    public int AddRoom(ERoomType roomType)
    {
        // Create a new room
        SNode node = new SNode()
        {
            RoomID = _nextID++,
            RoomType = roomType,
        };

        // Add to the graph
        _nodes.Add(node);
        _graph.Add(new List<int>());

        // Return room index
        return _nextID - 1;
    }

    public void ConnectRoom(int roomA, int roomB)
    {
        // Connect two rooms
        _graph[roomA].Add(roomB);
        _graph[roomB].Add(roomA);
    }

    public int ConnectNewRoomToPrev(ERoomType roomType)
    {
        int newID = AddRoom(roomType);
        ConnectRoom(newID - 1, newID);
        return newID;
    }

    public int ConnectNewRoomToAnother(ERoomType roomType, int prevRoomID)
    {
        int newID = AddRoom(roomType);
        ConnectRoom(prevRoomID, newID);
        return newID;
    }
}
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

    private void ConnectRoom(int roomA, int roomB)
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

    public void GeneratePreMidBossGraph()
    {
        int prevRoomID = AddRoom(ERoomType.Entrance);
        SetStartRoom(prevRoomID);
        ConnectNewRoomToPrev(ERoomType.MidBoss);
    }

    public void GeneratePostMidBossGraph()
    {
        int prevRoomID = AddRoom(ERoomType.Entrance);
        SetStartRoom(prevRoomID);
        ConnectNewRoomToPrev(ERoomType.Boss);
    }
    
    public void SetDefaultType()
    {
        int prevRoomID = AddRoom(ERoomType.Entrance);
        SetStartRoom(prevRoomID);
        
        ConnectNewRoomToPrev(ERoomType.MidBoss);
        //ConnectNewRoomToPrev(ERoomType.Normal);
        // ConnectNewRoomToPrev(ERoomType.Normal);
        // ConnectNewRoomToPrev(ERoomType.Normal);
        // ConnectNewRoomToPrev(ERoomType.Normal);
        //
        // ConnectNewRoomToPrev(ERoomType.Normal);
        // ConnectNewRoomToPrev(ERoomType.Normal);
        // ConnectNewRoomToPrev(ERoomType.Normal);
        // ConnectNewRoomToPrev(ERoomType.Normal);
        // ConnectNewRoomToPrev(ERoomType.Normal);
        //
        // ConnectNewRoomToPrev(ERoomType.Normal);
        // ConnectNewRoomToPrev(ERoomType.Normal);
        // ConnectNewRoomToPrev(ERoomType.Normal);
        // ConnectNewRoomToPrev(ERoomType.Normal);
        // ConnectNewRoomToPrev(ERoomType.Normal);
        //
        // ConnectNewRoomToPrev(ERoomType.Normal);
        // ConnectNewRoomToPrev(ERoomType.Normal);
        // ConnectNewRoomToPrev(ERoomType.Normal);
        // ConnectNewRoomToPrev(ERoomType.Normal);
        // ConnectNewRoomToPrev(ERoomType.Normal);
        //
        // ConnectNewRoomToPrev(ERoomType.Normal);
        // ConnectNewRoomToPrev(ERoomType.Normal);
        // ConnectNewRoomToPrev(ERoomType.Normal);
        // ConnectNewRoomToPrev(ERoomType.Normal);
        // ConnectNewRoomToPrev(ERoomType.Normal);
        
        
        //ConnectNewRoomToPrev(ERoomType.Teleport);
        // ConnectNewRoomToPrev(ERoomType.Normal);
        // ConnectNewRoomToPrev(ERoomType.Normal);
        // prevRoomID = ConnectNewRoomToPrev(ERoomType.Teleport);
        // int roomID = ConnectNewRoomToPrev(ERoomType.Normal);
        // ConnectNewRoomToPrev(ERoomType.Teleport);
        // ConnectNewRoomToPrev(ERoomType.Treasure);
        // roomID = ConnectNewRoomToAnother(ERoomType.Normal, roomID);
        // ConnectNewRoomToAnother(ERoomType.Shop, prevRoomID);
        // ConnectNewRoomToPrev(ERoomType.Teleport);
        // ConnectNewRoomToPrev(ERoomType.MidBoss);
        // ConnectNewRoomToPrev(ERoomType.Normal);
        // ConnectNewRoomToPrev(ERoomType.Normal);
        // ConnectNewRoomToPrev(ERoomType.Teleport);
        // ConnectNewRoomToPrev(ERoomType.Normal);
        // ConnectNewRoomToPrev(ERoomType.Normal);
        // ConnectNewRoomToPrev(ERoomType.Teleport);
        // roomID = ConnectNewRoomToAnother(ERoomType.Teleport, roomID);
        // ConnectNewRoomToPrev(ERoomType.Normal);
        // ConnectNewRoomToPrev(ERoomType.Normal);
        // ConnectNewRoomToPrev(ERoomType.Teleport);
        // ConnectNewRoomToAnother(ERoomType.Normal, roomID);
        // ConnectNewRoomToPrev(ERoomType.Normal);
        // ConnectNewRoomToPrev(ERoomType.Teleport);
        // ConnectNewRoomToPrev(ERoomType.Normal);
    }
}
using System.Collections.Generic;
using System.IO.Compression;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.Tilemaps;
using static UnityEditor.PlayerSettings;

public class LevelManager : Singleton<LevelManager>
{ 
    private int _maxHeight;
    private int _maxWidth;

    private Object[] _roomTemplates;

    private ECellType[,] _superGrid;

    private Vector2 _roomGenPos;
    private int _direction;

    private bool _isLevelGenComplete;
    private List<List<RoomBase>> _roomsByType;

    private List<Vector3Int> _corridorsH;
    private List<Vector3Int> _corridorsV;

    private LevelGraph _levelGraph;

    [NamedArray(typeof(ETilemapType))] public Tilemap[] Tilemaps;

    protected override void Awake()
    {
        base.Awake();
        _maxHeight = 1000;
        _maxWidth = 1000;
        _superGrid = new ECellType[_maxHeight, _maxWidth];
        List<List<Vector2>> _openings = new List<List<Vector2>>(4);
        _corridorsH = new List<Vector3Int>();
        _corridorsV = new List<Vector3Int>();
        InitialiseRooms();
    }

    private void Start()
    {
        // Populate Level Graph
        _levelGraph = new LevelGraph();
        int prevRoomID = _levelGraph.AddRoom(ERoomType.Entrance);
        _levelGraph.SetStartRoom(prevRoomID);
        _levelGraph.ConnectNewRoomToPrev(ERoomType.Normal);
        _levelGraph.ConnectNewRoomToPrev(ERoomType.Teleport);
        _levelGraph.ConnectNewRoomToPrev(ERoomType.Normal);
        _levelGraph.ConnectNewRoomToPrev(ERoomType.Normal);
        prevRoomID = _levelGraph.ConnectNewRoomToPrev(ERoomType.Teleport);
        int roomID = _levelGraph.ConnectNewRoomToPrev(ERoomType.Normal);
        _levelGraph.ConnectNewRoomToPrev(ERoomType.Teleport);
        _levelGraph.ConnectNewRoomToPrev(ERoomType.Treasure);
        _levelGraph.ConnectNewRoomToAnother(ERoomType.Normal, roomID);
        _levelGraph.ConnectNewRoomToAnother(ERoomType.Shop, prevRoomID);
        _levelGraph.ConnectNewRoomToPrev(ERoomType.Teleport);

        // Traverse level graph
        GenerateLevel();
    }

    private void InitialiseRooms()
    {
        _roomsByType = new List<List<RoomBase>>((int)ERoomType.MAX);
        for (int i = 0; i < (int)ERoomType.MAX; i++)
        {
            _roomsByType.Add(new List<RoomBase>());
        }
        _roomTemplates = Utility.LoadAllObjectsFromPath("Prefabs/LevelGeneration/Rooms/");

        // Initialise all rooms
        foreach (var room in _roomTemplates)
        {
            RoomBase roomBase = room.GetComponent<RoomBase>();
            roomBase.Initialise();
            _roomsByType[(int)roomBase.RoomType].Add(roomBase);            
        }
    }

    private void GenerateLevel()
    {
        // Clear tiles
        for (int i = 0; i < (int)ETilemapType.MAX; i++)
        {
            Tilemaps[i].ClearAllTiles();
        }

        // Add starting room
        SRoomInfo roomInfo = new SRoomInfo
            (_levelGraph.GetStartRoom(),
             new List<SDoorInfo>{ new SDoorInfo(EConnectionType.Horizontal, new Vector3Int(500, 500, 0)),
                 new SDoorInfo(EConnectionType.Vertical, new Vector3Int(500, 500, 0)) }
            );

        List<bool> visited = new List<bool>(new bool[_levelGraph.GetNumRooms()]);
        Queue<SRoomInfo> toVisit = new();
        toVisit.Enqueue(roomInfo);

        // Procedurally generate rooms
        while (toVisit.Count > 0)
        {
            // Visit a room
            roomInfo = toVisit.Dequeue();
            int currRoomID = roomInfo.RoomID;
            ERoomType currRoomType = _levelGraph.GetRoomType(currRoomID);

            // If already visited, skip
            if (visited[currRoomID]) continue;
            visited[roomInfo.RoomID] = true;

            // Try to place a room
            List<SDoorInfo> newDoorInfos = TryPlaceRoom(currRoomType, roomInfo.PrevDoorInfos);

            // Add its connected rooms to the visit queue
            var nextRoomIDs = _levelGraph.GetConnectedRooms(currRoomID);
            foreach (var nextRoomID in nextRoomIDs)
            {
                //toVisit.Enqueue(new SRoomInfo(nextRoomID, newDoorInfos));
            }
        }
    }

    private List<SDoorInfo> TryPlaceRoom(ERoomType roomType, List<SDoorInfo> prevDoorInfos)
    {
        RoomBase roomToPlace = null;
        Door newDoorToConnect = null;
        Vector3Int prevDoorPos = Vector3Int.zero;

        // Pick a room from templates
        var templates = _roomsByType[(int)roomType];
        Debug.Assert(templates.Count > 0);

        // Start room needs no searching
        // TODO: Generalise
        if (roomType == ERoomType.Entrance)
        {
            prevDoorPos = prevDoorInfos[0].Position;
            roomToPlace = templates[Random.Range(0, templates.Count)];
            newDoorToConnect = roomToPlace.GetRandomDoor();
        }

        // Select a random room that can be place
        while (roomToPlace == null)
        {
            // Randomly choose a room from the templates

            // Check if the room can be placed

            // Stop searching if a room can be placed
        }

        // Place the found room
        AddRoomToWorld(roomToPlace, newDoorToConnect, prevDoorPos);
        return GetNewDoorPositionsExcept(roomToPlace, newDoorToConnect, prevDoorPos);
    }

    public List<SDoorInfo> GetNewDoorPositionsExcept(RoomBase room, Door newPlacedDoor, Vector3Int prevDoorPos)
    {
        List<Door>[] doors = room.GetAllDoors();
        List<SDoorInfo> doorInfos = new();
        SDoorInfo doorInfo = new SDoorInfo();
        Vector3Int newDoorPos = newPlacedDoor.GetPosition();

        for (int i = 0; i < doors.Length; i++)
        {
            foreach (Door door in doors[i])
            {
                if (door == newPlacedDoor && room.RoomType != ERoomType.Entrance)
                {
                    continue;
                }
                doorInfo.ConnectionType = door.ConnectionType;
                doorInfo.Position = prevDoorPos - newDoorPos + door.GetPosition();
                doorInfos.Add(doorInfo);
            }
        }

        return doorInfos;
    }

    private void AddRoomToWorld(RoomBase room, Door newDoor, Vector3Int prevDoorPos)
    {
        Vector3Int newDoorPos = newDoor.GetPosition();

        // Add corridor to the list


        // Paint wall layer
        Vector3Int maxYPos = Vector3Int.back;
        Tilemap wallTilemap = room.Tilemaps[(int)ETilemapType.Wall];
        wallTilemap.CompressBounds();
        var bounds = wallTilemap.cellBounds;

        int count = 0;
        for (int y = bounds.yMin; y < bounds.yMax; y++)
        {
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                Vector3Int localPos = new Vector3Int(x, y, 0);
                Vector3Int worldPos = Vector3Int.FloorToInt(wallTilemap.LocalToWorld(localPos));
                Vector3Int newWorldPos = prevDoorPos - newDoorPos + worldPos;
                
                if (wallTilemap.HasTile(localPos))
                {
                    if (maxYPos == Vector3Int.back && y == bounds.yMax - 1)
                    {
                        maxYPos = newWorldPos;
                        Debug.Log(newDoor.transform.position);
                        Debug.Log(newDoorPos);
                        Debug.Log(worldPos);
                        
                        Debug.Log(maxYPos);
                    }

                    // Set supergrid
                    _superGrid[newWorldPos.y, newWorldPos.x] = ECellType.Filled;
                    count++;

                    // Copy the tile to the main tilemap
                    Tilemaps[(int)ETilemapType.Wall].SetTile(newWorldPos, wallTilemap.GetTile(localPos));
                }
            }
        }

        // Graph traversal to fill the super grid
        Queue<Vector3Int> toVisits = new Queue<Vector3Int>();

        // For the first cell, search diagonal cell
        toVisits.Enqueue(maxYPos + Vector3Int.right + Vector3Int.down);

        // For other cells, search all four directions
        Vector3Int[] offsets = { Vector3Int.left, Vector3Int.right, Vector3Int.up, Vector3Int.down };

        while (toVisits.Count > 0)
        {
            Vector3Int curr = toVisits.Dequeue();
            foreach (var offset in offsets)
            {
                Vector3Int next = curr + offset;
                if (_superGrid[next.y, next.x] != ECellType.Filled)
                {
                    _superGrid[next.y, next.x] = ECellType.Filled;
                    toVisits.Enqueue(next);
                    count++;
                }
            }
        }

        // Paint other layers
        for (int i = 1; i < (int)ETilemapType.MAX; i++)
        {
            BoundsInt localBounds = room.Tilemaps[i].cellBounds;
            BoundsInt worldBounds = localBounds;
            worldBounds.SetMinMax(prevDoorPos - newDoorPos + localBounds.min,
                prevDoorPos - newDoorPos + localBounds.max);

            var tiles = room.Tilemaps[i].GetTilesBlock(localBounds);
            Tilemaps[i].SetTilesBlock(worldBounds, tiles);
        }
    }
}

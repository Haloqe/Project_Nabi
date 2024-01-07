using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
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
    private List<List<RoomBase>> _roomsByType;

    private ECellType[,] _superGrid;
    private List<Vector3Int>[] _corridors;
    private List<List<SDoorInfo>> _openDoorInfos;

    private LevelGraph _levelGraph;

    [NamedArray(typeof(ETilemapType))] public Tilemap[] Tilemaps;

    private EDoorDirection[] _matchDirections;
    Vector3Int[] _newDoorOffsets;

    protected override void Awake()
    {
        base.Awake();
        _maxHeight = 1000;
        _maxWidth = 1000;
        _superGrid = new ECellType[_maxHeight, _maxWidth];
        _corridors = new List<Vector3Int>[2];
        _corridors[0] = new List<Vector3Int>();
        _corridors[1] = new List<Vector3Int>();
        _matchDirections = new EDoorDirection[]
        {
            EDoorDirection.Down, EDoorDirection.Up, 
            EDoorDirection.Right, EDoorDirection.Left 
        };
        _newDoorOffsets = new Vector3Int[]
        {
            Vector3Int.up, Vector3Int.down,
            Vector3Int.left, Vector3Int.right
        };
        InitialiseRooms();
    }

    private void Start()
    {
        GenerateLevelGraph();

        // Traverse level graph
        GenerateLevel();
    }

    // TEMP
    private void GenerateLevelGraph()
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
        roomID = _levelGraph.ConnectNewRoomToAnother(ERoomType.Normal, roomID);
        _levelGraph.ConnectNewRoomToAnother(ERoomType.Shop, prevRoomID);
        _levelGraph.ConnectNewRoomToPrev(ERoomType.Teleport);
        _levelGraph.ConnectNewRoomToPrev(ERoomType.Normal);
        _levelGraph.ConnectNewRoomToPrev(ERoomType.Normal);
        _levelGraph.ConnectNewRoomToPrev(ERoomType.Teleport);
        _levelGraph.ConnectNewRoomToPrev(ERoomType.Normal);
        _levelGraph.ConnectNewRoomToPrev(ERoomType.Normal);
        _levelGraph.ConnectNewRoomToPrev(ERoomType.Teleport);
        roomID = _levelGraph.ConnectNewRoomToAnother(ERoomType.Teleport, roomID);
        _levelGraph.ConnectNewRoomToPrev(ERoomType.Normal);
        _levelGraph.ConnectNewRoomToPrev(ERoomType.Normal);
        _levelGraph.ConnectNewRoomToPrev(ERoomType.Teleport);
        _levelGraph.ConnectNewRoomToAnother(ERoomType.Normal, roomID);
        _levelGraph.ConnectNewRoomToPrev(ERoomType.Normal);
        _levelGraph.ConnectNewRoomToPrev(ERoomType.Teleport);
        _levelGraph.ConnectNewRoomToPrev(ERoomType.Normal);

        // +1 for the virtual first room
        _openDoorInfos = new List<List<SDoorInfo>>(_levelGraph.GetNumRooms() + 1);
        for (int i = 0; i < _openDoorInfos.Capacity; i++) _openDoorInfos.Add(new List<SDoorInfo>());
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
        _openDoorInfos[_levelGraph.GetNumRooms()] = new List<SDoorInfo>{
                 new SDoorInfo(EConnectionType.Vertical, EDoorDirection.Up, new Vector3Int(500, 499, 0)),
                 new SDoorInfo(EConnectionType.Vertical, EDoorDirection.Down, new Vector3Int(500, 501, 0)),
                 new SDoorInfo(EConnectionType.Horizontal, EDoorDirection.Left, new Vector3Int(501, 500, 0)),
                 new SDoorInfo(EConnectionType.Horizontal, EDoorDirection.Right, new Vector3Int(499, 500, 0)) };

        SRoomInfo roomInfo = new SRoomInfo(_levelGraph.GetNumRooms(), _levelGraph.GetStartRoom());

        List<bool> visited = new List<bool>(new bool[_levelGraph.GetNumRooms()]);
        Queue<SRoomInfo> toVisit = new();
        toVisit.Enqueue(roomInfo);

        // Procedurally generate rooms
        while (toVisit.Count > 0)
        {
            // Get the next room to place
            roomInfo = toVisit.Dequeue();
            int currRoomID = roomInfo.RoomID;

            // If already placed, skip
            if (visited[currRoomID]) continue;
            visited[roomInfo.RoomID] = true;

            // Place the room
            var res = PlaceRoom(roomInfo.PrevRoomID, currRoomID);
            if (!res) continue;

            // Add its connected rooms to the visit queue
            var nextRoomIDs = _levelGraph.GetConnectedRooms(currRoomID);
            foreach (var nextRoomID in nextRoomIDs)
            {
                toVisit.Enqueue(new SRoomInfo(roomInfo.RoomID, nextRoomID));
            }
        }
    }

    private bool PlaceRoom(int prevRoomID, int currRoomID)
    {
        ERoomType roomType = _levelGraph.GetRoomType(currRoomID);
        var rnd = new System.Random();
        RoomBase roomToPlace = null;
        Door newDoorToPlace = null;
        Vector3Int newDoorPos = Vector3Int.zero;

        // Pick a room from templates
        var templates = _roomsByType[(int)roomType];
        if (templates.Count == 0)
        {
            Debug.LogError("No templates for [" + roomType.ToString() + "]. Generate templates.");
            return false;
        }

        if (_openDoorInfos[prevRoomID].Count == 0)
        {
            Debug.LogError("Failed to connect room [" + roomType.ToString() + "][" + currRoomID + " to type ["
                + _levelGraph.GetRoomType(prevRoomID) + "][" + prevRoomID + "]\nThere is no door in the previous room.");
            return false;
        }

        // Select a random room that can be placed
        HashSet<int> triedRoomIdxs = new HashSet<int>();
        while (roomToPlace == null)
        {
            if (triedRoomIdxs.Count == templates.Count)
            {
                Debug.LogError("Failed to connect room [" + roomType.ToString() + "][" + currRoomID + " to type ["
                    + _levelGraph.GetRoomType(prevRoomID) + "][" + prevRoomID + "]\nNot enough space or not enough door in the new room");
                return false;
            }
            
            // Randomly choose a room from the templates
            int roomIdx;
            do { roomIdx = Random.Range(0, templates.Count); }
            while (triedRoomIdxs.Contains(roomIdx));
            triedRoomIdxs.Add(roomIdx);
            roomToPlace = templates[roomIdx];

            // Check if the room can be placed
            // Iterate over all previous doors and all new doors
            bool canBePlaced = false;
            var newDoors = roomToPlace.GetAllDoors();

            // Shuffle the door indices order for randomness            
            IEnumerable<int>[] shuffledNewDoorsIdxs = {
                Enumerable.Range(0, newDoors[0].Count).OrderBy(x => rnd.NextDouble()).ToArray(),
                Enumerable.Range(0, newDoors[1].Count).OrderBy(x => rnd.NextDouble()).ToArray(),
            };

            foreach (var prevDoor in _openDoorInfos[prevRoomID])
            {
                var newDoorsSameType = newDoors[(int)prevDoor.ConnectionType];

                // Filter different door connection type
                if (newDoorsSameType.Count == 0) continue;

                // Try every door of the same connection type
                newDoorPos = prevDoor.Position + _newDoorOffsets[(int)prevDoor.Direction];
                EDoorDirection matchDirection = _matchDirections[(int)prevDoor.Direction];

                foreach (var doorIdx in shuffledNewDoorsIdxs[(int)prevDoor.ConnectionType])
                {
                    newDoorToPlace = newDoorsSameType[doorIdx];

                    // TEMP FLIP
                    if (newDoorToPlace.Direction != matchDirection) continue;

                    // Check if the door can be placed
                    if (CanRoomBePlaced(roomToPlace, newDoorToPlace, newDoorPos))
                    {
                        canBePlaced = true;
                        _openDoorInfos[prevRoomID].Remove(prevDoor);
                        break;
                    }
                }
                if (canBePlaced) break;
            }

            // Stop searching if a room can be placed
            // If not, keep searching
            if (!canBePlaced) roomToPlace = null;
        }

        // Place the found room
        AddRoomToWorld(roomToPlace, newDoorToPlace, newDoorPos);
        _openDoorInfos[currRoomID] = GetDoorWorldPositionsExceptNew(roomToPlace, newDoorToPlace, newDoorPos);

        return true;
    }

    private bool CanRoomBePlaced(RoomBase room, Door newDoor, Vector3Int doorWorldPos)
    {
        var wallTilemap = room.Tilemaps[(int)ETilemapType.Wall];
        wallTilemap.CompressBounds();
        BoundsInt localBounds = wallTilemap.cellBounds;
        Vector3Int doorLocalPos = newDoor.GetLocalPosition();
        Vector3Int maxYPos = Vector3Int.back;
        List<Vector3Int> toFillPositions = new();
        bool canBePlaced = true;

        int count = 0;

        // Check wall tiles
        foreach (var pos in localBounds.allPositionsWithin)
        {
            if (wallTilemap.HasTile(pos))
            {
                Vector3Int tileLocalPos = new Vector3Int(pos.x, pos.y, 0);
                //Vector3Int worldPos = Vector3Int.FloorToInt(wallTilemap.LocalToWorld(localPos));
                Vector3Int newWorldPos = doorWorldPos - doorLocalPos + tileLocalPos;

                if (maxYPos == Vector3Int.back && pos.y == localBounds.yMax - 1)
                {
                    maxYPos = newWorldPos;
                }
                if (_superGrid[newWorldPos.y, newWorldPos.x] == ECellType.Filled)
                {
                    canBePlaced = false;
                    break;
                }
                else
                {
                    _superGrid[newWorldPos.y, newWorldPos.x] = ECellType.ToBeFilled;
                    toFillPositions.Add(newWorldPos);
                    count++;
                }
            }
        }
        
        // Check inner tiles
        if (canBePlaced)
        {
            // Graph traversal to check the super grid
            // Start search at the first diagonal cell from the top left corner
            Queue<Vector3Int> toVisits = new Queue<Vector3Int>();
            toVisits.Enqueue(maxYPos + Vector3Int.right + Vector3Int.down);

            // Four directions to search from each cell
            Vector3Int[] offsets = { Vector3Int.left, Vector3Int.right, Vector3Int.up, Vector3Int.down };

            while (toVisits.Count > 0)
            {
                Vector3Int curr = toVisits.Dequeue();

                switch (_superGrid[curr.y, curr.x])
                {
                    // If the cell is already filled, the room cannot be placed
                    case ECellType.Filled:
                        {
                            canBePlaced = false;
                            break;
                        }

                    // If the search hits the wall of this new room, no further search in this cell
                    case ECellType.ToBeFilled:
                        {
                            break;
                        }

                    // If empty cell, propagate search to nearby cells
                    case ECellType.Empty:
                        {
                            _superGrid[curr.y, curr.x] = ECellType.ToBeFilled;
                            count++;
                            foreach (var offset in offsets)
                            {
                                toVisits.Enqueue(curr + offset);
                            }
                            break;
                        }
                }            
            }
        }

        // Update the super grid
        ECellType updateType = canBePlaced ? ECellType.Filled : ECellType.Empty;
        foreach (var pos in toFillPositions)
        {
            _superGrid[pos.y, pos.x] = updateType;
        }
        if (canBePlaced) Debug.Log("Total: " + count);

        return canBePlaced;
    }

    private List<SDoorInfo> GetDoorWorldPositionsExceptNew(RoomBase room, Door newDoor, Vector3Int doorWorldPos)
    {
        List<Door>[] doors = room.GetAllDoors();
        List<SDoorInfo> doorInfos = new();
        SDoorInfo doorInfo = new SDoorInfo();
        Vector3Int doorLocalPos = newDoor.GetLocalPosition();

        for (int i = 0; i < doors.Length; i++)
        {
            foreach (Door door in doors[i])
            {
                if (door == newDoor && room.RoomType != ERoomType.Entrance)
                {
                    continue;
                }
                doorInfo.ConnectionType = door.ConnectionType;
                doorInfo.Direction = door.Direction;
                doorInfo.Position = doorWorldPos - doorLocalPos + door.GetLocalPosition();
                doorInfos.Add(doorInfo);
            }
        }

        return doorInfos;
    }

    private void AddRoomToWorld(RoomBase room, Door door, Vector3Int doorWorldPos)
    {
        // Paint all layers
        Vector3Int doorLocalPos = door.GetLocalPosition();
        for (int i = 0; i < (int)ETilemapType.MAX; i++)
        {
            room.Tilemaps[i].CompressBounds();
            BoundsInt localBounds = room.Tilemaps[i].cellBounds;
            BoundsInt worldBounds = localBounds;
            worldBounds.SetMinMax(doorWorldPos - doorLocalPos + localBounds.min,
                doorWorldPos - doorLocalPos + localBounds.max);

            var tiles = room.Tilemaps[i].GetTilesBlock(localBounds);
            Tilemaps[i].SetTilesBlock(worldBounds, tiles);
        }

        // Add corridor to the list except for the first room (entrance)
        if (room.RoomType != ERoomType.Entrance)
        {
            _corridors[(int)door.ConnectionType].Add(doorWorldPos);
            
            // Remove wall tile at the corridor position -> New door
            var otherDoorPos = door.ConnectionType == EConnectionType.Vertical ?
            new Vector3Int(doorWorldPos.x + 1, doorWorldPos.y) : new Vector3Int(doorWorldPos.x, doorWorldPos.y + 1);
            Tilemaps[(int)ETilemapType.Wall].SetTile(doorWorldPos, null);
            Tilemaps[(int)ETilemapType.Wall].SetTile(otherDoorPos, null);

            // Remove wall tile at the corridor position -> Previous door
            Tilemaps[(int)ETilemapType.Wall].SetTile(doorWorldPos + _newDoorOffsets[(int)door.Direction], null);
            Tilemaps[(int)ETilemapType.Wall].SetTile(otherDoorPos + _newDoorOffsets[(int)door.Direction], null);


            Debug.Log("New Door removed [" + door.Direction.ToString() + "] at "
                + (doorWorldPos)
                + " " + (otherDoorPos));

            Debug.Log("Prev Door removed [" + door.Direction.ToString() + "] at "
                + (doorWorldPos + _newDoorOffsets[(int)door.Direction])
                + " "  + (otherDoorPos + _newDoorOffsets[(int)door.Direction]));
        }

        Debug.Log("Background: " + room.Tilemaps[(int)ETilemapType.Background].GetTilesRangeCount(
            room.Tilemaps[(int)ETilemapType.Background].cellBounds.min,
            room.Tilemaps[(int)ETilemapType.Background].cellBounds.max));
    }
}

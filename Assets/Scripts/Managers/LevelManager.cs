using System;
using Cinemachine;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class LevelManager : Singleton<LevelManager>
{ 
    private int _maxHeight;
    private int _maxWidth;

    private Transform _roomsContainer;
    private List<GameObject> _generatedRooms;
    private Object[] _roomTemplates;
    private List<List<RoomBase>> _roomsByType;

    private ECellType[,] _superGrid;
    private List<Vector3Int>[] _corridors;
    private List<List<SDoorInfo>> _openDoorInfos;

    private LevelGraph _levelGraph;
    // TODO remove TEMP
    private Vector3 _playerSpawnPos = Vector3.back;
    
    private Tilemap _mapTilemap;
    private Tilemap _superWallTilemap;
    public TileBase WallRuleTile;
    
    // minimapTiles
    [SerializeField] private TileBase[] MinimapTiles;
    private EDoorDirection[] _matchDirections;
    Vector3Int[] _newDoorOffsets;
    
    // Clockwork spawners
    [SerializeField] private int clockworkLimit = 10;
    private List<ClockworkSpawner[]> _clockworkSpawnersByRoom;

    protected override void Awake()
    {
        base.Awake();
        if (_toBeDestroyed) return;
        
        _maxHeight = 1000;
        _maxWidth = 1000;
        _superGrid = new ECellType[_maxHeight, _maxWidth];
        _corridors = new[]
        {
            new List<Vector3Int>(), new List<Vector3Int>()
        };
        _matchDirections = new[]
        {
            EDoorDirection.Down, EDoorDirection.Up, 
            EDoorDirection.Right, EDoorDirection.Left 
        };
        _newDoorOffsets = new[]
        {
            Vector3Int.up, Vector3Int.down,
            Vector3Int.left, Vector3Int.right
        };
        _generatedRooms = new List<GameObject>();

        InitialiseRooms();
        GenerateLevelGraph();
    }
    
    public void Generate()
    {
        // Reset values
        Array.Clear(_superGrid, 0, _superGrid.Length);
        _corridors[0].Clear();
        _corridors[1].Clear();
        _generatedRooms.Clear();
        _clockworkSpawnersByRoom = new List<ClockworkSpawner[]>();
        _roomsContainer = GameObject.Find("Rooms").transform;
        _mapTilemap = GameObject.Find("Map").GetComponent<Tilemap>();
        _superWallTilemap = GameObject.FindWithTag("Ground").GetComponent<Tilemap>();
        
        // Level generation
        GenerateLevel();
        PostProcessLevel();
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
        _levelGraph.ConnectNewRoomToPrev(ERoomType.MidBoss);
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
            roomBase.Tilemaps[(int)ETilemapType.Wall].CompressBounds();
            _roomsByType[(int)roomBase.RoomType].Add(roomBase);            
        }
    }

    private void GenerateLevel()
    {
        // Clear tiles
        _superWallTilemap.ClearAllTiles();
        //MapTilemap.ClearAllTiles();

        // Add starting room
        _openDoorInfos[_levelGraph.GetNumRooms()] = new List<SDoorInfo>{
                 new SDoorInfo(EConnectionType.Vertical, EDoorDirection.Up, new Vector3Int(500, 499, 0)),
                 new SDoorInfo(EConnectionType.Vertical, EDoorDirection.Down, new Vector3Int(500, 501, 0)),
                 new SDoorInfo(EConnectionType.Horizontal, EDoorDirection.Left, new Vector3Int(501, 500, 0)),
                 new SDoorInfo(EConnectionType.Horizontal, EDoorDirection.Right, new Vector3Int(499, 500, 0)) };
        
        SRoomInfo roomInfo = new SRoomInfo(_levelGraph.GetNumRooms(), _levelGraph.GetStartRoom());
        List<bool> visited = new List<bool>(new bool[_levelGraph.GetNumRooms()]);
        Stack<SRoomInfo> toVisit = new Stack<SRoomInfo>();
        toVisit.Push(roomInfo);

        // Procedurally generate rooms
        while (toVisit.Count > 0)
        {
            // Get the next room to place
            roomInfo = toVisit.Pop();
            int currRoomID = roomInfo.RoomID;

            // If already placed, skip
            if (visited[currRoomID]) continue;
            visited[roomInfo.RoomID] = true;

            // Place the room
            var res = PlaceRoom(roomInfo.PrevRoomID, currRoomID);
            if (!res) continue;

            // Add its connected rooms to the visit stack
            var nextRoomIDs = _levelGraph.GetConnectedRooms(currRoomID);
            foreach (var nextRoomID in nextRoomIDs)
            {
                toVisit.Push(new SRoomInfo(roomInfo.RoomID, nextRoomID));
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
            Debug.LogError("No templates for [" + roomType + "]. Generate templates.");
            return false;
        }

        if (_openDoorInfos[prevRoomID].Count == 0)
        {
            Debug.LogError("Failed to connect room [" + roomType + "][" + currRoomID + "] to type ["
                + _levelGraph.GetRoomType(prevRoomID) + "][" + prevRoomID + "]\nThere is no door in the previous room.");
            return false;
        }

        // Select a random room that can be placed
        HashSet<int> triedRoomIdxs = new HashSet<int>();
        while (roomToPlace == null)
        {
            if (triedRoomIdxs.Count == templates.Count)
            {
                Debug.LogError("Failed to connect room [" + roomType + "][" + currRoomID + "] to type ["
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
        AddRoomToMinimap(roomToPlace, newDoorToPlace, newDoorPos);
        _openDoorInfos[currRoomID] = GetDoorWorldPositionsExceptNew(roomToPlace, newDoorToPlace, newDoorPos);

        return true;
    }

    private bool CanRoomBePlaced(RoomBase room, Door newDoor, Vector3Int doorWorldPos)
    {
        var wallTilemap = room.Tilemaps[(int)ETilemapType.Wall];
        BoundsInt localBounds = wallTilemap.cellBounds;
        Vector3Int doorLocalPos = newDoor.GetLocalPosition();
        Vector3Int maxYPos = Vector3Int.back;
        List<Vector3Int> toFillPositions = new();
        bool canBePlaced = true;

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
        // Instantiate room
        Vector3Int genPosition = doorWorldPos - door.GetLocalPosition();
        var roomObj = Instantiate(room.gameObject, genPosition, Quaternion.identity);
        roomObj.transform.parent = _roomsContainer;
        _generatedRooms.Add(roomObj);

        // Hide doors
        roomObj.GetComponent<RoomBase>().HideAllDoors();

        // Draw the wall layer in the main tilemap layer
        var roomWallTilemap = roomObj.GetComponent<RoomBase>().Tilemaps[(int)ETilemapType.Wall];
        roomWallTilemap.GetComponent<TilemapRenderer>().enabled = false;

        Vector3Int doorLocalPos = door.GetLocalPosition();
        BoundsInt localBounds = roomWallTilemap.cellBounds;
        BoundsInt worldBounds = localBounds;
        worldBounds.SetMinMax(doorWorldPos - doorLocalPos + localBounds.min,
            doorWorldPos - doorLocalPos + localBounds.max);
        _superWallTilemap.SetTilesBlock(worldBounds, roomWallTilemap.GetTilesBlock(localBounds));

        // Add corridor to the list except for the first room (entrance)
        if (room.RoomType != ERoomType.Entrance)
        {
            _corridors[(int)door.ConnectionType].Add(doorWorldPos);
            
            // Remove wall tile at the corridor position -> New door
            var otherDoorPos = door.ConnectionType == EConnectionType.Vertical ?
            new Vector3Int(doorWorldPos.x + 1, doorWorldPos.y) : new Vector3Int(doorWorldPos.x, doorWorldPos.y + 1);
            _superWallTilemap.SetTile(doorWorldPos, null);
            _superWallTilemap.SetTile(otherDoorPos, null);

            // Remove wall tile at the corridor position -> Previous door
            _superWallTilemap.SetTile(doorWorldPos + _newDoorOffsets[(int)door.Direction], null);
            _superWallTilemap.SetTile(otherDoorPos + _newDoorOffsets[(int)door.Direction], null);
        }
        
        // Save spawners
        _clockworkSpawnersByRoom.Add(roomObj.transform.GetComponentsInChildren<ClockworkSpawner>());
    }

    private void PostProcessLevel()
    {
        //AddSurroundingWallTiles();
        GenerateMinimap();
        SetClockworkSpawners();
    }
    
    private void SetClockworkSpawners()
    {
        // Split in the middle
        var roomsCount = _clockworkSpawnersByRoom.Count;
        var roomMid = roomsCount % 2 == 0 ? roomsCount / 2 - 1 : roomsCount / 2; // the last room index to be included in the first half

        // Combine spawners into a list
        int[] fixedSpawnerCounts = {0,0};
        List<ClockworkSpawner>[] availableSpawners = { new List<ClockworkSpawner>(), new List<ClockworkSpawner>() };

        int[] sectionStarts = { 0, roomMid + 1 };
        int[] sectionEnds = { roomMid + 1, roomsCount };
        for (int section = 0; section < 2; section++)
        {
            for (int roomIdx = sectionStarts[section]; roomIdx < sectionEnds[section]; roomIdx++)
            {
                foreach (var spawner in _clockworkSpawnersByRoom[roomIdx])
                {
                    if (!spawner.isFixedSpawn) availableSpawners[section].Add(spawner);
                    else fixedSpawnerCounts[section]++;
                }
            }
        }
        
        // Compute the number of clockwork spawners to activate in each section
        int totalSpawnCount = Mathf.Min(clockworkLimit - fixedSpawnerCounts.Sum(), availableSpawners[0].Count + availableSpawners[1].Count);
        double[] spawnDistributions = {0.6, 0.4};
        int[] spawnCounts = new int[spawnDistributions.Length];
        int sumOfPreviousGroups = 0;

        for (int i = 0; i < spawnDistributions.Length; i++)
        {
            if (i == spawnDistributions.Length - 1)
            {
                spawnCounts[i] = totalSpawnCount - sumOfPreviousGroups;
            }
            else
            {
                spawnCounts[i] = (int)(totalSpawnCount * spawnDistributions[i]);
                sumOfPreviousGroups += spawnCounts[i];
            }
        }
        
        // Randomly select clockwork spawners to activate
        var rand = new System.Random();
        for (int section = 0; section < 2; section++)
        {
            var shuffledSpawners = availableSpawners[section].OrderBy(x => rand.Next()).ToList();
            
            // Destroy spawners that are not selected
            for (int i = spawnCounts[section]; i < availableSpawners[section].Count; i++)
            {
                Destroy(shuffledSpawners[i].gameObject);
            }
        }
    }

    private void AddSurroundingWallTiles()
    {
        _superWallTilemap.CompressBounds();
        var wallBounds = _superWallTilemap.cellBounds;
        wallBounds.SetMinMax(wallBounds.min + Vector3Int.left * 9 + Vector3Int.down * 4,
            wallBounds.max + Vector3Int.right * 10 + Vector3Int.up * 5);

        // Graph traversal to fill the unused wall area
        Queue<Vector3Int> toVisit = new Queue<Vector3Int>();
        toVisit.Enqueue(wallBounds.min);

        // Four directions to search from each cell
        Vector3Int[] offsets = { Vector3Int.left, Vector3Int.right, Vector3Int.up, Vector3Int.down };

        while (toVisit.Count > 0)
        {
            var curr = toVisit.Dequeue();

            // If out of bounds, skip
            if (curr.x < wallBounds.xMin || curr.y < wallBounds.yMin || curr.x > wallBounds.xMax || curr.y > wallBounds.yMax)
            {
                continue;
            }

            // If is filled by wall, skip
            if (_superWallTilemap.HasTile(curr))
            {
                continue;
            }

            // Fill by wall
            _superWallTilemap.SetTile(curr, WallRuleTile);

            // Add neighbour cells
            foreach (var offset in offsets)
            {
                toVisit.Enqueue(curr + offset);
            }
        }
    }

    public void SpawnPlayer()
    {
        // TODO commented for debug
        // var playerStart = _generatedRooms[0].transform.Find("PlayerStart");
        // var player = PlayerController.Instance == null ? 
        //     Instantiate(GameManager.Instance.Player) : PlayerController.Instance.gameObject;
        // player.transform.position = playerStart.position;
        // GameObject.Find("Virtual Camera").GetComponent<CinemachineVirtualCamera>().Follow = player.transform;
        // PlayerEvents.spawned.Invoke();

        // DEBUG TEMP CODE FROM HERE
        PlayerController playerController = PlayerController.Instance;
        Transform playerObject = null;
        
        if (playerController == null)
        {
            playerObject = Instantiate(GameManager.Instance.Player).gameObject.transform;
            _playerSpawnPos = GameObject.Find("PlayerStart").transform.position;
        }
        else
        {
            playerObject = playerController.gameObject.transform;
    
            // If player spawn pos undefined (first game run + not random generated map (debug map))
            if (_playerSpawnPos == Vector3.back)
                _playerSpawnPos = playerObject.position;
        }
        playerObject.position = _playerSpawnPos;
        GameObject.Find("Virtual Camera").GetComponent<CinemachineVirtualCamera>().Follow = playerObject;
        
        PlayerEvents.spawned.Invoke();
    }

    private void GenerateMinimap()
    {
        _superWallTilemap.CompressBounds();

        foreach (var localPos in _superWallTilemap.cellBounds.allPositionsWithin)
        {   
            if (_superWallTilemap.HasTile(localPos))
                _mapTilemap.SetTile(localPos, MinimapTiles[1]);
        }
    }

    private void AddRoomToMinimap(RoomBase room, Door door, Vector3Int doorWorldPos)
    {
        Vector3Int doorLocalPos = door.GetLocalPosition();

        // In the order of background -> collideable
        for (int layerIdx = 2; layerIdx > 0; layerIdx--)
        {
            var tilemap = room.Tilemaps[layerIdx];
            tilemap.CompressBounds();

            List<Vector3Int> tileWorldPositions = new List<Vector3Int>();
            List<TileBase> tileBases = new List<TileBase>();
            foreach (var localPos in tilemap.cellBounds.allPositionsWithin)
            {   
                if (tilemap.HasTile(localPos))
                    tileWorldPositions.Add(doorWorldPos - doorLocalPos + localPos);
            }
            for (int i = 0; i < tileWorldPositions.Count; i++)
                tileBases.Add(MinimapTiles[layerIdx]);
            
            _mapTilemap.SetTiles(tileWorldPositions.ToArray(), tileBases.ToArray());
        }
    }
}

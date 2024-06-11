using System;
using System.Collections;
using Cinemachine;
using System.Collections.Generic;
using System.Linq;
using Pathfinding;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class LevelManager : Singleton<LevelManager>
{ 
    private int _maxHeight;
    private int _maxWidth;

    private LevelGraph _levelGraph;
    private List<GameObject> _generatedRooms;
    private Object[] _roomTemplates;
    private List<List<RoomBase>> _roomsByType;

    private ECellType[,] _superGrid;
    private List<Vector3Int>[] _corridors;
    private List<List<Door>> _availableDoors;

    private Tilemap _mapTilemap;
    private Tilemap _superWallTilemap;
    private Transform _roomsContainer;
    public TileBase WallRuleTile;
    
    // minimapTiles
    [SerializeField] private TileBase[] MinimapTiles;
    
    // helpers
    private EDoorDirection[] _matchDirections;
    private Vector3Int[] _connectedDoorOffsets;
    private Vector3Int[] _traverseDirections;
    
    // Spawners
    [SerializeField] private int clockworkLimit = 10;
    private List<ClockworkSpawner[]> _clockworkSpawnersByRoom;
    [SerializeField] [NamedArray(typeof(EFlowerType))] private GameObject[] flowerPrefabs;
    private List<Vector3> _flowerSpawners;
    private List<HiddenPortal> _hiddenPortalSpawners;
    
    // A Star
    private bool _aStarScanEnded;
    private bool _levelGenerationEnded;
    
    // Secret Rooms
    private List<HiddenRoom>[] _hiddenRoomsByLevel;
    
    // Spawn points
    private Vector3 _metaSpawnPoint;
    public Transform CombatSpawnPoint { get; private set; }
    
    protected override void Awake()
    {
        base.Awake();
        if (IsToBeDestroyed) return;
        
        _maxHeight = 2500;
        _maxWidth = 2500;
        _superGrid = new ECellType[_maxHeight, _maxWidth];
        _corridors = new[]
        {
            new List<Vector3Int>(), new List<Vector3Int>(),
        };
        _matchDirections = new[] 
        {
            EDoorDirection.Down, EDoorDirection.Up, EDoorDirection.Right, EDoorDirection.Left,
        };
        _connectedDoorOffsets = new[]
        {
            Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right,
        };
        _traverseDirections = new[]
        {
            Vector3Int.right, Vector3Int.right, Vector3Int.up, Vector3Int.up,
        };
        _generatedRooms = new List<GameObject>();

        // Spawners
        _flowerSpawners = new List<Vector3>();
        _hiddenPortalSpawners = new List<HiddenPortal>();
        
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
        _flowerSpawners.Clear();
        _hiddenPortalSpawners.Clear();
        
        // Find game objects
        Transform root = GameObject.Find("SuperTilemaps").transform;
        _mapTilemap = root.Find("Map").GetComponent<Tilemap>();
        _superWallTilemap = root.Find("SuperWall").GetComponent<Tilemap>();
        _roomsContainer = GameObject.Find("Rooms").transform;
        _clockworkSpawnersByRoom = new List<ClockworkSpawner[]>();
        _metaSpawnPoint = GameObject.FindWithTag("PlayerStart_Meta").transform.position;
        
        // Level generation
        _levelGenerationEnded = false;
        _aStarScanEnded = false;
        GenerateLevel();
    }

    // TEMP
    private void GenerateLevelGraph()
    {
        // Populate Level Graph
        _levelGraph = new LevelGraph();
        _levelGraph.SetDefaultType();

        // +1 for the virtual first room
        _availableDoors = new List<List<Door>>(_levelGraph.GetNumRooms() + 1);
        for (int i = 0; i < _availableDoors.Capacity; i++) _availableDoors.Add(new List<Door>());
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
        // Add starting room
        var startingVirtualRoom = new GameObject("StartingRoom");
        var door1 = startingVirtualRoom.AddComponent<Door>();
        var door2 = startingVirtualRoom.AddComponent<Door>();
        var door3 = startingVirtualRoom.AddComponent<Door>();
        var door4 = startingVirtualRoom.AddComponent<Door>();
        door1.SetValues(EConnectionType.Vertical, EDoorDirection.Up, new Vector3Int(1000, 999, 0), 3,4,6);
        door2.SetValues(EConnectionType.Vertical, EDoorDirection.Down, new Vector3Int(1000, 1001, 0), 3,4,6);
        door3.SetValues(EConnectionType.Horizontal, EDoorDirection.Left, new Vector3Int(501, 1000, 0), 3,4,6);
        door4.SetValues(EConnectionType.Horizontal, EDoorDirection.Right, new Vector3Int(999, 1000, 0), 3,4,6);
        _availableDoors[_levelGraph.GetNumRooms()] = new List<Door>{ door1, door2, door3, door4 };
        
        // Initialise
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
        CombatSpawnPoint = _generatedRooms[0].transform.Find("PlayerStart");
        Debug.AssertFormat(CombatSpawnPoint != null, "Player spawn point is not set in the entrance room. Please add the prefab '/Prefabs/Player/PlayerStart' to the room prefab.");
        
        _levelGenerationEnded = true;
        StartCoroutine(LevelGenerationCoroutine());
    }

    private bool PlaceRoom(int prevRoomID, int currRoomID)
    {
        // If the previous room has no door to connect to, return
        ERoomType roomType = _levelGraph.GetRoomType(currRoomID);
        if (_availableDoors[prevRoomID].Count == 0)
        {
            Debug.LogError("Failed to connect room [" + roomType + "][" + currRoomID + "] to type ["
                + _levelGraph.GetRoomType(prevRoomID) + "][" + prevRoomID + "]\nThere is no door in the previous room.");
            return false;
        }
        
        // Start searching for the next room
        RoomBase newRoom = null;
        Door newDoorChosen = null;

        // Select the room templates of the desired room type
        var templates = _roomsByType[(int)roomType];
        
        // No template of the desired room type?
        if (templates.Count == 0)
        {
            Debug.LogError("No templates for the type [" + roomType + "]. Generate templates.");
            return false;
        }
        
        // Shuffle the rooms for randomness
        List<int> shuffledRoomIdxs = Utility.ShuffleList(Enumerable.Range(0, templates.Count).ToList());
        int tryIdx = 0;
        
        // Select a random room that can be placed
        while (newRoom == null)
        {
            if (tryIdx == templates.Count)
            {
                Debug.LogError("Failed to connect room [" + roomType + "][" + currRoomID + "] to type ["
                    + _levelGraph.GetRoomType(prevRoomID) + "][" + prevRoomID + "]\nNot enough space or not enough door in the new room");
                // TryAddCorridor()
                return false;
            }
            
            // Randomly choose a room from the templates
            newRoom = templates[shuffledRoomIdxs[tryIdx++]];
            
            // Shuffle the door indices order for randomness
            var shuffledNewDoorIdxs = Utility.ShuffleList(newRoom.GetAllDoorsCombined());

            // Iterate over all previous doors and all new doors
            // Check if the room can be placed
            bool canBePlaced = false;
            foreach (var prevDoor in _availableDoors[prevRoomID])
            {
                // Try every door in the new room
                foreach (Door newDoorCandidate in shuffledNewDoorIdxs)
                {
                    // If doors do not match at all, no need to check for the room placement
                    if (!DoDoorsMatch(prevDoor, newDoorCandidate)) continue;
                    
                    // If the room cannot be placed (space already occupied), skip
                    if (!CanRoomBePlaced(newRoom, prevDoor, newDoorCandidate)) continue;
                    
                    // The room can be connected by this door
                    _availableDoors[prevRoomID].Remove(prevDoor);
                    newDoorChosen = newDoorCandidate;
                    canBePlaced = true;
                    break;
                }
                if (canBePlaced) break;
            }

            // Stop searching if a room can be placed
            // If not, keep searching
            if (!canBePlaced) newRoom = null;
        }

        // Place the found room
        var instantiatedRoom = AddRoomToWorld(newRoom, newDoorChosen);
        AddRoomToMinimap(newRoom, newDoorChosen);
        _availableDoors[currRoomID] = GetAvailableDoorWorldPositions(instantiatedRoom.GetComponent<RoomBase>(), newDoorChosen);

        return true;
    }

    private bool DoDoorsMatch(Door prevDoor, Door newDoor)
    {
        // If the directions do not match, the two doors cannot be connected
        EDoorDirection matchingDoorDirection = _matchDirections[(int)prevDoor.Direction];
        if (newDoor.Direction != matchingDoorDirection) return false;
        
        // Do the door sizes match?
        if (newDoor.minSize > prevDoor.maxSize) return false;
        
        // Otherwise, worth matching rooms
        return true;
    }
    
    // Naive approach (to be optimised later.. hopefully)
    //  1. We have one previous door and one new door candidate, each with min and max door size, and total size (width/height)
    //  2. Match the bottom leftmost positions of the two doors
    //  3. For each door size (from smaller max -> larger min) x, => Prefer larger door size
    //       a) For each possible door position (start from the leftmost bottom +- x, depending on the connection type)
    //           b) Try connect the two rooms
    //           c) If succeed, stop. Remove tiles at the door positions.
    //           d) If fail, move to the next possible door position
    //              -> Optimisation: Is there a need to try different door position?
    //                  * In the horizontal connection type, we traverse bottom to up.
    //                    If the blocking tile is above the smaller door, no need to traverse.
    //                    If the blocking tile is ... more pre-checks
    //                  * In the vertical connection type, we traverse left to right.
    //                    If the blocking tile is on the right of the smaller door, no need to traverse.
    //       e) If all possible door positions are tried and still no success, decrement door size by 1. Go back to step (a).
    //  4. If all possible door sizes are tried and yielded no success, move on to the next new door candidate. Go back to step (1).
    
    private bool CanRoomBePlaced(RoomBase newRoom, Door prevDoor, Door newDoor)
    {
        // Match the bottom left position of the two doors first. This is our starting position
        Vector3Int newDoorBlPosWorld = prevDoor.RangeWorldBLPosition + _connectedDoorOffsets[(int)prevDoor.Direction];
        
        // The range of door sizes that are possible from the pair
        int smallestPossibleSize = Math.Max(prevDoor.minSize, newDoor.minSize);
        int largestPossibleSize = Math.Min(prevDoor.maxSize, newDoor.maxSize);
        //int newDoorSize = Math.Min(prevDoor.doorSize, newDoor.doorSize);
        
        // Iterate from the largest door size (startDoorSize) to the smallest door size (endDoorSize)
        HashSet<Vector3Int> triedBLPositions = new HashSet<Vector3Int>();
        for (int doorSize = /*newDoorSize*/largestPossibleSize; doorSize >= /*newDoorSize*/smallestPossibleSize; doorSize--)
        {
            // From the starting position, iterate until the end position, which is prevDoor's opposite end - doorSize
            // Fix the new door position from the new door possible range
            int maxRange = prevDoor.CandidateRange - doorSize + 1;
            for (int offset = 0; offset < maxRange; offset++)
            {
                Vector3Int matchBlPosWorld = newDoorBlPosWorld + offset * _traverseDirections[(int)prevDoor.ConnectionType];
                Vector3Int matchBlPosLocal = newDoor.RangeLocalBLPosition + offset * _traverseDirections[(int)prevDoor.ConnectionType];
                if (triedBLPositions.Contains(matchBlPosWorld)) continue;
                triedBLPositions.Add(matchBlPosWorld);
                var blockedWorldPos = GetOverlappingTile(newRoom, matchBlPosLocal, matchBlPosWorld);
                
                // Can this room be placed?
                if (blockedWorldPos == Vector3Int.back)
                {
                    newDoor.DoorLocalBLPosition = matchBlPosLocal;
                    newDoor.DoorWorldBLPosition = matchBlPosWorld;
                    newDoor.DoorSizeReal = doorSize;//newDoorSize;
                    return true;
                }
                
                // Optimisation if cannot be placed
                if (prevDoor.ConnectionType == EConnectionType.Horizontal)
                {
                    // If the blocking tile is above the newDoorBLPos, no need to traverse.
                    // Different prev door pos nor larger new door size will work 
                    // Should go downwards instead, by trying different new door pos
                    if (blockedWorldPos.y > matchBlPosWorld.y) break;
                }
                else
                {
                    if (blockedWorldPos.x > matchBlPosWorld.x) break;
                }
            }
            
            // Fix the prev door position from the prev door possible range
            // Change the new door position instead
            maxRange = newDoor.CandidateRange - doorSize + 1;
            for (int offset = 0; offset < maxRange; offset++)
            {
                Vector3Int matchBlPosWorld = newDoorBlPosWorld - offset * _traverseDirections[(int)prevDoor.ConnectionType];
                Vector3Int matchBlPosLocal = newDoor.RangeLocalBLPosition - offset * _traverseDirections[(int)prevDoor.ConnectionType];
                var blockedWorldPos = GetOverlappingTile(newRoom, matchBlPosLocal, matchBlPosWorld);
                
                // Can room be placed?
                if (blockedWorldPos == Vector3Int.back)
                {
                    newDoor.DoorLocalBLPosition = matchBlPosLocal;
                    newDoor.DoorWorldBLPosition = matchBlPosWorld;
                    newDoor.DoorSizeReal = doorSize;//newDoorSize;
                    return true;
                }
                
                // Optimisation if cannot be placed
                if (prevDoor.ConnectionType == EConnectionType.Horizontal)
                {
                    // If the blocking tile is below the newDoorBLPos, no need to traverse.
                    // Different prev door pos nor larger new door size will work 
                    // Should go upwards instead, by trying different prev door pos
                    if (blockedWorldPos.y < matchBlPosWorld.y) break;
                }
                else
                {
                    if (blockedWorldPos.x < matchBlPosWorld.x) break;
                }
            }
        }
        return false;
    }
    
    
    // If the room can be placed, return Vector3Int.back. Otherwise return the world position Vector3Int of the blocked tile.
    private Vector3Int GetOverlappingTile(RoomBase newRoom, Vector3Int BLNewDoorPosLocal, Vector3Int BLNewDoorPosWorld)
    {
        var newRoomWallTilemap = newRoom.Tilemaps[(int)ETilemapType.Wall];
        BoundsInt newRoomLocalBounds = newRoomWallTilemap.cellBounds;
        List<Vector3Int> toFillPositions = new List<Vector3Int>();
        Vector3Int maxYPos = Vector3Int.back; // Initialise
        Vector3Int blockedTileWorldPos = Vector3Int.back;
        bool canBePlaced = true;

        // Check wall tiles
        foreach (var localPos in newRoomLocalBounds.allPositionsWithin)
        {
            // Only handle wall tiles
            if (!newRoomWallTilemap.HasTile(localPos)) continue;
            
            // Compute the world position of the tile
            Vector3Int tileLocalPos = new Vector3Int(localPos.x, localPos.y, 0);
            Vector3Int newWorldPos = BLNewDoorPosWorld - BLNewDoorPosLocal + tileLocalPos;

            // Update the wall position with the largest Y
            if (maxYPos == Vector3Int.back && localPos.y == newRoomLocalBounds.yMax - 1)
            {
                maxYPos = newWorldPos;
            }
                
            // If the space is already occupied, the room cannot be placed
            if (_superGrid[newWorldPos.y, newWorldPos.x] == ECellType.Filled)
            {
                canBePlaced = false;
                blockedTileWorldPos = newWorldPos;
                break;
            }
            // Otherwise continue
            else
            {
                _superGrid[newWorldPos.y, newWorldPos.x] = ECellType.ToBeFilled;
                toFillPositions.Add(newWorldPos);
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
            Vector3Int[] offsets =
            {
                Vector3Int.left, Vector3Int.right, Vector3Int.up, Vector3Int.down
            };

            while (toVisits.Count > 0)
            {
                Vector3Int curr = toVisits.Dequeue();

                switch (_superGrid[curr.y, curr.x])
                {
                    // If the cell is already filled, the room cannot be placed
                    case ECellType.Filled:
                        {
                            canBePlaced = false;
                            blockedTileWorldPos = curr;
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
        if (canBePlaced)
        {
            foreach (var pos in toFillPositions)
            {
                _superGrid[pos.y, pos.x] = updateType;
            }
        }

        return blockedTileWorldPos;
    }

    private List<Door> GetAvailableDoorWorldPositions(RoomBase newRoom, Door newDoorClosed)
    {
        // Get all available doors of the newly placed room
        newRoom.Initialise();
        List<Door>[] allDoorsNewRoom = newRoom.GetAllDoorsByType();
        List<Door> newAvailableDoors = new List<Door>();
        
        // Update the world position of the possible range of door position
        foreach (var doors in allDoorsNewRoom)
        {
            foreach (Door door in doors)
            {
                if (door == newDoorClosed && newRoom.RoomType != ERoomType.Entrance)
                {
                    continue;
                }
                door.RangeWorldBLPosition = door.RangeLocalBLPosition;
                newAvailableDoors.Add(door);
            }
        }
        
        // Shuffle the doors
        var shuffledDoors = Utility.ShuffleList(newAvailableDoors);
        
        // When done, hide all doors
        newRoom.HideAllDoors();

        // Return the available doors
        return shuffledDoors;
    }

    private GameObject AddRoomToWorld(RoomBase newRoom, Door newDoor)
    {
        // Instantiate room
        Vector3Int genPosition = newDoor.DoorWorldBLPosition - newDoor.DoorLocalBLPosition;
        var roomObj = Instantiate(newRoom.gameObject, genPosition, Quaternion.identity);
        roomObj.transform.parent = _roomsContainer;
        _generatedRooms.Add(roomObj);

        // Draw the wall layer in the main tilemap layer
        var roomWallTilemap = roomObj.GetComponent<RoomBase>().Tilemaps[(int)ETilemapType.Wall];
        roomWallTilemap.GetComponent<TilemapRenderer>().enabled = false;
        Vector3Int doorLocalPos = newDoor.RangeLocalBLPosition;
        BoundsInt localBounds = roomWallTilemap.cellBounds;
        BoundsInt worldBounds = localBounds;
        worldBounds.SetMinMax(newDoor.DoorWorldBLPosition - doorLocalPos + localBounds.min, newDoor.DoorWorldBLPosition - doorLocalPos + localBounds.max);
        _superWallTilemap.SetTilesBlock(worldBounds, roomWallTilemap.GetTilesBlock(localBounds));

        if (newRoom.RoomType != ERoomType.Entrance)
        {
            // Remove wall tiles located at the door position
            for (int offset = 0; offset < newDoor.DoorSizeReal; offset++)
            {
                Vector3Int newDoorPos = newDoor.DoorWorldBLPosition + offset * _traverseDirections[(int)newDoor.Direction];
                Vector3Int prevDoorPos = newDoorPos + _connectedDoorOffsets[(int)newDoor.Direction];
                _superWallTilemap.SetTile(newDoorPos, null);
                _superWallTilemap.SetTile(prevDoorPos, null);
            }
        }
        
        // Save spawners
        _clockworkSpawnersByRoom.Add(roomObj.transform.GetComponentsInChildren<ClockworkSpawner>());
        _flowerSpawners.Add(roomObj.transform.Find("FlowerSpawner").transform.position);
        _hiddenPortalSpawners.AddRange(roomObj.transform.GetComponentsInChildren<HiddenPortal>());

        // Return instantiated room game object
        return roomObj;
    }

    private void PostProcessLevel()
    {
        // AddSurroundingWallTiles();
        GenerateMinimap();
        SetClockworkSpawners();
        SetFlowerSpawners();
        SetHiddenPortalSpawners();
        SetHiddenRooms();
        SetAStarGrid();
    }
    
    private void SetClockworkSpawners()
    {
        // Should not spawn clockworks?
        if (clockworkLimit <= 0)
        {
            foreach (var spawnersInRoom in _clockworkSpawnersByRoom)
            {
                foreach (var spawner in spawnersInRoom)
                {
                    Destroy(spawner.gameObject);
                }
            }
            return;
        }
        
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
        int totalSpawnCount = Math.Max(0, Math.Min(clockworkLimit - fixedSpawnerCounts.Sum(), availableSpawners[0].Count + availableSpawners[1].Count));
        if (totalSpawnCount == 0) return;
        
        // If there are spawners to be spawned, distribute the spawners
        double[] spawnDistributions = {0.6, 0.4}; // 60% spawners in the first half, 40% spawners in the second half
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
        
        // Randomly select clockwork spawners to activate and destroy the unselected ones
        for (int section = 0; section < 2; section++)
        {
            var shuffledSpawners = Utility.ShuffleList(availableSpawners[section]);
            for (int i = spawnCounts[section]; i < availableSpawners[section].Count; i++)
            {
                Destroy(shuffledSpawners[i].gameObject);
            }
        }
    }
    
    // Determine where to spawn flowers
    private void SetFlowerSpawners()
    {
        // Randomly select spawn positions
        List<Vector3> flowerSpawnPointsUsed = new List<Vector3>();
        List<Vector3> flowerSpawnPointsNotUsed = new List<Vector3>();
        foreach (var flowerSpawner in _flowerSpawners)
        {
            // 33% spawn
            if (Random.value <= 0.33f)
            {
                flowerSpawnPointsUsed.Add(flowerSpawner);
            }
            else
            {
                flowerSpawnPointsNotUsed.Add(flowerSpawner);
            }
        }
        
        // Check count limit
        int numRooms = _generatedRooms.Count;
        int minFlowersCount = numRooms / 3 - 2;
        int maxFlowersCount = numRooms / 3 + 2;
        Debug.AssertFormat(_flowerSpawners.Count >= minFlowersCount, "Not enough flower spawners added to the templates.");
        
        // Exceeded the upper limit?
        while (flowerSpawnPointsUsed.Count > maxFlowersCount)
        {
            flowerSpawnPointsUsed.RemoveAt(Random.Range(0, flowerSpawnPointsUsed.Count));
        }
        
        // Below the lower limit?
        while (flowerSpawnPointsUsed.Count < minFlowersCount)
        {
            int randIdx = Random.Range(0, flowerSpawnPointsNotUsed.Count);
            Vector3 newlyAddedSpawnPoint = flowerSpawnPointsNotUsed[randIdx];
            flowerSpawnPointsNotUsed.RemoveAt(randIdx);
            flowerSpawnPointsUsed.Add(newlyAddedSpawnPoint);
        }
        
        // Spawn flowers
        foreach (Vector3 flowerSpawnPoint in flowerSpawnPointsUsed)
        {
            Instantiate(flowerPrefabs[Random.Range(0, (int)EFlowerType.MAX)], flowerSpawnPoint, Quaternion.identity);
        }
    }

    private void SetHiddenPortalSpawners()
    {
        if (_hiddenPortalSpawners.Count == 0) return;
        
        // Randomly select spawn positions
        List<HiddenPortal> portalSpawnersUsed = new List<HiddenPortal>();
        List<HiddenPortal> portalSpawnersNotUsed = new List<HiddenPortal>();
        foreach (var hiddenPortal in _hiddenPortalSpawners)
        {
            if (hiddenPortal.ShouldSpawn())
            {
                portalSpawnersUsed.Add(hiddenPortal);
            }
            else
            {
                portalSpawnersNotUsed.Add(hiddenPortal);
            }
        }
        
        // Check count limit
        int minPortalsCount = 3;
        int maxPortalsCount = 4;
        Debug.AssertFormat(_hiddenPortalSpawners.Count >= minPortalsCount, "Not enough portal spawners added to the templates.");
        
        // Exceeded the upper limit?
        while (portalSpawnersUsed.Count > maxPortalsCount)
        {
            // Remove a random portal based on spawn chance
            HiddenPortal portalToRemove = WeightedRandomPortalSelection(portalSpawnersUsed);
            portalSpawnersUsed.Remove(portalToRemove);
            portalSpawnersNotUsed.Add(portalToRemove);
        }

        // Below the lower limit?
        while (portalSpawnersUsed.Count < minPortalsCount)
        {
            // Add a random portal based on spawn chance
            HiddenPortal portalToAdd = WeightedRandomPortalSelection(portalSpawnersNotUsed);
            portalSpawnersNotUsed.Remove(portalToAdd);
            portalSpawnersUsed.Add(portalToAdd);
        }
        
        // Deactivate unused portals
        foreach (var unusedPortal in portalSpawnersNotUsed)
        {
            unusedPortal.gameObject.SetActive(false);
        }
    }

    // Weighted random selection function
    private HiddenPortal WeightedRandomPortalSelection(List<HiddenPortal> portals)
    {
        // Calculate total spawn chance
        float totalSpawnChance = portals.Sum(portal => portal.SpawnChance);

        // Generate a random number between 0 and total spawn chance
        float randomNumber = Random.Range(0, totalSpawnChance);

        // Determine which portal to select
        float cumulative = 0;
        foreach (var portal in portals)
        {
            cumulative += portal.SpawnChance;
            if (randomNumber <= cumulative)
            {
                // Return the selected portal
                return portal;
            }
        }

        return null; // This should never happen if the spawn chances are set up correctly
    }

    private void TempFunction(AstarPath script)
    {
        _aStarScanEnded = true;
    }
    
    private void SetAStarGrid()
    {
        AstarData data = AstarPath.active.data;
        GridGraph gridGraph = data.AddGraph(typeof(GridGraph)) as GridGraph;
        
        // Set properties
        gridGraph.is2D = true;
        gridGraph.collision.use2D = true;
        gridGraph.collision.mask = LayerMask.GetMask("Platform");
        
        // Set bounds
        _superWallTilemap.CompressBounds();
        var wallBounds = _superWallTilemap.cellBounds;
        gridGraph.center = wallBounds.center;
        gridGraph.SetDimensions(wallBounds.size.x * 2, wallBounds.size.y * 2, nodeSize:0.5f);
        
        // Wait for the scan to end
        _aStarScanEnded = false;
        AstarPath.OnLatePostScan += TempFunction;
        AstarPath.active.Scan();
        StartCoroutine(PostProcessCoroutine());
    }

    private IEnumerator LevelGenerationCoroutine()
    {
        yield return new WaitUntil(() => _levelGenerationEnded);
        PostProcessLevel();
    }
    
    private IEnumerator PostProcessCoroutine()
    {
        yield return new WaitUntil(() => _aStarScanEnded);
        GameManager.Instance.hasGameLoadEnded = true;
    }
    
    //  Find all hidden rooms and classify by levels
    private void SetHiddenRooms()
    {
        _hiddenRoomsByLevel = new [] { new List<HiddenRoom>(), new List<HiddenRoom>(), new List<HiddenRoom>() };
        var hiddenRooms = FindObjectsByType<HiddenRoom>(FindObjectsSortMode.None);
        foreach (var hiddenRoom in hiddenRooms)
        {
            _hiddenRoomsByLevel[hiddenRoom.roomLevel].Add(hiddenRoom);
        }
    }

    public List<HiddenRoom> GetHiddenRooms(int roomLevel)
    {
        Debug.AssertFormat(_hiddenRoomsByLevel != null, "Hidden room array is not set.");
        Debug.AssertFormat(_hiddenRoomsByLevel[roomLevel].Count > 0,
            $"No hidden room [level {roomLevel}] is found. Please add and try again.");
        return _hiddenRoomsByLevel[roomLevel];
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
        Transform playerObject = null;
        Vector3 playerSpawnPoint;
        ESceneType currScene = GameManager.Instance.ActiveScene;
        
        // Find player
        // Player is not spawned
        if (PlayerController.Instance == null)
        {
            playerObject = Instantiate(GameManager.Instance.PlayerPrefab).gameObject.transform;
        }
        // Player exists already
        else
        {
            playerObject = PlayerController.Instance.transform;
        }
        
        // Find spawn point
        if (currScene == ESceneType.CombatMap)
        {
            // Spawn at meta map
            playerSpawnPoint = _metaSpawnPoint;
        }
        else
        {
            // Boss map or debug map
            var spawnpoint = GameObject.FindWithTag("PlayerStart");
            if (spawnpoint != null) playerSpawnPoint = spawnpoint.transform.position;
            else playerSpawnPoint = playerObject.transform.position;
        }
        playerObject.position = playerSpawnPoint;
        
        // Set camera follow target
        // GameObject.Find("Virtual Camera").GetComponent<CinemachineVirtualCamera>().Follow = playerObject;
        PlayerEvents.Spawned.Invoke();
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

    private void AddRoomToMinimap(RoomBase newRoom, Door newDoor)
    {
        // In the order of background -> collideable
        for (int layerIdx = 2; layerIdx > 0; layerIdx--)
        {
            var tilemap = newRoom.Tilemaps[layerIdx];
            tilemap.CompressBounds();

            List<Vector3Int> tileWorldPositions = new List<Vector3Int>();
            List<TileBase> tileBases = new List<TileBase>();
            foreach (var localPos in tilemap.cellBounds.allPositionsWithin)
            {   
                if (tilemap.HasTile(localPos))
                    tileWorldPositions.Add(newDoor.DoorWorldBLPosition - newDoor.DoorLocalBLPosition + localPos);
            }
            for (int i = 0; i < tileWorldPositions.Count; i++)
                tileBases.Add(MinimapTiles[layerIdx]);
            
            _mapTilemap.SetTiles(tileWorldPositions.ToArray(), tileBases.ToArray());
        }
    }
}

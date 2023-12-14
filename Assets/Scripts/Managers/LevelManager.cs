using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LevelManager : Singleton<LevelManager>
{
    [SerializeField] private Transform _startingPointsContainer;
    private List<Transform> _startingPoints;
    private Transform _roomsContainer;
    private Object[] _rooms;

    private Vector2 _roomGenPos;
    private int _direction;

    private float _roomWidth = 10;
    private float _roomHeight = 10;

    public Transform Bounds;
    private Vector2 _minBounds;
    private Vector2 _maxBounds;

    private bool _isLevelGenComplete;


    protected override void Awake()
    {
        base.Awake();
        _roomsContainer = GameObject.Find("Rooms").transform;
        _rooms = Utility.LoadAllObjectsFromPath("Prefabs/Maps/");
        _startingPoints = new List<Transform>();
        foreach (Transform startingPoint in _startingPointsContainer)
        {
            _startingPoints.Add(startingPoint);
        }

        // Get bounds
        //_maxBounds.y = Bounds.GetChild(0).transform.position.y - _roomHeight / 2.0f - 0.5f; // top
        _minBounds.y = Bounds.GetChild(1).transform.position.y + _roomHeight / 2.0f + 0.5f; // bottom
        _minBounds.x = Bounds.GetChild(2).transform.position.x + _roomWidth / 2.0f + 0.5f;  // left
        _maxBounds.x = Bounds.GetChild(3).transform.position.x - _roomWidth / 2.0f - 0.5f;  // right
    }

    private void Start()
    {
        // Generate starting room
        int idx = Random.Range(0, _startingPoints.Count);
        _roomGenPos = _startingPoints[idx].position;
        var room = Instantiate(_rooms[0], _roomGenPos, Quaternion.identity);
        room.GameObject().transform.SetParent(_roomsContainer);
        _direction = Random.Range(0, 6);

        // Generate other rooms
        for (int i = 0; i < 15; i++)
        {
            GenerateRoom();
            //if (_isLevelGenComplete) break;
        }
    }

    private void SetDirection()
    {
        switch (_direction)
        {
            // Try move right
            case 0:
            case 1:
                {
                    if (_roomGenPos.x < _maxBounds.x)
                    {
                        // Move right
                        _roomGenPos = new Vector2(_roomGenPos.x + _roomWidth, _roomGenPos.y);

                        // Cannot move left next
                        do { _direction = Random.Range(0, 6); }
                        while (_direction == 2 || _direction == 3);
                    }
                    else
                    {
                        // Can only move left or up
                        _direction = Random.Range(0, 6);
                        if (_direction == 0) _direction = 2;
                        else if (_direction == 1) _direction = 3;
                        else if (_direction == 5) _direction = 4;
                        SetDirection();
                    }
                }
                break;

            // Try move left
            case 2:
            case 3:
                {
                    if (_roomGenPos.x > _minBounds.x)
                    {
                        // Move left
                        _roomGenPos = new Vector2(_roomGenPos.x - _roomWidth, _roomGenPos.y);

                        // Cannot move right next
                        do { _direction = Random.Range(0, 6); }
                        while (_direction == 0 || _direction == 1);
                    }
                    else
                    {
                        // Can only move right or up
                        _direction = Random.Range(0, 6);
                        if (_direction == 2) _direction = 0;
                        else if (_direction == 3) _direction = 1;
                        else if (_direction == 5) _direction = 4;
                        SetDirection();
                    }
                }
                break;

            // Try move up
            case 4:
                {
                    // Move up
                    _roomGenPos = new Vector2(_roomGenPos.x, _roomGenPos.y + _roomHeight);

                    // Cannot move down next
                    do { _direction = Random.Range(0, 6); }
                    while (_direction == 5);
                }
                break;

            // Try move down
            case 5:
                {
                    if (_roomGenPos.y > _minBounds.y)
                    {
                        // Move down
                        _roomGenPos = new Vector2(_roomGenPos.x, _roomGenPos.y - _roomHeight);

                        // Cannot move up next
                        do { _direction = Random.Range(0, 6); }
                        while (_direction == 4);
                    }
                    else
                    {
                        // Cannot move down
                        do { _direction = Random.Range(0, 6); }
                        while (_direction == 5);

                        // TEMP
                        SetDirection();
                    }
                }
                break;
        }
    }

    private void GenerateRoom()
    {
        SetDirection();
        var room = Instantiate(_rooms[0], _roomGenPos, Quaternion.identity);
        room.GameObject().transform.SetParent(_roomsContainer);
    }
}

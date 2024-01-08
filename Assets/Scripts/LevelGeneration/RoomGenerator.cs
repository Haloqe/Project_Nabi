using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomGenerator : MonoBehaviour
{
    [SerializeField] private Transform _tilePointsContainer;
    [SerializeField] private Tilemap _platformTilemap;

    private void Start()
    {
        GeneratePlatformTiles();
    }

    private void GeneratePlatformTiles()
    {
        TileBase platformRuleTile = Utility.LoadTileFromPath("PlatformRuleTile");
        foreach (Transform tilePoint in _tilePointsContainer)
        {
            _platformTilemap.SetTile(
                Vector3Int.FloorToInt(tilePoint.localPosition), platformRuleTile);
        }
    }
}

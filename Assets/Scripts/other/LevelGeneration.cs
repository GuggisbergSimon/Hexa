using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGeneration : MonoBehaviour
{
    [SerializeField] private VisualizationMode _visualizationMode = VisualizationMode.Height;
    [SerializeField] private int _mapWidthInTiles = 0, _mapDepthInTiles = 0;
    [SerializeField] private GameObject _tilePrefab = null;
    [SerializeField] private float _mapScale = 3.0f;
    [SerializeField] private TerrainType[] _heightTerrainTypes = null;
    [SerializeField] private TerrainType[] _heatTerrainTypes = null;
    [SerializeField] private float _heightMultiplier = 3.0f;
    [SerializeField] private AnimationCurve _heightCurve = null;
    [SerializeField] private Wave[] _waves = null;
    [SerializeField] private Wave[] heatWaves = null;

    private void Start()
    {
        GenerateMap();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GenerateMap();
        }
    }

    private void GenerateMap()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        // get the tile dimensions from the tile Prefab
        Vector3 tileSize = _tilePrefab.GetComponent<MeshRenderer>().bounds.size;
        int tileWidth = (int) tileSize.x;
        int tileDepth = (int) tileSize.z;
        // for each Tile, instantiate a Tile in the correct position
        for (int xTileIndex = 0; xTileIndex < _mapWidthInTiles; xTileIndex++)
        {
            for (int zTileIndex = 0; zTileIndex < _mapDepthInTiles; zTileIndex++)
            {
                // calculate the tile position based on the X and Z indices
                Vector3 tilePosition = new Vector3(gameObject.transform.position.x + xTileIndex * tileWidth,
                    gameObject.transform.position.y, gameObject.transform.position.z + zTileIndex * tileDepth);
                // instantiate a new Tile
                GameObject tile = Instantiate(_tilePrefab, tilePosition, Quaternion.identity, transform) as GameObject;
                float zOffset = _mapDepthInTiles * 10 / 2 + 10 / 2;
                tile.GetComponent<TileGeneration>().GenerateTile(_mapScale, _heightTerrainTypes, _heatTerrainTypes,
                    _heightMultiplier, _heightCurve, _waves, heatWaves, zOffset, zOffset,
                    _visualizationMode);
            }
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class TerrainType
{
    public string name;
    public float height;
    public Color color;
}

public enum VisualizationMode
{
    Height,
    Heat
}

public class TileGeneration : MonoBehaviour
{
    [SerializeField] private NoiseMapGeneration _noiseMapGeneration = null;
    [SerializeField] private MeshRenderer _tileRenderer = null;
    [SerializeField] private MeshFilter _meshFilter = null;
    [SerializeField] private MeshCollider _meshCollider = null;

    public void GenerateTile(float levelScale, TerrainType[] heightTerrainTypes, TerrainType[] heatTerrainTypes,
        float heightMultiplier, AnimationCurve heightCurve, Wave[] waves, Wave[] heatWaves, float centerVertexZ,
        float maxDistanceZ, VisualizationMode visualizationMode)
    {
        // calculate tile depth and width based on the mesh vertices
        Vector3[] meshVertices = _meshFilter.mesh.vertices;
        int tileDepth = (int) Mathf.Sqrt(meshVertices.Length);
        int tileWidth = tileDepth;

        // calculate the offsets based on the tile position
        float offsetX = -gameObject.transform.position.x;
        float offsetZ = -gameObject.transform.position.z;
        // generate a heightMap using noise
        float[,] heightMap =
            _noiseMapGeneration.GeneratePerlinNoiseMap(tileDepth, tileWidth, levelScale, offsetX, offsetZ, waves);
        //calculate vertex offset based on the Tile position and the distance between vertices
        Vector3 tileDimensions = _meshFilter.mesh.bounds.size;
        float distanceBetweenVertices = tileDimensions.z / (float) tileDepth;
        float vertexOffsetZ = gameObject.transform.position.z / distanceBetweenVertices;
        // generate a heatMap using uniform noise
        float[,] uniformHeatMap =
            _noiseMapGeneration.GenerateUniformNoiseMap(tileDepth, tileWidth, centerVertexZ, maxDistanceZ,
                vertexOffsetZ);
        // generate a heatMap using Perlin Noise
        float[,] randomHeatMap =
            _noiseMapGeneration.GeneratePerlinNoiseMap(tileDepth, tileWidth, levelScale, offsetX, offsetZ, heatWaves);
        float[,] heatMap = new float[tileDepth, tileWidth];
        for (int zIndex = 0; zIndex < tileDepth; zIndex++)
        {
            for (int xIndex = 0; xIndex < tileWidth; xIndex++)
            {
                // mix both heat maps together by multiplying their values
                heatMap[zIndex, xIndex] = uniformHeatMap[zIndex, xIndex] * randomHeatMap[zIndex, xIndex];
                //makes higher regions colder, by adding the height value to the heat map
                heatMap[zIndex, xIndex] += heightMap[zIndex, xIndex] * heightMap[zIndex, xIndex];
            }
        }

        // build a Texture2D from the height map
        Texture2D heightTexture = BuildTexture(heightMap, heightTerrainTypes);
        // build a Texture2D from the heat map
        Texture2D heatTexture = BuildTexture(uniformHeatMap, heatTerrainTypes);
        switch (visualizationMode)
        {
            case VisualizationMode.Height:
                _tileRenderer.material.mainTexture = heightTexture;
                break;
            case VisualizationMode.Heat:
                _tileRenderer.material.mainTexture = heatTexture;
                break;
        }

        // update the tile mesh vertices according to the height map
        UpdateMeshVertices(heightMap, heightMultiplier, heightCurve);
    }

    private void UpdateMeshVertices(float[,] heightMap, float heightMultiplier, AnimationCurve heightCurve)
    {
        int tileDepth = heightMap.GetLength(0);
        int tileWidth = heightMap.GetLength(1);
        Vector3[] meshVertices = _meshFilter.mesh.vertices;
        // iterate through all the heightMap coordinates, updating the vertex index
        int vertexIndex = 0;
        for (int zIndex = 0; zIndex < tileDepth; zIndex++)
        {
            for (int xIndex = 0; xIndex < tileWidth; xIndex++)
            {
                float height = heightMap[zIndex, xIndex];
                Vector3 vertex = meshVertices[vertexIndex];
                // change the vertex Y coordinate, proportional to the height value
                meshVertices[vertexIndex] =
                    new Vector3(vertex.x, heightCurve.Evaluate(height) * heightMultiplier, vertex.z);
                vertexIndex++;
            }
        }

        // update the vertices in the mesh and update its properties
        _meshFilter.mesh.vertices = meshVertices;
        _meshFilter.mesh.RecalculateBounds();
        _meshFilter.mesh.RecalculateNormals();
        // update the mesh collider
        _meshCollider.sharedMesh = _meshFilter.mesh;
    }

    private Texture2D BuildTexture(float[,] heightMap, TerrainType[] terrainTypes)
    {
        int tileDepth = heightMap.GetLength(0);
        int tileWidth = heightMap.GetLength(1);

        Color[] colorMap = new Color[tileDepth * tileWidth];
        for (int zIndex = 0; zIndex < tileDepth; zIndex++)
        {
            for (int xIndex = 0; xIndex < tileWidth; xIndex++)
            {
                // transform the 2D map index is an Array index
                int colorIndex = zIndex * tileWidth + xIndex;
                float height = heightMap[zIndex, xIndex];
                // choose a terrain type according to the height value
                TerrainType terrainType = ChooseTerrainType(height, terrainTypes);
                // assign as color a shade of grey proportional to the height value
                colorMap[colorIndex] = terrainType.color;
            }
        }

        // create a new texture and set its pixel colors
        Texture2D tileTexture = new Texture2D(tileWidth, tileDepth);
        tileTexture.wrapMode = TextureWrapMode.Clamp;
        tileTexture.SetPixels(colorMap);
        tileTexture.Apply();
        return tileTexture;
    }

    private TerrainType ChooseTerrainType(float height, TerrainType[] terrainTypes)
    {
        // for each terrain type, check if the height is lower than the one for the terrain type
        foreach (var terrainType in terrainTypes)
        {
            // return the first terrain type whose height is higher than the generated one
            if (height < terrainType.height)
            {
                return terrainType;
            }
        }

        return terrainTypes[terrainTypes.Length - 1];
    }
}
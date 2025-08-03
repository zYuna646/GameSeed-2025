using UnityEngine;
using System.Collections.Generic;

public class ChessBoardSpawner : MonoBehaviour
{
    [Header("Board Configuration")]
    public int boardSize = 8;
    public float tileSizeInUnits = 1f;
    public float tileHeight = 0.1f;

    [Header("Tile Prefabs")]
    public GameObject whiteTilePrefab;
    public GameObject blackTilePrefab;

    [Header("Board Layout")]
    public Vector3 boardOrigin = Vector3.zero;
    public bool rotateBoard = false;

    [Header("Tile Materials")]
    public Material whiteTileMaterial;
    public Material blackTileMaterial;

    [Header("Rendering Options")]
    public bool useProceduralMesh = false;
    public bool addColliders = true;

    [System.Serializable]
    public class TileData
    {
        public string notation;
        public Vector3 worldPosition;
        public Color tileColor;
        public GameObject tileObject;
    }

    public List<TileData> boardTiles = new List<TileData>();

    private void Start()
    {
        SpawnChessBoard();
    }

    public void SpawnChessBoard()
    {
        // Clear existing tiles if any
        ClearExistingBoard();

        // Create new board
        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                // Determine tile color
                bool isWhiteTile = (x + y) % 2 == 0;
                
                // Calculate world position
                Vector3 tilePosition = CalculateTilePosition(x, y);

                // Create tile
                GameObject tilePrefab = isWhiteTile ? whiteTilePrefab : blackTilePrefab;
                GameObject tileObject = CreateTile(tilePosition, isWhiteTile, x, y);

                // Store tile data
                TileData tileData = new TileData
                {
                    notation = $"{(char)('A' + x)}{y + 1}",
                    worldPosition = tilePosition,
                    tileColor = isWhiteTile ? Color.white : Color.black,
                    tileObject = tileObject
                };

                boardTiles.Add(tileData);
            }
        }
    }

    private Vector3 CalculateTilePosition(int x, int y)
    {
        // Calculate base position
        Vector3 basePosition = boardOrigin + new Vector3(
            x * tileSizeInUnits, 
            0, 
            y * tileSizeInUnits
        );

        // Optional board rotation
        if (rotateBoard)
        {
            basePosition = Quaternion.Euler(0, 180, 0) * basePosition;
        }

        return basePosition;
    }

    private GameObject CreateTile(Vector3 position, bool isWhiteTile, int x, int y)
    {
        GameObject tilePrefab = isWhiteTile ? whiteTilePrefab : blackTilePrefab;
        
        if (useProceduralMesh)
        {
            return CreateProceduralTile(position, isWhiteTile, x, y);
        }
        
        // Instantiate prefab tile
        GameObject tileObject = Instantiate(
            tilePrefab, 
            position, 
            Quaternion.identity, 
            transform
        );

        // Apply material if specified
        if (isWhiteTile && whiteTileMaterial)
        {
            ApplyMaterialToTile(tileObject, whiteTileMaterial);
        }
        else if (!isWhiteTile && blackTileMaterial)
        {
            ApplyMaterialToTile(tileObject, blackTileMaterial);
        }

        // Add collider if needed
        if (addColliders)
        {
            AddTileCollider(tileObject);
        }

        return tileObject;
    }

    private GameObject CreateProceduralTile(Vector3 position, bool isWhiteTile, int x, int y)
    {
        // Create a simple procedural tile mesh
        GameObject tileObject = new GameObject($"Tile_{(char)('A' + x)}{y + 1}");
        tileObject.transform.position = position;
        tileObject.transform.parent = transform;

        // Add MeshFilter
        MeshFilter meshFilter = tileObject.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;

        // Create vertices
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(0, 0, 0),
            new Vector3(tileSizeInUnits, 0, 0),
            new Vector3(tileSizeInUnits, 0, tileSizeInUnits),
            new Vector3(0, 0, tileSizeInUnits)
        };

        // Create triangles
        int[] triangles = new int[]
        {
            0, 2, 1,
            0, 3, 2
        };

        // Assign mesh data
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        // Add MeshRenderer
        MeshRenderer renderer = tileObject.AddComponent<MeshRenderer>();
        renderer.material = isWhiteTile ? whiteTileMaterial : blackTileMaterial;

        // Add collider if needed
        if (addColliders)
        {
            AddTileCollider(tileObject);
        }

        return tileObject;
    }

    private void ApplyMaterialToTile(GameObject tileObject, Material material)
    {
        Renderer renderer = tileObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = material;
        }
    }

    private void AddTileCollider(GameObject tileObject)
    {
        BoxCollider collider = tileObject.AddComponent<BoxCollider>();
        collider.size = new Vector3(tileSizeInUnits, tileHeight, tileSizeInUnits);
        collider.center = new Vector3(tileSizeInUnits / 2, tileHeight / 2, tileSizeInUnits / 2);
    }

    private void ClearExistingBoard()
    {
        // Destroy existing tiles
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Clear tile data list
        boardTiles.Clear();
    }

    // Utility method to get tile by notation
    public TileData GetTileByNotation(string notation)
    {
        return boardTiles.Find(tile => tile.notation == notation);
    }

    // Utility method to get tile by world position
    public TileData GetTileByWorldPosition(Vector3 worldPosition)
    {
        return boardTiles.Find(tile => 
            Vector3.Distance(tile.worldPosition, worldPosition) < tileSizeInUnits / 2
        );
    }

    // Optional method to regenerate board with new parameters
    public void RegenerateBoard(int newSize, float newTileSize)
    {
        boardSize = newSize;
        tileSizeInUnits = newTileSize;
        SpawnChessBoard();
    }
} 
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// The base class from which all maze generators should derive from
/// Ideally will hold all essential information needed to create, modify and define a maze
/// </summary>
public abstract class MazeGenerator : MonoBehaviour
{
    //Defines the dimensions of the maze
    [Header("Maze Dimensions")]
    [Range(10, 255)] public int mazeWidth = 10;
    [Range(10, 255)] public int mazeHeight = 10;
    protected int[,] gridMap;

    //Defines the targetted tilemaps and their assosiated visuals
    [Header("TileMap Setup")]
    public Tilemap wallTileMap = null;
    public Tile wallTile = null;
    public Tilemap floorTileMap = null;
    public Tile floorTile = null;

    //Variables which affect the visual generation of the maze
    [Header("Tiles generated per second")]
    [Range(0, 0.25f)] public float wallGenerationSpeed = 0.01f;
    [Range(0, 0.25f)] public float floorGenerationSpeed = 0.01f;

    //Stores important pathway data for generation at the end
    protected List<PathwayData> generatedPathways = new List<PathwayData>();

    // Start is called before the first frame update
    protected virtual void Start()
    {
        //Error handling for if a tilemap or tile was not supplied
        if (wallTileMap == null) throw new MissingReferenceException("Missing wall tile map on " + gameObject.name + "!");
        if (wallTile == null) throw new MissingReferenceException("Missing wall tile asset on " + gameObject.name + "!");
        if (wallTileMap == null) throw new MissingReferenceException("Missing floor tile map on " + gameObject.name + "!");
        if (wallTileMap == null) throw new MissingReferenceException("Missing tile tile on " + gameObject.name + "!");

        //Initialize the maze base
        InitializeMazeBase();
    }

    //Initializes the flooring and dimensions of the maze
    protected void InitializeMazeBase()
    {
        //Declare a 2d array grid map to make working with tile positions easier
        gridMap = new int[mazeWidth, mazeHeight];
    }

    protected void FloorGenerationDelayed()
    {
        //Start a courotine to create a delayed floor population effect
        StartCoroutine(GenerationCourotine("Floor", floorTileMap, floorGenerationSpeed));
    }

    protected void WallGenerationDelayed()
    {
        //Start a courotine to create a delayed wall population effect
        StartCoroutine(GenerationCourotine("Wall", wallTileMap, wallGenerationSpeed));
    }

    private IEnumerator GenerationCourotine(string generatedType, Tilemap affectedTileMap, float generationSpeed)
    {
        //Debug the start of floor generation
        Debug.Log("Generating " + generatedType + " tiles at timestamp: " + Time.time);

        //Iterate over rows
        for (int row = 0; row < gridMap.GetLength(0); row++)
        {
            //Iterate over columns
            for (int column = 0; column < gridMap.GetLength(1); column++)
            {
                //Paint the floor layer
                floorTileMap.SetTile(new Vector3Int(row, column, 0), floorTile);

                //Yield a wait for seconds function based on the floor generation speed
                yield return new WaitForSeconds(floorGenerationSpeed);
            }
        }

        //Debug the completion of floor generation
        Debug.Log("Finished " + generatedType + " generating floor tiles at timestamp : " + Time.time);
    }
}

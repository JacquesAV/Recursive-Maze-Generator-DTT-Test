using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Diagnostics;

/// <summary>
/// Maze generator that uses the recursive division algorithm
/// This functions by defining the dimensions of a room and continuously subdividing it until no space remains
/// </summary>
public class RecursiveDivisionMazeGenerator : MazeGenerator
{
    // Start is called before the first frame update
    protected override void Start()
    {
        //Use the base maze generator start method
        base.Start();

        //Generate recursive maze using stored parameters
        GenerateRecursiveDivisionMaze();
    }

    public void GenerateRecursiveDivisionMaze()
    {
        //Clear tile maps
        wallTileMap.ClearAllTiles();
        floorTileMap.ClearAllTiles();
        generatedPathways.Clear();

        //Generate a delayed floor
        FloorGenerationDelayed();

        //Create two chambers based on starting parameters
        CreateDoubleChamber(mazeWidth, mazeHeight, 0, 0);

        //Start a courotine to create a delayed wall population effect
        StartCoroutine(WallGenerationCourotine());
    }

    //Based on inputted
    private void CreateDoubleChamber(int startingWidth, int startingHeight, int originRow, int originColumn)
    {
        VerticalChamberSplit(startingWidth, startingHeight, originRow, originColumn);
        //HorizontalChamberSplit(startingWidth, startingHeight, originRow, originColumn);


        return;


        //Prioritize splits across the width and height
        if (startingWidth > startingHeight)
        {
            //Check if vertically splittable
            if (startingWidth >= 2) //Vertical
            {
                VerticalChamberSplit(startingWidth, startingHeight, originRow, originColumn);
            }
        }
        else
        {
            //Check if horizontally splittable
            if (startingHeight >= 2) //Horizontal
            {
                HorizontalChamberSplit(startingWidth, startingHeight, originRow, originColumn);
            }
        }
    }

    //Splits the chamber in two based on the remaining width and height
    private void VerticalChamberSplit(int startingWidth, int startingHeight, int originRow, int originColumn)
    {
        //Temporary variable to help track the available space in the chamber as it splits
        int remainingSpace = startingWidth;

        //Random width within bounds that allow for minimum 2 rooms, calculated from path size and wall guaranteed padding
        int randomWidth = Random.Range(2, remainingSpace - 2);

        //Room 1
        generatedPathways.Add(new PathwayData(
            new Vector2Int(startingWidth - remainingSpace + originRow, originColumn), //Position
            randomWidth + 1, startingHeight)); //Dimensions

        //Room 2
        generatedPathways.Add(new PathwayData(
            new Vector2Int(originRow + randomWidth, originColumn), //Position
            startingWidth - randomWidth, startingHeight)); //Dimensions
    }

    //Splits the chamber in two based on the remaining width and height
    private void HorizontalChamberSplit(int startingWidth, int startingHeight, int originRow, int originColumn)
    {
        //Temporary variable to help track the available space in the chamber as it splits
        int remainingSpace = startingHeight;

        //Random height within bounds that allow for minimum 2 rooms, calculated from path size and wall guaranteed padding
        int randomHeight = Random.Range(2, remainingSpace - 2);

        //Room 1
        generatedPathways.Add(new PathwayData(
            new Vector2Int(originRow, startingHeight - remainingSpace - originColumn), //Position
            startingWidth, randomHeight + 1)); //Dimensions

        //Room 2
        generatedPathways.Add(new PathwayData(
            new Vector2Int(originRow, originColumn + randomHeight), //Position
            startingWidth, startingHeight - randomHeight)); //Dimensions
    }

    protected IEnumerator WallGenerationCourotine()
    {
        //Debug the start of floor generation
        Debug.Log("Generating wall tiles at timestamp: " + Time.time);

        //Get each position point for the pathway
        foreach (PathwayData pathway in generatedPathways)
        {
            //Iterate over rows
            for (int row = 0; row < pathway.pathWidth; row++)
            {
                //Iterate over columns
                for (int column = 0; column < pathway.pathHeight; column++)
                {
                    //Paint only the walls (perimeter of this "rectangle" row/column check)
                    if (row == 0 || row == pathway.pathWidth - 1 || column == 0 || column == pathway.pathHeight - 1)
                    {
                        //For now we will paint the entire pathway dimension to test the divison algorithm
                        wallTileMap.SetTile(new Vector3Int(row + pathway.tileOrigin.x, column + pathway.tileOrigin.y, 0), wallTile);

                        //Yield a wait for seconds function based on the wall generation speed
                        yield return new WaitForSecondsRealtime(wallGenerationSpeed);
                    }
                }
            }
        }

        //Debug the completion of floor generation
        Debug.Log("Finished generating wall tiles at timestamp : " + Time.time);
    }

    private bool IsChamberSplittable(int givenWidth, int givenHeight)
    {
        //Check if vertically or horizontally splittable
        return (givenWidth >= 3 || givenHeight >= 3);
    }
}

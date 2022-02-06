using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        //Clear tile maps & pathway data
        wallTileMap.ClearAllTiles();
        floorTileMap.ClearAllTiles();
        generatedPathways.Clear();

        //Generate a delayed floor
        FloorGenerationDelayed();

        //Create two chambers based on starting parameters and continue until no space remains
        StartRecursiveChamberDivision(mazeWidth, mazeHeight, 0, 0);

        //Start a courotine to create a delayed wall population effect
        StartCoroutine(WallGenerationCourotine());
    }

    //Based on inputted
    private void StartRecursiveChamberDivision(int startingWidth, int startingHeight, int originRow, int originColumn)
    {
        //Prioritize splits across the width then height
        if (startingWidth > startingHeight)
        {
            //Check if vertically splittable
            if (startingWidth > 3)
            {
                VerticalChamberSplit(startingWidth, startingHeight, originRow, originColumn);
            }
        }
        else
        {
            //Check if horizontally splittable
            if (startingHeight > 3)
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

        //Check if still splittable, otherwise add the pathway
        if (IsChamberSplittable(randomWidth + 1, startingHeight))
        {
            //Room 1
            generatedPathways.Add(new PathwayData(
                new Vector2Int(startingWidth - remainingSpace + originRow, originColumn), //Position
                randomWidth + 1, startingHeight)); //Dimensions

            //Calls loop again for first room division
            StartRecursiveChamberDivision(randomWidth + 1, startingHeight, startingWidth - remainingSpace + originRow, originColumn);
        }

        //Check if still splittable, else create the room
        if (IsChamberSplittable(startingWidth - randomWidth + 1, startingHeight))
        {
            //Room 2
            generatedPathways.Add(new PathwayData(
                new Vector2Int(originRow + randomWidth, originColumn), //Position
                startingWidth - randomWidth, startingHeight)); //Dimensions

            //Calls loop again for second room division
            StartRecursiveChamberDivision(startingWidth - randomWidth, startingHeight, randomWidth + originRow, originColumn);
        }
    }

    //Splits the chamber in two based on the remaining width and height
    private void HorizontalChamberSplit(int startingWidth, int startingHeight, int originRow, int originColumn)
    {
        //Temporary variable to help track the available space in the chamber as it splits
        int remainingSpace = startingHeight;

        //Random height within bounds that allow for minimum 2 rooms, calculated from path size and wall guaranteed padding
        int randomHeight = Random.Range(2, remainingSpace - 2);

        //Check if still splittable, else create the room
        if (IsChamberSplittable(startingWidth, randomHeight + 1))
        {
            //Room 1
            generatedPathways.Add(new PathwayData(
                new Vector2Int(originRow, startingHeight - remainingSpace - originColumn), //Position
                startingWidth, randomHeight + 1)); //Dimensions

            //Calls loop again for first room division
            StartRecursiveChamberDivision(startingWidth, randomHeight + 1, originRow, startingHeight - remainingSpace + originColumn);
        }

        //Check if still splittable, else create the room
        if (IsChamberSplittable(startingWidth, startingHeight - randomHeight + 1))
        {
            //Room 2
            generatedPathways.Add(new PathwayData(
                new Vector2Int(originRow, originColumn + randomHeight), //Position
                startingWidth, startingHeight - randomHeight)); //Dimensions

            //Calls loop again for second room division
            StartRecursiveChamberDivision(startingWidth, startingHeight - randomHeight, originRow, originColumn + randomHeight);
        }
    }

    protected IEnumerator WallGenerationCourotine()
    {
        //Debug the start of floor generation
        Debug.Log("Generating wall tiles at timestamp: " + Time.time);

        //Temporary list that will be filtered for duplicate coordinates
        List<Vector3Int> filteredWalls = new List<Vector3Int>();

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
                        //Add to the temporary list the position of the wall
                        filteredWalls.Add(new Vector3Int(row + pathway.tileOrigin.x, column + pathway.tileOrigin.y, 0));
                    }
                }
            }
        }

        //Remove all duplicate/overlapping walls to improve generation time
        filteredWalls = filteredWalls.Distinct().ToList();

        //Iterate over the filtered list
        foreach (Vector3Int wall in filteredWalls)
        {
            //Paint each wall tile
            wallTileMap.SetTile(wall, wallTile);

            //Yield a wait for seconds function based on the wall generation speed
            //Courotines have a limitation where waiting for less than a millisecond is not possible, and so in order to allow for animation skipping a check for 0 is here
            if (wallGenerationSpeed != 0.0f) yield return new WaitForSecondsRealtime(wallGenerationSpeed);
        }


        //Debug the completion of floor generation
        Debug.Log("Finished generating wall tiles at timestamp : " + Time.time);
    }

    private bool IsChamberSplittable(int givenWidth, int givenHeight)
    {
        //Check if vertically or horizontally splittable
        //The minimum size is 3 as this accounts for a single space hallway as well as its neighbouring walls
        return (givenWidth > 3 || givenHeight > 3);
    }
}

using System.Collections;
using System.Collections.Generic;
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
        //VerticalChamberSplit(startingWidth, startingHeight, originRow, originColumn);
        //HorizontalChamberSplit(startingWidth, startingHeight, originRow, originColumn);

        //Prioritize splits across the width then height
        if (startingWidth > startingHeight)
        {
            //Check if vertically splittable
            if (startingWidth > 4)
            {
                VerticalChamberSplit(startingWidth, startingHeight, originRow, originColumn);
            }
        }
        else
        {
            //Check if horizontally splittable
            if (startingHeight > 4)
            {
                HorizontalChamberSplit(startingWidth, startingHeight, originRow, originColumn);
            }
        }
    }

    //Splits the chamber in two based on the remaining width and height
    private void VerticalChamberSplit(int startingWidth, int startingHeight, int originRow, int originColumn)
    {
        if (startingWidth <= 4) return;

        //Temporary variable to help track the available space in the chamber as it splits
        int remainingSpace = startingWidth;

        //Random width within bounds that allow for minimum 2 rooms
        int randomWidth = Random.Range(3, remainingSpace - 1);

        //Room 1
        generatedPathways.Add(new PathwayData(
            new Vector2Int(startingWidth - remainingSpace + originRow, originColumn), //Position
            randomWidth, startingHeight)); //Dimensions

        //Calls loop again for first room division
        StartRecursiveChamberDivision(randomWidth, startingHeight, startingWidth - remainingSpace + originRow, originColumn);

        //Room 2
        generatedPathways.Add(new PathwayData(
            new Vector2Int(originRow + randomWidth - 1, originColumn), //Position
            startingWidth - randomWidth + 1, startingHeight)); //Dimensions

        //Calls loop again for second room division
        StartRecursiveChamberDivision(startingWidth - randomWidth + 1, startingHeight, randomWidth + originRow - 1, originColumn);

    }

    //Splits the chamber in two based on the remaining width and height
    private void HorizontalChamberSplit(int startingWidth, int startingHeight, int originRow, int originColumn)
    {
        if (startingHeight <= 4) return;

        //Temporary variable to help track the available space in the chamber as it splits
        int remainingSpace = startingHeight;

        //Random height within bounds that allow for minimum 2 rooms, calculated from path size and wall guaranteed padding
        int randomHeight = Random.Range(3, remainingSpace - 1);

        //Room 1
        generatedPathways.Add(new PathwayData(
            new Vector2Int(originRow, startingHeight - remainingSpace + originColumn), //Position //Original issues with splitting isolated to here, ABS is used to correct it to never be negative
            startingWidth, randomHeight )); //Dimensions

        //Calls loop again for first room division
        StartRecursiveChamberDivision(startingWidth, randomHeight, originRow, startingHeight - remainingSpace + originColumn);

        //Room 2
        generatedPathways.Add(new PathwayData(
            new Vector2Int(originRow, originColumn + randomHeight - 1), //Position
            startingWidth, startingHeight - randomHeight + 1)); //Dimensions

        //Calls loop again for second room division
        StartRecursiveChamberDivision(startingWidth, startingHeight - randomHeight + 1, originRow, originColumn + randomHeight - 1);
    }

    protected IEnumerator WallGenerationCourotine()
    {
        //Debug the start of floor generation
        Debug.Log("Generating wall tiles at timestamp: " + Time.time);

        //Temporary list that will be filtered for duplicate coordinates
        Dictionary<Vector3Int, Color> filteredWalls = new Dictionary<Vector3Int, Color>();

        //Get each position point for the pathway
        foreach (PathwayData pathway in generatedPathways)
        {
            //Color for the current path being worked on
            Color segmentColor = Random.ColorHSV();

            //Iterate over rows
            for (int row = 0; row < pathway.pathWidth; row++)
            {
                //Iterate over columns
                for (int column = 0; column < pathway.pathHeight; column++)
                {
                    //Paint only the walls (perimeter of this "rectangle" row/column check)
                    if (row == 0 || row == pathway.pathWidth - 1 || column == 0 || column == pathway.pathHeight - 1)
                    {
                        Vector3Int position = new Vector3Int(row + pathway.tileOrigin.x, column + pathway.tileOrigin.y, 0);
                        if (!filteredWalls.ContainsKey(position))
                        {
                            //Add to the temporary list the position of the wall with a random color
                            filteredWalls.Add(position, segmentColor);
                        }
                        else
                        {
                            filteredWalls[position] = segmentColor;
                        }
                    }
                }
            }
        }

        //Remove all duplicate/overlapping walls to improve generation time
        //filteredWalls = filteredWalls.Distinct().ToList();

        //Iterate over the filtered list
        // foreach (Vector3Int wall in filteredWalls)
        foreach (KeyValuePair<Vector3Int, Color> wall in filteredWalls)
        {
            wallTile.color = wall.Value;

            //Paint each wall tile
            wallTileMap.SetTile(wall.Key, wallTile);

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

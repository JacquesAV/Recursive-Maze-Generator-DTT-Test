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
    //Stores important pathway data for generation at the end
    private List<PathwayData> generatedPathways = new List<PathwayData>();

    //Tracks the connectors between pathways that will be generated and corrected at the end
    private List<PathwayConnector> generatedConnectors = new List<PathwayConnector>();

    //Tracks the corners of every room, this is essential in correcting the positions for connections between hallways
    private List<Vector3Int> generatedCorners = new List<Vector3Int>();

    //Color for the generated corners when debugging
    public Color cornerDebugColor = Color.white;

    //Internal booleans to help with tracking completed portions of generation
    public bool completedPathwayGeneration = false;
    public bool completedCornerGeneration = false;
    public bool completedConnectorGeneration = false;

    //Internal boolean to track if a maze generation is in progress, this should be used to prevent maze generation spam
    public bool isMazeCurrentlyGenerating = false;

    // Start is called before the first frame update
    protected override void Start()
    {
        //Use the base maze generator start method
        base.Start();
    }

    //Generate recursive maze using stored parameters
    public override void GenerateMaze()
    {
        //Reject requests to generate while still busy
        if (isMazeCurrentlyGenerating)
        {
            Debug.Log("Generation request denied, still busy with the last request...");
            return;
        }
        else
        {
            //Reset parameters that help track the progress of the generator
            ResetTrackingParameters();

            //Start a courotine to track the progress of the maze generation
            StartCoroutine(MazeGenerationProgressTracker());
        }

        //Initialize the base of the maze (dimensions)
        InitializeMazeBase();

        //Clear tile maps
        wallTileMap.ClearAllTiles();
        floorTileMap.ClearAllTiles();

        //Clear generated data
        generatedPathways.Clear();
        generatedCorners.Clear();
        generatedConnectors.Clear();

        //Generate a delayed floor
        FloorGenerationDelayed();

        //Create two chambers based on starting parameters and continue until no space remains
        RunRecursiveChamberDivision(mazeWidth, mazeHeight, 0, 0, true);

        //Start a courotine to create a delayed wall population effect
        StartCoroutine(WallGenerationCourotine());

        //Start a courotine to create a delayed corner population effect (if debugging), otherwise skip this step and mark it as complete
        if (shouldDebugVisually) StartCoroutine(CornersGenerationCourotine()); else completedCornerGeneration = true;

        //Start a courotine to create a delayed connector population effect
        StartCoroutine(ConnectorGenerationCourotine());
    }

    private void ResetTrackingParameters()
    {
        //Clear the tracking booleans
        completedPathwayGeneration = false;
        completedCornerGeneration = false;
        completedConnectorGeneration = false;
    }

    #region Recursive Division Calculations
    //Split chambers/pathways based on inputted values
    private void RunRecursiveChamberDivision(int startingWidth, int startingHeight, int originRow, int originColumn, bool isFirstDivision)
    {
        //Prioritize splits across the width then height
        if (startingWidth > startingHeight)
        {
            //Check if vertically splittable
            if (startingWidth > 4)
            {
                VerticalChamberSplit(startingWidth, startingHeight, originRow, originColumn, isFirstDivision);
            }
        }
        else
        {
            //Check if horizontally splittable
            if (startingHeight > 4)
            {
                HorizontalChamberSplit(startingWidth, startingHeight, originRow, originColumn, isFirstDivision);
            }
        }
    }

    //Splits the chamber in two based on the remaining width and height
    private void VerticalChamberSplit(int startingWidth, int startingHeight, int originRow, int originColumn, bool isFirstDivision)
    {
        //Temporary variable to help track the available space in the chamber as it splits
        int remainingSpace = startingWidth;

        //Random width within bounds that allow for minimum 2 rooms
        int randomWidth = Random.Range(3, remainingSpace - 1);

        //Room 1
        SavePathwayData(new PathwayData(
            new Vector2Int(startingWidth - remainingSpace + originRow, originColumn), //Position
            randomWidth, startingHeight)); //Dimensions

        //Calls loop again for first room division
        RunRecursiveChamberDivision(randomWidth, startingHeight, startingWidth - remainingSpace + originRow, originColumn, false);

        //Room 2
        SavePathwayData(new PathwayData(
            new Vector2Int(originRow + randomWidth - 1, originColumn), //Position
            startingWidth - randomWidth + 1, startingHeight)); //Dimensions

        //Calls loop again for second room division
        RunRecursiveChamberDivision(startingWidth - randomWidth + 1, startingHeight, randomWidth + originRow - 1, originColumn, false);

        //Pathway connector bounds setup
        int maximumConnectorPosition = originColumn + startingHeight - 2;
        int minimumConnectorPosition = originColumn + 1;

        //Random position, subject to change based on room overlaps
        Vector3Int connectorPosition = new Vector3Int(originRow + randomWidth - 1, Random.Range(minimumConnectorPosition, maximumConnectorPosition), 0);

        //Add the connector to the list for generation later
        generatedConnectors.Add(new PathwayConnector(connectorPosition, maximumConnectorPosition, minimumConnectorPosition, true));

        //If these are the first two divided chambers, add two entrances to the maze
        if (isFirstDivision)
        {
            //Leftward entrance
            connectorPosition = new Vector3Int(originRow, Random.Range(minimumConnectorPosition, maximumConnectorPosition), 0);

            //Add the connector to the list for generation later
            generatedConnectors.Add(new PathwayConnector(connectorPosition, maximumConnectorPosition, minimumConnectorPosition, true));

            //Upward entrance
            connectorPosition = new Vector3Int(originRow + startingWidth - 1, Random.Range(minimumConnectorPosition, maximumConnectorPosition), 0);

            //Add the connector to the list for generation later
            generatedConnectors.Add(new PathwayConnector(connectorPosition, maximumConnectorPosition, minimumConnectorPosition, true));
        }
    }

    //Splits the chamber in two based on the remaining width and height
    private void HorizontalChamberSplit(int startingWidth, int startingHeight, int originRow, int originColumn, bool isFirstDivision)
    {
        //Temporary variable to help track the available space in the chamber as it splits
        int remainingSpace = startingHeight;

        //Random height within bounds that allow for minimum 2 rooms, calculated from path size and wall guaranteed padding
        int randomHeight = Random.Range(3, remainingSpace - 1);

        //Room 1
        SavePathwayData(new PathwayData(
            new Vector2Int(originRow, startingHeight - remainingSpace + originColumn), //Position //Original issues with splitting isolated to here, ABS is used to correct it to never be negative
            startingWidth, randomHeight )); //Dimensions

        //Calls loop again for first room division
        RunRecursiveChamberDivision(startingWidth, randomHeight, originRow, startingHeight - remainingSpace + originColumn, false);

        //Room 2
        SavePathwayData(new PathwayData(
            new Vector2Int(originRow, originColumn + randomHeight - 1), //Position
            startingWidth, startingHeight - randomHeight + 1)); //Dimensions

        //Calls loop again for second room division
        RunRecursiveChamberDivision(startingWidth, startingHeight - randomHeight + 1, originRow, originColumn + randomHeight - 1, false);

        //Pathway connector bounds setup
        int maximumConnectorPosition = originRow + startingWidth - 2;
        int minimumConnectorPosition = originRow + 1;

        //Random position, subject to change based on room overlaps
        Vector3Int connectorPosition = new Vector3Int(Random.Range(minimumConnectorPosition, maximumConnectorPosition), originColumn + randomHeight - 1, 0);

        //Add the connector to the list for generation later
        generatedConnectors.Add(new PathwayConnector(connectorPosition, maximumConnectorPosition, minimumConnectorPosition, false));

        //If these are the first two divided chambers, add two entrances to the maze
        if (isFirstDivision)
        {
            //Leftward entrance
            connectorPosition = new Vector3Int(Random.Range(minimumConnectorPosition, maximumConnectorPosition), originColumn, 0);

            //Add the connector to the list for generation later
            generatedConnectors.Add(new PathwayConnector(connectorPosition, maximumConnectorPosition, minimumConnectorPosition, false));

            //Upward entrance
            connectorPosition = new Vector3Int(Random.Range(minimumConnectorPosition, maximumConnectorPosition), originColumn + startingHeight - 1, 0);

            //Add the connector to the list for generation later
            generatedConnectors.Add(new PathwayConnector(connectorPosition, maximumConnectorPosition, minimumConnectorPosition, false));
        }
    }
    #endregion

    private void SavePathwayData(PathwayData givenPathway)
    {
        //Saves the pathway to a list that will be generated later on
        generatedPathways.Add(givenPathway);

        //Saves the corners of this pathway in order to make door position correction easier
        generatedCorners.Add(new Vector3Int(givenPathway.tileOrigin.x, givenPathway.tileOrigin.y + givenPathway.pathHeight - 1, 0)); //Top Left
        generatedCorners.Add(new Vector3Int(givenPathway.tileOrigin.x, givenPathway.tileOrigin.y, 0)); //Bottom Left
        generatedCorners.Add(new Vector3Int(givenPathway.tileOrigin.x + givenPathway.pathWidth - 1, givenPathway.tileOrigin.y + givenPathway.pathHeight - 1, 0)); //Top Right
        generatedCorners.Add(new Vector3Int(givenPathway.tileOrigin.x + givenPathway.pathWidth - 1, givenPathway.tileOrigin.y, 0)); //Bottom Right
    }

    private IEnumerator WallGenerationCourotine()
    {
        //Debug the start of floor generation
        Debug.Log("Generating wall tiles at timestamp: " + Time.time);

        //Temporary list that will be filtered for duplicate coordinates
        Dictionary<Vector3Int, Color> filteredWalls = GetFilteredColoredPathways(generatedPathways);

        //Iterate over the filtered list
        foreach (KeyValuePair<Vector3Int, Color> wall in filteredWalls)
        {
            //Paint each wall tile
            wallTileMap.SetTile(wall.Key, wallTile);

            //Set the new color (if debugging)
            if (shouldDebugVisually) wallTileMap.SetColor(wall.Key, wall.Value);

            //Yield a wait for seconds function based on the wall generation speed
            //Courotines have a limitation where waiting for less than a millisecond is not possible, and so in order to allow for animation skipping a check for 0 is here
            if (wallGenerationSpeed != 0.0f && !shouldImmediatelyGenerate) yield return new WaitForSecondsRealtime(wallGenerationSpeed);
        }

        //Mark generation as completed
        completedPathwayGeneration = true;

        //Debug the completion of floor generation
        Debug.Log("Finished generating wall tiles at timestamp : " + Time.time);
    }

    private Dictionary<Vector3Int, Color> GetFilteredColoredPathways(List<PathwayData> givenPathways)
    {
        //Temporary list that will be filtered for duplicate coordinates
        Dictionary<Vector3Int, Color> filteredWalls = new Dictionary<Vector3Int, Color>();

        //Get each position point for the pathway filtering through duplicates
        foreach (PathwayData pathway in givenPathways)
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
                        //Position of the checked tile
                        Vector3Int position = new Vector3Int(row + pathway.tileOrigin.x, column + pathway.tileOrigin.y, 0);

                        //If not already saved
                        if (!filteredWalls.ContainsKey(position))
                        {
                            //Add to the temporary list the position of the wall with a random color
                            filteredWalls.Add(position, segmentColor);
                        }
                        //Otherwise set the color so that it matches the most recent wall segment
                        else
                        {
                            filteredWalls[position] = segmentColor;
                        }
                    }
                }
            }
        }

        //Return the filtered dictionary
        return filteredWalls;
    }

    private IEnumerator CornersGenerationCourotine()
    {
        //While the walls have not been generated yet, wait
        while (!completedPathwayGeneration)
        {
            yield return null;
        }

        //Debug the start of corner generation
        Debug.Log("Generating wall corners at timestamp: " + Time.time);

        //Clear the corners list of duplicated to speed up generation
        generatedCorners = generatedCorners.Distinct().ToList();

        //Iterate over the filtered list
        foreach (Vector3Int corner in generatedCorners)
        {
            //Paint each wall tile
            wallTileMap.SetTile(corner, wallTile);

            //Set the new color (if debugging)
            if (shouldDebugVisually) wallTileMap.SetColor(corner, cornerDebugColor);

            //Yield a wait for seconds function based on the wall generation speed
            //Courotines have a limitation where waiting for less than a millisecond is not possible, and so in order to allow for animation skipping a check for 0 is here
            if (wallGenerationSpeed != 0.0f && !shouldImmediatelyGenerate) yield return new WaitForSecondsRealtime(wallGenerationSpeed);
        }

        //Mark generation as completed
        completedCornerGeneration = true;

        //Debug the completion of corner generation
        Debug.Log("Finished generating corner tiles at timestamp : " + Time.time);
    }

    private IEnumerator ConnectorGenerationCourotine()
    {
        //While the corners have not been generated yet, wait
        while (!completedCornerGeneration || !completedPathwayGeneration)
        {
            yield return null;
        }

        //Verify all connector positions before generating them
        VerifyAllConnectorPositions();

        //Debug the start of connector generation
        Debug.Log("Generating connector tiles at timestamp: " + Time.time);

        //Get each position point for the pathway connector
        foreach (PathwayConnector connector in generatedConnectors)
        {
            //Paint each tile
            wallTileMap.SetTile(connector.connectorPosition, null);

            //Yield a wait for seconds function based on the wall generation speed
            //Courotines have a limitation where waiting for less than a millisecond is not possible, and so in order to allow for animation skipping a check for 0 is here
            if (wallGenerationSpeed != 0.0f && !shouldImmediatelyGenerate) yield return new WaitForSecondsRealtime(wallGenerationSpeed);
        }

        //Mark generation as completed
        completedConnectorGeneration = true;

        //Debug the completion of connector generation
        Debug.Log("Finished generating connector tiles at timestamp : " + Time.time);
    }

    private void VerifyAllConnectorPositions()
    {
        //Debug the start of connector cerification
        Debug.Log("Verifying connector tiles at timestamp: " + Time.time);

        //Temporary variable to track how many connectors were verified
        int modifiedConnectors = 0;

        //Iterate over each connector and reposition if needed
        foreach (PathwayConnector connector in generatedConnectors)
        {
            //Check if not valid
            if (!IsConnectorValid(connector))
            {
                //Find a valid position and update the connector position
                connector.UpdatePosition(GetNewValidConnectorPosition(connector));

                //Add to the tracker of modified connectors
                modifiedConnectors++;
            }
        }

        //Debug the completion of connector verification
        Debug.Log("Finished verifying " + generatedConnectors.Count + " and correcting " + modifiedConnectors + " connector tiles at timestamp : " + Time.time);
    }

    private IEnumerator MazeGenerationProgressTracker()
    {
        //Declare that the maze is being generated
        isMazeCurrentlyGenerating = true;

        //Track the time between the start of the maze generation and the end of it
        Debug.Log("Generating Recursive Division Maze at timestamp: " + Time.time);

        //Wait until all steps are completed
        while(!completedPathwayGeneration || !completedCornerGeneration || !completedConnectorGeneration)
        {
            yield return null;
        }

        //Reset the helper bool
        isMazeCurrentlyGenerating = false;

        //Debug completion time
        Debug.Log("Completed Recursive Division Maze at timestamp: " + Time.time);
    }

    private bool IsConnectorValid(Vector3Int givenPosition)
    {
        //Check if contained within the
        if (generatedCorners.Contains(givenPosition))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private bool IsConnectorValid(PathwayConnector givenConnector)
    {
        return IsConnectorValid(givenConnector.connectorPosition);
    }

    private Vector3Int GetNewValidConnectorPosition(PathwayConnector givenConnector)
    {
        //Get a list of potential alternative positions
        List<Vector3Int> positionAlternatives = new List<Vector3Int>();

        //Pupulate the list with all alternatives
        for (int i = givenConnector.minimumPosition; i <= givenConnector.maximumPosition; i++)
        {
            //Check if should set vertical or horizontal position
            if (givenConnector.isVerticalConnector)
            {
                //Add on the vertical axis
                positionAlternatives.Add(new Vector3Int(givenConnector.connectorPosition.x, i, 0));
            }
            else
            {
                //Add on the horizontal axis
                positionAlternatives.Add(new Vector3Int(i, givenConnector.connectorPosition.y, 0));
            }
        }

        //Filter the list for valid alternatives
        foreach(Vector3Int position in positionAlternatives.ToList())
        {
            //If within the corners list
            if(generatedCorners.Contains(position))
            {
                //Remove the position
                positionAlternatives.Remove(position);
            }
        }

        //If positions were found
        if(positionAlternatives.Count > 0)
        {
            //Return random alternative
            return positionAlternatives[Random.Range(0, positionAlternatives.Count)];
        }
        else
        {
            //Otherwise return the original position with a warning
            Debug.LogWarning("No door alternative was found for position " + givenConnector.connectorPosition + ", this should only happen if compared parameters were not correctly implemented!");
            return givenConnector.connectorPosition;
        }
    }
}

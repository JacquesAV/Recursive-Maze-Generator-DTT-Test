using UnityEngine;

/// <summary>
/// Informational struct to help store data about pathway connectors generated
/// Most of the data stored in this struct will be used for correcting the position in case of overlaps with walls
/// </summary>
public struct PathwayConnector
{
    //Used in position placement & correction
    public Vector3Int connectorPosition;
    public int maximumPosition;
    public int minimumPosition;

    //Tracks whether the connector is horizontally or vertically attaching pathways
    public bool isVerticalConnector;

    //Constructor
    public PathwayConnector(Vector3Int givenLocation, int givenMaximum, int givenMinimum, bool isVertical)
    {
        maximumPosition = givenMaximum;
        minimumPosition = givenMinimum;
        isVerticalConnector = isVertical;
        connectorPosition = givenLocation;
    }
}

using UnityEngine;

/// <summary>
/// Informational class to help store data about pathway connectors generated
/// Most of the data stored in this class will be used for correcting the position in case of overlaps with walls
/// Previously this would have been a struct, however in order to make it modifiable (and therefore correctable), a class is used
/// </summary>
public class PathwayConnector
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

    public void UpdatePosition(Vector3Int givenPosition)
    {
        connectorPosition = givenPosition;
    }
}

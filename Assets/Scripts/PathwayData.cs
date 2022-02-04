using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Informational struct to help store data about pathways generated
/// </summary>
public struct PathwayData
{
    //Origin position of the [athway relative to its tilemap
    public Vector3Int tileOrigin;

    //Dimensions of the pathway
    public int pathWidth;
    public int pathHeight;

    //Constructor
    public PathwayData(Vector2Int givenOrigin, int givenWidth, int givenHeight)
    {
        tileOrigin = new Vector3Int(givenOrigin.x, givenOrigin.y, 0);
        pathWidth = givenWidth;
        pathHeight = givenHeight;
    }
}

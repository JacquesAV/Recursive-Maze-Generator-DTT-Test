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
    }
}

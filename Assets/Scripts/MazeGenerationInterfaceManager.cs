using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class will serve as the central point for managing UI important to maze generation
/// </summary>
public class MazeGenerationInterfaceManager : MonoBehaviour
{
    //The current maze generator that this manager should contact
    public MazeGenerator mazeGenerator;

    //Sliders for width and height
    public Slider widthSlider = null;
    public Slider heightSlider = null;

    //Counter text
    public TextMeshProUGUI widthText = null;
    public TextMeshProUGUI heightText = null;

    //Toggle button
    public Toggle debugToggleButton = null;

    //Generate button
    public Button generateButton = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

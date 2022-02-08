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

    //Generate button
    public Button generateButton = null;

    //Toggle buttons
    public Toggle immediateGenerationToggle = null;
    public Toggle debugToggle = null;

    public void OnEnable()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        //Initialize values for the sliders & debug toggle
        InitializeSliders();
        InitializeToggles();

        //Subscribe to relevant update methods
        widthSlider.onValueChanged.AddListener(UpdateWidth);
        heightSlider.onValueChanged.AddListener(UpdateHeight);

        //Subscribe to debugger togglers
        debugToggle.onValueChanged.AddListener(ToggleDebugger);
        immediateGenerationToggle.onValueChanged.AddListener(ToggleImmediateGeneration);

        //Trigger a maze generation when pressed
        generateButton.onClick.AddListener(mazeGenerator.GenerateMaze);
    }
    private void InitializeSliders()
    {
        //Set the current values and limits based on inspector defaults
        //Width
        widthSlider.minValue = mazeGenerator.dimensionLimits.x;
        widthSlider.maxValue = mazeGenerator.dimensionLimits.y;
        widthSlider.value = mazeGenerator.mazeWidth;

        //Height
        heightSlider.minValue = mazeGenerator.dimensionLimits.x;
        heightSlider.maxValue = mazeGenerator.dimensionLimits.y;
        heightSlider.value = mazeGenerator.mazeHeight;

        UpdateDimensionsText();
    }

    private void UpdateDimensionsText()
    {
        //Update the text to display the current dimensions from the sliders
        widthText.text = ((int)mazeGenerator.mazeWidth).ToString();
        heightText.text = ((int)mazeGenerator.mazeHeight).ToString();
    }

    private void InitializeToggles()
    {
        debugToggle.isOn = mazeGenerator.shouldDebugVisually;
        immediateGenerationToggle.isOn = mazeGenerator.shouldImmediatelyGenerate;
    }

    private void UpdateHeight(float givenHeight)
    {
        //Update the height in the generator and text
        mazeGenerator.mazeHeight = (int)givenHeight;

        UpdateDimensionsText();
    }

    private void UpdateWidth(float givenWidth)
    {
        //Update the width in the generator and text
        mazeGenerator.mazeWidth = (int)givenWidth;

        UpdateDimensionsText();
    }

    private void ToggleDebugger(bool givenDebug)
    {
        //Update the bool in the generator
        mazeGenerator.shouldDebugVisually = givenDebug;
    }

    private void ToggleImmediateGeneration(bool givenToggle)
    {
        //Update the bool in the generator
        mazeGenerator.shouldImmediatelyGenerate = givenToggle;
    }
}

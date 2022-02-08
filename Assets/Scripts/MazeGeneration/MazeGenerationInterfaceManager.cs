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

    // Start is called before the first frame update
    private void Start()
    {
        //Error handling for if any UI elements were not added
        //Normally this would be handled automatically through finding matching objects, however the scope of this project deemed it unnecessary
        if (widthSlider == null) throw new MissingReferenceException("Missing width slider reference on " + gameObject.name + "!");
        if (heightSlider == null) throw new MissingReferenceException("Missing height slider reference on " + gameObject.name + "!");
        if (widthText == null) throw new MissingReferenceException("Missing width track text reference on " + gameObject.name + "!");
        if (heightText == null) throw new MissingReferenceException("Missing height track text reference on " + gameObject.name + "!");
        if (generateButton == null) throw new MissingReferenceException("Missing generation button reference on " + gameObject.name + "!");
        if (mazeGenerator == null) throw new MissingReferenceException("Missing maze generator reference on " + gameObject.name + "!");
        if (immediateGenerationToggle == null) throw new MissingReferenceException("Missing immediate generation toggler reference on " + gameObject.name + "!");
        if (debugToggle == null) throw new MissingReferenceException("Missing debug toggler reference on " + gameObject.name + "!");
    }
    private void OnEnable()
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

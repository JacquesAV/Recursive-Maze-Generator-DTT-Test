using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Camera manager that helps snap to the current dimensions of the screen and sets a zoom to fit the maze
/// </summary>
public class MazeCameraManager : MonoBehaviour
{
    //The camera that should be moved
    public Camera mazeCamera = null;

    //The UI tab that will be subtracted from camera positioning and the canvas for size comparison
    public RectTransform containerUI = null;
    public RectTransform canvasUI = null;

    //Generate button
    public Button generateButton = null;

    //The current maze generator that this manager should contact
    public MazeGenerator mazeGenerator = null;

    //How much padding should exist around the maze when changing the camera size
    [Range(0, 2)] public float cameraMazePadding = 2;

    // Start is called before the first frame update
    private void Start()
    {
        //Error handling for if any UI elements were not added
        //Normally this would be handled automatically through finding matching objects, however the scope of this project deemed it unnecessary
        if (mazeCamera == null) throw new MissingReferenceException("Missing camera reference on " + gameObject.name + "!");
        if (containerUI == null) throw new MissingReferenceException("Missing container UI reference on " + gameObject.name + "!");
        if (canvasUI == null) throw new MissingReferenceException("Missing canvas UI reference on " + gameObject.name + "!");
        if (generateButton == null) throw new MissingReferenceException("Missing generation button reference on " + gameObject.name + "!");
        if (mazeGenerator == null) throw new MissingReferenceException("Missing maze generator reference on " + gameObject.name + "!");

        //Subscribe to debugger togglers
        generateButton.onClick.AddListener(SnapMazeCamera);
    }

    //Snap camera to current maze dimensions
    //This only functions when the aspect ratio stays the same during a session
    //Can and will become incorrectly spaced if changed mid simulation
    private void SnapMazeCamera()
    {
        //Temporary variable to track and change new camera position
        Vector3 newCameraPosition = new Vector3(0, 0, mazeCamera.transform.position.z);

        //Reset the camera to this static position to improve consistency when making new translations
        mazeCamera.transform.position = newCameraPosition;

        //Set the zoom on the camera to be equal to half the width or height (whichever is greater) so that it fits on screen
        //Padding is added
        if (mazeGenerator.mazeWidth > mazeGenerator.mazeHeight)
        {
            mazeCamera.orthographicSize = (float)(mazeGenerator.mazeWidth + cameraMazePadding) / 2;
        }
        else
        {
            mazeCamera.orthographicSize = (float)(mazeGenerator.mazeHeight + cameraMazePadding) / 2;
        }

        //Determine how much of the screen space is reserved for the UI and subtract 1 to adjust it for the maze
        float screenRatioUI =  1 - (canvasUI.rect.width - containerUI.rect.width) / canvasUI.rect.width;

        //Convert the screens used space to usable units to shift the maze
        Vector2 screenInUnits = mazeCamera.ViewportToWorldPoint(new Vector2(1, 1));

        //Set to view the maze "center" (accounting for the generation UI in units)
        newCameraPosition.x = (float)mazeGenerator.mazeWidth / 2 - (screenInUnits.x * screenRatioUI);
        newCameraPosition.y = (float)mazeGenerator.mazeHeight / 2;

        //Set the new camera position
        mazeCamera.transform.position = newCameraPosition;
    }
}

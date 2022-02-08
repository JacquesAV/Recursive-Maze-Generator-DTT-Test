using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Camera manager that helps snap to the current dimensions of the screen and 
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

    // Start is called before the first frame update
    void Start()
    {
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

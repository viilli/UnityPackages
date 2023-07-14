using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;

//my custom namespaces and dependencies
using vilistaa.EditorInputSystem;
using vilistaa.Web;
using vilistaa.arrayHandler;
using vilistaa.EditorSceneExtension;


public class D2Vector3ArrayEditor: EditorWindow
{
    #region Editor window

    [MenuItem("Window/2dEditor/2Dvector3ArrayEditor")]
    public static void ShowWindow()
    {
        GetWindow(typeof(D2Vector3ArrayEditor));
    }


    #endregion

    //Save the window data so it wont be cleared after domain reload (happends when you save a code or go to play mode)

    // Script
    [SerializeField]
    private MonoBehaviour scriptInstance;
    [SerializeField]
    private static SerializedObject scriptObject;

    // Array
    [SerializeField]
    private string selectedArrayName;
    [SerializeField]
    private static SerializedProperty selectedArrayProperty;

    // All arrays
    [SerializeField]
    private List<string> arrayNames = new List<string>();

    // Checks for editing
    private bool allowEditBySystem; // If GUI is setuped correcly
    private bool allowEditByUser; // if user wants to edit
    private static bool allowEdit; // combination of those. This is just for readiblity

    // Point
    // When no point is selected this will be -1
    private static int selectedPointIndex = -1;
    private static Vector3 LastPosition;

    private static List<Vector3> points;

    //Making sure I wont delete just created point;
    private static bool CreatedAPoint = false;


    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    //This is where cutom window updates
    private void OnGUI()
    {
        #region Information about window
        EditorGUILayout.LabelField("Advanced vector3 array editor", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("To add/move/delete the points you can use buttons \"O\" or \"F15\" (can be set as macro in mouse", EditorStyles.wordWrappedLabel);

        if (EditorGUILayout.LinkButton("Documentation"))
        {
            // Web is custom WebInterface. You can find it from github https://github.com/viilli/UnityPackages
            Web.OpenWebsite("https://github.com/viilli/UnityPackages");
        }
        #endregion

        #region The dymamic GUI

        scriptInstance = EditorGUILayout.ObjectField("Script instance", scriptInstance, typeof(MonoBehaviour), true) as MonoBehaviour;
        if (scriptInstance != null)
        {
            // make the sript to SerializedObject
            if (scriptObject == null || scriptObject.targetObject != scriptInstance)
            {
                scriptObject = new SerializedObject(scriptInstance);
                scriptObject.Update();
            }

            // select array
            arrayNames = ArrayHandler.FindArrayNamesByType(scriptInstance, typeof(Vector3[]));
            // Array properties
            if (arrayNames.Count > 0)
            {
                // Display the array
                int selectedIndex = EditorGUILayout.Popup("Array", arrayNames.IndexOf(selectedArrayName), arrayNames.ToArray());

                // If array is selected
                if (selectedIndex != -1)
                {
                    selectedArrayName = arrayNames[selectedIndex];
                    selectedArrayProperty = scriptObject.FindProperty(selectedArrayName);
                    allowEditBySystem = true; // Allows editing when array is selected
                }
                else allowEditBySystem = false; // Make sure user can't edit while no array is selected
            }
            else allowEditBySystem = false; // Make sure user can't edit while there is no array in script that are compactible

        }
        else allowEditBySystem = false; // Make sure user can't edit while there is no script selected;

        // Displays the editing togle when GUI is configured correcly
        if (allowEditBySystem)
        {
            allowEditByUser = EditorGUILayout.Toggle("Allow editing", allowEditByUser);
        }

        //Combine allowEdit checks. It's just one line if statement.
        allowEdit = (allowEditBySystem && allowEditByUser) ? true : false;

        //Displays the array on the GUI window
        points = DisplayArray();

        // Apply all modifications to all variables if the script is specified
        if (scriptObject != null) scriptObject.ApplyModifiedProperties();

        #endregion
    }

    // This is called when Scene GUI is updated
    public static void OnSceneGUI(SceneView s)
    {
        // Current event
        Event currentEvent = Event.current;

        // editing is allowed
        if (allowEdit)
        {
            /*
             * I use keys "O" and "F15" for detectin button presses.
             * If you like you can define macro to your mouse that triggers F15.
             * As this script isn't compactible with normal mouse buttons
             * EditorInputSystem is custom InputSystem for editor. You can find it from githoub https://github.com/viilli/UnityPackages
             */

            // When key is pressed down. 
            if (EditorInputSystem.GetKeyDown(KeyCode.O) || EditorInputSystem.GetKeyDown(KeyCode.F15))
            {
                Debug.Log("keydown");
                //Set it on begin to -1 for making sure that last point isn't carried on.
                selectedPointIndex = -1;

                // Get mouse position in world space
                Vector3 mouseWorldPosition = EditorSceneExtension.MousePosInWorld2D(currentEvent);

                // Get the closest point
                (int closestPoint, float distance) = ClosestPoint(mouseWorldPosition, points);

                // If mouse is touching a point it selects it.
                if (distance < 0.2f) selectedPointIndex = closestPoint;

                // If there isn't any point selected -> create new one
                if (selectedPointIndex == -1)
                {

                    (int firstPoint, int SecondPoint, float dist) = closestLine(mouseWorldPosition, points);

                    // Adding point between point's if cursor is close to a line
                    if (dist < 0.2f)
                    {
                        Debug.Log("Addin between");
                        selectedPointIndex = (firstPoint + 1) % selectedArrayProperty.arraySize;
                        selectedArrayProperty.InsertArrayElementAtIndex(selectedPointIndex);
                        SerializedProperty newe = selectedArrayProperty.GetArrayElementAtIndex(selectedPointIndex);
                        newe.vector3Value = mouseWorldPosition;
                        scriptObject.ApplyModifiedProperties();
                    }

                    // Adding new Point to last index
                    else
                    {
                        selectedArrayProperty.arraySize++;
                        var newElement = selectedArrayProperty.GetArrayElementAtIndex(selectedArrayProperty.arraySize - 1);
                        newElement.vector3Value = mouseWorldPosition;
                        LastPosition = mouseWorldPosition;
                        CreatedAPoint = true;
                        scriptObject.ApplyModifiedProperties();
                    }
                }
                else
                {
                    LastPosition = mouseWorldPosition;
                }
            }

            //When key is pressed.
            else if (EditorInputSystem.GetKey(KeyCode.O) || EditorInputSystem.GetKey(KeyCode.F15))
            {
                // If there is a point selected
                if (selectedPointIndex != -1)
                {
                    // Get mouse position in world space
                    Vector3 mouseWorldPosition = EditorSceneExtension.MousePosInWorld2D(currentEvent);

                    // If the mouse is moved, it will move the point. Distance of the mouse required to move before this
                    // is triggerd is 0.01f
                    if (Vector3.Distance(mouseWorldPosition, LastPosition) > 0.01f)
                    {
                        var selectedElement = selectedArrayProperty.GetArrayElementAtIndex(selectedPointIndex);
                        selectedElement.vector3Value = mouseWorldPosition;
                        scriptObject.ApplyModifiedProperties();
                    }
                }
            }
            //When key is released
            else if (EditorInputSystem.GetKeyUp(KeyCode.O) || EditorInputSystem.GetKeyUp(KeyCode.F15))
            {
                // Used to delete points
                if (!CreatedAPoint)
                {
                    // Get mouse position in world space
                    Vector3 mouseWorldPosition = EditorSceneExtension.MousePosInWorld2D(currentEvent);
                    (int point, float dist) = ClosestPoint(mouseWorldPosition, points);

                    // If mouse is close enough and it hasn't moved point will be deleted
                    if (dist < 0.2f && Vector3.Distance(LastPosition, mouseWorldPosition) < 0.01f)
                    {
                        selectedArrayProperty.DeleteArrayElementAtIndex(point);
                    }
                }
                else CreatedAPoint = false;
            }
        }
        RepaintAll();
        PaintTheScene();
    }

    private static List<Vector3> DisplayArray()
    {

        List<Vector3> arr = new List<Vector3>();
        GUILayout.Label("Array");
        if (GUILayout.Button("Clear Array"))
        {
            selectedArrayProperty.ClearArray();
            Handles.DrawSolidDisc(Vector3.zero, Vector3.forward, 1f);

        }


        if (selectedArrayProperty != null)
        {
            for (int i = 0; i < selectedArrayProperty.arraySize; i++)
            {
                arr.Add(selectedArrayProperty.GetArrayElementAtIndex(i).vector3Value);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(selectedArrayProperty.GetArrayElementAtIndex(i), GUIContent.none);
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    selectedArrayProperty.DeleteArrayElementAtIndex(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        return arr;
    }
    #region ClosestPoint()
    public static (int, float) ClosestPoint(Vector3 input, Vector3[] vectorArray)
    {
        float distance = float.MaxValue;
        int idx = 0;

        for (int i = 0; i < vectorArray.Length; i++)
        {
            float newDist = Vector3.Distance(vectorArray[i], input);
            if (distance > newDist)
            {
                distance = newDist;
                idx = i;
            }
        }

        return (idx, distance);
    }

    public static (int, float) ClosestPoint(Vector3 input, List<Vector3> vectorArray)
    {
        float distance = float.MaxValue;
        int idx = -1;

        for (int i = 0; i < vectorArray.Count; i++)
        {
            float newDist = Vector3.Distance(vectorArray[i], input);
            if (distance > newDist)
            {
                distance = newDist;
                idx = i;
            }
        }

        return (idx, distance);
    }

    #endregion

    #region CloseLine()
    /// <summary>
    /// distance between point and a line
    /// </summary>
    /// <param name="input"></param>
    /// <param name="vectorArray"></param>
    /// <returns>first point, second point, distance</returns>
    public static (int, int, float) closestLine(Vector3 input, Vector3[] vectorArray)
    {
        int first = -1;
        int second = 1;
        float dist = float.MaxValue;

        for (int pointIdx = 0; pointIdx < selectedArrayProperty.arraySize; pointIdx++)
        {
            Vector3 currentPoint = vectorArray[pointIdx];
            Vector3 nextPoint = vectorArray[(pointIdx + 1) % vectorArray.Length];
            float distanceToLine = HandleUtility.DistancePointLine(input, currentPoint, nextPoint);

            if (dist > distanceToLine)
            {
                first = pointIdx;
                second = (pointIdx + 1) % vectorArray.Length;
                dist = distanceToLine;
            }
        }

        return (first, second, dist);
    }
    /// <summary>
    /// distance between point and a line
    /// </summary>
    /// <param name="input"></param>
    /// <param name="vectorArray"></param>
    /// <returns>first point, second point, distance</returns>
    public static (int, int, float) closestLine(Vector3 input, List<Vector3> vectorArray)
    {
        int first = -1;
        int second = 1;
        float dist = float.MaxValue;

        for (int pointIdx = 0; pointIdx < selectedArrayProperty.arraySize; pointIdx++)
        {
            Vector3 currentPoint = vectorArray[pointIdx];
            Vector3 nextPoint = vectorArray[(pointIdx + 1) % vectorArray.Count];
            float distanceToLine = HandleUtility.DistancePointLine(input, currentPoint, nextPoint);

            if (dist > distanceToLine)
            {
                first = pointIdx;
                second = (pointIdx + 1) % vectorArray.Count;
                dist = distanceToLine;
            }
        }

        return (first, second, dist);
    }
    #endregion

    public static void PaintTheScene()
    {
        if (points != null)
        {
            for (int j = 0; j < points.Count; j++)
            {
                var currentElement = points[j];

                Vector3 currentPos = new Vector3(currentElement.x, currentElement.y, 0);

                var nextElement = points[(j + 1) % points.Count];
                Vector3 nextPos = new Vector3(nextElement.x, nextElement.y, 0);
                Handles.DrawSolidDisc(currentPos, Vector3.forward, 0.2f);
                Handles.DrawLine(currentPos, nextPos);

                // Print the index next to the current point
                Handles.Label(currentPos + new Vector3(0.5f, 0, 0), j.ToString());
            }
        }
    }

    private static void RepaintAll()
    {
        // Iterate through all open EditorWindows
        // Find all open EditorWindows
        var windows = Resources.FindObjectsOfTypeAll<D2Vector3ArrayEditor>();
        foreach (var window in windows)
        {
            // Check if the window is an instance of your custom editor window
            if (window.GetType() == typeof(D2Vector3ArrayEditor))
            {
                window.Repaint();
            }
        }
    }

}



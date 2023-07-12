using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace vilistaa.EditorSceneExtension
{
    public class EditorSceneExtension
    {
        public static Vector3 MousePosInWorld2D(Event currentEvent)
        {
            Vector2 mousePosition = currentEvent.mousePosition;
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            Vector3 worldPosition = ray.origin + ray.direction;
            Vector3 flatWorldPosition = new Vector3(worldPosition.x, worldPosition.y, 0);
            return flatWorldPosition;
        }
    }
}
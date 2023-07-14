using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace vilistaa.EditorInputSystem
{
    public static class EditorInputSystem
    {
        private static Dictionary<KeyCode, bool> previousFrameKeyStates = new Dictionary<KeyCode, bool>();

        public static bool GetKeyDown(KeyCode keyCode)
        {
            bool currentKeyState = Event.current != null && Event.current.isKey && Event.current.type == EventType.KeyDown && Event.current.keyCode == keyCode;
            bool previousKeyState = false;
            previousFrameKeyStates.TryGetValue(keyCode, out previousKeyState);

            if (currentKeyState && !previousKeyState)
            {
                previousFrameKeyStates[keyCode] = true;
                Debug.Log("KeyDown: " + keyCode);
                return true;
            }

            return false;
        }

        public static bool GetKeyUp(KeyCode keyCode)
        {
            bool currentKeyState = Event.current != null && Event.current.isKey && Event.current.type == EventType.KeyUp && Event.current.keyCode == keyCode;
            bool previousKeyState = false;
            previousFrameKeyStates.TryGetValue(keyCode, out previousKeyState);

            if (currentKeyState && previousKeyState)
            {
                previousFrameKeyStates[keyCode] = false;
                Debug.Log("KeyUp: " + keyCode);
                return true;
            }

            return false;
        }

        public static bool GetKey(KeyCode keyCode)
        {
            bool currentKeyState = Event.current != null && Event.current.isKey && Event.current.type == EventType.KeyDown && Event.current.keyCode == keyCode;
            bool previousKeyState = false;
            previousFrameKeyStates.TryGetValue(keyCode, out previousKeyState);

            if (currentKeyState && previousKeyState)
            {
                previousFrameKeyStates[keyCode] = true;
                return true;
            }

            return false;
        }
    }
}

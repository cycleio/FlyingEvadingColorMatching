#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/*
 * Usage:
 * 1. Copy this cs file
 * 2. Do Util/Reset Input settings
 * 
 * memo: set only axis
 */

/// <summary>
/// Generator of Input settings
/// </summary>
public class InputSettingGenerator
{
    /// <summary>
    /// Info for each inputs
    /// </summary>
    private struct InputAxis
    {
        public enum AxisType
        {
            KeyOrMouseButton = 0,
            MouseMovement = 1,
            JoystickAxis = 2
        };

        public string name;
        public string descriptiveName;
        public string descriptiveNegativeName;
        public string negativeButton;
        public string positiveButton;
        public string altNegativeButton;
        public string altPositiveButton;

        public float gravity;
        public float dead;
        public float sensitivity;

        public bool snap;
        public bool invert;

        public AxisType type;

        public int axis; // 0-indexed
        public int joyNum; // 0 for all gamepads. 1~ for each correspond gamepad.

        public InputAxis(int padNum, bool isAxis, string _name)
        {
            name = _name;
            descriptiveName = "";
            descriptiveNegativeName = "";
            snap = false;
            invert = false;

            negativeButton = "";
            positiveButton = "";
            altNegativeButton = "";
            altPositiveButton = "";
            axis = 0;

            if (isAxis)
            {
                if (padNum >= 0)
                {
                    gravity = 0.0f;
                    dead = 0.19f;
                    sensitivity = 1.0f;
                    joyNum = padNum;
                    type = AxisType.JoystickAxis;
                }
                else
                {
                    gravity = 3.0f;
                    dead = 0.001f;
                    sensitivity = 3.0f;
                    joyNum = 0;
                    type = AxisType.KeyOrMouseButton;

                    snap = true;
                }
            }
            else
            {
                gravity = 1000.0f;
                dead = 0.001f;
                sensitivity = 1000.0f;
                joyNum = padNum >= 0 ? padNum : 0;
                type = AxisType.KeyOrMouseButton;
            }
        }
    }

    SerializedObject serializedObject;
    SerializedProperty axesProperty;
    

    // ###### must be same as InputManager
    private static readonly int padNum = 4;
    private static readonly int padButtonNum = 10;
    private static readonly int padAxisNum = 2;
    private static readonly int keyboardAxisNum = 4;

    private static string GetButtonName(int player, int buttonNumber)
    {
        if (player <= 0) return $"Keyboard_Button{buttonNumber}";
        return $"Pad{player}_Button{buttonNumber}";
    }

    private static string GetAxisName(int player, int axisNumber)
    {
        if (player <= 0) return $"Keyboard_Axis{axisNumber}";
        return $"Pad{player}_Axis{axisNumber}";
    }
    // ######

    /// <summary>
    /// Input setting Generator
    /// </summary>
    [MenuItem("Util/Reset Input settings")]
    public static void ResetInputSettings()
    {
        string[] usableKeyboardButtons = {
            "z", "x", "c" // modify this
        };


        var generator = new InputSettingGenerator();

        generator.Clear();

        // for pads
        for(int i = 1; i <= padNum; ++i)
        {
            // buttons
            for (int j = 0; j < padButtonNum; ++j)
            {
                generator.AddButton(GetAxisName(i, j), $"joystick button {j}", i);
            }

            // axes
            for (int j = 0; j < padAxisNum; ++j)
            {
                generator.AddPadAxis(GetAxisName(i, j), i, j);
            }
        }

        // for keyboard
        for(int j = 0; j < usableKeyboardButtons.Length; ++j)
        {
            generator.AddButton(GetButtonName(-1, j), usableKeyboardButtons[j]);
        }

        generator.AddKeyAxis(GetAxisName(-1, 0), "right", "left");
        generator.AddKeyAxis(GetAxisName(-1, 1), "up", "down");
        generator.AddKeyAxis(GetAxisName(-1, 2), "a", "d");
        generator.AddKeyAxis(GetAxisName(-1, 3), "w", "s");
    }
    

    private InputSettingGenerator()
    {
        serializedObject = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
        axesProperty = serializedObject.FindProperty("m_Axes");
    }

    private void AddButton(string name, string posButton, int padNum = -1, string altPosButton = "")
    {
        var axis = new InputAxis(padNum < 0 ? -1 : padNum, false, name);
        axis.positiveButton = posButton;
        axis.altPositiveButton = altPosButton;
        AddAxis(ref axis);
    }

    private void AddKeyAxis(string name, string posButton, string negButton, string altPosButton = "", string altNegButton = "")
    {
        var axis = new InputAxis(-1, true, name);
        axis.positiveButton = posButton;
        axis.negativeButton = negButton;
        axis.altPositiveButton = altPosButton;
        axis.altNegativeButton = altNegButton;
        AddAxis(ref axis);
    }

    private void AddPadAxis(string name, int padNum, int axisNum, bool invert = false)
    {
        if (padNum < 0) Debug.LogError("padNum must be >=0");
        if (axisNum < 0) Debug.LogError("axisNum must be >=0");

        var axis = new InputAxis(padNum, true, name);
        axis.axis = axisNum;
        axis.invert = invert;
        AddAxis(ref axis);
    }

    private void AddAxis(ref InputAxis axis)
    {
        SerializedProperty axesProperty = serializedObject.FindProperty("m_Axes");

        axesProperty.arraySize++;
        serializedObject.ApplyModifiedProperties();

        SerializedProperty child = axesProperty.GetArrayElementAtIndex(axesProperty.arraySize - 1).Copy();
        child.Next(true);
        do
        {
            if (child.name == "m_Name") child.stringValue = axis.name;
            if (child.name == "descriptiveName") child.stringValue = axis.descriptiveName;
            if (child.name == "descriptiveNegativeName") child.stringValue = axis.descriptiveNegativeName;
            if (child.name == "negativeButton") child.stringValue = axis.negativeButton;
            if (child.name == "positiveButton") child.stringValue = axis.positiveButton;
            if (child.name == "altNegativeButton") child.stringValue = axis.altNegativeButton;
            if (child.name == "altPositiveButton") child.stringValue = axis.altPositiveButton;
            if (child.name == "gravity") child.floatValue = axis.gravity;
            if (child.name == "dead") child.floatValue = axis.dead;
            if (child.name == "sensitivity") child.floatValue = axis.sensitivity;
            if (child.name == "snap") child.boolValue = axis.snap;
            if (child.name == "invert") child.boolValue = axis.invert;
            if (child.name == "type") child.intValue = (int)axis.type;
            if (child.name == "axis") child.intValue = axis.axis;
            if (child.name == "joyNum") child.intValue = axis.joyNum;
        }
        while (child.Next(false));

        serializedObject.ApplyModifiedProperties();
    }

    private void Clear()
    {
        axesProperty.ClearArray();
        serializedObject.ApplyModifiedProperties();
    }
}

#endif
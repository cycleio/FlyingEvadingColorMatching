#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;


/// <summary>
/// InputManagerを設定するためのクラス
/// </summary>
namespace CycleUtils
{
    public class InputManagerGenerator
    {
        private class InputAxis
        {
            public enum AxisType
            {
                KeyOrMouseButton = 0,
                MouseMovement = 1,
                JoystickAxis = 2
            };

            public readonly string name;
            public static readonly string descriptiveName = "";
            public static readonly string descriptiveNegativeName = "";
            public string negativeButton;
            public string positiveButton;
            public string altNegativeButton;
            public string altPositiveButton;

            public float gravity = 0;
            public float dead = 0;
            public float sensitivity = 0;

            public bool snap = false;
            public bool invert = false;

            public int Type { get { return (int)type; } }
            public AxisType type = AxisType.KeyOrMouseButton;

            public int axis = 1; // 0-indexed
            public int joyNum = 0; // 0 for all pads, 1~ for a correspond pad

            private InputAxis(string name)
            {
                Debug.Assert(name != null && name != "");
                this.name = name;
            }

            public static InputAxis CreateButton(string name, string positiveButton, string altPositiveButton = "")
            {
                Debug.Assert(positiveButton != null && positiveButton != "");
                Debug.Assert(altPositiveButton != null);

                var axis = new InputAxis(name);
                axis.positiveButton = positiveButton;
                axis.altPositiveButton = altPositiveButton;
                axis.gravity = 1000;
                axis.dead = 0.001f;
                axis.sensitivity = 1000;
                axis.type = AxisType.KeyOrMouseButton;

                return axis;
            }

            public static InputAxis CreatePadAxis(string name, int joystickNum, int axisNum, bool invert = false)
            {
                Debug.Assert(joystickNum >= 0);
                Debug.Assert(axisNum >= 0);

                var axis = new InputAxis(name);
                axis.gravity = 0.0f;
                axis.dead = 0.2f;
                axis.sensitivity = 1.0f;
                axis.axis = axisNum;
                axis.joyNum = joystickNum;
                axis.invert = invert;
                axis.type = AxisType.JoystickAxis;

                return axis;
            }

            public static InputAxis CreateKeyAxis(string name, string positiveButton, string negativeButton, string altPositiveButton = "", string altNegativeButton = "")
            {
                Debug.Assert(positiveButton != null && positiveButton != "");
                Debug.Assert(altPositiveButton != null);
                Debug.Assert(negativeButton != null && negativeButton != "");
                Debug.Assert(altNegativeButton != null);

                var axis = new InputAxis(name);
                axis.negativeButton = negativeButton;
                axis.positiveButton = positiveButton;
                axis.altNegativeButton = altNegativeButton;
                axis.altPositiveButton = altPositiveButton;
                axis.snap = true;
                axis.gravity = 3;
                axis.sensitivity = 3;
                axis.dead = 0.001f;
                axis.type = AxisType.KeyOrMouseButton;

                return axis;
            }
        }

        private static SerializedObject serializedObject;

        [MenuItem("Edit/CycleUtils/Regenerate Inputs")]
        private static void RegenerateInputSettings()
        {
            // Load InputManager.asset
            serializedObject = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
            var axesProperty = serializedObject.FindProperty("m_Axes");

            // Load
            var inputSetting = Resources.Load<InputSettings>("InputManagerSettings");

            // Clear setting
            axesProperty.ClearArray();

            // Keyboard Initialize
            for(int i = 0; i < inputSetting.keyboardAxesMapping.Length; ++i)
            {
                var keyAxis = inputSetting.keyboardAxesMapping[i];
                AddAxis(axesProperty,
                    InputAxis.CreateKeyAxis($"Keyboard_Axis{i}", keyAxis.positive, keyAxis.negative));
            }
            for (int i = 0; i < inputSetting.keyboardButtonsMapping.Length; ++i)
            {
                AddAxis(axesProperty,
                    InputAxis.CreateButton($"Keyboard_Button{i}", inputSetting.keyboardButtonsMapping[i]));
            }

            // Joystick(s) Initialize
            for (int i = 1; i <= inputSetting.padNum; ++i)
            {
                for (int j = 0; j < inputSetting.padAxisNum; ++j)
                {
                    AddAxis(axesProperty,
                        InputAxis.CreatePadAxis($"Joystick{i}_Axis{j}", i, j));
                    // todo: invert
                }

                for (int j = 0; j < inputSetting.padButtonNum; ++j)
                {
                    AddAxis(axesProperty,
                        InputAxis.CreateButton($"Joystick{i}_Button{j}", $"joystick {i} button {j}"));
                }
            }

            // Apply changes
            serializedObject.ApplyModifiedProperties();
        }

        private static void AddAxis(SerializedProperty axesProperty, InputAxis axis)
        {
            ++axesProperty.arraySize;
            serializedObject.ApplyModifiedProperties();

            SerializedProperty child = axesProperty.GetArrayElementAtIndex(axesProperty.arraySize - 1).Copy();
            child.Next(true);
            do
            {
                if (child.name == "m_Name") child.stringValue = axis.name;
                if (child.name == "descriptiveName") child.stringValue = InputAxis.descriptiveName;
                if (child.name == "descriptiveNegativeName") child.stringValue = InputAxis.descriptiveNegativeName;
                if (child.name == "negativeButton") child.stringValue = axis.negativeButton;
                if (child.name == "positiveButton") child.stringValue = axis.positiveButton;
                if (child.name == "altNegativeButton") child.stringValue = axis.altNegativeButton;
                if (child.name == "altPositiveButton") child.stringValue = axis.altPositiveButton;
                if (child.name == "gravity") child.floatValue = axis.gravity;
                if (child.name == "dead") child.floatValue = axis.dead;
                if (child.name == "sensitivity") child.floatValue = axis.sensitivity;
                if (child.name == "invert") child.boolValue = axis.invert;
                if (child.name == "type") child.intValue = axis.Type;
                if (child.name == "axis") child.intValue = axis.axis;
                if (child.name == "joyNum") child.intValue = axis.joyNum;
            }
            while (child.Next(false));
        }
    }
}
#endif

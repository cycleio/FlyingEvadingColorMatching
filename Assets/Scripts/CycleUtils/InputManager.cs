using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System.Linq;

namespace CycleUtils
{
    /// <summery>
    /// Input Manager.
    /// 
    /// </summery>
    public class InputManager
    {
        public static InputManager Instance { get; } = new InputManager();
        public static readonly float axisThreshold = 0.7f;

        private readonly InputSettings settings;
        private Dictionary<string, int> axisMapping;
        private Dictionary<string, int> buttonMapping;

        public int JoystickNum { get { return settings.padNum; } }
        public int JoystickAxisNum { get { return settings.padAxisNum; } }
        public int JoystickButtonNum { get { return settings.padButtonNum; } }

        public FloatReactiveProperty GetAxisRaw(int joystickId, int axisId)
        {
            return inputProperties[joystickId].Axes[axisId].axis;
        }

        public BoolReactiveProperty GetAxisPositive(int joystickId, int axisId)
        {
            return inputProperties[joystickId].Axes[axisId].positive;
        }

        public BoolReactiveProperty GetAxisNegative(int joystickId, int axisId)
        {
            return inputProperties[joystickId].Axes[axisId].negative;
        }

        public BoolReactiveProperty GetButton(int joystickId, int buttonId)
        {
            return inputProperties[joystickId].Buttons[buttonId];
        }

        /*public static string GetAxisName(int player, int axisNumber)
        {
            if (player <= 0) return $"Keyboard_Axis{axisNumber}";
            return $"Pad{player}_Axis{axisNumber}";
        }*/

        #region Private Variables / Methods
        private class InputReactiveProperties
        {
            public struct AxisWithThreshold
            {
                public FloatReactiveProperty axis;
                public BoolReactiveProperty positive;
                public BoolReactiveProperty negative;
            }

            public List<AxisWithThreshold> Axes { get; } = new List<AxisWithThreshold>();
            public List<BoolReactiveProperty> Buttons { get; } = new List<BoolReactiveProperty>();

            private CompositeDisposable disposables = new CompositeDisposable();

            public void AddAxis(string inputName)
            {
                AxisWithThreshold properties = new AxisWithThreshold();
                properties.axis = new FloatReactiveProperty();
                properties.positive = new BoolReactiveProperty();
                properties.negative = new BoolReactiveProperty();
                Observable.EveryUpdate()
                    .Subscribe(_ =>
                    {
                        var input = Input.GetAxisRaw(inputName);
                        properties.axis.SetValueAndForceNotify(input);
                        properties.positive.SetValueAndForceNotify(input > axisThreshold);
                        properties.negative.SetValueAndForceNotify(input < -axisThreshold);
                    })
                    .AddTo(disposables);
                Axes.Add(properties);
            }

            public void AddButton(string inputName)
            {
                BoolReactiveProperty property = new BoolReactiveProperty();
                Observable.EveryUpdate()
                    .Subscribe(_ =>
                    {
                        property.SetValueAndForceNotify(Input.GetButton(inputName));
                    })
                    .AddTo(disposables);
                Buttons.Add(property);
            }

            public void AddKeyAsButton(string inputName)
            {
                BoolReactiveProperty property = new BoolReactiveProperty();
                Observable.EveryUpdate()
                    .Subscribe(_ =>
                    {
                        property.SetValueAndForceNotify(Input.GetKey(inputName));
                    })
                    .AddTo(disposables);
                Buttons.Add(property);
            }

            public void Clear()
            {
                disposables.Dispose();
                Axes.Clear();
                Buttons.Clear();
            }
        }

        private List<InputReactiveProperties> inputProperties = new List<InputReactiveProperties>();

        private InputManager()
        {
            settings = Resources.Load<InputSettings>("InputSettings");

            // how to get list of axes/buttons?

            var property = new InputReactiveProperties();
            property.AddAxis("Horizontal");
            //property.AddAxis("Vertical");
            property.AddButton("ColorChangeKey");
            /*property.AddAxis("ColorHorizontal");
            property.AddAxis("ColorVertical");*/
            inputProperties.Add(property);

            SingleAssignmentDisposable disposable = new SingleAssignmentDisposable();
            disposable.Disposable = Observable.OnceApplicationQuit()
                .Subscribe(_ =>
                {
                    inputProperties.Clear();
                    disposable.Dispose();
                });
        }
        #endregion

        /*public void TestProperties()
        {
            foreach(var prop in inputProperties)
            {
                prop.Buttons.ForEach(val => val.Where(v => v).Subscribe(_ => Debug.Log))
            }

            foreach (var prop in Buttons) prop.Value.Where(val => val)
                      .Subscribe(_ => Debug.Log(Enum.GetName(typeof(ButtonNames), prop.Key) + " On"));
            foreach (var prop in PositiveThresholdAxis) prop.Value.Where(val => val)
                      .Subscribe(_ => Debug.Log(Enum.GetName(typeof(AxisNames), prop.Key) + "(Positive) On"));
            foreach (var prop in NegativeThresholdAxis) prop.Value.Where(val => val)
                      .Subscribe(_ => Debug.Log(Enum.GetName(typeof(AxisNames), prop.Key) + "(Negative) On"));
        }*/
    }
}

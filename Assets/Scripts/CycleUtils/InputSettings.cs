using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CycleUtils {

    [CreateAssetMenu(fileName = "InputSettings", menuName = "CycleUtils/InputSetting")]
    public class InputSettings : ScriptableObject
    {
        
        public int padNum = 4;
        public int padAxisNum = 2;
        public int padButtonNum = 10;

        [System.Serializable]
        public struct KeyboardAxisMap
        {
            public string positive;
            public string negative;
        }
        public KeyboardAxisMap[] keyboardAxesMapping;
        public string[] keyboardButtonsMapping;


    }
}

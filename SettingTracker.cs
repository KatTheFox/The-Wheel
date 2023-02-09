using System;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using UnityEngine;

    public class SettingTracker: ISettingSubscriber    {
        private static float[] factorMap = new float[9]
        {
            0.2f,
            0.5f,
            2f,
            3f,
            5f,
            8f,
            13f,
            21f,
            34f
        };

        public void WhenSettingUpdated(object newValue)
        {
            if (!(newValue is int num1))
                num1 = 1;
            int num2 = num1;
            int index = Mathf.Min(SettingTracker.factorMap.Length - 1, Mathf.Max(num2, 0));
            TheWheel.speedStep = SettingTracker.factorMap[index] * 0.05f;
        }

        public void BeforeSettingUpdated(object newValue)
        {
        }
    }

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dubi.Functions;

namespace Dubi.Tools.Lines
{
    public class Accelerator : MonoBehaviour
    {
        [System.Serializable]
        public struct AcceleratorValues
        {
            public AcceleratorValues(float goalSpeed, ChangeSpeed type, float changeDuration)
            {
                this.goalSpeed = goalSpeed;
                this.type = type;
                this.changeDuration = changeDuration;
            }

            public float goalSpeed;
            public ChangeSpeed type;
            public float changeDuration;
        }

        public enum ChangeSpeed
        {
            Instant,
            Linear,
            EaseInOut,
        }


        public float speed;   
        Coroutine speedRoutine;   

        public void ChangeSpeedTo(AcceleratorValues accValues)
        {
            if (this.speedRoutine != null)
            {
                StopCoroutine(this.speedRoutine);
            }

            AnimationCurve curve = null;

            switch (accValues.type)
            {
                case ChangeSpeed.EaseInOut:
                    curve = AnimationCurves.EaseInToEaseTop();
                    break;

                case ChangeSpeed.Linear:
                    curve = AnimationCurves.Linear();
                    break;

                case ChangeSpeed.Instant:
                    this.speed = accValues.goalSpeed;
                    return;
            }
        
            this.speedRoutine = StartCoroutine(Coroutines.FloatOverTime(accValues.changeDuration, curve, this.speed, accValues.goalSpeed, result => this.speed = result));
        }
    }
}

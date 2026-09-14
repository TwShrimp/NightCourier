using UnityEngine;
using UnityEngine.InputSystem;

namespace NightCourier.Vehicle
{
    public sealed class CarInput : MonoBehaviour
    {
        public float Throttle { get; private set; }
        public float Steering { get; private set; }
        public bool Brake { get; private set; }
        private bool resetRequested;

        private void Update()
        {
            Throttle = 0f;
            Steering = 0f;
            Brake = false;
            bool forward = false, reverse = false, right = false, left = false;
            foreach (var device in InputSystem.devices)
            {
                if (device is not Keyboard keyboard || !keyboard.enabled) continue;
                forward |= keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed;
                reverse |= keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed;
                right |= keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed;
                left |= keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed;
                Brake |= keyboard.spaceKey.isPressed;
                resetRequested |= keyboard.rKey.wasPressedThisFrame;
            }
            Throttle = (forward ? 1f : 0f) - (reverse ? 1f : 0f);
            Steering = (right ? 1f : 0f) - (left ? 1f : 0f);
        }

        public bool ConsumeReset()
        {
            bool requested = resetRequested;
            resetRequested = false;
            return requested;
        }
    }
}

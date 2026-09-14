using UnityEngine;
using UnityEngine.InputSystem;

namespace NightCourier.Vehicle
{
    public enum TurnSignalMode
    {
        Off,
        Left,
        Right,
        Hazard
    }

    /// <summary>Owns the pickup's functional lamps and persistent turn-signal controls.</summary>
    [DefaultExecutionOrder(50)]
    public sealed class VehicleLighting : MonoBehaviour
    {
        private const float BlinkInterval = 0.5f;
        private const float HeadlightHoldSeconds = 2f;
        private ArcadeCarController controller;
        private Renderer[] brakeLamps;
        private Renderer[] reverseLamps;
        private Renderer[] leftIndicators;
        private Renderer[] rightIndicators;
        private Light[] brakeGlow;
        private Light[] reverseGlow;
        private Light[] leftGlow;
        private Light[] rightGlow;
        private Renderer[] headlamps;
        private Renderer[] rearRunningLamps;
        private Light[] headlightGlow;
        private float blinkStartedAt;
        private float bothKeysStartedAt;
        private bool bothKeysWereHeld;
        private bool longHoldTriggered;
        private TurnSignalMode signalBeforeBoth;

        public TurnSignalMode SignalMode { get; private set; }
        public bool BrakeLightsOn { get; private set; }
        public bool ReverseLightsOn { get; private set; }
        public bool LeftIndicatorOn { get; private set; }
        public bool RightIndicatorOn { get; private set; }
        public bool HeadlightsOn { get; private set; } = true;

        public void Configure(
            ArcadeCarController vehicle,
            Renderer[] brake,
            Renderer[] reverse,
            Renderer[] left,
            Renderer[] right,
            Light[] brakeLights,
            Light[] reverseLights,
            Light[] leftLights,
            Light[] rightLights,
            Renderer[] headlightRenderers,
            Renderer[] runningRearRenderers,
            Light[] headlightLights)
        {
            controller = vehicle;
            brakeLamps = brake;
            reverseLamps = reverse;
            leftIndicators = left;
            rightIndicators = right;
            brakeGlow = brakeLights;
            reverseGlow = reverseLights;
            leftGlow = leftLights;
            rightGlow = rightLights;
            headlamps = headlightRenderers;
            rearRunningLamps = runningRearRenderers;
            headlightGlow = headlightLights;
            Apply(true);
        }

        private void Update()
        {
            ReadSignalInput();
            BrakeLightsOn = controller != null && controller.IsBraking;
            ReverseLightsOn = controller != null && controller.IsReversing;
            bool blinkOn = SignalMode != TurnSignalMode.Off
                && Mathf.Repeat(Time.unscaledTime - blinkStartedAt, BlinkInterval * 2) < BlinkInterval;
            LeftIndicatorOn = blinkOn && (SignalMode == TurnSignalMode.Left || SignalMode == TurnSignalMode.Hazard);
            RightIndicatorOn = blinkOn && (SignalMode == TurnSignalMode.Right || SignalMode == TurnSignalMode.Hazard);
            Apply(true);
        }

        private void ReadSignalInput()
        {
            bool leftPressed = false, rightPressed = false, leftHeld = false, rightHeld = false;
            foreach (var device in InputSystem.devices)
            {
                if (device is not Keyboard keyboard || !keyboard.enabled) continue;
                leftPressed |= keyboard.commaKey.wasPressedThisFrame;
                rightPressed |= keyboard.periodKey.wasPressedThisFrame;
                leftHeld |= keyboard.commaKey.isPressed;
                rightHeld |= keyboard.periodKey.isPressed;
            }
            bool bothHeld = leftHeld && rightHeld;
            if (bothHeld)
            {
                if (!bothKeysWereHeld)
                {
                    bothKeysStartedAt = Time.unscaledTime;
                    longHoldTriggered = false;
                    bothKeysWereHeld = true;
                    signalBeforeBoth = SignalMode;
                    SetSignal(TurnSignalMode.Off);
                }
                else if (!longHoldTriggered && Time.unscaledTime - bothKeysStartedAt >= HeadlightHoldSeconds)
                {
                    HeadlightsOn = !HeadlightsOn;
                    longHoldTriggered = true;
                }
                return;
            }
            if (bothKeysWereHeld)
            {
                bothKeysWereHeld = false;
                if (!longHoldTriggered)
                    SetSignal(signalBeforeBoth == TurnSignalMode.Hazard ? TurnSignalMode.Off : TurnSignalMode.Hazard);
                return;
            }
            if (leftPressed)
                SetSignal(SignalMode == TurnSignalMode.Left ? TurnSignalMode.Off : TurnSignalMode.Left);
            else if (rightPressed)
                SetSignal(SignalMode == TurnSignalMode.Right ? TurnSignalMode.Off : TurnSignalMode.Right);
        }

        private void SetSignal(TurnSignalMode mode)
        {
            SignalMode = mode;
            blinkStartedAt = Time.unscaledTime;
        }

        private void Apply(bool useCurrentState)
        {
            SetActive(brakeLamps, useCurrentState && BrakeLightsOn);
            SetActive(reverseLamps, useCurrentState && ReverseLightsOn);
            SetActive(leftIndicators, useCurrentState && LeftIndicatorOn);
            SetActive(rightIndicators, useCurrentState && RightIndicatorOn);
            SetActive(brakeGlow, useCurrentState && BrakeLightsOn);
            SetActive(reverseGlow, useCurrentState && ReverseLightsOn);
            SetActive(leftGlow, useCurrentState && LeftIndicatorOn);
            SetActive(rightGlow, useCurrentState && RightIndicatorOn);
            SetActive(headlamps, HeadlightsOn);
            SetActive(rearRunningLamps, HeadlightsOn);
            SetActive(headlightGlow, HeadlightsOn);
        }

        private static void SetActive(Renderer[] renderers, bool active)
        {
            if (renderers == null) return;
            foreach (var item in renderers)
                if (item != null) item.enabled = active;
        }

        private static void SetActive(Light[] lights, bool active)
        {
            if (lights == null) return;
            foreach (var item in lights)
                if (item != null) item.enabled = active;
        }
    }
}

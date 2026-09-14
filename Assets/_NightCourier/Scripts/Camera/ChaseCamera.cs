using System.Collections.Generic;
using NightCourier.Vehicle;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NightCourier.CameraSystem
{
    public enum CameraViewMode
    {
        OrbitChase,
        Cockpit,
        Hood,
        FrontLeftWheel
    }

    /// <summary>Four driving views with per-view speed effects and resettable mouse look.</summary>
    [RequireComponent(typeof(Camera))]
    public sealed class ChaseCamera : MonoBehaviour
    {
        private const float DefaultFieldOfView = 62;
        private ArcadeCarController car;
        private CockpitDisplay cockpit;
        private Rigidbody targetBody;
        private Camera lens;
        private CameraViewMode view;
        private float lookYaw;
        private float lookPitch;
        private bool viewChanged;
        private float viewTransition = 1;
        private Vector3 transitionPosition;
        private Quaternion transitionRotation;
        private readonly List<Renderer> cockpitGlazing = new List<Renderer>();

        public CameraViewMode CurrentView => view;
        public bool LookInputEnabled { get; set; } = true;
        public float LookYaw => lookYaw;
        public float LookPitch => lookPitch;
        public float ViewTransition => viewTransition;
        public string CurrentViewName => view switch
        {
            CameraViewMode.Cockpit => "COCKPIT",
            CameraViewMode.Hood => "HOOD",
            CameraViewMode.FrontLeftWheel => "FRONT WHEEL",
            _ => "ORBIT CHASE"
        };

        public void Follow(ArcadeCarController vehicle, CockpitDisplay instruments)
        {
            car = vehicle;
            cockpit = instruments;
            targetBody = car.GetComponent<Rigidbody>();
            lens = GetComponent<Camera>();
            cockpitGlazing.Clear();
            foreach (Renderer renderer in car.GetComponentsInChildren<Renderer>(true))
            {
                string label = renderer.gameObject.name.ToLowerInvariant();
                if (label.Contains("windscreen") || label.Contains("windshield") ||
                    label.Contains("curved cabin glass") || label.Contains("van cargo body"))
                    cockpitGlazing.Add(renderer);
            }
            ResetViewAngle();
            ApplyViewVisibility();
            SetPose(true);
        }

        private void Update()
        {
            if (car == null) return;
            bool changeView = false;
            foreach (var device in InputSystem.devices)
                if (device is Keyboard keyboard)
                    changeView |= keyboard.enabled && keyboard.cKey.wasPressedThisFrame;
            if (changeView)
            {
                view = (CameraViewMode)(((int)view + 1) % 4);
                ResetViewAngle();
                viewChanged = true;
                viewTransition = 0;
                transitionPosition = transform.position;
                transitionRotation = transform.rotation;
                ApplyViewVisibility();
            }

            if (!LookInputEnabled) return;
            Vector2 delta = Vector2.zero;
            foreach (var device in InputSystem.devices)
                if (device is Mouse mouse && mouse.enabled && mouse.leftButton.isPressed)
                    delta += mouse.delta.ReadValue();
            if (delta == Vector2.zero) return;
            delta = Vector2.ClampMagnitude(delta, 80);
            lookYaw = Mathf.Clamp(lookYaw + delta.x * 0.16f, -150, 150);
            lookPitch = Mathf.Clamp(lookPitch - delta.y * 0.12f, -60, 70);
        }

        private void LateUpdate()
        {
            if (car == null) return;
            if (viewChanged) viewTransition = 0;
            viewTransition = Mathf.MoveTowards(viewTransition, 1, Time.unscaledDeltaTime / .52f);
            SetPose(false);
            UpdateFieldOfView(false);
            viewChanged = false;
        }

        private void SetPose(bool immediate)
        {
            Transform target = car.transform;
            if (view == CameraViewMode.OrbitChase)
            {
                Vector3 focus = target.position + Vector3.up * 1.1f;
                Quaternion orbit = Quaternion.Euler(27 + lookPitch, target.eulerAngles.y + lookYaw, 0);
                // Pull-back belongs only to the external chase view.
                float speedEffect = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(15, 150, car.SpeedKph));
                Vector3 position = focus + orbit * (Vector3.back * (9.7f + speedEffect * 1.8f));
                Vector3 boom = position - focus;
                if (Physics.SphereCast(focus, 0.25f, boom.normalized, out var hit, boom.magnitude,
                    Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore) && hit.rigidbody != targetBody)
                    position = focus + boom.normalized * Mathf.Max(0.5f, hit.distance - 0.15f);
                Quaternion rotation = Quaternion.LookRotation(focus - position, Vector3.up);
                if (immediate || Vector3.Distance(transform.position, position) > 25)
                    transform.SetPositionAndRotation(position, rotation);
                else if (viewTransition < 1)
                {
                    float transitionBlend = Mathf.SmoothStep(0, 1, viewTransition);
                    transform.position = Vector3.Lerp(transitionPosition, position, transitionBlend);
                    transform.rotation = Quaternion.Slerp(transitionRotation, rotation, transitionBlend);
                }
                else
                {
                    float blend = 1 - Mathf.Exp(-7 * Time.deltaTime);
                    transform.position = Vector3.Lerp(transform.position, position, blend);
                    transform.rotation = Quaternion.Slerp(transform.rotation, rotation, blend);
                }
                return;
            }

            Vector3 localPosition;
            Vector3 baseEuler;
            bool sport = car.TargetTopSpeedKph > 250;
            bool van = car.TargetTopSpeedKph < 180;
            if (view == CameraViewMode.Cockpit)
            {
                localPosition = sport ? new Vector3(-.36f, .82f, .05f) :
                    van ? new Vector3(-.4f, 1.28f, 1.43f) : new Vector3(-0.43f, 1.2f, -0.05f);
                baseEuler = sport ? new Vector3(.5f, 0, 0) : new Vector3(1.5f, 0, 0);
            }
            else if (view == CameraViewMode.Hood)
            {
                localPosition = sport ? new Vector3(0, .67f, 1.72f) : new Vector3(0, 0.95f, 1.55f);
                baseEuler = new Vector3(2.5f, 0, 0);
            }
            else
            {
                // Slightly behind and outside the wheel: the tire remains in the lower-right
                // edge while the main sightline stays on the road ahead.
                localPosition = sport ? new Vector3(-1.38f, .05f, 1.28f) : new Vector3(-1.42f, 0.12f, 0.75f);
                baseEuler = new Vector3(1, 4, 0);
            }

            Vector3 positionOnCar = target.TransformPoint(localPosition);
            Quaternion rotationOnCar = target.rotation * Quaternion.Euler(
                baseEuler.x + lookPitch, baseEuler.y + lookYaw, 0);
            // Mounted views remain locked after the short C-key transition; acceleration
            // never adds a chase-camera pull-back to these views.
            if (immediate)
                transform.SetPositionAndRotation(positionOnCar, rotationOnCar);
            else if (viewTransition < 1)
            {
                float transitionBlend = Mathf.SmoothStep(0, 1, viewTransition);
                transform.position = Vector3.Lerp(transitionPosition, positionOnCar, transitionBlend);
                transform.rotation = Quaternion.Slerp(transitionRotation, rotationOnCar, transitionBlend);
            }
            else
                transform.SetPositionAndRotation(positionOnCar, rotationOnCar);
        }

        private void UpdateFieldOfView(bool immediate)
        {
            if (lens == null) return;
            // Cockpit uses a mild speed-dependent wide-angle cue without moving the camera.
            float speedEffect = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(20, 180, car.SpeedKph));
            float targetFov = view == CameraViewMode.Cockpit
                ? DefaultFieldOfView + speedEffect * 8
                : DefaultFieldOfView;
            lens.fieldOfView = immediate
                ? targetFov
                : Mathf.Lerp(lens.fieldOfView, targetFov, 1 - Mathf.Exp(-5 * Time.deltaTime));
        }

        private void ResetViewAngle()
        {
            lookYaw = 0;
            lookPitch = 0;
        }

        private void ApplyViewVisibility()
        {
            if (cockpit != null) cockpit.SetVisible(view == CameraViewMode.Cockpit);
            foreach (Renderer glazing in cockpitGlazing)
                if (glazing != null) glazing.enabled = view != CameraViewMode.Cockpit;
        }
    }
}

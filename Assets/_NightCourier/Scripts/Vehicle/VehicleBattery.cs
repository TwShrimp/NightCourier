using UnityEngine;

namespace NightCourier.Vehicle
{
    /// <summary>Distance-based battery use with automatic charging while parked at an EV bay.</summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class VehicleBattery : MonoBehaviour
    {
        private const float ConsumptionPerMetre = 0.00018f;
        private const float ChargePerSecond = 0.16f;
        private Rigidbody body;
        private Vector3 previousPosition;

        public float Charge01 { get; private set; } = 0.5f;
        public bool IsCharging { get; private set; }
        public float DrivePowerFactor => Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0, 0.025f, Charge01));

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            previousPosition = transform.position;
        }

        private void FixedUpdate()
        {
            float distance = Vector3.Distance(transform.position, previousPosition);
            previousPosition = transform.position;
            // Large jumps are recovery/teleport operations and must not consume a battery pack.
            if (distance < 10f && body.linearVelocity.magnitude > 0.5f && !IsCharging)
                Charge01 = Mathf.Max(0, Charge01 - distance * ConsumptionPerMetre);
        }

        private void Update()
        {
            IsCharging = false;
            if (body.linearVelocity.magnitude * 3.6f > 5f) return;
            foreach (var station in EVChargingStation.Stations)
            {
                if (station == null || Vector2.Distance(new Vector2(transform.position.x, transform.position.z),
                    new Vector2(station.transform.position.x, station.transform.position.z)) > station.ChargingRadius)
                    continue;
                IsCharging = true;
                Charge01 = Mathf.Min(1, Charge01 + ChargePerSecond * Time.deltaTime);
                break;
            }
        }
    }
}

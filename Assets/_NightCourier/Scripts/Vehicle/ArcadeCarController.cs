using UnityEngine;

namespace NightCourier.Vehicle
{
    /// <summary>AWD electric pickup: wheel forces own movement; Rigidbody retains collision impulses.</summary>
    [RequireComponent(typeof(Rigidbody), typeof(CarInput))]
    public sealed class ArcadeCarController : MonoBehaviour
    {
        [SerializeField, Min(1)] private float serviceBrakeTorque = 3500;
        private Rigidbody body;
        private CarInput input;
        private Vector3 spawnPosition;
        private Quaternion spawnRotation;
        private WheelCollider[] wheels;
        private Transform[] wheelVisuals;
        private PhysicsMaterial chassisMaterial;
        private VehicleBattery battery;
        private VehiclePerformanceProfile performance;
        private readonly TwoSpeedTransmission transmission = new TwoSpeedTransmission();
        public bool HasTwoSpeedTransmission => performance.TwoSpeed;
        public int CurrentGear => transmission.Gear;
        public bool IsShifting => performance.TwoSpeed && transmission.IsShifting;
        public float ShiftTorqueFactor => performance.TwoSpeed ? transmission.TorqueFactor : 1;
        public float MotorRpm => transmission.MotorRpm(ForwardSpeed, performance.WheelRadius);
        public string DriveLabel => IsReversing ? "R" : performance.TwoSpeed ? "D" + CurrentGear : "D";
        private float steering;
        private float lastObstacleImpact = -10;
        public bool IsGrounded { get; private set; }
        public bool IsBraking => input != null && (input.Brake || input.Throttle * ForwardSpeed < -0.5f);
        public bool IsReversing => ForwardSpeed < -0.5f;
        public float SpeedKph => body == null ? 0 : body.linearVelocity.magnitude * 3.6f;
        public float SteeringAngle => steering;
        public string PerformanceName => performance.Name;
        public float TargetTopSpeedKph => performance.TargetTopSpeedKph;
        private float ForwardSpeed => body == null ? 0 : Vector3.Dot(body.linearVelocity, transform.forward);

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            input = GetComponent<CarInput>();
            battery = GetComponent<VehicleBattery>();
            performance = VehiclePerformanceProfiles.Pickup;
            body.mass = performance.Mass;
            body.centerOfMass = performance.CenterOfMass;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.solverIterations = 12;
            body.solverVelocityIterations = 4;
            body.constraints = RigidbodyConstraints.None;
            body.maxAngularVelocity = 4;
            chassisMaterial = new PhysicsMaterial("Low bounce pickup body")
            {
                staticFriction = 0.1f, dynamicFriction = 0.1f,
                frictionCombine = PhysicsMaterialCombine.Minimum,
                bounciness = 0, bounceCombine = PhysicsMaterialCombine.Minimum
            };
            foreach (var collider in GetComponents<Collider>()) collider.sharedMaterial = chassisMaterial;
            // Procedural construction moves the Transform before physics has synced.
            spawnPosition = transform.position;
            spawnRotation = transform.rotation;
        }

        public void ConfigureWheels(WheelCollider[] colliders, Transform[] visuals)
        {
            wheels = colliders;
            wheelVisuals = visuals;
            foreach (var wheel in wheels)
            {
                wheel.mass = 28;
                wheel.forceAppPointDistance = 0.18f;
                wheel.wheelDampingRate = 0.5f;
                wheel.suspensionSpring = new JointSpring { spring = 48000, damper = 6500, targetPosition = 0.5f };
                var forward = wheel.forwardFriction;
                forward.extremumSlip = 0.35f;
                forward.extremumValue = 1;
                forward.asymptoteSlip = 0.8f;
                forward.asymptoteValue = 0.8f;
                forward.stiffness = performance.ForwardGrip;
                wheel.forwardFriction = forward;
                var sideways = wheel.sidewaysFriction;
                sideways.extremumSlip = 0.24f;
                sideways.extremumValue = 1;
                sideways.asymptoteSlip = 0.68f;
                sideways.asymptoteValue = 0.95f;
                sideways.stiffness = performance.SideGrip;
                wheel.sidewaysFriction = sideways;
            }
            ApplyWheelLayout(performance);
            wheels[0].ConfigureVehicleSubsteps(10, 8, 12);
            body.ResetInertiaTensor();
        }

        public void ApplyPerformanceProfile(VehiclePerformanceProfile profile)
        {
            performance = profile;
            transmission.Reset();
            body.mass = profile.Mass;
            body.centerOfMass = profile.CenterOfMass;
            if (wheels != null)
            {
                ApplyWheelLayout(profile);
                foreach (WheelCollider wheel in wheels)
                {
                    WheelFrictionCurve forward = wheel.forwardFriction;
                    forward.stiffness = profile.ForwardGrip;
                    wheel.forwardFriction = forward;
                    WheelFrictionCurve sideways = wheel.sidewaysFriction;
                    sideways.stiffness = profile.SideGrip;
                    wheel.sidewaysFriction = sideways;
                }
            }
            body.ResetInertiaTensor();
        }

        private void ApplyWheelLayout(VehiclePerformanceProfile profile)
        {
            for (int i = 0; i < wheels.Length; i++)
            {
                int side = i % 2 == 0 ? -1 : 1;
                wheels[i].transform.localPosition = new Vector3(side * profile.TrackHalfWidth,
                    profile.WheelAnchorY, i < 2 ? profile.FrontAxleZ : profile.RearAxleZ);
                wheels[i].radius = profile.WheelRadius;
                wheels[i].suspensionDistance = profile.SuspensionDistance;
            }
        }

        private void FixedUpdate()
        {
            if (wheels == null) return;
            if (input.ConsumeReset() || body.position.y < -8)
            {
                body.position = spawnPosition;
                body.rotation = spawnRotation;
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                steering = 0;
                transmission.Reset();
                foreach (var wheel in wheels) { wheel.motorTorque = 0; wheel.brakeTorque = serviceBrakeTorque; wheel.steerAngle = 0; }
                return;
            }
            float forwardSpeed = Vector3.Dot(body.linearVelocity, transform.forward);
            float speed = body.linearVelocity.magnitude;
            if (performance.TwoSpeed) transmission.Step(forwardSpeed * 3.6f, Time.fixedDeltaTime);
            float steeringTarget = input.Steering * ElectricDriveModel.SteeringLimit(speed, performance);
            float steeringRate = ElectricDriveModel.SteeringResponse(speed, performance);
            // Returning the wheel may be slightly quicker than turning it, while
            // both directions remain filtered for binary keyboard input.
            if (Mathf.Abs(steeringTarget) < Mathf.Abs(steering)) steeringRate *= 1.2f;
            steering = Mathf.MoveTowards(steering, steeringTarget, steeringRate * Time.fixedDeltaTime);
            bool opposite = input.Throttle * forwardSpeed < -0.5f;
            bool parking = Mathf.Abs(input.Throttle) < 0.01f && speed < 0.4f;
            bool braking = input.Brake || opposite || parking;
            float availablePower = battery == null ? 1 : battery.DrivePowerFactor;
            float force = braking ? 0 : input.Throttle * ElectricDriveModel.DriveForce(speed, performance, CurrentGear)
                * availablePower * ShiftTorqueFactor;
            if (input.Throttle < 0) force *= Mathf.Clamp01(1 - Mathf.Pow(Mathf.Abs(forwardSpeed) / 9, 2));
            IsGrounded = false;
            int groundedWheels = 0;
            int pavementWheels = 0;
            for (int i = 0; i < wheels.Length; i++)
            {
                var wheel = wheels[i];
                wheel.steerAngle = i < 2 ? steering : 0;
                wheel.motorTorque = force * wheel.radius / wheels.Length;
                wheel.brakeTorque = braking ? serviceBrakeTorque : Mathf.Abs(input.Throttle) < 0.01f ? 110 : 0;
                IsGrounded |= wheel.isGrounded;
                if (wheel.GetGroundHit(out var hit))
                {
                    groundedWheels++;
                    if (hit.collider.name == "Raised sidewalk") pavementWheels++;
                }
            }
            if (!IsGrounded) return;
            if (speed > 0.1f) body.AddForce(-body.linearVelocity.normalized * ElectricDriveModel.Resistance(speed, performance));
            // Surface resistance must never use brakeTorque: PhysX can keep a
            // stationary wheel locked even while motor torque is being applied.
            if (speed > 0.5f && pavementWheels > 0)
                body.AddForce(-body.linearVelocity.normalized * (850f * pavementWheels / wheels.Length));
            // Gentle arcade stability assistance supplements tire grip without
            // overwriting velocity or cancelling recent obstacle collision impulses.
            if (groundedWheels >= 3 && Time.time - lastObstacleImpact > 0.3f)
            {
                float lateralSpeed = Vector3.Dot(body.linearVelocity, transform.right);
                float correction = Mathf.Clamp(-lateralSpeed * performance.Stability, -8f, 8f);
                body.AddForce(transform.right * correction, ForceMode.Acceleration);
            }
            body.AddForce(-transform.up * Mathf.Min(speed * speed * 2, 6000));
            AntiRoll(wheels[0], wheels[1]);
            AntiRoll(wheels[2], wheels[3]);
        }

        private void AntiRoll(WheelCollider left, WheelCollider right)
        {
            bool onLeft = left.GetGroundHit(out var leftHit);
            bool onRight = right.GetGroundHit(out var rightHit);
            float leftTravel = onLeft ? (-left.transform.InverseTransformPoint(leftHit.point).y - left.radius) / left.suspensionDistance : 1;
            float rightTravel = onRight ? (-right.transform.InverseTransformPoint(rightHit.point).y - right.radius) / right.suspensionDistance : 1;
            float force = (leftTravel - rightTravel) * 5500;
            if (onLeft) body.AddForceAtPosition(left.transform.up * -force, left.transform.position);
            if (onRight) body.AddForceAtPosition(right.transform.up * force, right.transform.position);
        }

        private void LateUpdate()
        {
            if (wheels == null) return;
            for (int i = 0; i < wheels.Length; i++)
            {
                wheels[i].GetWorldPose(out var position, out var rotation);
                wheelVisuals[i].SetPositionAndRotation(position, rotation);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            for (int i = 0; i < collision.contactCount; i++)
                if (Mathf.Abs(collision.GetContact(i).normal.y) < 0.5f)
                    lastObstacleImpact = Time.time;
        }

        private void OnDestroy()
        {
            if (chassisMaterial != null) Destroy(chassisMaterial);
        }
    }
}

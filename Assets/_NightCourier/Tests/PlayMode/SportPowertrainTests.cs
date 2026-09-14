using NightCourier.Vehicle;
using NUnit.Framework;
using UnityEngine;

namespace NightCourier.Tests
{
    public sealed class SportPowertrainTests
    {
        [Test]
        public void ShiftDropsMotorSpeedWithoutGearHunting()
        {
            var gearbox = new TwoSpeedTransmission();
            gearbox.Step(109, .02f);
            float before = gearbox.MotorRpm(110 / 3.6f, .39f);
            gearbox.Step(110, .02f);
            Assert.That(gearbox.Gear, Is.EqualTo(2));
            Assert.That(gearbox.MotorRpm(110 / 3.6f, .39f), Is.LessThan(before * .6f));
            gearbox.Step(110, .14f);
            Assert.That(gearbox.TorqueFactor, Is.InRange(.35f, .45f));
            gearbox.Step(100, .2f);
            Assert.That(gearbox.IsShifting, Is.False);
            Assert.That(gearbox.TorqueFactor, Is.EqualTo(1));
            foreach(float speed in new[]{109f,100f,80f,76f}) gearbox.Step(speed,.1f);
            Assert.That(gearbox.Gear, Is.EqualTo(2));
            gearbox.Step(74,.02f);
            Assert.That(gearbox.Gear, Is.EqualTo(1));
            gearbox.Step(-10,.02f);
            Assert.That(gearbox.IsShifting, Is.False);
        }

        [Test]
        public void SportAcceleratesThroughShiftAndSettlesAt320()
        {
            var profile=VehiclePerformanceProfiles.Sport;
            var gearbox=new TwoSpeedTransmission();
            float speed=0, atTen=0;
            int shifts=0, previous=1;
            for(int i=0;i<6000;i++)
            {
                gearbox.Step(speed*3.6f,.02f);
                if(gearbox.Gear!=previous) shifts++;
                previous=gearbox.Gear;
                float force=ElectricDriveModel.DriveForce(speed,profile,gearbox.Gear)*gearbox.TorqueFactor;
                speed=Mathf.Max(0,speed+(force-ElectricDriveModel.Resistance(speed,profile))/profile.Mass*.02f);
                if(i==500) atTen=speed*3.6f;
            }
            Assert.That(shifts, Is.EqualTo(1));
            Assert.That(atTen, Is.GreaterThan(160));
            Assert.That(speed*3.6f, Is.InRange(319f,320.1f));
            Assert.That(ElectricDriveModel.DriveForce(90,profile),Is.LessThan(ElectricDriveModel.Resistance(90,profile)));
            Assert.That(VehiclePerformanceProfiles.Pickup.TwoSpeed,Is.False);
            Assert.That(VehiclePerformanceProfiles.Van.TwoSpeed,Is.False);
            Debug.Log($"[Sport model] 10s {atTen:F1} km/h; terminal {speed*3.6f:F2} km/h; {shifts} shift");
        }

        [Test]
        public void SportSoundIsFiniteBoundedAndLoopsCleanly()
        {
            AudioClip clip=NightCourier.Prototype.Vehicles.Sport.SportMotorSound.CreateLoop();
            try
            {
                var samples=new float[clip.samples];clip.GetData(samples,0);
                float energy=0;
                foreach(float s in samples)
                {
                    Assert.That(float.IsNaN(s)||float.IsInfinity(s),Is.False);
                    Assert.That(Mathf.Abs(s),Is.LessThan(1));
                    energy+=s*s;
                }
                Assert.That(Mathf.Sqrt(energy/samples.Length),Is.InRange(.1f,.5f));
                Assert.That(Mathf.Abs(samples[0]-samples[samples.Length-1]),Is.LessThan(.02f));
            }
            finally { Object.DestroyImmediate(clip); }
        }
    }
}

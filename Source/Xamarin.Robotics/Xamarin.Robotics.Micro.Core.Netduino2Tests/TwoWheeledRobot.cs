using Microsoft.SPOT;
using SecretLabs.NETMF.Hardware.Netduino;
using System;
using System.Threading;
using Xamarin.Robotics.Micro.Devices;
using Xamarin.Robotics.Micro.Generators;
using Xamarin.Robotics.Micro.Motors;
using Xamarin.Robotics.Micro.Sensors.Proximity;
using Xamarin.Robotics.Micro.SpecializedBlocks;

namespace Xamarin.Robotics.Micro.Core.Netduino2Tests
{
    public class TwoWheeledRobot : BlockBase
    {
        IDCMotor leftMotor;
        IDCMotor rightMotor;

        /// <summary>
        /// Speed from 0 to 1.
        /// </summary>
        public InputPort SpeedInput { get; private set; }

        /// <summary>
        /// Direction to travel. 0 = straight ahead. -1 if full left turn, +1 is full right turn.
        /// </summary>
        public InputPort DirectionInput { get; private set; }

        /// <summary>
        /// Direction to spin. 0 = no spinning. -1 if full left spinning, +1 is full right spinning.
        /// Spinning is an exclusive action of driving with Speed and Direction
        /// </summary>
        public InputPort SpinInput { get; private set; }

        /// <summary>
        /// Not all motors are created equal. Change this value to slow/speed up the left motor.
        /// </summary>
        public InputPort LeftMotorCalibrationInput { get; private set; }

        /// <summary>
        /// Not all motors are created equal. Change this value to slow/speed up the right motor.
        /// </summary>
        public InputPort RightMotorCalibrationInput { get; private set; }

        public TwoWheeledRobot (IDCMotor leftMotor, IDCMotor rightMotor)
        {
            this.leftMotor = leftMotor;
            this.rightMotor = rightMotor;

            DirectionInput = AddInput ("DirectionInput", Units.Scalar);
            SpeedInput = AddInput ("SpeedInput", Units.Ratio);
            SpinInput = AddInput ("SpinInput", Units.Scalar);

            Update ();

            SpeedInput.ValueChanged += (s, e) => Update ();
            DirectionInput.ValueChanged += (s, e) => Update ();
            SpinInput.ValueChanged += (s, e) => Update ();
        }

        void Update ()
        {
            var spinning = System.Math.Abs (SpinInput.Value) > 0.01;
            if (spinning) {

                var rate = System.Math.Min (System.Math.Abs (SpinInput.Value), 1);

                if (SpinInput.Value < 0) {
                    leftMotor.SpeedInput.Value = -rate;
                    rightMotor.SpeedInput.Value = rate;
                }
                else {
                    leftMotor.SpeedInput.Value = rate;
                    rightMotor.SpeedInput.Value = -rate;
                }

            }
            else {
                // Debug.Print ("Robot Direction = " + DirectionInput.Value);

                // The motors always move forwards
                var dir = System.Math.Max (System.Math.Min (DirectionInput.Value, 1), -1);
                var spd = System.Math.Max (System.Math.Min (SpeedInput.Value, 1), 0);

                if (System.Math.Abs (dir) < 0.01) {
                    leftMotor.SpeedInput.Value = spd;
                    rightMotor.SpeedInput.Value = spd;
                    return;
                }

                // We turn by slowing one wheel down in proportion
                // to the steepness of the turn
                if (dir < 0) {
                    // To turn left, favor the right wheel
                    leftMotor.SpeedInput.Value = spd * (1 + dir) * LeftMotorCalibrationInput.Value;
                    rightMotor.SpeedInput.Value = spd * RightMotorCalibrationInput.Value;
                }
                else {
                    // To turn right, favor the left wheel
                    leftMotor.SpeedInput.Value = spd * LeftMotorCalibrationInput.Value;
                    rightMotor.SpeedInput.Value = spd * (1 - dir) * RightMotorCalibrationInput.Value;
                }
            }
        }
    }
}

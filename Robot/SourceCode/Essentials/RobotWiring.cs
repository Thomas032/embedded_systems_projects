using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Robot.Essentials
{
    public static class RobotWiring
    {
        public class Out
        {
            public const int PORT = 1;
            // motor signals
            public const byte BASE = 0x04; // the base of the robot
            public const byte ARM = 0x08; // the first moveable arm
            public const byte NECK = 0x10; // the second moveable arm
            public const byte GRIP = 0x20; // the gripper

            // control signals
            public static byte TACT = 0x01; // signal for sending clock signals
            public static byte DIR = 0x02; // signal for setting the direction
        }

        public static class In
        {
            public const int PORT = 0;
            // each of the inputs relates to one IR sensor on the robot
            public const byte BASE_IR = 0x01;
            public const byte ARM_IR = 0x02;
            public const byte NECK_IR = 0x04;
            public const byte GRIP_IR = 0x08;
        }

        public class General
        {
            public const int FREQ = 450; // max frequency = 450Hz
            public const long TOT_DLEAY = 1_000_000 / FREQ;// total delay between steps in micro seconds (T = (1/f))
            public const long HALF_DELAY = TOT_DLEAY / 2; // delay between each phase of a step (2 phases -> from 1 to 0)
            public const int BASE_CROSS_STEPS = 1_500; // the number of steps needed to cross the IR gate
            public const int STRADA_STEP = 1; // slow robot move
            public const int SPORT_STEP = 10; // faster robot move
            public const int CORSA_STEP = 25; // super-fast robot
        }
        public static int GetPinID(byte pinAddress)
        {
            // function for converting bit address to a single int (ID of the pin on the port)
            return (int)Math.Log(pinAddress, 2);
        }
    }
}

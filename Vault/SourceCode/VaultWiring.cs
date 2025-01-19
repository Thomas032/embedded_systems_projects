using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trezor.Essentials
{
    public static class VaultWiring
    {
        public class Out
        {
            public class PORT1
            {
                public const int PORT = 0;
                public const int PROM_0 = 0x01;
                public const int PROM_1 = 0x02;
                public const int PROM_2 = 0x04;
                public const int PROM_3 = 0x08;
                public const int PROM_4 = 0x10;

                public const int UNLOCK_PIN = 0x20;
                public const int MOTOR_PIN = 0x40;
                public const int LIGHT_PIN = 0x80;
            }
            public class PORT2
            {
                public const int PORT = 1;

                public const int SOUND_PIN = 0x01;

                public const int MULTIPLEX_0 = 0x02; // 0b0000 0010
                public const int MULTIPLEX_1 = 0x04; // 0b0000 0100
                public const int MULTIPLEX_2 = 0x08; // 0b0000 1000
                public const int MULTIPLEX_3 = 0x10; // 0b0001 0000
            }
        }

        public static class In
        {
            public const int PORT = 0;

            public const int KEYBOARD_PIN = 0x01;
            public const int DOOR_PIN = 0x02;
        }

        public class General
        {
            public const bool INVERSE_LOGIC = true;
            public const int LIGHT_FREQ = 50000; // max PWM frequency

            public const long LIGHT_MODULO = 1_000_000 / LIGHT_FREQ;
            public const int DEFAULT_CODE = 1234; // default vault code
            public const int MAX_ERROR = 3; // max error count
            public const string CODE_FILE = "code.txt"; // file with the code
        }

        /// <summary>
        /// A method to get the pin index on a port from its byte address.
        /// </summary>
        public static int GetPinID(byte pinAddress)
        {
            return (int)Math.Log(pinAddress, 2);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Trezor.Essentials
{
    public partial class VaultModel
    {
        public BitOperations bitOperations { get; set; }
        public  PreciseDelay preciseDelay { get; set; }
        public Board board { get; set; }
        public System.Timers.Timer doorOpenTimer { get; set; }
        public System.Timers.Timer lightTimer { get; set; }

        public System.Timers.Timer readingTimer { get; set; }

        public Thread portThread { get; set; }

        public Dictionary<int, int> holdCounterMap = new Dictionary<int, int>()
        {
            {0, 0},
            {1, 0},
            {2, 0},
            {3, 0}
        };

        public Dictionary<int, char> displayData = new Dictionary<int, char>()
        {
            {0, '0'},
            {1, '0'},
            {2, '0'},
            {3, '0'}
        };

        int selectedDial = 0;

        int errorCounter = 0;

        bool editorMode = false;
        public bool canStart = false;

        public bool lastLight = false;

        public int holdNorm = 10;
        public byte port1 = 0xFF;
        public byte port2 = 0xFF;

        public Dictionary<char, byte> map = new Dictionary<char, byte>(){
            {'0', 0x00},
            {'1', 0x01},
            {'2', 0x02},
            {'3', 0x03},
            {'4', 0x04},
            {'5', 0x05},
            {'6', 0x06},
            {'7', 0x07},
            {'8', 0x08},
            {'9', 0x09},
            {'E',0x0E},
            {'R', 0x17 },
            {' ', 0x1D },
            {'-', 0x1E }
        };

        /// <summary>
        /// Constructor of the VaultModel object.
        /// </summary>
        public VaultModel()
        {
            // iniciatlie the global objects
            bitOperations = new BitOperations();
            preciseDelay = new PreciseDelay();
            board = new Board();

            doorOpenTimer = new System.Timers.Timer();
            doorOpenTimer.Interval = 60_000;
            doorOpenTimer.Elapsed += (sender, e) => Beep(300);

            readingTimer = new System.Timers.Timer();
            readingTimer.Interval = 100;
            readingTimer.Elapsed += CheckSensors;
            readingTimer.AutoReset = true;
            readingTimer.Start();

            lightTimer = new System.Timers.Timer();
            readingTimer.Interval = 1;
            readingTimer.Elapsed += SwitchLight;
            readingTimer.AutoReset = true;

        
            // initiate the port thread
            portThread = new Thread(PortThreadInterrupt);
            portThread.Priority = ThreadPriority.Highest;
            portThread.Start();
        }

        /// <summary>
        /// Interrupt handler for the light timer. Its main function is to switch the light on and off.
        /// </summary>
        private void SwitchLight(object? sender, ElapsedEventArgs e)
        {
            lastLight = !lastLight;
            port1 = bitOperations.SetBit(port1, VaultWiring.GetPinID(VaultWiring.Out.PORT1.LIGHT_PIN), lastLight);
        }

        /// <summary>
        /// Method for resetting the ports to their default state.
        /// </summary>
        public void ResetPorts()
        {
            board.SetByte(VaultWiring.Out.PORT1.PORT, 0xFF);
            board.SetByte(VaultWiring.Out.PORT2.PORT, 0xFF);
        }

        /// <summary>
        /// Timer interrupt handler for checking the door sensor status every N seconds.
        /// </summary>
        private void CheckSensors(object? sender, ElapsedEventArgs e)
        {
            if (board.GetBit(VaultWiring.In.PORT, VaultWiring.GetPinID(VaultWiring.In.DOOR_PIN)))
            {
                // door is opened -> light full brightness
                // ideally I will have a pwm thread which will be set to full brightness
                editorMode = true;

                Console.WriteLine("Doors opened");

                if (lightTimer.Enabled)
                {
                    lightTimer.Enabled = false;
                }
                // set the pin high -> light full brightness
                port1 = bitOperations.SetBit(port1, VaultWiring.GetPinID(VaultWiring.Out.PORT1.LIGHT_PIN), false);
            }
            else
            {
                // door is closed
                editorMode = false;

                if(lightTimer.Enabled == false)
                {
                    // PERFORM PWM
                    Console.WriteLine("Starting PWM");
                    lightTimer.Enabled = true;
                }
                //port1 = bitOperations.SetBit(port1, VaultWiring.GetPinID(VaultWiring.Out.PORT1.LIGHT_PIN), true);

            }
        }

        /// <summary>
        /// A method for displaying the code on the vault display by multiplexing the 7-segment displays.
        /// </summary>
        public void DisplayCode()
        {
            // method for displaying the code on the vault
            for (int i = 0; i < displayData.Count; i++)
            {
                // get the current character
                char character = displayData[i];
                byte value = map[character];

                // set PROM data
                
                port1 = (byte)((port1 & 0b11100000) | (0x1F & value));

                port2 = bitOperations.MergeBytes(port2, (byte)(0xFF ^ (VaultWiring.Out.PORT2.MULTIPLEX_0 << i)));

                preciseDelay.DelayMicroseconds(5000);

                // check button
                CheckButtton(i);

                // unset the port2 state to hight
                port2 = bitOperations.SetBit(port2, i+1);
                preciseDelay.DelayMicroseconds(50);
            }
        }

        /// <summary>
        /// A method for checking the button status and performing the corresponding action.
        /// </summary>
        public void CheckButtton(int btnIndex)
        {
            if(board.GetBit(VaultWiring.In.PORT, VaultWiring.GetPinID(VaultWiring.In.KEYBOARD_PIN)))
            {
                // log. 1 on port -> nothing to perform -> return
                return;
            }

            holdCounterMap[btnIndex]++;

            if(btnIndex == 0 && holdCounterMap[btnIndex] % holdNorm == 0)
            {
                // mode button is pressed -> check the code
                Console.WriteLine("MODE");

                if(editorMode)
                {
                    // if the door is opened -> change the code
                    // write the code to the file
                    string code = new string(displayData.Values.ToArray());
                    File.WriteAllText(VaultWiring.General.CODE_FILE, code);
                    Console.WriteLine("Code changed to: " + code);
                    Beep(200);
                }
                else
                {
                    // if the door is closed -> check the code
                    if (validCode())
                    {
                        // open the door
                        unlockDoors();
                    }
                    else
                    {
                        errorCounter++;
                        if (errorCounter > VaultWiring.General.MAX_ERROR - 1)
                        {
                            errorCounter = 0;
                            Beep(1500);
                        }
                    }
                }

                holdCounterMap[btnIndex] = 0;
            }
            if (btnIndex == 1 && holdCounterMap[btnIndex] % holdNorm == 0)
            {
                // button up is pressed -> increment the current value
                int newValue = Int32.Parse(displayData[selectedDial].ToString()) + 1;
                if(newValue > 9)
                {
                    newValue = 0;
                }
                displayData[selectedDial] = Char.Parse(newValue.ToString());
                Console.WriteLine("UP");

                holdCounterMap[btnIndex] = 0;
            }
            if (btnIndex == 2 && holdCounterMap[btnIndex] % holdNorm == 0)
            {
                // button down is pressed -> decrement the current value
                int newValue = Int32.Parse(displayData[selectedDial].ToString()) - 1;
                if (newValue < 0)
                {
                    newValue = 9;
                }
                displayData[selectedDial] = Char.Parse(newValue.ToString());

                Console.WriteLine("DOWN");
                holdCounterMap[btnIndex] = 0;
            }
            if(btnIndex == 3 && holdCounterMap[btnIndex] % holdNorm == 0)
            {
                // set button is pressed -> select the next dial and beep
                Console.WriteLine("SET");
                selectedDial++;
                if(selectedDial > displayData.Count - 1)
                {
                    selectedDial = 0;
                }
                holdCounterMap[btnIndex] = 0;

                Beep(100);
            }
        }
        /// <summary>
        /// A mtethod for unlocking the doors and starting the door open timer to signalize the doors being opened for too long.
        /// </summary>
        private void unlockDoors()
        {
            port1 = bitOperations.SetBit(port1, VaultWiring.GetPinID(0x20));
            preciseDelay.DelayMicroseconds(100000);
            port1 = bitOperations.SetBit(port1, VaultWiring.GetPinID(0x20), true);
        }

        /// <summary>
        /// A method for checking the current code with the code stored in the code.txt file.
        /// </summary>
        private bool validCode()
        {
            int code;
            // read code.txt file and compare the value with the current code
            try
            {
                string[] lines = File.ReadAllLines(VaultWiring.General.CODE_FILE);
                code = Int32.Parse(lines[0]);
            }
            catch
            {
                // if file doesa not exist
                File.WriteAllText(VaultWiring.General.CODE_FILE, VaultWiring.General.DEFAULT_CODE.ToString());
                code = VaultWiring.General.DEFAULT_CODE;
            }

            int currentCode = Int32.Parse(new string(displayData.Values.ToArray()));
            return code == currentCode;
        }

        /// <summary>
        /// A method for beeping the sound for a given interval.
        /// </summary>
        public void Beep(int interval)
        {
            // set beep port high wait and set beep low and wait
            for(int i=0; i < interval; i++)
            {
                port2 = bitOperations.SetBit(port2, VaultWiring.GetPinID(VaultWiring.Out.PORT2.SOUND_PIN), true);
                preciseDelay.DelayMicroseconds(1000);
                port2 = bitOperations.SetBit(port2, VaultWiring.GetPinID(VaultWiring.Out.PORT2.SOUND_PIN));
                preciseDelay.DelayMicroseconds(250);
            }
        }

        /// <summary>
        /// A method for asigning the current state of the ports to the HW ports.
        /// Helps with the access management to physical ports.
        /// </summary>
        public void PortThreadInterrupt()
        {
            Console.WriteLine("Thread interrupt started");

            // method solely for writing the curent state of the ports to the HW ports
            while (true)
            {
                // assign the current state of the ports to the HW port
                board.SetByte(VaultWiring.Out.PORT1.PORT, port1);
                board.SetByte(VaultWiring.Out.PORT2.PORT, port2);
            }
        }
    }
}

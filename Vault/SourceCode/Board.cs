using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Automation.BDaq;
using Trezor.Essentials;

namespace Trezor.Essentials
{
    public partial class Board
    {
        public DeviceInformation dev { get; set; }
        public InstantDoCtrl DO { get; set; }
        public InstantDiCtrl DI { get; set; }

        public BitOperations bitOperations { get; set; }

        public Board()
        {
            InitObjects();
            PortSetup();
        }

        /// <summary>
        /// Method for initializing the device related objects
        /// </summary>
        public void InitObjects()
        {
            // basic device information
            dev = new DeviceInformation
            {
                Description = "PCIE-1730,BID#0",
                DeviceMode = AccessMode.ModeWrite
            };

            // Digitial output information
            DO = new InstantDoCtrl();
            DO.SelectedDevice = dev;
            DO.LoadProfile("CardProfile.xml");

            // Digital input information
            DI = new InstantDiCtrl();
            DI.SelectedDevice = dev;
            DI.LoadProfile("CardProfile.xml");

            // init of external objects
            bitOperations = new BitOperations();
        }

        /// <summary>
        /// Init method for setting up the ouput ports
        /// </summary>
        public void PortSetup()
        {
            if(VaultWiring.General.INVERSE_LOGIC)
            {
                // set the default state of the ports to 0xFF
                SetByte(0, 0xFF);
                SetByte(1, 0xFF);
            }
            else
            {
                // set the default state of the ports to 0x00
                SetByte(0, 0x00);
                SetByte(1, 0x00);
            }
        }

        /// <summary>
        /// Method for setting a byte to a desired port
        /// </summary>
        public void SetByte(int port, byte data)
        {
            // fucntion for setting a byte to a desired port 
            DO.Write(port, data);
        }

        /// <summary>
        /// Method that sets a bit on a given port to a log. 1
        /// </summary>
        public void SetBit(int port, int pinAddr)
        {
            // get the port stratus
            byte status = GetPortState(port);

            // get the bite index
            int index = VaultWiring.GetPinID((byte)pinAddr);

            // edit the desired bit
            byte data;
            if(VaultWiring.General.INVERSE_LOGIC)
            {
                // bring the particicuar bit down
                data = bitOperations.SetBit(status, index, false);
            }
            else
            {
                // bring the particular bit low
                data = bitOperations.SetBit(status, index, true);
            }
            SetByte(port, data);
        }

        /// <summary>
        /// Method for returning the current state of the bit on a given port
        /// </summary>
        /// <returns>
        /// A boolean value representing the state of the bit
        /// </returns>
        public bool GetBit(int port, int bitAddress)
        {
            byte data;
            DI.Read(port, out data);

            if (bitOperations.IsHigh(data, bitAddress))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Method for returning the current state of the port
        /// </summary>
        /// <returns>
        /// A byte representation of the port state
        /// </returns>
        public byte GetPortState(int port)
        {
            byte state;
            DO.Read(port, out state);
            return state;
        }

        /// <summary>
        /// Method for showing an error message
        /// </summary>
        public void ShowError(string error)
        {
            Console.WriteLine(error);
        }
    }
}

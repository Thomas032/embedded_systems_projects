using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Automation.BDaq;

namespace Robot.Essentials
{
    internal class Board
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

        public void InitObjects()
        {
            // initialize board related objects

            // basic device information
            dev = new DeviceInformation
            {
                Description = "PCI-1756,BID#0",
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

        public void PortSetup()
        {
            // Set log. 1 to all of the output ports as the model is active in log. 0
            SetByte(0, 0xFF);
            SetByte(1, 0xFF);
        }

        public void SetByte(int port, byte data)
        {
            // fucntion for setting a byte to a desired port 
            DO.Write(port, data);
        }

        public bool GetBit(int port, int bitAddress)
        {
            // function that checks if a bit with a certain address is high or low
            byte data;
            DI.Read(port, out data);

            if(bitOperations.IsHigh(data, bitAddress))
            {
                return true;
            }

            return false;
        }

        public byte GetPortState(int port)
        {
            // function that returns the current state of a port
            byte state;
            DO.Read(port, out state);
            return state;
        }

    }
}

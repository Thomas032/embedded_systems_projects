using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HidLibrary;
using static System.Windows.Forms.AxHost;


namespace Robot.Essentials
{
    public struct JoystickState
    {
        public double accelX, accelY;
    }
    public class Controller
    {
        public bool buttonOne, buttonTwo, buttonThree, buttonFour;
        public bool buttonLeft, buttonRight, buttonTop, buttonBottom;
        public bool buttonFrontL, buttonFrontR;
        public JoystickState joystickLeft, joystickRight;
        public int vendorID;
        public double zeroOffset;
        public bool connected;
        public HidDevice controller;

        public Controller(int userVedorID = 0x2563, double userZeroOffset = 0.015)
        {
            vendorID = userVedorID;
            zeroOffset = userZeroOffset;
        }

        public bool ConnectController()
        {
            // function for establishing connection with the gamepad
            if(controller != null)
            {
                // already connected
                return true;
            }

            controller = HidDevices.Enumerate(vendorID).FirstOrDefault(); // get controller instance or null if not connected

            if(controller != null)
            {
                controller.OpenDevice(); // open HID interface for communication
                controller.MonitorDeviceEvents = true; // monitor the actions of the controller
                controller.Removed += ControllerDisconnected;
                controller.ReadReport(OnControllerReport);
                controller.ReadAsync();// enable reading asynchonously
                connected = true;

            }
            connected = false;
            return controller != null;
        }

        public void OnControllerReport(HidReport report)
        {
            // function for calling the report parser method

            UpdateControllerState(report);

            if (controller != null)
            {
                controller.ReadReport(OnControllerReport);
            }
        }


        public void UpdateControllerState(HidReport report)
        {
            // function for updating the values of global variables based on the report the gamepad sends
            if ((report.ReportId != 0) || (report.Data.Length != 27))
            {
                return;     // probably other controller type / report type data passed
            }

            buttonOne = (report.Data[11] == 0xff);
            buttonTwo = (report.Data[12] == 0xff);
            buttonThree = (report.Data[13] == 0xff);
            buttonFour = (report.Data[14] == 0xff);
            buttonFrontL = ((report.Data[15] | report.Data[16]) > 0x80);
            buttonFrontR = ((report.Data[17] | report.Data[18]) > 0x80);
            buttonLeft = (report.Data[8] == 0xff);
            buttonRight = (report.Data[7] == 0xff);
            buttonTop = (report.Data[9] == 0xff);
            buttonBottom = (report.Data[10] == 0xff);
            buttonFrontL = ((report.Data[15] | report.Data[16]) > 0x80);
            buttonFrontR = ((report.Data[17] | report.Data[18]) > 0x80);
            joystickLeft.accelX = Math.Min(report.Data[3] - 127, 127) / 127.0;
            joystickLeft.accelY = Math.Min(report.Data[4] - 127, 127) / 127.0;
            joystickRight.accelX = Math.Min(report.Data[5] - 127, 127) / 127.0;
            joystickRight.accelY = Math.Min(report.Data[6] - 127, 127) / 127.0;

            // Deadband around 0 for joystick position
            if (Math.Abs(joystickLeft.accelX) < zeroOffset) joystickLeft.accelX = 0;
            if (Math.Abs(joystickLeft.accelY) < zeroOffset) joystickLeft.accelY = 0;
            if (Math.Abs(joystickRight.accelX) < zeroOffset) joystickRight.accelX = 0;
            if (Math.Abs(joystickRight.accelY) < zeroOffset) joystickRight.accelY = 0;
        }
        public void ControllerDisconnected()
        {
            // function for cling the connection with the gamepad
            controller.CloseDevice(); // close connection with controller
            controller = null; // reset the controller var
            connected = false;
        }

    }
}

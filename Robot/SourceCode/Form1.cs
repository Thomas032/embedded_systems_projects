using Microsoft.Win32;
using Robot.Essentials;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;


namespace Robot
{
    public partial class Form1 : Form
    {
        System.Timers.Timer controllerTimer { get; set; }
        BitOperations bitOperations { get; set; }
        PreciseDelay preciseDelay { get; set; }

        Controller controller { get; set; }

        RobotModel robotModel { get; set; }

        FileOperations fileOperations { get; set; }

        // global variables
        int controllerPullTimer = 10;
        int currentMode = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void SetupObjects()
        {
            // function for setting up the global objects and timer

            // setup global objects
            bitOperations = new BitOperations();
            preciseDelay = new PreciseDelay();
            controller = new Controller();
            robotModel = new RobotModel(this);
            fileOperations = new FileOperations();

            // setup timer object
            controllerTimer = new System.Timers.Timer();
            controllerTimer.Interval = controllerPullTimer;
            controllerTimer.AutoReset = true;
            controllerTimer.Elapsed += UpdateControllerData;
            controllerTimer.Start();

        }

        private void UpdateControllerData(object sender, ElapsedEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                // display the current controller data in form
                HandleControllerLogic();
            });
        }

        private void SetupComponents()
        {
            // function for the initial setup of components
            controller_connected_panel.Visible = false;
            controller_disconnected_panel.Visible = true;

            controller_pressed_btn.Text = "";
            motor_info_text.Text = "";
            direction_info_text.Text = "";

            // disable the init panel 
            ShowMotorInitData(false, false, false, false);

            // set current mode
            UpdateSpeedPanel();

            // set the file name input to disbaled
            file_name_text.Enabled = false;

            // disbale the move panel
            move_panel.Enabled = true;

            // set sombobox as pure combobox
            available_file_combo.DropDownStyle = ComboBoxStyle.DropDownList;

            // fill the combobox with data
            FillCombo();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            // function for inicializing the form and objects related to it
            // enable reading key press
            this.KeyPreview = true;
            SetupObjects();
            controller.ConnectController();
            SetupComponents();

            // run the motor init in async
            await Task.Run(() => robotModel.InitializeMotors());
        }

        private void Form1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // function for disconnecting the controller, so no problems arise
            controller.ControllerDisconnected();
            controller = null;
        }

        private void FillCombo()
        {
            // function that fills the combobox with data about usable files
            available_file_combo.Items.Clear();
            foreach(string file in fileOperations.GetAvailableFiles())
            {
                available_file_combo.Items.Add(file);
            }
        }
        private void HandleControllerLogic()
        {
            // function for displaying controller related info in the form

            // connection status
            if (controller.controller != null)
            {
                controller_connected_panel.Visible = true;
                controller_disconnected_panel.Visible = false;
            }
            else
            {
                controller_connected_panel.Visible = false;
                controller_disconnected_panel.Visible = true;
            }

            // show the latest clicked button
            controller_pressed_btn.Text = GetPressedBtnText(controller);

            // handle movement
            HandleMovement(controller);
        }

        public void HandleMovement(Controller controller)
        {
            // function for deciding which motor to move with
            
            // MOVEMENT controlls
            if (controller.buttonRight)
            {
                //robotModel.Step('B', 'R');
                robotModel.Move('B', 'R', currentMode);
            }

            if (controller.buttonLeft)
            {
                //robotModel.Step('B', 'L');
                robotModel.Move('B', 'L', currentMode);
            }

            if (controller.buttonTop)
            {
                //robotModel.Step('A', 'L');
                robotModel.Move('A', 'L', currentMode);
            }
            if (controller.buttonBottom)
            {
                //robotModel.Step('A', 'R');
                robotModel.Move('A', 'R', currentMode);
            }

            if(controller.joystickLeft.accelX == 0 && controller.joystickLeft.accelY == -1)
            {
                robotModel.Move('N', 'R', currentMode);
            }

            if (controller.joystickLeft.accelX == 0 && controller.joystickLeft.accelY == 1)
            {
                robotModel.Move('N', 'L', currentMode);
            }

            if (controller.buttonFrontR)
            {
                //robotModel.Step('G', 'R');
                robotModel.Move('G', 'R', currentMode);
            }

            if (controller.buttonFrontL)
            {
                //robotModel.Step('G', 'L');
                robotModel.Move('G', 'L', currentMode);
            }

            // GENERAL controlls
            if (controller.buttonThree)
            {
                // switch drive mode
                SwitchMode();
                //controller.buttonThreeLast = true;
            }


            if (controller.buttonTwo)
            {
                // start/stop the recording based on the nternal state of variables
                HandleFileIO();
            }
        }

        public void HandleFileIO()
        {
            // function for starting to record or saving the recorded file

            if(fileOperations.fileWriter == null)
            {
                // the connecion has not been established yet
                move_panel.Enabled = false;  
                file_name_text.Enabled = true;
                file_name_text.Text = " .bot"; // reset the value
                file_name_text.Select(0, 1);
            }
            else 
            {
                // connsection has been established -> end the recording process
                robotModel.recording = false;
                move_panel.Enabled = true;
                fileOperations.BreakConncetion();

                // refresh the data in combobx
                FillCombo();
                PanelNormal();
            }
        }

        public void SwitchMode()
        {
            // function for swittchin the drive mode var and changing the view

            currentMode++;
            if(currentMode >= 3)
            {
                currentMode = 0;
            }
            UpdateSpeedPanel();
        }

        public void UpdateSpeedPanel()
        {
            // function for handling the speed related panel logic
            strada_panel.Visible = currentMode == 0;
            sport_panel.Visible = currentMode == 1;
            corsa_panel.Visible = currentMode == 2;
        }

        public void PanelRecording()
        {
            // show the correct data when the programme is recording
            recording_text.Text = "ZAPNUTO";
            recording_panel.BackColor = Color.Red;
            recording_panel.ForeColor = Color.Black;
        }

        public void PanelNormal()
        {
            // show the correct data when the programme is not recording
            recording_text.Text = "VYPNUTO";
            recording_panel.BackColor = Color.Gray;
            recording_panel.ForeColor = Color.White;
        }

        public string GetPressedBtnText(Controller controller)
        {
            // function translating the internal state of buttons to string
            // Check each button and return the name of the pressed button
            if (controller.buttonOne) return "ButtonOne";
            if (controller.buttonTwo) return "ButtonTwo";
            if (controller.buttonThree) return "ButtonThree";
            if (controller.buttonFour) return "ButtonFour";
            if (controller.buttonFrontL) return "ButtonFrontL";
            if (controller.buttonFrontR) return "ButtonFrontR";
            if (controller.buttonLeft) return "ButtonLeft";
            if (controller.buttonRight) return "ButtonRight";
            if (controller.buttonTop) return "ButtonTop";
            if (controller.buttonBottom) return "ButtonBottom";

            // Check if JoystickLeft is tilted
            if (Math.Abs(controller.joystickLeft.accelX) > controller.zeroOffset ||
                Math.Abs(controller.joystickLeft.accelY) > controller.zeroOffset)
            {
                return $"JoyL: X={controller.joystickLeft.accelX}, Y={controller.joystickLeft.accelY}";
            }

            // Check if JoystickRight is tilted
            if (Math.Abs(controller.joystickRight.accelX) > controller.zeroOffset ||
                Math.Abs(controller.joystickRight.accelY) > controller.zeroOffset)
            {
                return $"JoyR: X={controller.joystickRight.accelX}, Y={controller.joystickRight.accelY}";
            }

            // If no button is pressed and no joystick is tilted, return an empty string or null
            return "";
        }

        public void ShowMotorInitData(bool baseMotorPanel, bool armMotorPanel, bool neckMotorPanel, bool gripperMotorPanel)
        {
            // function for showing the current status of robot arm inicialization
            init_base_panel.Visible = baseMotorPanel;
            init_arm_panel.Visible = armMotorPanel;
            init_neck_panel.Visible = neckMotorPanel;
            init_gripper_panel.Visible = gripperMotorPanel;
        }

        public void ShowMotorInfoData(char motor, char dir)
        {
            // function for displaying info about the current motor and direction
            Dictionary<char, string> motorDict = new Dictionary<char, string>{
                { 'B', "BASE" },
                { 'A', "ARM" },
                { 'N', "NECK" },
                { 'G', "GRIPPER" }
            };
            Dictionary<char, string> dirDict = new Dictionary<char, string>{
                { 'L', "LEFT/UP" },
                { 'R', "RIGHT/DOWN" },
            };

            motor_info_text.Text = motorDict[motor];
            direction_info_text.Text = dirDict[dir];
        }

        public bool ValidFileName(string name)
        {
            // function for assessing the validity of a file name with regular expressions
            if (name.EndsWith(".bot"))
            {
                return true;
            }
            return false;
        }

        private void keyDown(object sender, KeyEventArgs e)
        {
            // function for handling key down events
            // in the context of this app, only the filename input will be handeled

            if(e.KeyValue != 13)
            {
                // if not enter
                return;
            }

            // enter pressed -> check if input valid
            string name = file_name_text.Text;
            if (!ValidFileName(name))
            {
                file_name_text.Enabled = true;
                file_name_text.Text = " .bot";
                file_name_text.Select(0, 1);
                ShowError("Invalid file name");
                return;
            }
            
            bool status = fileOperations.InitFile(name);
            if (!status)
            {
                // if init went wrong ->  abort
                file_name_text.Enabled = true;
                file_name_text.Text = " .bot";
                file_name_text.Select(0, 1);
                ShowError("Invalid file name");
                return;
            }

            // init the motors to start at a reference point before recording
            robotModel.InitializeMotors();

            robotModel.recording = true;
            PanelRecording();
        }

        public void ShowError(string msg)
        {
            // function for displaying errors by showing a messagebox
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void move_button_Click(object sender, EventArgs e)
        {
            // function for handling the user selected file
            string selectedName = available_file_combo.SelectedItem as string;

            if(selectedName == "")
            {
                // if the selected name is blank -> make sure to show an error
                ShowError("Prosím vyberte soubor!");
                return;
            }

            // move the robot accordingly to the file
            robotModel.MoveByLine(selectedName);

        }
    }
}
